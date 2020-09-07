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
        public abstract Node CreateNode(GameObject target);
    }
}
