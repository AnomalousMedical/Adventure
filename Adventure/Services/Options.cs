using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class Options
    {
        public bool Fullscreen { get; set; }
#if RELEASE
        = true;
#endif
    }
}
