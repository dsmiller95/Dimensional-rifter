using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.NodeFactories
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
