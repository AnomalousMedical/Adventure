﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OgreWrapper;

namespace libRocketPlugin
{
    public class RocketQueueListener : RenderQueueListener
    {
        private Context context;
        private RenderInterfaceOgre3D renderInterface;

        public RocketQueueListener(Context context, RenderInterfaceOgre3D renderInterface)
        {
            this.context = context;
            this.renderInterface = renderInterface;
        }

        public void preRenderQueues()
        {

        }

        public void postRenderQueues()
        {

        }

        public void renderQueueStarted(byte queueGroupId, string invocation, ref bool skipThisInvocation)
        {
            if (queueGroupId == 100)// && Ogre::Root::getSingleton().getRenderSystem()->_getViewport()->getOverlaysEnabled())
            {
                context.Update();

                renderInterface.ConfigureRenderSystem();
                context.Render();
            }
        }

        public void renderQueueEnded(byte queueGroupId, string invocation, ref bool repeatThisInvocation)
        {

        }
    }
}
