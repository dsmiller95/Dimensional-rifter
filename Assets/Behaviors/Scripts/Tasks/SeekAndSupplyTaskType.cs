using Assets.Behaviors.Scripts.FunctionalStates;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Building;
using Assets.WorldObjects.Members.Food;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    [CreateAssetMenu(fileName = "SeekAndSupply", menuName = "Tasks/SeekAndSupply", order = 10)]
    public class SeekAndSupplyTaskType : TaskType
    {
        public SupplyType myTaskSupplyType;

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
                var gatherable = pair.Value.Item2.GetComponent<IGatherable>();
                var gatheredResource = gatherable.GatherableType;
                var resultState = new Gathering(gatheredResource);
                resultState
                    .ContinueWith(new Supplying(member => SupplyDeliveryFilterByResourceAvailable(member, gatheredResource)))
                    .ContinueWith(returnToState);
                return resultState;
            }
            throw new System.Exception("Gathering requres a navigation member");
        }

        private (Supplyable, TileMapMember)? GetPossibleSupplyPair(
            IEnumerable<TileMapMember> reachableGatherables,
            IEnumerable<TileMapMember> reachableSuppliables)
        {
            var availableResources = new Dictionary<Resource, IList<TileMapMember>>();

            var gatherableMembers = reachableGatherables
                .Select(x => new { resource = x.GetComponent<IGatherable>().GatherableType, member = x });

            var gathererIterator = gatherableMembers.GetEnumerator();
            var supplyableMembers = reachableSuppliables
                .SelectMany(x => x.GetComponents<Supplyable>());
            

            foreach (var supplyable in supplyableMembers)
            {
                var requirements = supplyable.ValidSupplyTypes();
                foreach (var resource in requirements)
                {
                    if (availableResources.TryGetValue(resource, out var memberList))
                    {
                        return (supplyable, memberList.First());
                    }
                }
                while (gathererIterator.MoveNext())
                {
                    var currentResource = gathererIterator.Current;
                    if(requirements.Contains(currentResource.resource))
                    {
                        return (supplyable, currentResource.member);
                    }

                    IList<TileMapMember> gatherablesOfType;
                    if(!availableResources.TryGetValue(currentResource.resource, out gatherablesOfType))
                    {
                        gatherablesOfType = new List<TileMapMember>();
                        availableResources[currentResource.resource] = gatherablesOfType;
                    }
                    gatherablesOfType.Add(currentResource.member);
                }
            }
            return null;
        }

        private bool GatheringFilter(TileMapMember member)
        {
            return member.GetComponent<IGatherable>()?.CanGather() ?? false;
        }
        private bool SupplyDeliveryFilterByResourceAvailable(TileMapMember member, Resource gatheredResource)
        {
            var supplyables = member.GetComponents<Supplyable>();
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
            var supplyables = member.GetComponents<Supplyable>();
            return supplyables.Any(x => SupplyDeliveryFilter(x));
        }
        private bool SupplyDeliveryFilter(Supplyable supplyable)
        {
            return supplyable?.SupplyType == myTaskSupplyType && supplyable.CanRecieveSupply();
        }
    }
}
