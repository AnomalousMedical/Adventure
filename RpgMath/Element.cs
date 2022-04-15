using System;
using System.Collections.Generic;
using System.Text;

namespace RpgMath
{
    public enum Element
    {
        None = 0,
        Piercing,
        Slashing,
        Bludgeoning,
        Fire,
        Ice,
        Electricity,
        Acid,
        Light,
        Darkness,
        Water,
        Poison,
        Air,
        Earth,
        Healing,
        MpRestore,
        RandStart = Piercing,
        RandEnd = Healing //Healing will not be included with the way c# random works if you use this
    }
}