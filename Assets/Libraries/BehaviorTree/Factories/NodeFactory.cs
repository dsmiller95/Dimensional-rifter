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
        public Node CreateNode(GameObject target)
        {
            var newNode = OnCreateNode(target);
#if UNITY_EDITOR
            newNode.Label = name;
#endif
            newNode.UniqueID = GetInstanceID();
            return newNode;
        }

        protected abstract Node OnCreateNode(GameObject target);
    }
}
