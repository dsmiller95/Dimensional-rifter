using Assets.Scripts.ResourceManagement;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class FindItemSourceTarget : FindTargetPath
    {
        private Resource resourceToGrab;
        private string maxAmountInBlackboard;
        private string itemClaimProperty;

        private ItemSourceType[] validItemSources;

        public FindItemSourceTarget(
            GameObject gameObject,
            ItemSourceType[] validItemSources,
            Resource resource,
            string maxAmountInBlackboard,
            string pathTargetProperty,
            string itemClaimProperty) : base(gameObject, pathTargetProperty)
        {
            resourceToGrab = resource;
            this.maxAmountInBlackboard = maxAmountInBlackboard;
            this.itemClaimProperty = itemClaimProperty;
            this.validItemSources = validItemSources;
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            var path = componentValue
                .GetClosestOfTypeWithPath(
                    member => ItemSourceFilterByResourceType(member, resourceToGrab)
                );
            if (path.HasValue)
            {
                var targetSource = path.Value.targetMember.GetComponent<IItemSource>();
                float amount = -1;
                blackboard.TryGetValueOfType(maxAmountInBlackboard, out amount);
                var claim = targetSource.ClaimSubtractionFromSource(resourceToGrab, amount);
                blackboard.SetValue(itemClaimProperty, claim);
            }
            return path;
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
                    && itemSource.HasClaimableResource(resourceToFind);
            });
        }

        public override void Reset(Blackboard blackboard)
        {
            base.Reset(blackboard);
            if(blackboard.TryGetValueOfType(itemClaimProperty, out ResourceAllocation itemClaim))
            {
                itemClaim.Release();
            }
            blackboard.ClearValue(itemClaimProperty);
        }
    }
}
