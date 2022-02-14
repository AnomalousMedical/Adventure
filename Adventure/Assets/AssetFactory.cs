using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets
{
    interface IAssetFactory
    {
        IPlayerSprite CreatePlayer(string name);
        ISpriteAsset CreateEquipment(string name);
    }

    class AssetFactory : IAssetFactory
    {
        private readonly ISimpleActivator simpleActivator;

        public AssetFactory(ISimpleActivator simpleActivator)
        {
            this.simpleActivator = simpleActivator;
        }

        public ISpriteAsset CreateEquipment(String name)
        {
            return simpleActivator.CreateInstance<ISpriteAsset>($"Adventure.Assets.Equipment.{name}");
        }

        public IPlayerSprite CreatePlayer(String name)
        {
            return simpleActivator.CreateInstance<IPlayerSprite>($"Adventure.Assets.Players.{name}");
        }
    }
}
