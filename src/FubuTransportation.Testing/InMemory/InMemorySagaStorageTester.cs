﻿using System;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.InMemory;
using FubuTransportation.Sagas;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.InMemory
{
    [TestFixture]
    public class when_building_the_object_def_for_an_in_memory_saga_storage_happy_path
    {
        private ObjectDef objectDef;

        [SetUp]
        public void SetUp()
        {
            var storage = new InMemorySagaStorage();

            objectDef = storage.RepositoryFor(new SagaTypes
            {
                MessageType = typeof(SagaMessageOne),
                StateType = typeof(MySagaState)
            });
        }

        [Test]
        public void should_be_in_memory_repository_type()
        {
            objectDef.Type.ShouldEqual(typeof(InMemorySagaRepository<MySagaState, SagaMessageOne>));
        }

        [Test]
        public void state_id_getter()
        {
            var state = new MySagaState
            {
                Id = Guid.NewGuid()
            };

            objectDef.FindDependencyValueFor<Func<MySagaState, Guid>>()
                (state).ShouldEqual(state.Id);
        }

        [Test]
        public void message_id_getter()
        {
            var message = new SagaMessageOne()
            {
                CorrelationId = Guid.NewGuid()
            };

            objectDef.FindDependencyValueFor<Func<SagaMessageOne, Guid>>()
                (message).ShouldEqual(message.CorrelationId);
        }
    }


    [TestFixture]
    public class InMemorySagaStorageTester
    {
        [Test]
        public void sad_path()
        {
            new InMemorySagaStorage().RepositoryFor(new SagaTypes
            {
                MessageType = GetType(),
                StateType = GetType()
            }).ShouldBeNull();
        }
    }

    public class MySagaState
    {
        public Guid Id { get; set; }
    }

    public class SagaMessageOne
    {
        public Guid CorrelationId { get; set; }
    }

    public class SagaMessageTwo
    {
        public Guid CorrelationId { get; set; }
    }

    public class SagaMessageThree
    {
        public Guid CorrelationId { get; set; }
    }

    public class SimpleSagaHandler : IStatefulSaga<Sagas.MySagaState>
    {
        public bool IsCompleted()
        {
            throw new NotImplementedException();
        }

        public Sagas.MySagaState State { get; set; }

        public void Start(Sagas.SagaMessageOne one)
        {

        }

        public void Second(Sagas.SagaMessageTwo two)
        {

        }

        public void Last(Sagas.SagaMessageThree three)
        {

        }
    }
}