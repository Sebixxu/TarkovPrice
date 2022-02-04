using System.Collections.Generic;
using TarkovPrice.XmlModels;

namespace TarkovPrice.DataModels
{
    public class ItemsData
    {
        public List<ItemData> ItemsCollection { get; set; } = new List<ItemData>();

    }
}