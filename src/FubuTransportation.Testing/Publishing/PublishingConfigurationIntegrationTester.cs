﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using FubuMVC.Core;
using FubuMVC.Core.Ajax;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Resources.Conneg;
using FubuMVC.Katana;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Web;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap;

namespace FubuTransportation.Testing.Publishing
{
    [TestFixture]
    public class PublishingConfigurationIntegrationTester
    {
        private BehaviorGraph theGraph;
        private BehaviorChain chain;
        private FubuRuntime theRuntime;
        private Container container;
        private IServiceBus theServiceBus;

        [SetUp]
        public void SetUp()
        {
            FubuTransport.AllQueuesInMemory = true;

            container = new Container();
            container.Inject(new TransportSettings
            {
                DelayMessagePolling = Int32.MaxValue,
                ListenerCleanupPolling = Int32.MaxValue
            });
            theServiceBus = MockRepository.GenerateMock<IServiceBus>();

            var registry = new FubuRegistry();
            registry.Actions.IncludeType<MessageOnePublisher>();

            theRuntime = FubuApplication.For(registry).StructureMap(container).Bootstrap();
            theGraph = theRuntime.Factory.Get<BehaviorGraph>();
            chain = theGraph.BehaviorFor<MessageOnePublisher>(x => x.post_message1(null));

            container.Inject(theServiceBus);
        
        }

        [TearDown]
        public void Teardown()
        {
            theRuntime.Dispose();
        }

        [Test]
        public void end_to_end_test()
        {
            using (var server = new EmbeddedFubuMvcServer(theRuntime))
            {
                var response = server.Endpoints.PostJson(new Message1Input());

                theServiceBus.AssertWasCalled(x => x.Send(new Message1()), x => x.IgnoreArguments());


                response.StatusCode.ShouldEqual(HttpStatusCode.OK);
                response.ReadAsText().ShouldContain("\"success\":true");

            }
        }

        [Test]
        public void should_find_the_IEventPublishers_loaded_into_memory()
        {
            chain.ShouldNotBeNull();
            theGraph.BehaviorFor<MessageTwoPublisher>(x => x.post_message2(null))
                .ShouldNotBeNull();
        }

        [Test]
        public void input_type_should_be_the_input_type_of_the_publisher_method()
        {
            chain.InputType().ShouldEqual(typeof(Message1Input));
        }

        [Test]
        public void resource_type_should_be_AjaxContinuation()
        {
            chain.ResourceType().ShouldEqual(typeof(AjaxContinuation));
        }

        [Test]
        public void should_be_a_PublishEvent_node_directly_after_the_publishing_action()
        {
            // diagnostics are in here now.
            chain.FirstCall().Next.ShouldBeOfType<SendsMessage>()
                .EventType.ShouldEqual(typeof(Message1));
        }

    }

    public class MessageOnePublisher : ISendMessages
    {
        public Message1 post_message1(Message1Input input)
        {
            return new Message1();
        }

        
    }

    public class MessageTwoPublisher : ISendMessages
    {
        public Message2 post_message2(Message2Input input)
        {
            return new Message2();
        }
    }

    public class Message1Input{}
    public class Message2Input{}
}