using DiligentEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class Options
    {
        public GraphicsEngine.RenderApi RenderApi { get; set; } = GraphicsEngine.RenderApi.D3D12;

        public bool Fullscreen { get; set; }
    }
}
