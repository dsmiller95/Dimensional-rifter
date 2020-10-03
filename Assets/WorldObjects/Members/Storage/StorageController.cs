using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradeModeling.Inventories;
using UniRx;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    [Serializable]
    public class StorageSaveData
    {
        public int capacity;
        public SaveableInventoryAmount<Resource>[] inventoryContents;
    }

    [DisallowMultipleComponent]
    public class StorageController : MonoBehaviour,
        ISuppliable, IItemSource,
        IMemberSaveable, IInterestingInfo
    {
        public SuppliableType supplyClassification;
        public SuppliableType SuppliableClassification => supplyClassification;

        public ItemSourceType sourceClassification;
        public ItemSourceType ItemSourceType => sourceClassification;

        public int inventoryCapacity = 10;
        private SpaceFillingInventory<Resource> myInventory;


        public StorageErrandSource storingCleanupErrandSource;

        private void Awake()
        {
            storingCleanupErrandSource.RegisterSuppliable(this);
            storingCleanupErrandSource.RegisterItemSource(this);
        }

        #region IMemberSaveable
        public string IdentifierInsideMember()
        {
            return "Storage";
        }

        public object GetSaveObject()
        {
            return new StorageSaveData
            {
                capacity = inventoryCapacity,
                inventoryContents = myInventory.GetSerializableInventoryAmounts()
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is StorageSaveData typedSaveData)
            {
                inventoryCapacity = typedSaveData.capacity;
                myInventory = SpaceFillingInventory<Resource>
                    .GetEmptyInventoryAllSpaceFilling<Resource>(inventoryCapacity);
                myInventory.SetSerializedToInventory(typedSaveData.inventoryContents);
            }
            else
            {
                myInventory = SpaceFillingInventory<Resource>
                    .GetEmptyInventoryAllSpaceFilling<Resource>(inventoryCapacity);
            }
        }
        #endregion

        #region ISuppliable
        public bool CanRecieveSupply()
        {
            return myInventory.remainingCapacity > 0;
        }

        public bool IsResourceSupplyable(Resource resource)
        {
            return myInventory.CanFitMoreOf(resource);
        }

        public bool SupplyAllFrom(InventoryHoldingController inventory)
        {
            var toastString = new StringBuilder();
            if (!inventory.PullAllItemsFromSelfIntoInv(
                myInventory,
                gameObject,
                toastString))
            {
                return false;
            };

            ToastProvider.ShowToast(
                toastString.ToString(),
                gameObject
                );
            return true;
        }

        public bool SupplyFrom(InventoryHoldingController inventoryToTakeFrom, Resource resourceType)
        {
            var toastString = new StringBuilder();
            if (!inventoryToTakeFrom.PullItemFromSelf(myInventory, resourceType, gameObject, toastString))
            {
                return false;
            }

            ToastProvider.ShowToast(
                toastString.ToString(),
                gameObject
                );
            return true;
        }

        public ISet<Resource> ValidSupplyTypes()
        {
            return myInventory.GetResourcesWithSpace();
        }
        #endregion

        #region IItemSource
        public IEnumerable<Resource> AvailableTypes()
        {
            return myInventory.GetResourcesWithAny();
        }

        public bool HasResource(Resource resource)
        {
            return myInventory.Get(resource) > 0;
        }

        public void GatherAllInto(InventoryHoldingController inventoryToGatherInto)
        {
            var toastMessage = new StringBuilder();
            if (inventoryToGatherInto.GrabAllItemsIntoSelf(
                myInventory,
                gameObject,
                toastMessage))
            {
                ToastProvider.ShowToast(
                    toastMessage.ToString(),
                    gameObject
                    );
            }
        }

        public void GatherInto(InventoryHoldingController inventoryToGatherInto, Resource resourceType, float amount = -1)
        {
            var toastMessage = new StringBuilder();
            if (inventoryToGatherInto.GrabItemIntoSelf(
                myInventory,
                resourceType,
                gameObject,
                toastMessage,
                amount))
            {
                ToastProvider.ShowToast(
                    toastMessage.ToString(),
                    gameObject
                    );
            }
        }

        #endregion

        #region IInterestingInfo
        public string GetCurrentInfo()
        {
            var info = new StringBuilder("Storage: \n");
            info.AppendLine($"Capacity: {inventoryCapacity}, remaining: {myInventory.remainingCapacity}");
            SerializeInventoryInto(myInventory, info);

            return info.ToString();
        }
        private void SerializeInventoryInto(IInventory<Resource> inventory, StringBuilder builder)
        {
            foreach (var resource in inventory.GetCurrentResourceAmounts().Where(x => x.Value > 1e-5))
            {
                builder.AppendLine($"{Enum.GetName(typeof(Resource), resource.Key)}: {resource.Value:F1}");
            }
        }
        #endregion

    }
}