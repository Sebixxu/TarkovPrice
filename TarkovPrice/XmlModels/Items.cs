using System.Collections.Generic;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace TarkovPrice.XmlModels
{
    [XmlRoot("items")]

    public class Items
    {
        [XmlElement("item")]
        public List<Item> ItemsCollection { get; set; }
    }
}