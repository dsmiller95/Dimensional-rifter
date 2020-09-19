using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Finds a pair of one <see cref="IItemSource"/> and one <see cref="ISuppliable"/> which can be provided to by that source
    ///     Stores the resource type for which a valid supply pair exists
    /// </summary>
    public class EnsureAvailableResourceSupplyPair : ComponentMemberLeaf<TileMapNavigationMember>
    {
        private string resourceTypeProperty;

        private ISet<ItemSourceType> validItemSources;
        private SuppliableType validSupplyTarget;

        public EnsureAvailableResourceSupplyPair(
            GameObject gameObject,
            ItemSourceType[] validItemSources,
            SuppliableType validItemTarget,
            string resourceTypeProperty) : base(gameObject)
        {
            this.resourceTypeProperty = resourceTypeProperty;

            this.validItemSources = new HashSet<ItemSourceType>(validItemSources);
            validSupplyTarget = validItemTarget;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            var content = componentValue.currentRegion.universalContentTracker.allMembers;
            var resourceType = GetPossibleSupplyPair(
                content.Where(GatheringFilter).Where(x => componentValue.IsReachable(x)),
                content.Where(SupplyDeliveryFilter).Where(x => componentValue.IsReachable(x))
                );
            if (!resourceType.HasValue)
            {
                return NodeStatus.FAILURE;
            }

            blackboard.SetValue(resourceTypeProperty, resourceType.Value);
            return NodeStatus.SUCCESS;
        }

        public override void Reset(Blackboard blackboard)
        {
            blackboard.ClearValue(resourceTypeProperty);
        }

        private Resource? GetPossibleSupplyPair(
            IEnumerable<TileMapMember> reachableGatherables,
            IEnumerable<TileMapMember> reachableSuppliables)
        {
            var availableResources = new Dictionary<Resource, IList<TileMapMember>>();

            var gatherableMembers = reachableGatherables
                .Select(x => new { resources = new HashSet<Resource>(x.GetComponent<IItemSource>().AvailableTypes()), member = x });

            var gathererIterator = gatherableMembers.GetEnumerator();
            var supplyableMembers = reachableSuppliables
                .SelectMany(x => x.GetComponents<ISuppliable>());

            foreach (var supplyable in supplyableMembers)
            {
                var requirements = supplyable.ValidSupplyTypes();
                foreach (var resource in requirements)
                {
                    if (availableResources.TryGetValue(resource, out var memberList))
                    {
                        return resource;
                    }
                }
                while (gathererIterator.MoveNext())
                {
                    var currentResource = gathererIterator.Current;
                    if (requirements.Overlaps(currentResource.resources))
                    {
                        var overlap = requirements.Intersect(currentResource.resources);
                        return overlap.First();
                    }

                    IList<TileMapMember> gatherablesOfType;
                    foreach (var resourceType in currentResource.resources)
                    {
                        if (!availableResources.TryGetValue(resourceType, out gatherablesOfType))
                        {
                            gatherablesOfType = new List<TileMapMember>();
                            availableResources[resourceType] = gatherablesOfType;
                        }
                        gatherablesOfType.Add(currentResource.member);
                    }
                }
            }
            return null;
        }

        private bool GatheringFilter(TileMapMember member)
        {
            var suppliers = member.GetComponents<IItemSource>().Where(x => validItemSources.Contains(x.ItemSourceType));
            return suppliers.Any();
        }
        private bool SupplyDeliveryFilter(TileMapMember member)
        {
            var supplyables = member.GetComponents<ISuppliable>();
            return supplyables.Any(x => SupplyDeliveryFilter(x));
        }
        private bool SupplyDeliveryFilter(ISuppliable supplyable)
        {
            return supplyable?.SuppliableClassification == validSupplyTarget && supplyable.CanRecieveSupply();
        }

    }
}
