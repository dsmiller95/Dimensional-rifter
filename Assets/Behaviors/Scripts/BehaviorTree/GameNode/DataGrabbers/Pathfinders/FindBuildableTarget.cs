using Assets.WorldObjects;
using Assets.WorldObjects.Members.Building;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class FindBuildableTarget : FindTargetPath
    {

        public FindBuildableTarget(
            GameObject gameObject,
            string pathTargetProperty) : base(gameObject, pathTargetProperty)
        {
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            return componentValue
                .GetClosestOfTypeWithPath(BuildableFilter);
        }

        private bool BuildableFilter(TileMapMember member)
        {
            var buildable = member.GetComponent<Buildable>();
            return buildable != null && buildable.CanBuild();
        }
    }
}
