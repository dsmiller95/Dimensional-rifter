using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree.Factories
{
    /// <summary>
    /// scriptable object which can build a behavior node, attaching it to
    ///     the given GameObject
    /// </summary>
    public abstract class NodeFactory : ScriptableObject
    {

#if UNITY_EDITOR
        public static System.Random FACTORY_RANDOM;
#endif

        /// <summary>
        /// 0: no children
        /// 2: exactly 2 children
        /// -1: unlimited children
        /// </summary>
        /// <returns></returns>
        public abstract int GetValidChildCount();

        public BehaviorNode CreateNode(GameObject target)
        {
            var newNode = OnCreateNode(target);
#if UNITY_EDITOR
            newNode.Label = name;
#endif
            newNode.UniqueID = GetInstanceID();
            return newNode;
        }

        protected abstract BehaviorNode OnCreateNode(GameObject target);
    }
}
