using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    interface IShopManager
    {
        bool AllowShop { get; }

        void AddShopBlock(object shopBlock);
        void RemoveShopBlock(object shopBlock);
    }

    class ShopManager : IShopManager
    {
        private HashSet<Object> shopBlocks = new HashSet<Object>();

        public void AddShopBlock(Object shopBlock)
        {
            shopBlocks.Add(shopBlock);
        }

        public void RemoveShopBlock(Object shopBlock)
        {
            shopBlocks.Remove(shopBlock);
        }

        public bool AllowShop => !shopBlocks.Any();
    }
}
