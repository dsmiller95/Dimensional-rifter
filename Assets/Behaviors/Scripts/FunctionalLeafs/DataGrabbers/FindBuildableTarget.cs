using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.WorldObjects.Members.Building;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers
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
