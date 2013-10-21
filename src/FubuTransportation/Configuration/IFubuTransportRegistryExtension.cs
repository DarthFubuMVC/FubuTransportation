﻿namespace FubuTransportation.Configuration
{
    /// <summary>
    /// Use to create reusable extensions to FubuTransportation
    /// applications
    /// </summary>
    public interface IFubuTransportRegistryExtension
    {
        void Configure(FubuTransportRegistry registry);
    }
}