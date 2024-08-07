﻿using Engine.Threads;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Engine.Platform
{
    public class UpdateTimer
    {
        protected List<UpdateListener> updateListeners = new List<UpdateListener>();

        protected Clock clock = new Clock();
        protected SystemTimer systemTimer;
        private readonly ILogger<UpdateTimer> logger;
        private readonly CoroutineRunner coroutineRunner;
        private readonly MainThreadSynchronizationContext mainThreadSynchronizationContext;
        protected Int64 maxDelta;
        protected Int64 framerateCap = 0; //The amount of time between frames for the framerate cap.

        protected bool started = false;

        private int listenerUpdateIndex = -1;

        private Int64 deltaTime;
        private Int64 frameStartTime;
        private Int64 lastTime;
        private Int64 totalFrameTime;

        /// <summary>
        /// Create a new UpdateTimer. The SystemMesssageListener field specifies
        /// an UpdateListener that processes the OS message loop. This will be
        /// called for every fixed and full speed update to process messages as
        /// much as possible.
        /// </summary>
        /// <param name="systemTimer">The SystemTimer to get high performance time measurements from.</param>
        /// <param name="systemMessageListener">The UpdateListener that processses system messages.</param>
        public UpdateTimer(SystemTimer systemTimer, ILogger<UpdateTimer> logger, CoroutineRunner coroutineRunner, MainThreadSynchronizationContext mainThreadSynchronizationContext)
        {
            this.systemTimer = systemTimer;
            this.logger = logger;
            this.coroutineRunner = coroutineRunner;
            this.mainThreadSynchronizationContext = mainThreadSynchronizationContext;
            maxDelta = 100000;
        }

        public void OnIdle()
        {
            if (started)
            {
                frameStartTime = systemTimer.getCurrentTime();
                deltaTime = frameStartTime - lastTime;

                if (deltaTime > maxDelta)
                {
                    deltaTime = maxDelta;
                    fireExceededMaxDelta();
                }

                fireUpdate(frameStartTime, deltaTime);

                //cap the framerate if required
                PerformanceMonitor.start("Energy Saver");
                totalFrameTime = systemTimer.getCurrentTime() - frameStartTime;
                while (totalFrameTime < framerateCap)
                {
                    long sleepTime = framerateCap - totalFrameTime;
                    int sleepMs = (int)(sleepTime / 1000);
                    if (sleepMs > 0)
                    {
                        System.Threading.Thread.Sleep(sleepMs);
                    }
                    totalFrameTime = systemTimer.getCurrentTime() - frameStartTime;
                }
                PerformanceMonitor.stop("Energy Saver");

                lastTime = frameStartTime;
            }
            else
            {
                startLoop();
            }
        }

        /// <summary>
        /// Add an update listener to get updates from the fixed updater part of the timer.
        /// </summary>
        /// <param name="listener">The listener to add</param>
        public void addUpdateListener(UpdateListener listener)
        {
            updateListeners.Add(listener);
        }

        /// <summary>
        /// Remove an update listener from the fixed updater part of the timer.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void removeUpdateListener(UpdateListener listener)
        {
            int index = updateListeners.IndexOf(listener);
            if (index != -1)
            {
                updateListeners.RemoveAt(index);
                //Adjust the iteration index backwards if the element being removed is before or on the index.
                //This way nothing gets skipped.
                if (index <= listenerUpdateIndex)
                {
                    --listenerUpdateIndex;
                }
            }
        }

        /// <summary>
        /// Starts the loop iterating at the set update frequency.  This function will return
        /// once the loop is stopped.
        /// </summary>
        public bool startLoop()
        {
            if (!systemTimer.initialize())
            {
                return false;
            }
            systemTimer.Accurate = framerateCap > 0;

            fireLoopStarted();

            deltaTime = 0;
            frameStartTime = 0;
            lastTime = systemTimer.getCurrentTime();
            totalFrameTime = 0;

            started = true;

            return true;
        }

        /// <summary>
        /// Stops the loop.
        /// </summary>
        public void stopLoop()
        {
            started = false;
        }

        /// <summary>
        /// Reset the last time to be the current time. Call after a long delay to avoid falling way behind.
        /// </summary>
        public void resetLastTime()
        {
            frameStartTime = systemTimer.getCurrentTime();
        }

        /// <summary>
        /// Set the maximum delta that the timer can report. If the true delta
        /// is greater than this value it will be clamped. The default is 100000 ms
        /// (10 fps). This will cause the simulation to run slow if it is
        /// running at less than the max delta.
        /// </summary>
        public Int64 MaxDelta
        {
            get
            {
                return maxDelta;
            }
            set
            {
                maxDelta = value;
            }
        }

        public Int64 FramerateCap
        {
            get
            {
                if (framerateCap > 0)
                {
                    return 1000000 / framerateCap;
                }
                return 0;
            }
            set
            {
                if (value > 0)
                {
                    framerateCap = 1000000 / value;
                }
                else
                {
                    framerateCap = 0;
                }
            }
        }

        /// <summary>
        /// Get the elapsed time in microseconds
        /// </summary>
        public Int64 ElapsedTime
        {
            get
            {
                return systemTimer.getCurrentTime();
            }
        }

        private bool live = true;
        public bool Live { get => live; set => live = value; }

        /// <summary>
        /// Fire an update.
        /// </summary>
        protected virtual void fireUpdate(Int64 currentTimeMicro, Int64 deltaTimeMicro)
        {
            clock.setTimeMicroseconds(currentTimeMicro, deltaTimeMicro);

            if (live)
            {
                //Update active coroutines, do this first so later steps can add them and they won't
                //have their counters increased until the next frame
                coroutineRunner.Update(clock);

                //Iterate manually, this way listeners can be added/removed during the iteration of this loop.
                //If a listener is removed the fixedUpdateIndex will be adjusted if needed.
                for (listenerUpdateIndex = 0; listenerUpdateIndex < updateListeners.Count; ++listenerUpdateIndex)
                {
                    updateListeners[listenerUpdateIndex].sendUpdate(clock);
                }
                listenerUpdateIndex = -1;
            }
            else
            {
                //Iterate manually, this way listeners can be added/removed during the iteration of this loop.
                //If a listener is removed the fixedUpdateIndex will be adjusted if needed.
                for (listenerUpdateIndex = 0; listenerUpdateIndex < updateListeners.Count; ++listenerUpdateIndex)
                {
                    updateListeners[listenerUpdateIndex].pauseUpdate(clock);
                }
                listenerUpdateIndex = -1;
            }

            mainThreadSynchronizationContext.PumpCurrentQueue();

            //Sync the thread manager frames back to the main thread
            ThreadManager._doInvoke();
        }

        /// <summary>
        /// Fire the loopStaring event.
        /// </summary>
        protected virtual void fireLoopStarted()
        {
            foreach (UpdateListener listener in updateListeners)
            {
                listener.loopStarting();
            }
        }

        /// <summary>
        /// Fire the exceededMaxDelta event.
        /// </summary>
        protected virtual void fireExceededMaxDelta()
        {
            foreach (UpdateListener listener in updateListeners)
            {
                listener.exceededMaxDelta();
            }
        }
    }
}
