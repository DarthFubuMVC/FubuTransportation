﻿using FubuCore;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Subscriptions;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class LightningQueuesTransport_disabled_behavior_Tester : InteractionContext<LightningQueuesTransport>
    {
        private LightningQueueSettings theSettings;

        protected override void beforeEach()
        {
            theSettings = new LightningQueueSettings();
            Services.Inject(theSettings);
        }

        [Test]
        public void have_channels_and_not_set_to_disabled()
        {
            theSettings.DisableIfNoChannels = false;
            theSettings.Disabled = false;

            ClassUnderTest.Disabled(new ChannelNode[]{new ChannelNode(), })
                .ShouldBeFalse();
        }


        [Test]
        public void disable_if_settings_disabled_is_true()
        {
            theSettings.DisableIfNoChannels = false;
            theSettings.Disabled = true;

            ClassUnderTest.Disabled(new ChannelNode[] { new ChannelNode(), })
                .ShouldBeTrue();
        }

        [Test]
        public void not_disabled_if_disabling_on_no_channels_is_on_but_there_are_channels()
        {
            theSettings.DisableIfNoChannels = true;
            theSettings.Disabled = false;

            // You have a matching channel
            ClassUnderTest.Disabled(new ChannelNode[] { new ChannelNode(), })
                .ShouldBeFalse();
        }

        [Test]
        public void disable_if_there_are_no_channels()
        {
            theSettings.DisableIfNoChannels = true;
            theSettings.Disabled = false;

            // You have a matching channel
            ClassUnderTest.Disabled(new ChannelNode[0])
                .ShouldBeTrue();
        }
    }

    [TestFixture]
    public class Start_with_FubuTransportation_LightningQueues_but_no_channels
    {
        [Test]
        public void do_not_blow_up()
        {
            var container = new Container();
            container.Inject(new LightningQueueSettings
            {
                DisableIfNoChannels = true
            });

            using (var runtime = FubuTransport.DefaultPolicies().StructureMap(container).Bootstrap())
            {
                // just looking for the absence of an exception here
            }
        }

        [Test]
        public void does_blow_up_if_not_opted_into_the_disable_behavior()
        {
            var container = new Container();
            container.Inject(new LightningQueueSettings
            {
                DisableIfNoChannels = false
            });

            Exception<FubuException>.ShouldBeThrownBy(() => {
                using (var runtime = FubuTransport.DefaultPolicies().StructureMap(container).Bootstrap())
                {
                }
            });


        }
    }
}