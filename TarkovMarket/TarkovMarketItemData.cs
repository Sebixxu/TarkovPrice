using System;

namespace TarkovMarket
{
    public class TarkovMarketItemData
    {
        public Guid UId { get; set; }
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public string ShortName { get; set; }
        public int Price { get; set; }
        public int BasePrice { get; set; }
        public int Avg24hPrice { get; set; }
        public int Avg7DaysPrice { get; set; }
        public string TraderName { get; set; }
        public int TraderPrice { get; set; }
        public char TraderPriceCur { get; set; }
        public DateTime Updated { get; set; }
        public int Slots { get; set; }
        public float Diff24h { get; set; }
        public float Diff7Days { get; set; }
        public string Icon { get; set; }
        public string  Link { get; set; }
        public string WikiLink { get; set; }
        public string Img { get; set; }
        public string ImgBig { get; set; }
        public string BsgId { get; set; }
        public bool IsFunctional { get; set; }
        public string Reference { get; set; }
    }
}