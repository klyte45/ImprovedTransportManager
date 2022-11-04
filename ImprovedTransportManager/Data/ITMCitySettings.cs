using Kwytto.Data;
using System.Xml.Serialization;

namespace ImprovedTransportManager.Data
{
    public class ITMCitySettings : DataExtensionBase<ITMCitySettings>
    {
        public override string SaveId => $"K45_ITM_ITMCitySettings";

        [XmlAttribute("expertMode")]
        public bool expertMode = false;
        [XmlAttribute("expressBuses")]
        public bool expressBuses = true;
        [XmlAttribute("exporessTrams")]
        public bool expressTrams = true;
        [XmlAttribute("expressTrolleybus")]
        public bool expressTrolleybus = true;
        [XmlAttribute("disableUnbunchingTerminals")]
        public bool disableUnbunchingTerminals = false;

    }
}
