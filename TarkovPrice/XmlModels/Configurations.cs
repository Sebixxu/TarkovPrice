using System.Collections.Generic;
using System.Xml.Serialization;

namespace TarkovPrice.XmlModels
{
    [XmlRoot("configurations")]

    public class Configurations
    {
        [XmlElement("configuration")]
        public List<Configuration> ConfigurationCollection { get; set; } = new List<Configuration>();
    }
}