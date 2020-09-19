using Assets.WorldObjects;
using Assets.WorldObjects.Members.InteractionInterfaces;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class FindHarvestableTarget : FindTargetPath
    {

        public FindHarvestableTarget(
            GameObject gameObject,
            string pathTargetProperty) : base(gameObject, pathTargetProperty)
        {
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            return componentValue
                .GetClosestOfTypeWithPath(HarvestableFilter);
        }

        private bool HarvestableFilter(TileMapMember member)
        {
            var harvestable = member.GetComponent<IHarvestable>();
            return harvestable != null && harvestable.HarvestReady();
        }
    }
}
