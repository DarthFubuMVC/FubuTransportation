using System;
using FubuCore;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.Behaviors;
using FubuTransportation.Runtime;

namespace FubuTransportation.Registration.Nodes
{
    public class SagaNode : BehaviorNode
    {
        private readonly Type _handlerType;
        private readonly Type _inputType;
        private readonly Type _behaviorType;
        private readonly bool _canStartSaga;
        private ObjectDef _repositoryDef;

        public SagaNode(Type handlerType, Type sagaStateType, Type inputType)
        {
            _handlerType = handlerType;
            _inputType = inputType;
            _behaviorType = typeof(SagaHandlerBehavior<,,>).MakeGenericType(handlerType, sagaStateType, inputType);
        }

        protected override ObjectDef buildObjectDef()
        {
            var objectDef = new ObjectDef(_behaviorType);
            objectDef.Dependency(createSagaSetterLambda());
            objectDef.Dependency(createIsCompletedGetterLambda());
            objectDef.Dependency(new ConfiguredDependency(typeof(ISagaRepository<>).MakeGenericType(_inputType), _repositoryDef));
            return objectDef;
        }

        public void SagaRepositoryByCorrelationId(ObjectDef repositoryDef)
        {
            useSagaRepository(repositoryDef);
            //TODO This seems hokey and fragile, should validation also check for interface template?
            if (_repositoryDef.Type.IsOpenGeneric())
            {
                _repositoryDef = new ObjectDef(_repositoryDef.Type.MakeGenericType(_inputType));
            }

            if (_inputType.HasProperty("CorrelationId"))
            {
                _repositoryDef.Dependency(createCorrelationIdGetterLambda());
            }
        }

        public void SagaRepositoryByAlternateId(ObjectDef repositoryDef)
        {
            useSagaRepository(repositoryDef);
        }

        private void useSagaRepository(ObjectDef repositoryDef)
        {
            if(!repositoryDef.Type.ImplementsInterfaceTemplate(typeof(ISagaRepository<>)))
                throw new ArgumentException("repositoryDef");

            _repositoryDef = repositoryDef;
        }

        private ValueDependency createCorrelationIdGetterLambda()
        {
            var property = _inputType.GetProperty("CorrelationId");
            var func = FuncBuilder.CompileGetter(property);
            return new ValueDependency(func.GetType(), func);
        }

        private ValueDependency createSagaSetterLambda()
        {
            var property = _handlerType.GetProperty("State");
            var func = FuncBuilder.CompileSetter(property);
            return new ValueDependency(func.GetType(), func);
        }

        private ValueDependency createIsCompletedGetterLambda()
        {
            var property = _handlerType.GetProperty("IsCompleted");
            var func = FuncBuilder.CompileGetter(property);
            return new ValueDependency(func.GetType(), func);
        }

        public override BehaviorCategory Category
        {
            get { return BehaviorCategory.Wrapper; }
        }
    }
}