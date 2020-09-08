using Assets.WorldObjects;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class SetRandomWalkTarget : FindTargetPath
    {
        private int randomWalkLength;
        public SetRandomWalkTarget(
            GameObject gameObject,
            string pathTargetProperty,
            int randomWalkLength) : base(gameObject, pathTargetProperty)
        {
            this.randomWalkLength = randomWalkLength;
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            return componentValue
                .GetRandomPathOfLength(randomWalkLength);
        }
    }
}
