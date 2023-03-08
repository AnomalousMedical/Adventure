using System;

namespace Adventure.Battle
{
    class ContextTriggerManager
    {
        private bool activated = false;
        private bool spamPrevention = true;

        /// <summary>
        /// Will be true if the player activated the skill without spamming.
        /// </summary>
        public bool Activated => activated;

        /// <summary>
        /// Will be true if the player spammed the button more than once.
        /// </summary>
        public bool Spammed => !spamPrevention;

        public void CheckTrigger(IBattleTarget target, bool allowTrigger)
        {
            if (target.TryContextTrigger())
            {
                if (activated)
                {
                    activated = false;
                    spamPrevention = false;
                }
                else
                {
                    activated = spamPrevention;
                    if (!allowTrigger)
                    {
                        activated = false;
                        spamPrevention = false;
                    }
                }
            }
        }
    }
}
