using System.Xml.Serialization;

namespace TarkovPrice.XmlModels
{
    public class Item
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("itemName")]
        public string ItemName { get; set; }

        [XmlAttribute("imageName")]
        public string ImageName { get; set; }
    }
}