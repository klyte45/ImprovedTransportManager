extern alias UUI;

namespace ImprovedTransportManager.TransportSystems
{
    public class TransportInfoContainer
    {
        public TransportInfo Local { get; internal set; }
        public TransportInfo Intercity { get; internal set; }
    }
}
