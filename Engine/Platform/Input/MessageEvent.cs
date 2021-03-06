using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Platform
{
    public delegate void MessageEventCallback(EventLayer eventLayer);

    /// <summary>
    /// This is an event.  It can tell the client if it is down or up and if this
    /// was the first frame the event was down or up.  Events can be thought of as
    /// buttons.
    /// </summary>
    public abstract class MessageEvent
    {
        public MessageEvent(Object eventLayerKey)
        {
            this.EventLayerKey = eventLayerKey;
        }

        /// <summary>
        /// The key for the event layer this event should register on.
        /// </summary>
        public Object EventLayerKey { get; private set; }

        /// <summary>
        /// The update function, do not call this outside EventManager.
        /// </summary>
        /// <param name="eventLayer"></param>
        /// <param name="allowProcessing"></param>
        protected internal abstract void update(EventLayer eventLayer, bool allowProcessing, Clock clock);
    }
}
