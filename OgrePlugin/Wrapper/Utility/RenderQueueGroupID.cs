﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OgreWrapper
{
    enum RenderQueueGroupID : byte
    {
        /// Use this queue for objects which must be rendered first e.g. backgrounds
        RENDER_QUEUE_BACKGROUND = 0,
        /// First queue (after backgrounds), used for skyboxes if rendered first
        RENDER_QUEUE_SKIES_EARLY = 5,
        RENDER_QUEUE_1 = 10,
        RENDER_QUEUE_2 = 20,
        RENDER_QUEUE_WORLD_GEOMETRY_1 = 25,
        RENDER_QUEUE_3 = 30,
        RENDER_QUEUE_4 = 40,
        /// The default render queue
        RENDER_QUEUE_MAIN = 50,
        RENDER_QUEUE_6 = 60,
        RENDER_QUEUE_7 = 70,
        RENDER_QUEUE_WORLD_GEOMETRY_2 = 75,
        RENDER_QUEUE_8 = 80,
        RENDER_QUEUE_9 = 90,
        /// Penultimate queue(before overlays), used for skyboxes if rendered last
        RENDER_QUEUE_SKIES_LATE = 95,
        /// Use this queue for objects which must be rendered last e.g. overlays
        RENDER_QUEUE_OVERLAY = 100,
        /// Final possible render queue, don't exceed this
        RENDER_QUEUE_MAX = 105
    }
}
