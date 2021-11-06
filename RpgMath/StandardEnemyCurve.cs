﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMath
{
    public class StandardEnemyCurve
    {
        public long GetHp(int level)
        {
            if(level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(40f, 200f, (level) / 10f);
            }
            else if(level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(150f, 300f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(450f, 1200f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(1700f, 4200f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(6000f, 11000f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(12000f, 16000f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(20000f, 22000f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(27000f, 38000f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(40000f, 51000f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(55000f, 65000f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetAttack(int level)
        {
            //Pretty linear overall
            if (level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(6f, 25f, (level) / 10f);
            }
            else if (level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(25f, 47f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(47f, 65f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(65f, 90f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(90f, 115f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(115f, 145f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(145f, 165f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(165f, 200f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(200f, 225f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(225f, 255f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetDefense(int level)
        {
            //Defense could go up to 2x for badass enemies
            if (level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(8f, 25f, (level) / 10f);
            }
            else if (level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(25f, 45f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(45f, 55f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(55f, 80f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(80f, 105f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(105f, 135f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(135f, 155f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(155f, 180f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(180f, 215f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(215f, 250f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetAttackPercent(int level)
        {
            if (level < 100)
            {
                //Just always 100
                return 100L;
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetMagicAttackPercent(int level)
        {
            if (level < 100)
            {
                //Just always 100
                return 100L;
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetLuck(int level)
        {
            if (level < 100)
            {
                //Luck is not really level based, just have lucky enemies
                //Consider 0-10, 20-30, 40 and 50
                return 3L;
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetDefensePercent(int level)
        {
            if (level < 100)
            {
                //This is how dodgy the target is. This is at least 1 for everything, so the default curve is just 1
                //Could increase for badass or have dodgier enemies
                //Consider also 20, 40 or 100 (out of 255) for other good dodge percents to try
                //Some between 1 -20 too, but this is more enemy type dependent, not scaling per level
                return 1L;
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetMagicDefensePercent(int level)
        {
            if (level < 100)
            {
                //This is how dodgy the target is. All magic has a built in dodge, so this is 0 by default.
                //Could increase for badass or have dodgier enemies
                //Consider also 20, 40 or 100 (out of 255) for other good dodge percents to try
                //Some between 1 -20 too, but this is more enemy type dependent, not scaling per level
                return 0L;
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetDexterity(int level)
        {
            //Dexterity is mostly flat
            if (level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(50f, 50f, (level) / 10f);
            }
            else if (level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(50f, 50f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(50f, 58f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(58f, 75f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(75f, 100f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(100f, 120f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(120f, 140f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(140f, 158f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(158f, 175f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(175f, 200f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetMagicAttack(int level)
        {
            if (level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(5f, 20f, (level) / 10f);
            }
            else if (level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(20f, 25f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(25f, 45f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(45f, 60f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(60f, 90f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(90f, 120f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(120f, 145f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(145f, 175f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(175f, 225f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(225f, 255f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetMagicDefense(int level)
        {
            if (level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(5f, 20f, (level) / 10f);
            }
            else if (level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(20f, 35f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(35f, 50f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(50f, 75f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(75f, 110f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(110f, 135f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(135f, 165f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(165f, 200f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(200f, 225f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(225f, 255f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetMp(int level)
        {
            if (level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(10f, 40f, (level) / 10f);
            }
            else if (level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(40f, 60f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(60f, 100f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(100f, 140f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(140f, 200f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(200, 250f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(250f, 300f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(300f, 400f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(400f, 550f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(550f, 800f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetGold(int level)
        {
            if (level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(10f, 100f, (level) / 10f);
            }
            else if (level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(100f, 200f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(200f, 450f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(450f, 800f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(800f, 1100f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(1100f, 1350f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(1350f, 1700f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(1700f, 2000f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(2000f, 2300f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(2300f, 2800f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }

        public long GetXp(int level)
        {
            if (level < 10)
            {
                //1-10
                return (long)NumberFunctions.lerp(10f, 25f, (level) / 10f);
            }
            else if (level < 20)
            {
                //10-20
                return (long)NumberFunctions.lerp(25f, 200f, (level - 10) / 10f);
            }
            else if (level < 30)
            {
                //20-30
                return (long)NumberFunctions.lerp(200f, 550f, (level - 20) / 10f);
            }
            else if (level < 40)
            {
                //30-40
                return (long)NumberFunctions.lerp(550f, 1000f, (level - 30) / 10f);
            }
            else if (level < 50)
            {
                //40-50
                return (long)NumberFunctions.lerp(1000f, 1400f, (level - 40) / 10f);
            }
            else if (level < 60)
            {
                //50-60
                return (long)NumberFunctions.lerp(1400f, 2000f, (level - 50) / 10f);
            }
            else if (level < 70)
            {
                //60-70
                return (long)NumberFunctions.lerp(2000f, 2500f, (level - 60) / 10f);
            }
            else if (level < 80)
            {
                //70-80
                return (long)NumberFunctions.lerp(2500f, 3100f, (level - 70) / 10f);
            }
            else if (level < 90)
            {
                //80-90
                return (long)NumberFunctions.lerp(3100f, 3700f, (level - 80) / 10f);
            }
            else if (level < 100)
            {
                //90-99
                return (long)NumberFunctions.lerp(3700f, 4300f, (level - 90) / 10f);
            }

            throw new InvalidOperationException($"Level '{level}' is not supported.");
        }
    }
}