using Assets.WorldObjects;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class FindPathToBakedTarget : FindTargetPath
    {
        private TileMapMember target;
        private bool navigateToAdjacent;
        public FindPathToBakedTarget(
            GameObject movingActor,
            GameObject target,
            string pathTargetProperty,
            bool navigateToAdjacent = true) : base(movingActor, pathTargetProperty)
        {
            this.target = target.GetComponent<TileMapMember>();
            this.navigateToAdjacent = navigateToAdjacent;
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            return componentValue
                .GetPathTo(target, navigateToAdjacent);
        }
    }
}
