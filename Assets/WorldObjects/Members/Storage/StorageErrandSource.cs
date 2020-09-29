using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories;
using Assets.Scripts.Core;
using Assets.WorldObjects.Inventories;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    [CreateAssetMenu(fileName = "StorageErrandSource", menuName = "Behaviors/Errands/StorageErrandSource", order = 10)]
    public class StorageErrandSource : ErrandSourceObject<StoreErrand>, IErrandCompletionReciever<StoreErrand>
    {
        public ErrandBoard board;
        public StoreErrandType errandType; 
        public GenericSelector<IInventory<Resource>> transferInventory;

        public override ErrandType ErrandType => errandType;

        private ISet<IItemSource> itemSources;
        private ISet<ISuppliable> supplyTargets;

        public void Init()
        {
            itemSources = new HashSet<IItemSource>();
            supplyTargets = new HashSet<ISuppliable>();
            board.RegisterErrandSource(this);
        }

        public void RegisterItemSource(IItemSource itemSource)
        {
            itemSources.Add(itemSource);
            //TODO: maybe don't register every time anything happens
            //board.RegisterErrandSource(this);
        }
        public void DeRegisterItemSource(IItemSource itemSource)
        {
            itemSources.Remove(itemSource);
        }

        public void RegisterSuppliable(ISuppliable suppliable)
        {
            supplyTargets.Add(suppliable);
            //board.RegisterErrandSource(this);
        }
        public void DeRegisterSuppliable(ISuppliable itemSource)
        {
            supplyTargets.Remove(itemSource);
        }

        private (IItemSource, ISuppliable, Resource)? GetPossibleSupplyPair(
            IEnumerable<IItemSource> reachableGatherables,
            IEnumerable<ISuppliable> reachableSuppliables)
        {
            var availableResources = new Dictionary<Resource, IList<IItemSource>>();

            var gatherableMembers = reachableGatherables
                .Select(x => new { resources = new HashSet<Resource>(x.AvailableTypes()), member = x });

            var gathererIterator = gatherableMembers.GetEnumerator();
            var supplyableMembers = reachableSuppliables;

            foreach (var supplyable in supplyableMembers)
            {
                var requirements = supplyable.ValidSupplyTypes();
                foreach (var resource in requirements)
                {
                    if (availableResources.TryGetValue(resource, out var memberList))
                    {
                        return (memberList.First(), supplyable, resource);
                    }
                }
                while (gathererIterator.MoveNext())
                {
                    var currentResource = gathererIterator.Current;
                    if (requirements.Overlaps(currentResource.resources))
                    {
                        var overlap = requirements.Intersect(currentResource.resources);
                        return (currentResource.member, supplyable, overlap.First());
                    }

                    IList<IItemSource> gatherablesOfType;
                    foreach (var resourceType in currentResource.resources)
                    {
                        if (!availableResources.TryGetValue(resourceType, out gatherablesOfType))
                        {
                            gatherablesOfType = new List<IItemSource>();
                            availableResources[resourceType] = gatherablesOfType;
                        }
                        gatherablesOfType.Add(currentResource.member);
                    }
                }
            }
            return null;
        }


        #region Errands
        public override StoreErrand GetErrand(GameObject errandExecutor)
        {
            var executorNavigator = errandExecutor.GetComponent<TileMapNavigationMember>();
            var supplyPair = GetPossibleSupplyPair(
                itemSources.Where(source => FilterIfReachable(source, executorNavigator)),
                supplyTargets.Where(source => FilterIfReachable(source, executorNavigator)));
            if (!supplyPair.HasValue)
            {
                return null;
            }
            var (itemSource, suppliable, resource) = supplyPair.Value;
            return new StoreErrand(
                errandType,
                itemSource,
                suppliable,
                resource,
                float.MaxValue,
                errandExecutor,
                transferInventory,
                this);
        }

        private bool FilterIfReachable(object componentMember, TileMapNavigationMember navigator)
        {
            if (componentMember is Component component)
            {
                var member = component.GetComponent<TileMapMember>();
                return navigator.IsReachable(member);
            }
            return false;
        }


        public void ErrandAborted(StoreErrand errand)
        {
            Debug.Log($"Storing errand aborted for actor {errand.storingWorker.name}");
        }

        public void ErrandCompleted(StoreErrand errand)
        {
            Debug.Log($"Storing errand completed for actor {errand.storingWorker.name}");
        }
        #endregion
    }
}
