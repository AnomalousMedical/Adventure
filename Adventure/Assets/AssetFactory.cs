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
        IPlayerSprite CreatePlayerSprite(string name);
        ISpriteAsset CreateSprite(string name);
    }

    class AssetFactory : IAssetFactory
    {
        private readonly ISimpleActivator simpleActivator;

        public AssetFactory(ISimpleActivator simpleActivator)
        {
            this.simpleActivator = simpleActivator;
        }

        public ISpriteAsset CreateSprite(String name)
        {
            return simpleActivator.CreateInstance<ISpriteAsset>($"Adventure.Assets.Original.{name}");
        }

        public IPlayerSprite CreatePlayerSprite(String name)
        {
            return simpleActivator.CreateInstance<IPlayerSprite>($"Adventure.Assets.Original.{name}");
        }
    }
}
