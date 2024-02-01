using Adventure.Battle.Skills;
using System;

namespace Adventure.Battle
{
    interface ISkillFactory
    {
        ISkill CreateSkill(string name);
    }

    class SkillFactory : ISkillFactory
    {
        public SkillFactory()
        {

        }

        public ISkill CreateSkill(String name)
        {
            switch (name)
            {
                case nameof(Fire):
                    return new Fire();
                case nameof(StrongFire):
                    return new StrongFire();
                case nameof(ArchFire):
                    return new ArchFire();

                case nameof(Ice):
                    return new Ice();
                case nameof(StrongIce):
                    return new StrongIce();
                case nameof(ArchIce):
                    return new ArchIce();

                case nameof(Lightning):
                    return new Lightning();
                case nameof(StrongLightning):
                    return new StrongLightning();
                case nameof(ArchLightning):
                    return new ArchLightning();

                case nameof(IonShread):
                    return new IonShread();

                case nameof(BattleCry):
                    return new BattleCry();
                case nameof(WarCry):
                    return new WarCry();

                case nameof(Focus):
                    return new Focus();
                case nameof(IntenseFocus):
                    return new IntenseFocus();
                case nameof(Haste):
                    return new Haste();

                case nameof(Cure):
                    return new Cure();
                case nameof(MegaCure):
                    return new MegaCure();
                case nameof(UltraCure):
                    return new UltraCure();

                case nameof(Reanimate):
                    return new Reanimate();

                case nameof(Steal):
                    return new Steal();

                default:
                    throw new NotImplementedException(name);
            }
        }
    }
}
