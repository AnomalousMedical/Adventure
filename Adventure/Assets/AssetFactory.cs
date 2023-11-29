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
                case nameof(Buckler): return new Buckler();
                case nameof(DaggerNew): return new DaggerNew();
                case nameof(FancyBook): return new FancyBook();
                case nameof(FinalHammer): return new FinalHammer();
                case nameof(FinalShield): return new FinalShield();
                case nameof(FinalSpear): return new FinalSpear();
                case nameof(FinalSword): return new FinalSword();
                case nameof(Greatsword01): return new Greatsword01();
                case nameof(MaceLarge2New): return new MaceLarge2New();
                case nameof(ShieldOfReflection): return new ShieldOfReflection();
                case nameof(Spear2Old): return new Spear2Old();
                case nameof(FireStaff07): return new FireStaff07();
                case nameof(IceStaff07): return new IceStaff07();
                case nameof(ZapStaff07): return new ZapStaff07();
                case nameof(UltimateBook): return new UltimateBook();
                case nameof(UltimateDagger): return new UltimateDagger();
                case nameof(UltimateHammer): return new UltimateHammer();
                case nameof(UltimateShield): return new UltimateShield();
                case nameof(UltimateSpear): return new UltimateSpear();
                case nameof(UltimateStaff): return new UltimateStaff();
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
