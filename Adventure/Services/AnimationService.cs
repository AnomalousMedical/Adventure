using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    public interface IAnimationListener
    {
        void UpdateAnimation(Clock clock);
    }

    public interface IAnimationService
    {
        void AddListener(IAnimationListener listener);
        void RemoveListener(IAnimationListener listener);
        void Update(Clock clock);
    }

    public interface IAnimationService<T> : IAnimationService { }

    class AnimationService<T> : IAnimationService<T>
    {
        private List<IAnimationListener> listeners = new List<IAnimationListener>();

        public void AddListener(IAnimationListener listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(IAnimationListener listener)
        {
            listeners.Remove(listener);
        }

        public void Update(Clock clock)
        {
            foreach(var listener in listeners)
            {
                listener.UpdateAnimation(clock);
            }
        }
    }
}
