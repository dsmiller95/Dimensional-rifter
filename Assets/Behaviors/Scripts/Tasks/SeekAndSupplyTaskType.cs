using Assets.Behaviors.Scripts.FunctionalStates;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    [CreateAssetMenu(fileName = "SeekAndSupply", menuName = "Tasks/SeekAndSupply", order = 10)]
    public class SeekAndSupplyTaskType : TaskType
    {
        public ItemSourceType[] validItemSources;
        private ISet<ItemSourceType> _validItems;
        private ISet<ItemSourceType> ValidItemSourceTypes
        {
            get
            {
                if(_validItems == null)
                {
                    _validItems = new HashSet<ItemSourceType>(validItemSources);
                }
                return _validItems;
            }
        }

        public GenericSelector<IInventory<Resource>> selfInventoryToUse;
        public SuppliableType validSupplyTarget;

        public override IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState)
        {
            if (sourceMember is TileMapNavigationMember navigation)
            {
                var content = navigation.currentRegion.universalContentTracker.allMembers;
                var pair = GetPossibleSupplyPair(
                    content.Where(GatheringFilter).Where(x => navigation.IsReachable(x)),
                    content.Where(SupplyDeliveryFilter).Where(x => navigation.IsReachable(x))
                    );
                if (!pair.HasValue)
                {
                    return null;
                }
                var gatheredResource = pair.Value.Item3;
                var resultState = new Gathering(selfInventoryToUse, ValidItemSourceTypes, gatheredResource);
                resultState
                    .ContinueWith(new Supplying(selfInventoryToUse, member => SupplyDeliveryFilterByResourceAvailable(member, gatheredResource)))
                    .ContinueWith(returnToState);
                return resultState;
            }
            throw new System.Exception("Gathering requres a navigation member");
        }

        private (Suppliable, TileMapMember, Resource)? GetPossibleSupplyPair(
            IEnumerable<TileMapMember> reachableGatherables,
            IEnumerable<TileMapMember> reachableSuppliables)
        {
            var availableResources = new Dictionary<Resource, IList<TileMapMember>>();

            var gatherableMembers = reachableGatherables
                .Select(x => new { resources = new HashSet<Resource>(x.GetComponent<ItemSource>().AvailableTypes()), member = x });

            var gathererIterator = gatherableMembers.GetEnumerator();
            var supplyableMembers = reachableSuppliables
                .SelectMany(x => x.GetComponents<Suppliable>());

            foreach (var supplyable in supplyableMembers)
            {
                var requirements = supplyable.ValidSupplyTypes();
                foreach (var resource in requirements)
                {
                    if (availableResources.TryGetValue(resource, out var memberList))
                    {
                        return (supplyable, memberList.First(), resource);
                    }
                }
                while (gathererIterator.MoveNext())
                {
                    var currentResource = gathererIterator.Current;
                    if (requirements.Overlaps(currentResource.resources))
                    {
                        var overlap = requirements.Intersect(currentResource.resources);
                        return (supplyable, currentResource.member, overlap.First());
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
            var suppliers = member.GetComponents<ItemSource>().Where(x => ValidItemSourceTypes.Contains(x.SourceType));
            return suppliers.Any();
        }
        private bool SupplyDeliveryFilterByResourceAvailable(TileMapMember member, Resource gatheredResource)
        {
            var supplyables = member.GetComponents<Suppliable>();
            return supplyables.Any(supplyable =>
            {
                if (!SupplyDeliveryFilter(supplyable))
                {
                    return false;
                }
                return supplyable.IsResourceSupplyable(gatheredResource);
            });
        }
        private bool SupplyDeliveryFilter(TileMapMember member)
        {
            var supplyables = member.GetComponents<Suppliable>();
            return supplyables.Any(x => SupplyDeliveryFilter(x));
        }
        private bool SupplyDeliveryFilter(Suppliable supplyable)
        {
            return supplyable?.SupplyType == validSupplyTarget && supplyable.CanRecieveSupply();
        }
    }
}
