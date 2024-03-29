﻿using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    /// <summary>
    /// This is a base class for actions that can cause a coroutine to wait. Any
    /// subclasses of this are only valid to be created as part of a yield
    /// return statement so the action can be properly configured. If these
    /// classes are created incorrectly an exception will be thrown when they
    /// are triggered to resume their coroutine's execution.
    /// </summary>
    public abstract class YieldAction
    {
        IEnumerator<YieldAction> enumeration;
        private readonly CoroutineRunner coroutineRunner;

        internal YieldAction(CoroutineRunner coroutineRunner)
        {
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Set the coroutine that this action will execute when it is completed.
        /// </summary>
        /// <param name="enumeration">The enumeration to execute.</param>
        internal void setEnumeration(IEnumerator<YieldAction> enumeration)
        {
            this.enumeration = enumeration;
        }

        /// <summary>
        /// Call from a subclass when the execution of the coroutine should
        /// continue. This function will throw an exception if the coroutine was
        /// not properly set up.
        /// </summary>
        /// <throws>
        /// CoroutineException if the coroutine is null and cannot be resumed.
        /// </throws>
        protected void execute()
        {
            if (enumeration != null)
            {
                coroutineRunner.Continue(enumeration);
            }
            else
            {
                throw new CoroutineException(String.Format("There was an error resuming a coroutine for the action {0}. It is possible that the action was created outside of a yield return statement, which is invalid. Make sure all actions are created only as part of a yield return statement.", this.GetType().Name));
            }
        }

        public abstract bool tick(Clock clock);
    }
}
