﻿using System;
using System.Linq;
using FubuTestingSupport;
using FubuTransportation.Monitoring;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Monitoring
{
    [TestFixture]
    public class PersistentTaskAgentTester : InteractionContext<PersistentTaskAgent>
    {
        [Test]
        public void assert_available_happy()
        {
            ClassUnderTest.AssertAvailable().Wait();

            MockFor<IPersistentTask>().AssertWasCalled(x => x.AssertAvailable());
        }

        [Test]
        public void assert_available_sad_path()
        {
            var ex = new DivideByZeroException();

            MockFor<IPersistentTask>().Stub(x => x.AssertAvailable())
                .Throw(ex);

            var task = ClassUnderTest.AssertAvailable();

                task.Wait();



            task.Result.ShouldEqual(HealthStatus.Error);
        }

        [Test]
        public void activate_happy_path()
        {
            ClassUnderTest.Activate().Wait();
            MockFor<IPersistentTask>().AssertWasCalled(x => x.Activate());
        }

        [Test]
        public void deactivate_happy_path()
        {
            ClassUnderTest.Deactivate().Wait();
            MockFor<IPersistentTask>().AssertWasCalled(x => x.Deactivate());
        }

        [Test]
        public void assign_peers()
        {
            var peers = Services.CreateMockArrayFor<ITransportPeer>(4);

            var peer = MockFor<ITransportPeer>();

            MockFor<IPersistentTask>().Stub(x => x.SelectOwner(peers))
                .Return(peer.ToCompletionTask());

            var task = ClassUnderTest.AssignOwner(peers);
            task.Wait();

            task.Result.ShouldBeTheSameAs(peer);
        }
    }
}