using Adventure.Assets.Equipment;
using Adventure.Assets.Players;
using System;

namespace Adventure.Assets
{
    interface IAssetFactory
    {
        IPlayerSprite CreatePlayer(string name);
        ISpriteAsset CreateEquipment(string name);
    }

    class AssetFactory : IAssetFactory
    {
        public AssetFactory()
        {

        }

        public ISpriteAsset CreateEquipment(String name)
        {
            switch (name)
            {
                //Equipment
                case nameof(Dagger1): return new Dagger1();
                case nameof(Dagger2): return new Dagger2();
                case nameof(Dagger3): return new Dagger3();
                case nameof(Book1): return new Book1();
                case nameof(Book2): return new Book2();
                case nameof(Book3): return new Book3();
                case nameof(Hammer1): return new Hammer1();
                case nameof(Hammer2): return new Hammer2();
                case nameof(Hammer3): return new Hammer3();
                case nameof(Shield1): return new Shield1();
                case nameof(Shield2): return new Shield2();
                case nameof(Shield3): return new Shield3();
                case nameof(Spear1): return new Spear1();
                case nameof(Spear2): return new Spear2();
                case nameof(Spear3): return new Spear3();
                case nameof(Staff1): return new Staff1();
                case nameof(Staff2): return new Staff2();
                case nameof(Staff3): return new Staff3();
                case nameof(Sword1): return new Sword1();
                case nameof(Sword2): return new Sword2();
                case nameof(Sword3): return new Sword3();
                case nameof(Scimitar): return new Scimitar();
                case nameof(Trident): return new Trident();

                default: throw new NotImplementedException(name);
            }
        }

        public IPlayerSprite CreatePlayer(String name)
        {
            switch (name)
            {
                case nameof(ClericPlayerSprite): return new ClericPlayerSprite();
                case nameof(FighterPlayerSprite): return new FighterPlayerSprite();
                case nameof(MagePlayerSprite): return new MagePlayerSprite();
                case nameof(ThiefPlayerSprite): return new ThiefPlayerSprite();
                default: throw new NotImplementedException(name);
            }
        }
    }
}
