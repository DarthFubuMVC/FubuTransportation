﻿using System;
using FubuMVC.Core;
using FubuMVC.Core.Diagnostics;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using StructureMap;
using FubuMVC.StructureMap;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class FullStackConfigurationIntegrationTester
    {
        [Test]
        public void has_all_the_chains_we_expect()
        {
            var container = new Container();
            FubuTransport.For<MyFirstTransport>().StructureMap(container).Bootstrap();

            var graph = container.GetInstance<BehaviorGraph>();

            Console.WriteLine(FubuApplicationDescriber.WriteDescription());

            graph.Behaviors.Count(x => typeof (Foo1) == x.InputType()).ShouldEqual(1);
            graph.Behaviors.Count(x => typeof (Foo2) == x.InputType()).ShouldEqual(1);
            graph.Behaviors.Count(x => typeof (Foo3) == x.InputType()).ShouldEqual(1);
            graph.Behaviors.Count(x => typeof (Foo4) == x.InputType()).ShouldEqual(1);
        }

        [Test]
        public void has_all_the_chains_we_expect_through_FubuApplication()
        {
            var container = new Container();
            var registry = new FubuRegistry();
            registry.Import<MyFirstTransport>();

            FubuApplication.For(registry).StructureMap(container).Bootstrap();

            var graph = container.GetInstance<BehaviorGraph>();

            Console.WriteLine(FubuApplicationDescriber.WriteDescription());

            graph.Behaviors.Count(x => typeof(Foo1) == x.InputType()).ShouldEqual(1);
            graph.Behaviors.Count(x => typeof(Foo2) == x.InputType()).ShouldEqual(1);
            graph.Behaviors.Count(x => typeof(Foo3) == x.InputType()).ShouldEqual(1);
            graph.Behaviors.Count(x => typeof(Foo4) == x.InputType()).ShouldEqual(1);
        }
    }


    public class MyFirstTransport : FubuTransportRegistry
    {
        
    }


    public class MyConsumer
    {
        public void Foo1(Foo1 input) { }
        public void Foo2(Foo2 input) { }
        public void Foo3(Foo3 input) { }
    }

    public class MyOtherConsumer
    {
        public void Foo2(Foo2 input) { }
        public void Foo3(Foo3 input) { }
        public void Foo4(Foo4 input) { }
    }

    public class Foo1 { }
    public class Foo2 { }
    public class Foo3 { }
    public class Foo4 { }

}