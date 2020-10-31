using Assets.Tiling;
using Assets.WorldObjects;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class FindPathToCoordinate : FindTargetPath
    {
        private UniversalCoordinate target;
        private bool navigateToAdjacent;
        public FindPathToCoordinate(
            GameObject movingActor,
            UniversalCoordinate target,
            string pathTargetProperty,
            bool navigateToAdjacent = true) : base(movingActor, pathTargetProperty)
        {
            this.target = target;
            this.navigateToAdjacent = navigateToAdjacent;
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            return componentValue
                .GetPathTo(target, navigateToAdjacent);
        }
    }
}
