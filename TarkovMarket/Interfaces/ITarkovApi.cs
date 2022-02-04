using System.Collections.Generic;
using System.Threading.Tasks;
using TarkovMarket.Classes;

namespace TarkovMarket
{
    public interface ITarkovApi
    {
        Task<GetAllItemsResult> GetAllItems();
    }
}