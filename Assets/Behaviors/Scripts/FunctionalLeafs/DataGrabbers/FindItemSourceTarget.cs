using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers
{
    public class FindItemSourceTarget : FindTargetPath
    {
        private string resourceTypePropertyInBlackboard;
        private ItemSourceType[] validItemSources;

        public FindItemSourceTarget(
            GameObject gameObject,
            ItemSourceType[] validItemSources,
            string resourceTypeProperty,
            string pathTargetProperty) : base(gameObject, pathTargetProperty)
        {
            resourceTypePropertyInBlackboard = resourceTypeProperty;
            this.validItemSources = validItemSources;
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(resourceTypePropertyInBlackboard, out Resource resourceType))
            {
                return componentValue
                    .GetClosestOfTypeWithPath(
                        member => ItemSourceFilterByResourceType(member, resourceType)
                    );
            }
            return null;
        }

        private bool ItemSourceFilterByResourceType(
            TileMapMember member,
            Resource resourceToFind
            )
        {
            var itemSources = member.GetComponents<ItemSource>();
            return itemSources.Any(itemSource =>
            {
                return validItemSources.Contains(itemSource.SourceType)
                    && itemSource.HasResource(resourceToFind);
            });
        }
    }
}
