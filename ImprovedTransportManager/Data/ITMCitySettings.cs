using Kwytto.Data;
using System.Xml.Serialization;

namespace ImprovedTransportManager.Data
{
    public class ITMCitySettings : DataExtensionBase<ITMCitySettings>
    {
        public override string SaveId => $"K45_ITM_ITMCitySettings";

        [XmlAttribute("expertMode")]
        public bool expertMode = false;

    }
}
