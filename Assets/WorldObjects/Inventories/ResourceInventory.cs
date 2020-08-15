using Assets.WorldObjects.SaveObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UniRx;
using UniRx.Triggers;

namespace Assets.WorldObjects
{
    [Serializable]
    public class ResourceInventorySaveData
    {
        public SaveableInventoryAmount[] amounts;
    }

    [Serializable]
    public struct SaveableInventoryAmount
    {
        public Resource type;
        public float amount;
    }
    public class ResourceInventory : ObservableTriggerBase, ISaveable<ResourceInventorySaveData>
    {
        public IInventory<Resource> inventory;

        public SaveableInventoryAmount[] startingInventoryAmounts;

        private InventoryNotifier<Resource> inventoryNotifier;

        void Awake()
        {
            var initialInventory = new Dictionary<Resource, float>();
            SetupInventoryFromAmounts(startingInventoryAmounts);
            var resourceTypes = Enum.GetValues(typeof(Resource)).Cast<Resource>();
            foreach (var resource in resourceTypes)
            {
                // create the key with default. Emit set events in Start(); once everyone has had a chance to subscribe to updates
                initialInventory[resource] = 0;
            }

            inventory = new BasicInventory<Resource>(
                initialInventory);
            this.SetupInventoryFromAmounts(startingInventoryAmounts);

            inventoryNotifier = new InventoryNotifier<Resource>(inventory, 200);

            //make sure that the observables get initialized by now, at the latest
            ResourceAmountsChangedAsObservable();
            ResourceCapacityChangedAsObservable();

            inventoryNotifier.resourceCapacityChanges += OnResourceCapacityChanged;
            inventoryNotifier.resourceAmountChanged += OnResourceAmountsChanged;
        }

        private void SetupInventoryFromAmounts(IEnumerable<SaveableInventoryAmount> amounts)
        {
            foreach (var startingAmount in amounts)
            {
                inventory.SetAmount(startingAmount.type, startingAmount.amount).Execute();
            }
        }

        private ReplaySubject<ResourceChanged<Resource>> resourceAmountsChanged;
        private ReplaySubject<ResourceChanged<Resource>> resourceCapacityChanged;
        public IObservable<ResourceChanged<Resource>> ResourceAmountsChangedAsObservable()
        {
            return resourceAmountsChanged ?? (resourceAmountsChanged = new ReplaySubject<ResourceChanged<Resource>>());
        }
        private void OnResourceAmountsChanged(object sender, ResourceChanged<Resource> change)
        {
            resourceAmountsChanged.OnNext(change);
        }
        public IObservable<ResourceChanged<Resource>> ResourceCapacityChangedAsObservable()
        {
            return resourceCapacityChanged ?? (resourceCapacityChanged = new ReplaySubject<ResourceChanged<Resource>>());
        }
        private void OnResourceCapacityChanged(object sender, ResourceChanged<Resource> change)
        {
            resourceCapacityChanged.OnNext(change);
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            resourceAmountsChanged?.OnCompleted();
            resourceCapacityChanged?.OnCompleted();
        }

        public ResourceInventorySaveData GetSaveObject()
        {
            var saveAmounts = this.inventory.GetCurrentResourceAmounts().Select(x => new SaveableInventoryAmount
            {
                type = x.Key,
                amount = x.Value
            }).ToArray();
            return new ResourceInventorySaveData
            {
                amounts = saveAmounts 
            };
        }

        public void SetupFromSaveObject(ResourceInventorySaveData save)
        {
            this.SetupInventoryFromAmounts(save.amounts);
        }
    }
}
