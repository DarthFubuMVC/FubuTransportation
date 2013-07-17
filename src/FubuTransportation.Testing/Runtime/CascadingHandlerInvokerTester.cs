﻿using System;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class CascadingHandlerInvokerTester : InteractionContext<CascadingHandlerInvoker<ITargetHandler,Input,Output>>
    {
        private Input theInput;
        private Output expectedOutput;

        protected override void beforeEach()
        {
            Func<ITargetHandler, Input, Output> func = (c, i) => c.OneInOneOut(i);

            Services.Inject<Func<ITargetHandler, Input, Output>>(func);

            theInput = new Input();
            expectedOutput = new Output();

            var request = new InMemoryFubuRequest();
            request.Set(theInput);
            Services.Inject<IFubuRequest>(request);

            MockFor<ITargetHandler>().Expect(x => x.OneInOneOut(theInput)).Return(expectedOutput);

            ClassUnderTest.InsideBehavior = MockFor<IActionBehavior>();

            ClassUnderTest.Invoke();
        }

        [Test]
        public void should_invoke_the_handler_method()
        {
            VerifyCallsFor<ITargetHandler>();
        }

        [Test]
        public void records_the_output_back_to_outgoing_messages()
        {
            MockFor<IOutgoingMessages>().AssertWasCalled(x => x.Enqueue(expectedOutput));
        }

        [Test]
        public void continues_on_to_the_inside_behavior()
        {
            MockFor<IActionBehavior>().AssertWasCalled(x => x.Invoke());
        }
    }

    [TestFixture]
    public class CascadingHandlerInvoker_with_base_class_Tester : InteractionContext<CascadingHandlerInvoker<ITargetHandler, Input, Output>>
    {
        private SpecialInput theInput;
        private Output expectedOutput;

        protected override void beforeEach()
        {
            Func<ITargetHandler, Input, Output> func = (c, i) => c.OneInOneOut(i);

            Services.Inject<Func<ITargetHandler, Input, Output>>(func);

            theInput = new SpecialInput();
            expectedOutput = new Output();

            var request = new InMemoryFubuRequest();
            request.Set(theInput);
            Services.Inject<IFubuRequest>(request);

            MockFor<ITargetHandler>().Expect(x => x.OneInOneOut(theInput)).Return(expectedOutput);

            ClassUnderTest.InsideBehavior = MockFor<IActionBehavior>();

            ClassUnderTest.Invoke();
        }

        [Test]
        public void should_invoke_the_controller_method()
        {
            VerifyCallsFor<ITargetHandler>();
        }

        [Test]
        public void records_the_output_back_to_outgoing_messages()
        {
            MockFor<IOutgoingMessages>().AssertWasCalled(x => x.Enqueue(expectedOutput));
        }

        [Test]
        public void continues_on_to_the_inside_behavior()
        {
            MockFor<IActionBehavior>().AssertWasCalled(x => x.Invoke());
        }
    }

}