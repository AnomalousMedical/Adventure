using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Creators
{
    class ElementalStaffCreator
    {
        private readonly FireStaffCreator fireStaffCreator;
        private readonly IceStaffCreator iceStaffCreator;
        private readonly ZapStaffCreator zapStaffCreator;

        public ElementalStaffCreator
        (
            FireStaffCreator fireStaffCreator,
            IceStaffCreator iceStaffCreator,
            ZapStaffCreator zapStaffCreator
        )
        {
            this.fireStaffCreator = fireStaffCreator;
            this.iceStaffCreator = iceStaffCreator;
            this.zapStaffCreator = zapStaffCreator;
        }

        public IStaffCreator GetStaffCreator(Element element)
        {
            switch (element)
            {
                case Element.Fire:
                    return fireStaffCreator;
                case Element.Ice:
                    return iceStaffCreator;
                case Element.Electricity:
                    return zapStaffCreator;
            }

            return null;
        }
    }
}
