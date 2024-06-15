using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    public class SharpGuiOptions
    {

        public uint MaxNumberOfQuads = 1000;

        public uint MaxNumberOfTextQuads = 1000;

        /// <summary>
        /// The file name of the default font. Must be on the Virtual File System.
        /// </summary>
        public string DefaultFont { get; set; } = "Fonts/Roboto-Regular.ttf";
    }
}
