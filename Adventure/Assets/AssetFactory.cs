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
                case nameof(BattleAxe6): return new BattleAxe6();
                case nameof(Dagger1): return new Dagger1();
                case nameof(Dagger2): return new Dagger2();
                case nameof(Dagger3): return new Dagger3();
                case nameof(Book1): return new Book1();
                case nameof(Book2): return new Book2();
                case nameof(Book3): return new Book3();
                case nameof(FinalHammer): return new FinalHammer();
                case nameof(FinalSpear): return new FinalSpear();
                case nameof(FinalSword): return new FinalSword();
                case nameof(Greatsword01): return new Greatsword01();
                case nameof(MaceLarge2New): return new MaceLarge2New();
                case nameof(ShieldOfReflection): return new ShieldOfReflection();
                case nameof(Spear2Old): return new Spear2Old();
                case nameof(UltimateHammer): return new UltimateHammer();
                case nameof(UltimateSpear): return new UltimateSpear();
                case nameof(Shield1): return new Shield1();
                case nameof(Shield2): return new Shield2();
                case nameof(Shield3): return new Shield3();
                case nameof(Staff1): return new Staff1();
                case nameof(Staff2): return new Staff2();
                case nameof(Staff3): return new Staff3();
                case nameof(UltimateSword): return new UltimateSword();

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
