using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "NavigateToTarget", menuName = "Behaviors/Actions/NavigateToTarget", order = 10)]
    public class NavigateToTargetFactory : NodeFactory
    {
        public string blackboardPathProperty;
        public string blackboardTargetProperty;
        public bool ensureTarget = true;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new NavigateToTarget(
                target,
                blackboardPathProperty,
                blackboardTargetProperty,
                ensureTarget);
        }
    }
}
