using System;
using System.Xml.Serialization;

namespace ImprovedTransportManager.Xml
{
    public class ITMNodeSettingsXmlItem
    {
        [XmlIgnore]
        public InstanceID Id { get; set; }
        [XmlAttribute("nameRelativeInstanceId")]
        public string NameRelativeInstanceId
        {
            get
            {
                return Id.RawData.ToString("X8");
            }
            set
            {
                Id = new InstanceID
                {
                    RawData = Convert.ToUInt32(value, 16)
                };
            }
        }
    }
}
