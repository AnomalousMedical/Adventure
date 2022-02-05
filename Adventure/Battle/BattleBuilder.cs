﻿using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    interface IBattleBuilder
    {
        IEnumerable<Enemy> CreateEnemies(IObjectResolver objectResolver, Party party, IBiome biome, Random random);
    }

    class BattleBuilder : IBattleBuilder
    {
        private readonly List<Vector3> enemyLocations = new List<Vector3>();

        public BattleBuilder()
        {
            enemyLocations.Add(new Vector3(-5f, 0f, -4f));
            enemyLocations.Add(new Vector3(-4.5f, 0f, -2f));
            enemyLocations.Add(new Vector3(-4f, 0f,  0f));
            enemyLocations.Add(new Vector3(-3.5f, 0f,  2f));
            enemyLocations.Add(new Vector3(-3f, 0f,  4f));
            enemyLocations.Add(new Vector3(-2.5f, 0f,  6f));
        }

        public IEnumerable<Enemy> CreateEnemies(IObjectResolver objectResolver, Party party, IBiome biome, Random random)
        {
            var level = party.ActiveCharacterSheets.GetAverageLevel() * 4 / 5;
            if(level < 1)
            {
                level = 1;
            }

            var index = 0;
            foreach(var enemyType in GetEnemyBudget(level, random))
            {
                yield return objectResolver.Resolve<Enemy, Enemy.Desc>(c =>
                {
                    var location = enemyLocations[index];
                    var biomeEnemy = biome.GetEnemy(enemyType);
                    var curve = biomeEnemy.EnemyCurve;
                    c.Sprite = biomeEnemy.Asset.CreateSprite();
                    c.SpriteMaterial = biomeEnemy.Asset.CreateMaterial();
                    c.BattleStats = new BattleStats()
                    {
                        Hp = curve.GetHp(level, enemyType),
                        Mp = curve.GetMp(level, enemyType),
                        Attack = curve.GetAttack(level, enemyType),
                        AttackPercent = curve.GetAttackPercent(level, enemyType),
                        Defense = curve.GetDefense(level, enemyType),
                        DefensePercent = curve.GetDefensePercent(level, enemyType),
                        MagicAttack = curve.GetMagicAttack(level, enemyType),
                        MagicAttackPercent = curve.GetMagicAttackPercent(level, enemyType),
                        MagicDefensePercent = curve.GetMagicDefensePercent(level, enemyType),
                        MagicDefense = curve.GetMagicDefense(level, enemyType),
                        Dexterity = curve.GetDexterity(level, enemyType),
                        Luck = curve.GetLuck(level, enemyType),
                        Level = level,
                        Resistances = biomeEnemy.Resistances
                    };
                    c.Scale = curve.GetScale(level, enemyType);
                    c.Translation = new Vector3(location.x, c.Sprite.BaseScale.y * c.Scale.y / 2.0f, location.z);
                    c.GoldReward = curve.GetGold(level, enemyType);
                });
                ++index;
            }
        }

        public IEnumerable<EnemyType> GetEnemyBudget(int level, Random random)
        {
            const int extremelyRare = 5;
            const int rare = 15;
            const int lessRare = 25;

            var isExtremelyRare = random.Next(100) < extremelyRare;
            var isRare = !isExtremelyRare && random.Next(100) < rare;
            var isLessRare = !isRare && random.Next(100) < lessRare;

            int enemyBudget;

            if (level < 5)
            {
                //1-5
                enemyBudget = random.Next(3) + 1;
            }
            else if (level < 10)
            {
                //5-10
                if (isRare)
                {
                    enemyBudget = 4;
                }
                else
                {
                    enemyBudget = random.Next(2) + 2;
                }
            }
            else if (level < 20)
            {
                //10-20
                if (isRare)
                {
                    enemyBudget = 4;
                }
                else
                {
                    enemyBudget = random.Next(2) + 2;
                }
            }
            else if (level < 40)
            {
                //20-40
                if (isRare)
                {
                    enemyBudget = 5;
                }
                else if (isLessRare)
                {
                    enemyBudget = 4;
                }
                else
                {
                    enemyBudget = random.Next(2) + 2;
                }
            }
            else if (level < 60)
            {
                //40-60
                if (isLessRare)
                {
                    enemyBudget = 5;
                }
                else
                {
                    enemyBudget = random.Next(3) + 2;
                }
            }
            else if (level < 75)
            {
                //60-75
                if (isRare)
                {
                    enemyBudget = 6;
                }
                else if (isLessRare)
                {
                    enemyBudget = 5;
                }
                else
                {
                    enemyBudget = random.Next(3) + 2;
                }
            }
            else if (level < 90)
            {
                //75-90
                if (isLessRare)
                {
                    enemyBudget = 6;
                }
                else
                {
                    enemyBudget = random.Next(5) + 2;
                }
            }
            else if (level < 100)
            {
                enemyBudget = random.Next(6) + 2;
            }
            else
            {
                throw new InvalidOperationException($"Level '{level}' is not supported.");
            }

            //Roll for enemy type
            while (enemyBudget > 0)
            {
                var type = GetEnemyType(level, random);

                switch (type)
                {
                    case EnemyType.Badass:
                        enemyBudget -= 2;
                        if (enemyBudget < 0)
                        {
                            //Ran out of budget, even though we got a badass
                            type = EnemyType.Normal;
                            enemyBudget = 0;
                        }
                        break;
                    default:
                        enemyBudget -= 1;
                        break;
                }

                yield return type;
            }
        }

        public EnemyType GetEnemyType(int level, Random random)
        {
            const int extremelyRare = 5;
            const int rare = 15;
            const int lessRare = 25;

            var isExtremelyRare = random.Next(100) < extremelyRare;
            var isRare = !isExtremelyRare && random.Next(100) < rare;
            var isLessRare = !isRare && random.Next(100) < lessRare;

            if (level < 10)
            {
                //1-10
                return EnemyType.Normal;
            }
            else if (level < 20)
            {
                //10-20
                if (isExtremelyRare)
                {
                    return EnemyType.Peon;
                }
                return EnemyType.Normal;
            }
            else if (level < 40)
            {
                //20-40
                if (isExtremelyRare)
                {
                    return EnemyType.Badass;
                }
                if (isRare)
                {
                    return EnemyType.Peon;
                }
                return EnemyType.Normal;
            }
            else if (level < 100)
            {
                //90-99
                if (isRare)
                {
                    return EnemyType.Badass;
                }
                if (isLessRare)
                {
                    return EnemyType.Peon;
                }
                return EnemyType.Normal;
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }
    }
}
