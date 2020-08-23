using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Behaviors.UtilityStates
{
    public class Waiting<T> : IGenericStateHandler<T>
    {

        IList<(float, Action)> WaitSections;

        private float finalDelay;
        private IGenericStateHandler<T> returnToState;

        public Waiting()
        {
            WaitSections = new List<(float, Action)>();
        }

        public void Then(float delay, Action action)
        {
            WaitSections.Add((delay, action));
        }

        public void Finalize(float delay, IGenericStateHandler<T> returnToState)
        {
            finalDelay = delay;
            this.returnToState = returnToState;
        }

        private Queue<(float, Action)> pendingActions;
        private Action nextAction;
        private float nextTriggerTime;

        public IGenericStateHandler<T> HandleState(T data)
        {
            if (nextTriggerTime < Time.time)
            {
                if (nextAction == null)
                {
                    return returnToState;
                }
                nextAction?.Invoke();

                if (pendingActions.Count <= 0)
                {
                    Complete();
                }
                NextAction();
            }
            return this;
        }

        private void NextAction()
        {
            var nextSection = pendingActions.Dequeue();
            nextAction = nextSection.Item2;
            nextTriggerTime = Time.time + nextSection.Item1;
        }

        private void Complete()
        {
            nextAction = null;
            nextTriggerTime = Time.time + finalDelay;
        }


        public void TransitionIntoState(T data)
        {
            if (WaitSections != null && WaitSections.Count > 0)
            {
                pendingActions = new Queue<(float, Action)>(WaitSections);
                NextAction();
            }
            else
            {
                pendingActions = new Queue<(float, Action)>();
                Complete();
            }

        }
        public void TransitionOutOfState(T data)
        {
        }
    }
}