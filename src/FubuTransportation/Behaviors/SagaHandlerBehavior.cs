using System;
using FubuMVC.Core.Behaviors;
using FubuTransportation.Runtime;

namespace FubuTransportation.Behaviors
{
    public class SagaHandlerBehavior<THandler, TSagaState, TMessage> : IActionBehavior
        where THandler : class
        where TMessage : class
        where TSagaState : class, new()
    {
        private readonly THandler _handler;
        private readonly ISagaRepository<TMessage> _sagaRepository;
        private readonly Action<THandler, TSagaState> _stateSetter;
        private readonly Func<THandler, bool> _isComplete;

        public SagaHandlerBehavior(THandler handler, 
            ISagaRepository<TMessage> sagaRepository, 
            Action<THandler, TSagaState> stateSetter, 
            Func<THandler, bool> isComplete)
        {
            _handler = handler;
            _sagaRepository = sagaRepository;
            _stateSetter = stateSetter;
            _isComplete = isComplete;
        }

        public IActionBehavior Inner { get; set; }

        public void Invoke()
        {
            var state = _sagaRepository.Load<TSagaState>();

            //TODO Deal with non-initiating message through continuation, or discarded message?

            _stateSetter(_handler, state);
            Inner.Invoke();

            //TODO: Audit message for starting and completing?
            if (_isComplete(_handler))
            {
                _sagaRepository.Delete(state);
            }
            else
            {
                _sagaRepository.Save(state);
            }
        }

        public void InvokePartial()
        {
            Invoke();
        }
    }
}