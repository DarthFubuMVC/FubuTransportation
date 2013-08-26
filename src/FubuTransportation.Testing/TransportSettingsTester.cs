﻿using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class TransportSettingsTester
    {
        [Test]
        public void debug_is_disabled_by_default()
        {
            new TransportSettings().DebugEnabled.ShouldBeFalse();
        }

        [Test]
        public void the_default_delayed_message_polling_is_5_seconds()
        {
            new TransportSettings().DelayMessagePolling.ShouldEqual(5000);
        }

    }
}