using System.Xml.Serialization;

namespace TarkovPrice.XmlModels
{
    public class Configuration
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        public Configuration()
        {
            
        }

        public Configuration(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}