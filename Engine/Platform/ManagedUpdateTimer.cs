﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Engine.Platform
{
    public class ManagedUpdateTimer : UpdateTimer
    {
        protected UpdateListener systemMessageListener;
        private Int64 frameStartTime;

        public ManagedUpdateTimer(SystemTimer systemTimer, UpdateListener systemMessageListener)
            :base(systemTimer)
        {
            this.systemMessageListener = systemMessageListener;
        }

        /// <summary>
        /// Starts the loop iterating at the set update frequency.  This function will return
        /// once the loop is stopped.
        /// </summary>
        public override bool startLoop()
        {
            if (!systemTimer.initialize())
            {
                return false;
            }

            started = true;
            fireLoopStarted();

            Int64 deltaTime;
            Int64 totalTime = 0;
            Int64 lastTime = systemTimer.getCurrentTime();
            Int64 totalFrameTime;

            while (started)
            {
                frameStartTime = systemTimer.getCurrentTime();
                deltaTime = frameStartTime - lastTime;

                if (deltaTime > maxDelta)
                {
                    deltaTime = maxDelta;
                    fireExceededMaxDelta();
                }
                totalTime += deltaTime;
                if (totalTime > fixedFrequency * maxFrameSkip)
                {
                    totalTime = fixedFrequency * maxFrameSkip;
                }

                while (totalTime >= fixedFrequency)
                {
                    fireFixedUpdate(fixedFrequency);
                    totalTime -= fixedFrequency;
                }

                fireFullSpeedUpdate(deltaTime);

                lastTime = frameStartTime;

                //cap the framerate if required
                totalFrameTime = systemTimer.getCurrentTime() - frameStartTime;
                while (totalFrameTime < framerateCap)
                {
                    Thread.Sleep((int)((framerateCap - totalFrameTime) / 1000));
                    totalFrameTime = systemTimer.getCurrentTime() - frameStartTime;
                }
            }
            return true;
        }

        /// <summary>
        /// Reset the last time to be the current time. Call after a long delay to avoid frame skipping.
        /// </summary>
        public override void resetLastTime()
        {
            frameStartTime = systemTimer.getCurrentTime();
        }

        /// <summary>
        /// Fire a fixed update.
        /// </summary>
        protected override void fireFixedUpdate(long time)
        {
            base.fireFixedUpdate(time);
            systemMessageListener.sendUpdate(clock);
        }

        /// <summary>
        /// Fire a full speed update.
        /// </summary>
        /// <param name="deltaTime">The amount of time since the last full speed update in seconds.</param>
        protected override void fireFullSpeedUpdate(Int64 deltaTime)
        {
            base.fireFullSpeedUpdate(deltaTime);
            systemMessageListener.sendUpdate(clock);
        }

        /// <summary>
        /// Fire the loopStaring event.
        /// </summary>
        protected override void fireLoopStarted()
        {
            base.fireLoopStarted();
            systemMessageListener.loopStarting();
        }

        /// <summary>
        /// Fire the exceededMaxDelta event.
        /// </summary>
        protected override void fireExceededMaxDelta()
        {
            base.fireExceededMaxDelta();
            systemMessageListener.exceededMaxDelta();
        }
    }
}
