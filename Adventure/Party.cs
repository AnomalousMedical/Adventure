using RpgMath;
using Adventure.Assets;
using Adventure.Battle;
using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adventure
{
    class Party
    {
        private readonly Persistence persistence;

        public Party(Persistence persistence)
        {
            this.persistence = persistence;
        }

        public IEnumerable<Character> ActiveCharacters => persistence.Party.Members.Select(i =>
        {
            var assemblyName = typeof(Party).Assembly.GetName().Name;

            var character = new Character()
            {
                CharacterSheet = i.CharacterSheet,
                PlayerSprite = CreateInstance<IPlayerSprite>($"Adventure.Assets.Original.{i.PlayerSprite}"),
                PrimaryHandAsset = i.PrimaryHandAsset != null ? CreateInstance<ISpriteAsset>($"Adventure.Assets.Original.{i.PrimaryHandAsset}") : null,
                SecondaryHandAsset = i.SecondaryHandAsset != null ? CreateInstance<ISpriteAsset>($"Adventure.Assets.Original.{i.SecondaryHandAsset}") : null,
                Spells = i.Spells?.Select(s => CreateInstance<ISpell>($"Adventure.Battle.Spells.{s}"))
            };

            return character;
        });

        private T CreateInstance<T>(String name)
        {
            var type = Type.GetType(name);
            var instance = (T)Activator.CreateInstance(type);
            return instance;
        }

        public IEnumerable<CharacterSheet> ActiveCharacterSheets => persistence.Party.Members.Select(i => i.CharacterSheet);

        public long Gold
        {
            get
            {
                return persistence.Party.Gold;
            }
            set
            {
                persistence.Party.Gold = value;
            }
        }
    }
}
