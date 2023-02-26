namespace RpgMath
{
    public enum Element
    {
        None = 0,
        
        //Melee
        Piercing,
        Slashing,
        Bludgeoning,
        
        //Magic
        Fire,
        Ice,
        Electricity,
        
        //Healing
        Healing,
        MpRestore,

        //Ranges
        RandStart = Piercing,
        RandEnd = Healing, //Healing will not be included with the way c# random works if you use this
        MeleeStart = Piercing,
        MeleeEnd = Fire,//Fire will not be included with the way c# random works if you use this
        MagicStart = Fire,
        MagicEnd = Healing,//Healing will not be included with the way c# random works if you use this
    }
}