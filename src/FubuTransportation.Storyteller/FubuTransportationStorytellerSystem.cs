﻿using FubuTransportation.Configuration;
using FubuTransportation.Serenity;
using ServiceNode;
using WebsiteNode;

namespace FubuTransportation.Storyteller
{
    public class FubuTransportationStorytellerSystem : FubuTransportSystem<WebsiteApplication>
    {
        public FubuTransportationStorytellerSystem()
        {
            AddRemoteSubSystem("ServiceNode", x => {
                x.UseParallelServiceDirectory("ServiceNode");
                x.Setup.ShadowCopyFiles = false.ToString();
            });

            OnContextCreation(TextFileWriter.Clear);
        }


    }
}