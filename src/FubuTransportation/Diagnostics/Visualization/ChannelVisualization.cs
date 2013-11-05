using FubuTransportation.Configuration;

namespace FubuTransportation.Diagnostics.Visualization
{
    public class ChannelVisualization
    {
        public ChannelGraph Graph { get; set; }
        public TransportsTag Transports { get; set; }
        public ChannelsTableTag Channels { get; set; }
        public SerializersTag Serializers { get; set; }
    }
}
