using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class FindItemSourceTarget : FindTargetPath
    {
        private bool resourceFromBlackboard;
        private string resourceTypePropertyInBlackboard;
        private Resource resourceToGrab;

        private ItemSourceType[] validItemSources;

        public FindItemSourceTarget(
            GameObject gameObject,
            ItemSourceType[] validItemSources,
            string resourceTypeProperty,
            string pathTargetProperty) : base(gameObject, pathTargetProperty)
        {
            resourceFromBlackboard = true;
            resourceTypePropertyInBlackboard = resourceTypeProperty;
            this.validItemSources = validItemSources;
        }
        public FindItemSourceTarget(
            GameObject gameObject,
            ItemSourceType[] validItemSources,
            Resource resource,
            string pathTargetProperty) : base(gameObject, pathTargetProperty)
        {
            resourceFromBlackboard = false;
            resourceToGrab = resource;
            this.validItemSources = validItemSources;
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            Resource resourceType = resourceToGrab;
            if (resourceFromBlackboard && !blackboard.TryGetValueOfType(resourceTypePropertyInBlackboard, out resourceType))
            {
                return null;
            }
            return componentValue
                .GetClosestOfTypeWithPath(
                    member => ItemSourceFilterByResourceType(member, resourceType)
                );
        }

        private bool ItemSourceFilterByResourceType(
            TileMapMember member,
            Resource resourceToFind
            )
        {
            var itemSources = member.GetComponents<IItemSource>();
            return itemSources.Any(itemSource =>
            {
                return validItemSources.Contains(itemSource.ItemSourceType)
                    && itemSource.HasResource(resourceToFind);
            });
        }
    }
}
