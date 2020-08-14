using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UniRx;
using UniRx.Triggers;

namespace Assets.WorldObjects
{
    [Serializable]
    public struct StartingInventoryAmount
    {
        public Resource type;
        public float amount;
    }
    public class ResourceInventory : ObservableTriggerBase
    {
        public IInventory<Resource> inventory;

        public StartingInventoryAmount[] startingInventoryAmounts;

        private InventoryNotifier<Resource> inventoryNotifier;

        void Awake()
        {
            var initialInventory = new Dictionary<Resource, float>();
            var resourceTypes = Enum.GetValues(typeof(Resource)).Cast<Resource>();
            foreach (var resource in resourceTypes)
            {
                // create the key with default. Emit set events in Start(); once everyone has had a chance to subscribe to updates
                initialInventory[resource] = 0;
            }
            foreach (var startingAmount in startingInventoryAmounts)
            {
                initialInventory[startingAmount.type] = startingAmount.amount;
            }

            var itemSource = new BasicInventory<Resource>(
                initialInventory);

            inventory = itemSource; // new TradingInventoryAdapter<ResourceType>(itemSource, ResourceType.Gold);
            inventoryNotifier = new InventoryNotifier<Resource>(inventory, 200);

            //make sure that the observables get initialized by now, at the latest
            ResourceAmountsChangedAsObservable();
            ResourceCapacityChangedAsObservable();

            inventoryNotifier.resourceCapacityChanges += OnResourceCapacityChanged;
            inventoryNotifier.resourceAmountChanged += OnResourceAmountsChanged;
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

    }
}
