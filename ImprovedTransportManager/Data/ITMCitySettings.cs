using ICities;
using ImprovedTransportManager.TransportSystems;
using Kwytto.Data;
using Kwytto.Utils;
using System.IO;
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
        [XmlElement("MaintenaceCostsPer1000Passengers")]
        public SimpleEnumerableList<TransportSystemType, uint> costPerThousandPassengers = new SimpleEnumerableList<TransportSystemType, uint>();


        internal const string DEFAULTS_FILENAME = "CitySettingsDefault.xml";
        public static string DefaultsFilePath => Path.Combine(ModInstance.Instance.ModRootFolder, DEFAULTS_FILENAME);
        public override ITMCitySettings LoadDefaults(ISerializableData serializableData)
        {
            if (File.Exists(DefaultsFilePath))
            {
                try
                {
                    return XmlUtils.DefaultXmlDeserialize<ITMCitySettings>(File.ReadAllText(DefaultsFilePath));
                }
                catch { }
            }
            return null;
        }

        internal static void ExportAsDefault()
        {
            KFileUtils.EnsureFolderCreation(ModInstance.Instance.ModRootFolder);
            File.WriteAllText(DefaultsFilePath, XmlUtils.DefaultXmlSerialize(Instance));
        }

        internal static void ImportFromDefault()
        {
            if (Instance.LoadDefaults(null) is ITMCitySettings s)
            {
                Instance = s;
            }
        }
    }
}
