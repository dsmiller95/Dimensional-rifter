using Assets.Behaviors.Scripts.FunctionalStates;
using Assets.WorldObjects;
using Assets.WorldObjects.Members.Building;
using Assets.WorldObjects.Members.Food;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    [CreateAssetMenu(fileName = "SeekAndBuild", menuName = "Tasks/SeekAndBuild", order = 10)]
    public class SeekAndBuildTaskType : TaskType
    {
        public override IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState)
        {
            if (sourceMember is TileMapNavigationMember navigation)
            {
                var content = navigation.currentRegion.universalContentTracker.allMembers;
                var pair = GetPossibleBuildingPair(
                    content.Where(GatheringFilter).Where(x => navigation.IsReachable(x)),
                    content.Where(BuildingDeliveryFilter).Where(x => navigation.IsReachable(x))
                    );
                if (!pair.HasValue)
                {
                    return null;
                }
                var gatherable = pair.Value.Item2.GetComponent<IGatherable>();
                var gatheredResource = gatherable.GatherableType;
                var resultState = new Gathering(gatheredResource);
                resultState
                    .ContinueWith(new Storing(member => BuildingDeliveryFilterByResourceAvailable(member, gatheredResource)))
                    .ContinueWith(new Building())
                    .ContinueWith(returnToState);
                return resultState;
            }
            throw new System.Exception("Gathering requres a navigation member");
        }

        private (Buildable, TileMapMember)? GetPossibleBuildingPair(
            IEnumerable<TileMapMember> reachableGatherables,
            IEnumerable<TileMapMember> reachableBuildables)
        {
            var availableResources = new Dictionary<Resource, IList<TileMapMember>>();

            var gatherableMembers = reachableGatherables
                .Select(x => new { resource = x.GetComponent<IGatherable>().GatherableType, member = x });

            var gathererIterator = gatherableMembers.GetEnumerator();
            var buildableMembers = reachableBuildables
                .Select(x => x.GetComponent<Buildable>());
            

            foreach (var buildable in buildableMembers)
            {
                var requirements = buildable.ResourceRequirement.Value;
                if (availableResources.TryGetValue(requirements.type, out var memberList))
                {
                    return (buildable, memberList.First());
                }
                while (gathererIterator.MoveNext())
                {
                    var currentResource = gathererIterator.Current;
                    if(currentResource.resource == requirements.type)
                    {
                        return (buildable, currentResource.member);
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
        private bool BuildingDeliveryFilterByResourceAvailable(TileMapMember member, Resource gatheredResource)
        {
            var buildable = member.GetComponent<Buildable>();
            return BuildingDeliveryFilter(buildable) && buildable.ResourceRequirement.Value.type == gatheredResource;
        }
        private bool BuildingDeliveryFilter(TileMapMember member)
        {
            var buildable = member.GetComponent<Buildable>();
            return BuildingDeliveryFilter(buildable);
        }
        private bool BuildingDeliveryFilter(Buildable buildable)
        {
            return buildable != null && buildable.CanBeBuilt();
        }
    }
}
