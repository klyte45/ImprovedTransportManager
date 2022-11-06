using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ImprovedTransportManager.Xml
{
    public class ITMTransportLineXml
    {
        private string customIdentifier;

        [XmlAttribute("cachedTransportType")]
        public TransportSystemType CachedTransportType { get; set; }

        [XmlElement("AssetsList")]
        public SimpleXmlList<string> SelfAssetListXml
        {
            get => new SimpleXmlList<string>(SelfAssetList.Select(x => x.name));
            set => SelfAssetList = value.GetAllLoadedForType(CachedTransportType);
        }

        [XmlIgnore]
        public HashSet<VehicleInfo> SelfAssetList { get; private set; } = new HashSet<VehicleInfo>();

        [XmlElement("AllowedDepots")]
        public SimpleXmlList<ushort> AllowedDepotsXml
        {
            get => new SimpleXmlList<ushort>(AllowedDepots);
            set => AllowedDepots = new HashSet<ushort>(value);
        }

        [XmlIgnore]
        public HashSet<ushort> AllowedDepots { get; private set; } = new HashSet<ushort>();

        [XmlAttribute("customIdentifier")]
        public string CustomCode
        {
            get => customIdentifier; set
            {
                customIdentifier = value.TrimToNull();
            }
        }

        [XmlAttribute("budgetGroup")]
        public byte BudgetGroup { get; set; }
        [XmlAttribute("fareGroup")]
        public byte FareGroup { get; set; }
        [XmlAttribute("assetGroup")]
        public byte AssetGroup { get; set; }
        [XmlAttribute("depotGroup")]
        public byte DepotGroup { get; set; }
        [XmlAttribute("startAtTerminal")]
        public bool m_requireLineStartTerminal = true;
        [XmlAttribute("ignoreTerminalsMandatoryStop")]
        public bool m_ignoreTerminalsMandatoryStop;
    }
}
