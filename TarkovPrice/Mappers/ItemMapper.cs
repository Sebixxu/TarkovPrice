using System.Windows.Markup;
using TarkovPrice.DataModels;
using TarkovPrice.XmlModels;

namespace TarkovPrice.Mappers
{
    public static class ItemMapper
    {
        public static ItemsData XmlItemsToItemsData(this Items items)
        {
            var itemsData = new ItemsData();

            foreach (var item in items.ItemsCollection)
            {
                itemsData.ItemsCollection.Add(
                    new ItemData
                    {
                        Id = item.Id,
                        ImageName = item.ImageName,
                        ItemName = item.ItemName,
                        AlreadySearched = false
                    });
            }

            return itemsData;
        }
    }
}