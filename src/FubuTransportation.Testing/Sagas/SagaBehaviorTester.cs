﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuTestingSupport;
using FubuTransportation.Sagas;
using FubuTransportation.Testing.Runtime;
using NUnit.Framework;
using Rhino.Mocks;
using Message1 = FubuTransportation.Testing.Events.Message1;

namespace FubuTransportation.Testing.Sagas
{
    [TestFixture]
    public class when_the_state_can_be_found_and_not_completed : SagaBehaviorContext
    {
        protected override void theContextIs()
        {
            theInitialStateCanBeFound();
            theHandlerHasStateAfterTheMessageIsProcessed();
            isSagaCompletedByThisMessage = false;
        }

        [Test]
        public void should_set_the_state_on_the_handler()
        {
            theHandler.AssertWasCalled(x => x.State = theInitialState);
        }

        [Test]
        public void should_call_the_inner_behavior()
        {
            theInnerBehavior.AssertWasCalled(x => x.Invoke());
        }

        [Test]
        public void should_persist_the_state()
        {
            theRepository.AssertWasCalled(x => x.Save(theResultingState, theMessage));
        }
    }

    [TestFixture]
    public class when_the_state_can_be_found_and_is_completed_by_the_message : SagaBehaviorContext
    {
        protected override void theContextIs()
        {
            theInitialStateCanBeFound();
            theHandlerHasStateAfterTheMessageIsProcessed();
            isSagaCompletedByThisMessage = true;
        }

        [Test]
        public void should_set_the_state_on_the_handler()
        {
            theHandler.AssertWasCalled(x => x.State = theInitialState);
        }

        [Test]
        public void should_call_the_inner_behavior()
        {
            theInnerBehavior.AssertWasCalled(x => x.Invoke());
        }

        [Test]
        public void should_delete_the_state()
        {
            theRepository.AssertWasCalled(x => x.Delete(theResultingState, theMessage));
        }
    }

    [TestFixture]
    public class when_the_state_cannot_be_found_and_is_not_created_by_the_message : SagaBehaviorContext
    {
        protected override void theContextIs()
        {
            theInitialStateCannotBeFound();
            theHandlerHasNoStateAfterTheMessageIsProcessed();
            

            ClassUnderTest.Invoke();
        }

        [Test]
        public void should_call_the_inner_behavior()
        {
            theInnerBehavior.AssertWasCalled(x => x.Invoke());
        }


        [Test]
        public void updates_and_deletes_nothing()
        {
            theRepository.AssertWasNotCalled(x => x.Save(null, theMessage), x => x.IgnoreArguments());
            theRepository.AssertWasNotCalled(x => x.Delete(null, theMessage), x => x.IgnoreArguments());
        }
    }


    public abstract class SagaBehaviorContext : InteractionContext<SagaBehavior<SagaState, Message1, ITestingSagaHandler>>
    {
        protected Message1 theMessage;
        protected IList<Message1> theMessageList;
        protected SagaState theInitialState;
        protected ITestingSagaHandler theHandler;
        protected IActionBehavior theInnerBehavior;
        protected SagaState theResultingState = new SagaState();
        protected ISagaRepository<SagaState, Message1> theRepository;

        protected override void beforeEach()
        {
            theMessage = new Message1();
            theMessageList = new List<Message1>{new Message1()};
            MockFor<IFubuRequest>().Stub(x => x.Find<Message1>())
                                   .Return(theMessageList);

            theMessage = theMessageList.FirstOrDefault();
//            MockFor<IFubuRequest>().Stub(x => x.Get<Message1>())
//                .Return(theMessage);
            theInitialState = new SagaState();

            theHandler = MockFor<ITestingSagaHandler>();

            theInnerBehavior = MockFor<IActionBehavior>();
            ClassUnderTest.Inner = theInnerBehavior;

            theRepository = MockFor<ISagaRepository<SagaState, Message1>>();

            theContextIs();

            ClassUnderTest.Invoke();
        }

        protected abstract void theContextIs();

        protected bool isSagaCompletedByThisMessage
        {
            set { theHandler.Stub(x => x.IsCompleted()).Return(value); }
        }

        protected void theHandlerHasStateAfterTheMessageIsProcessed()
        {
            theHandler.Stub(x => x.State).Return(theResultingState);
        }

        protected void theHandlerHasNoStateAfterTheMessageIsProcessed()
        {
            // no - op
        }

        protected void theInitialStateCanBeFound()
        {
            MockFor<ISagaRepository<SagaState, Message1>>()
                .Stub(x => x.Find(theMessage)).Return(theInitialState);
        }

        protected void theInitialStateCannotBeFound()
        {
            // nothing
        }


    }



    public class SagaState
    {
        public Guid CorrelationId { get; set; }
    }

    public interface ITestingSagaHandler : IStatefulSaga<SagaState>
    {
        
    }

}