using System;
using UnityEngine;

namespace BehaviorTree.Nodes
{
    public abstract class ComponentMemberLeaf<T> : Leaf
    {
        protected T componentValue;
        public ComponentMemberLeaf(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            componentValue = gameObject.GetComponent<T>();
            if (componentValue == null)
            {
                throw new ArgumentException($"member does not have required component");
            }
        }
    }
}
