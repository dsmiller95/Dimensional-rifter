using Assets.Behaviors.Scripts.Utility_states;
using UnityEngine;

namespace Assets.Behaviors.Scripts.UtilityStates
{
    public class Delay<T> : ContinueableState<T>
    {
        private float delay;

        public Delay(float delay)
        {
            this.delay = delay;
        }

        private float nextTriggerTime;

        public override IGenericStateHandler<T> HandleState(T data)
        {
            if (nextTriggerTime < Time.time)
            {
                return next;
            }
            return this;
        }

        public override void TransitionIntoState(T data)
        {
            base.TransitionIntoState(data);
            nextTriggerTime = Time.time + delay;

        }

        public override void TransitionOutOfState(T data)
        {
        }

        public override string ToString()
        {
            return $"{delay:F1}s Delay";
        }
    }
}