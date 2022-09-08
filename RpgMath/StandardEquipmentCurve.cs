using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMath
{
    public interface IEquipmentCurve
    {
        long GetAttack(int level);
        long GetAttack(int level, float scale);
        long GetDefense(int level);
        long GetDefense(int level, float scale);
        long GetMDefense(int level);
        long GetMDefense(int level, float scale);
    }

    public class StandardEquipmentCurve : IEquipmentCurve
    {
        public long GetAttack(int level)
        {
            long value = level * 7 / 5;
            return value;
        }

        public long GetAttack(int level, float scale)
        {
            var value = (long)(GetAttack(level) * scale);
            return value;
        }

        public long GetDefense(int level)
        {
            long value = level * 3 / 2;
            return value;
        }

        public long GetDefense(int level, float scale)
        {
            var value = (long)(GetDefense(level) * scale);
            return value;
        }

        public long GetMDefense(int level)
        {
            long value = Math.Max(level - 10, 1); //Slope of 1, -10 y intercept
            return value;
        }

        public long GetMDefense(int level, float scale)
        {
            var value = (long)(GetMDefense(level) * scale);
            return value;
        }
    }
}
