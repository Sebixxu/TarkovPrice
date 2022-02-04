using System.Drawing;

namespace TarkovPrice.DataModels
{
    public class ItemData
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ImageName { get; set; }
        public Bitmap Image { get; set; }
        public bool AlreadySearched { get; set; }
    }
}