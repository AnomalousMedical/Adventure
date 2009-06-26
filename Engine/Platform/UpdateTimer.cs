﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Engine.Platform
{
    /// <summary>
    /// This class is an abstract base class for a timer.
    /// </summary>
    public class UpdateTimer
    {
        private List<UpdateListener> fixedListeners = new List<UpdateListener>();
        private List<UpdateListener> fullSpeedListeners = new List<UpdateListener>();
        private UpdateListener systemMessageListener;

        private Clock clock = new Clock();
        private SystemTimer systemTimer;

        double fixedFrequency;
        double maxDelta;
        int maxFrameSkip;
        double framerateCap;

        double totalTime = 0.0; //The total time for all frames that hasnt been processed

        bool started = false;

        /// <summary>
        /// Create a new UpdateTimer. The SystemMesssageListener field specifies
        /// an UpdateListener that processes the OS message loop. This will be
        /// called for every fixed and full speed update to process messages as
        /// much as possible.
        /// </summary>
        /// <param name="systemTimer">The SystemTimer to get high performance time measurements from.</param>
        /// <param name="systemMessageListener">The UpdateListener that processses system messages.</param>
        public UpdateTimer(SystemTimer systemTimer, UpdateListener systemMessageListener)
        {
            this.systemTimer = systemTimer;
            this.systemMessageListener = systemMessageListener;
            fixedFrequency = 1.0 / 60.0;
            maxDelta = 0.1;
            maxFrameSkip = 7;
            framerateCap = 0.0;
        }

        /// <summary>
        /// Add an update listener to get updates from the fixed updater part of the timer.
        /// </summary>
        /// <param name="listener">The listener to add</param>
        public void addFixedUpdateListener(UpdateListener listener)
        {
            fixedListeners.Add(listener);
        }

        /// <summary>
        /// Remove an update listener from the fixed updater part of the timer.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void removeFixedUpdateListener(UpdateListener listener)
        {
            fixedListeners.Remove(listener);
        }

        /// <summary>
        /// Add a listener that will be updated as fast a possible by the loop.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void addFullSpeedUpdateListener(UpdateListener listener)
        {
            fullSpeedListeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener from the full speed loop.
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void removeFullSpeedUpdateListener(UpdateListener listener)
        {
            fullSpeedListeners.Remove(listener);
        }

        /// <summary>
        /// Starts the loop iterating at the set update frequency.  This function will return
        /// once the loop is stopped.
        /// </summary>
        public bool startLoop()
        {
            if (systemTimer.initialize())
            {
                return false;
            }

            started = true;
            fireLoopStarted();
            systemTimer.prime();
            double deltaTime;
            totalTime = 0;
            int loops = 0;

            while (started)
            {
                deltaTime = systemTimer.getDelta();

                //If the time is faster than the framerate cap sleep for the difference.
                //This is not exact, but any error will be handled by the rest of the timer.
                while (deltaTime < framerateCap)
                {
                    Thread.Sleep((int)((framerateCap - deltaTime) * 1000));
                    deltaTime += systemTimer.getDelta();
                }

                if (deltaTime > maxDelta)
                {
                    deltaTime = maxDelta;
                    fireExceededMaxDelta();
                }
                totalTime += deltaTime;

                loops = 0;
                while (totalTime > fixedFrequency && loops < maxFrameSkip)
                {
                    fireFixedUpdate();
                    totalTime -= fixedFrequency;
                    loops++;
                }

                fireFullSpeedUpdate(deltaTime);
            }
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
        /// Sets the frequency of the fixed updates in seconds. The default is
        /// 1/60 (60Hz).
        /// </summary>
        public double FixedFrequency
        {
            get
            {
                return fixedFrequency;
            }
            set
            {
                fixedFrequency = value;
            }
        }

        /// <summary>
        /// Set the maximum delta that the timer can report. If the true delta
        /// is greater than this value it will be clamped. The default is 0.1
        /// (10 fps). This will cause the simulation to run slow if it is
        /// running at less than the max delta.
        /// </summary>
        public double MaxDelta
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

        /// <summary>
        /// Sets the maximum number of frames that can be skipped in the full
        /// speed updater if the fixed updater is running slow. The default is
        /// 7.
        /// </summary>
        public int MaxFrameSkip
        {
            get
            {
                return maxFrameSkip;
            }
            set
            {
                maxFrameSkip = value;
            }
        }

        public double FramerateCap
        {
            get
            {
                if (framerateCap > 0.9)
                {
                    return 1.0 / framerateCap;
                }
                return 0.0;
            }
            set
            {
                if (value > 0.9)
                {
                    framerateCap = 1.0 / value;
                }
                else
                {
                    framerateCap = 0.0;
                }
            }
        }

        /// <summary>
        /// Fire a fixed update.
        /// </summary>
        protected void fireFixedUpdate()
        {
            clock.setTimeSeconds(FixedFrequency);
            foreach (UpdateListener fixedListener in fixedListeners)
            {
                fixedListener.sendUpdate(clock);
            }
            systemMessageListener.sendUpdate(clock);
        }

        /// <summary>
        /// Fire a full speed update.
        /// </summary>
        /// <param name="deltaTime">The amount of time since the last full speed update in seconds.</param>
        protected void fireFullSpeedUpdate(double deltaTime)
        {
            clock.setTimeSeconds(deltaTime);
            foreach (UpdateListener fullSpeedListener in fullSpeedListeners)
            {
                fullSpeedListener.sendUpdate(clock);
            }
            systemMessageListener.sendUpdate(clock);
        }

        /// <summary>
        /// Fire the loopStaring event.
        /// </summary>
        protected void fireLoopStarted()
        {
            foreach (UpdateListener fixedListener in fixedListeners)
            {
                fixedListener.loopStarting();
            }
            foreach (UpdateListener fullSpeedListener in fullSpeedListeners)
            {
                fullSpeedListener.loopStarting();
            }
            systemMessageListener.loopStarting();
        }

        /// <summary>
        /// Fire the exceededMaxDelta event.
        /// </summary>
        protected void fireExceededMaxDelta()
        {
            foreach (UpdateListener fixedListener in fixedListeners)
            {
                fixedListener.exceededMaxDelta();
            }
            foreach (UpdateListener fullSpeedListener in fullSpeedListeners)
            {
                fullSpeedListener.exceededMaxDelta();
            }
            systemMessageListener.exceededMaxDelta();
        }
    }
}
