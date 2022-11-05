using System.Xml.Serialization;

namespace ImprovedTransportManager.Xml
{
    public class ITMVehicleAssetXml
    {
        [XmlAttribute]
        public int OverwrittenCapacity { get; set; }
    }
}
