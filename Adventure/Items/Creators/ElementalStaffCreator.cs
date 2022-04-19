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
        private readonly AcidStaffCreator acidStaffCreator;
        private readonly DarknessStaffCreator darknessStaffCreator;
        private readonly EarthStaffCreator earthStaffCreator;
        private readonly FireStaffCreator fireStaffCreator;
        private readonly IceStaffCreator iceStaffCreator;
        private readonly ZapStaffCreator zapStaffCreator;

        public ElementalStaffCreator
        (
            AcidStaffCreator acidStaffCreator,
            DarknessStaffCreator darknessStaffCreator,
            EarthStaffCreator earthStaffCreator,
            FireStaffCreator fireStaffCreator,
            IceStaffCreator iceStaffCreator,
            ZapStaffCreator zapStaffCreator
        )
        {
            this.acidStaffCreator = acidStaffCreator;
            this.darknessStaffCreator = darknessStaffCreator;
            this.earthStaffCreator = earthStaffCreator;
            this.fireStaffCreator = fireStaffCreator;
            this.iceStaffCreator = iceStaffCreator;
            this.zapStaffCreator = zapStaffCreator;
        }

        public IStaffCreator GetStaffCreator(Element element)
        {
            switch (element)
            {
                case Element.Acid:
                    return acidStaffCreator;
                case Element.Darkness:
                    return darknessStaffCreator;
                case Element.Earth:
                    return earthStaffCreator;
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
