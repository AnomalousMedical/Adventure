﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineMath;
using System.Xml;

namespace Engine.Saving.XMLSaver
{
    class XmlRay3 : XmlValue<Ray3>
    {
        public XmlRay3(XmlSaver xmlWriter)
            : base(xmlWriter, "Ray3")
        {

        }

        public override Ray3 parseValue(XmlReader xmlReader)
        {
            throw new NotImplementedException();//return new Ray3(
        }
    }
}
