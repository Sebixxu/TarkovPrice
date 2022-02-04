using System.Collections.Generic;

namespace TarkovMarket.Classes
{
    public class GetAllItemsResult
    {
        public IEnumerable<TarkovMarketItemData> TarkovMarketItemDatas { get; set; }
        public Status Status { get; set; }
        public string Message { get; set; } //TODO Rename Message?
    }
}