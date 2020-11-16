using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.Inventories;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    [Serializable]
    [Obsolete("Use Entities")]
    public class StorageErrandSourceSlice : IErrandSource<StoreErrand>
    {
        public StoreErrandType errandType;
        public ItemSourceType[] validSources;
        public SuppliableType supplyTypeTarget;

        public ErrandType ErrandType => errandType;
        private StorageErrandSource baseErrandSource;
        private ISet<ItemSourceType> validSourceSet;

        public void Init(StorageErrandSource errandSource)
        {
            baseErrandSource = errandSource;
            validSourceSet = new HashSet<ItemSourceType>(validSources);
        }

        public IErrandSourceNode<StoreErrand> GetErrand(GameObject errandExecutor)
        {
            var executorNavigator = errandExecutor.GetComponent<TileMapNavigationMember>();
            var validSources = baseErrandSource.itemSources
                .Where(source => validSourceSet.Contains(source.ItemSourceType) && FilterIfReachable(source, executorNavigator));
            var validTargets = baseErrandSource.supplyTargets
                .Where(supply => supply.SuppliableClassification == supplyTypeTarget && FilterIfReachable(supply, executorNavigator));
            var errand = baseErrandSource.GetErrand(
                errandExecutor,
                errandType,
                validSources,
                validTargets);
            return new ImmediateErrandSourceNode<StoreErrand>(errand);
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
    }

    [CreateAssetMenu(fileName = "StorageErrandSource", menuName = "Behaviors/Errands/StorageErrandSource", order = 10)]
    [Obsolete("Use Entities")]
    public class StorageErrandSource : ScriptableObject, IErrandCompletionReciever<StoreErrand>
    {
        public ErrandBoard board;
        public StorageErrandSourceSlice[] storageErrandSlices;


        public ISet<IItemSource> itemSources;
        public ISet<ISuppliable> supplyTargets;

        public void Init()
        {
            itemSources = new HashSet<IItemSource>();
            supplyTargets = new HashSet<ISuppliable>();

            //SaveSystemHooks.Instance.PreLoad += ClearItemSourcesAndSupplies;
            //SaveSystemHooks.Instance.PostLoad += RegisterStorageErrandSources;
        }

        private void ClearItemSourcesAndSupplies()
        {
            itemSources.Clear();
            supplyTargets.Clear();
        }

        private void RegisterStorageErrandSources()
        {
            foreach (var sourceSlice in storageErrandSlices)
            {
                sourceSlice.Init(this);
                board.RegisterErrandSource(sourceSlice);
            }
        }

        public void RegisterItemSource(IItemSource itemSource)
        {
            itemSources.Add(itemSource);
        }
        public void DeRegisterItemSource(IItemSource itemSource)
        {
            itemSources.Remove(itemSource);
        }

        public void RegisterSuppliable(ISuppliable suppliable)
        {
            supplyTargets.Add(suppliable);
        }
        public void DeRegisterSuppliable(ISuppliable suppliable)
        {
            supplyTargets.Remove(suppliable);
        }

        private (IItemSource, ISuppliable, Resource)? GetPossibleSupplyPair(
            IEnumerable<IItemSource> reachableGatherables,
            IEnumerable<ISuppliable> reachableSuppliables)
        {
            var availableResources = new Dictionary<Resource, IList<IItemSource>>();

            var gatherableMembers = reachableGatherables
                .Select(x => new { resources = new HashSet<Resource>(x.ClaimableTypes()), member = x });

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
        public StoreErrand GetErrand(
            GameObject errandExecutor,
            StoreErrandType errandType,
            IEnumerable<IItemSource> reachableGatherables,
            IEnumerable<ISuppliable> reachableSuppliables)
        {
            var supplyPair = GetPossibleSupplyPair(reachableGatherables, reachableSuppliables);
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
                this);
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
