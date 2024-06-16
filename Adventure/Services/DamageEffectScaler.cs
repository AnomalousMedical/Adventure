using SoundPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    static class DamageEffectScaler
    {
        public static float GetEffectScale(long currentMp, long mpCost)
        {
            var effectScale = 1.0f;
            if (currentMp < mpCost)
            {
                effectScale = (float)currentMp / mpCost;
            }
            return effectScale;
        }

        public static long ApplyEffect(long amount, float scale)
        {
            if(scale < 1.0f)
            {
                amount = (long)(amount * scale);
            }
            return amount;
        }

        public static int ApplyEffect(int amount, float scale)
        {
            if (scale < 1.0f)
            {
                amount = (int)(amount * scale);
            }
            return amount;
        }
    }
}
