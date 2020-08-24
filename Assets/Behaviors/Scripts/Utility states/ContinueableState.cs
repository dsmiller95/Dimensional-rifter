using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Utility_states
{
    public abstract class ContinueableState<T> : IGenericStateHandler<T>
    {
        protected IGenericStateHandler<T> next;
        public S ContinueWith<S>(S nextState) where S : IGenericStateHandler<T>
        {
            next = nextState;
            return nextState;
        }

        public abstract IGenericStateHandler<T> HandleState(T data);
        
        public virtual void TransitionIntoState(T data)
        {
            if (next == null)
            {
                Debug.LogWarning("No next state has been configured, this will result in a dead AI");
            }
        }

        public abstract void TransitionOutOfState(T data);
    }
}
