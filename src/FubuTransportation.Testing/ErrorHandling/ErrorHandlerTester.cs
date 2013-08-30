﻿using System;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using FubuTestingSupport;
using Rhino.Mocks;

namespace FubuTransportation.Testing.ErrorHandling
{
    [TestFixture]
    public class ErrorHandlerTester
    {
        [Test]
        public void continuation_is_move_to_error_queue_by_default()
        {
            new ErrorHandler().Continuation
                              .ShouldBeOfType<MoveToErrorQueue>();
        }

        [Test]
        public void matches_with_no_rules_is_true()
        {
            new ErrorHandler().Matches(ObjectMother.Envelope(), new Exception())
                .ShouldBeTrue();
        }

        [Test]
        public void if_there_are_conditions_all_conditions_must_be_true_to_match()
        {
            var exception = new Exception();
            var envelope = ObjectMother.Envelope();

            var matchingCondition1 = MockRepository.GenerateMock<IErrorCondition>();
            var matchingCondition2 = MockRepository.GenerateMock<IErrorCondition>();
            var matchingCondition3 = MockRepository.GenerateMock<IErrorCondition>();
            var conditionThatDoesNotMatch = MockRepository.GenerateMock<IErrorCondition>();

            matchingCondition1.Stub(x => x.Matches(envelope, exception)).Return(true);
            matchingCondition2.Stub(x => x.Matches(envelope, exception)).Return(true);
            matchingCondition3.Stub(x => x.Matches(envelope, exception)).Return(true);
            conditionThatDoesNotMatch.Stub(x => x.Matches(envelope, exception)).Return(false);

            var handler = new ErrorHandler();

            handler.AddCondition(matchingCondition1);
            handler.Matches(envelope, exception).ShouldBeTrue();

            handler.AddCondition(matchingCondition2);
            handler.Matches(envelope, exception).ShouldBeTrue();

            handler.AddCondition(matchingCondition3);
            handler.Matches(envelope, exception).ShouldBeTrue();

            handler.AddCondition(conditionThatDoesNotMatch);
            handler.Matches(envelope, exception).ShouldBeFalse();
        }

        [Test]
        public void if_nothing_matches_do_not_return_a_continuation()
        {
            var exception = new Exception();
            var envelope = ObjectMother.Envelope();

            var conditionThatDoesNotMatch = MockRepository.GenerateMock<IErrorCondition>();


            var handler = new ErrorHandler();
            handler.AddCondition(conditionThatDoesNotMatch);

            handler.DetermineContinuation(envelope, exception)
                .ShouldBeNull();
        }

        [Test]
        public void return_the_continuation_if_the_handler_matches()
        {
            var exception = new Exception();
            var envelope = ObjectMother.Envelope();
            
            var matchingCondition1 = MockRepository.GenerateMock<IErrorCondition>();
            matchingCondition1.Stub(x => x.Matches(envelope, exception)).Return(true);

            var handler = new ErrorHandler();

            handler.AddCondition(matchingCondition1);
            handler.Continuation = MockRepository.GenerateMock<IContinuation>();

            handler.Matches(envelope, exception).ShouldBeTrue();

            handler.DetermineContinuation(envelope, exception)
                .ShouldBeTheSameAs(handler.Continuation);
        }


    }
}