using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TarkovPrice.Interfaces;
using TarkovPrice.XmlModels;

namespace TarkovPrice
{
    public class FileAccess : IFileAccess
    {
        public Items ReadXmlItemsData()
        {
            Items items;

            var file = File.ReadAllText("data\\itemList.xml");

            XmlSerializer serializer = new XmlSerializer(typeof(Items));
            using (TextReader reader = new StringReader(file))
            {
                items = (Items)serializer.Deserialize(reader);
            }

            return items;
        }

        public bool IsConfigurationFileCreated()
        {
            return File.Exists("configuration.xml");
        }

        public Configurations ReadXmlConfigurationsData()
        {
            Configurations configurations;

            var file = File.ReadAllText("configuration.xml");

            XmlSerializer serializer = new XmlSerializer(typeof(Configurations));
            using (TextReader reader = new StringReader(file))
            {
                configurations = (Configurations)serializer.Deserialize(reader);
            }

            return configurations;
        }

        public void SaveConfigurationFile(Configurations configurations)
        {
            string xml;
            XmlSerializer xsSubmit = new XmlSerializer(typeof(Configurations));

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, configurations);
                    xml = sww.ToString();
                }
            }

            File.WriteAllText("configuration.xml", xml);
        }
    }
}