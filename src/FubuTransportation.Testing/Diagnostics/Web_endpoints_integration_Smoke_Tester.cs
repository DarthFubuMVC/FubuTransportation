﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Bottles;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration;
using FubuMVC.Katana;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Diagnostics.Visualization;
using FubuTransportation.InMemory;
using FubuTransportation.Polling;
using NUnit.Framework;

namespace FubuTransportation.Testing.Diagnostics
{
    [TestFixture]
    public class Web_endpoints_integration_Smoke_Tester
    {
        string appPath = Environment.CurrentDirectory
                .ParentDirectory().ParentDirectory().ParentDirectory()
                .AppendPath("FubuTransportation");


        [Test]
        public void the_message_handlers_visualization_can_be_shown()
        {
            using (var server = EmbeddedFubuMvcServer.For<DiagnosticApplication>(appPath))
            {
                server.Endpoints.Get<MessagesFubuDiagnostics>(x => x.get_messages())
                    .StatusCode.ShouldEqual(HttpStatusCode.OK);
            }

            InMemoryQueueManager.ClearAll();
        }

        [Test]
        public void the_channel_graph_visualization_can_be_shown()
        {
            using (var server = EmbeddedFubuMvcServer.For<DiagnosticApplication>(appPath))
            {
                server.Endpoints.Get<ChannelGraphFubuDiagnostics>(x => x.get_channels())
                    .StatusCode.ShouldEqual(HttpStatusCode.OK);
            }

            InMemoryQueueManager.ClearAll();
        }

        [Test, Explicit("Does work, but stupid pathing is defeating me here")]
        public void the_subscriptions_visualization_can_be_shown()
        {
            using (var server = EmbeddedFubuMvcServer.For<DiagnosticApplication>(appPath))
            {
                server.Endpoints.Get<SubscriptionsFubuDiagnostics>(x => x.get_subscriptions())
                    .StatusCode.ShouldEqual(HttpStatusCode.OK);
            }

            InMemoryQueueManager.ClearAll();
        }

        [Test]
        public void the_scheduled_job_visualization_can_be_shown()
        {
            using (var server = EmbeddedFubuMvcServer.For<DiagnosticApplication>(appPath))
            {
                server.Endpoints.Get<ScheduledJobsFubuDiagnostics>(x => x.get_scheduled_jobs())
                    .StatusCode.ShouldEqual(HttpStatusCode.OK);
            }

            InMemoryQueueManager.ClearAll();
        }
    }

    public class DiagnosticApplication : FubuTransportRegistry, IApplicationSource
    {
        public DiagnosticApplication()
        {
            EnableInMemoryTransport();
        }

        public FubuApplication BuildApplication()
        {
            var registry = new FubuRegistry();
            registry.Import<DiagnosticApplication>();

            registry.AlterSettings<TransportSettings>(x => {
                x.DelayMessagePolling = Int32.MaxValue;
                x.ListenerCleanupPolling = Int32.MaxValue;
            });

            return FubuApplication.For(registry).StructureMap();
        }
    }
}