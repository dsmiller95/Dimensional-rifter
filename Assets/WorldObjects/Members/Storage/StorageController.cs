using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.Inventories;
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
            }else
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

        public bool SupplyFrom(IInventory<Resource> inventoryToTakeFrom, Resource? resourceType = null)
        {
            var drainResult = resourceType.HasValue ?
                inventoryToTakeFrom.DrainAllInto(myInventory, resourceType.Value)
                :
                inventoryToTakeFrom.DrainAllInto(myInventory, inventoryToTakeFrom.GetAllResourceTypes().ToArray());
            return drainResult.Any(pair => pair.Value > 1e-5);
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

        public void GatherInto(IInventory<Resource> inventoryToGatherInto, Resource? resourceType = null, float amount = -1)
        {
            if(amount < 0)
            {
                Dictionary<Resource, float> transferredResult;
                if (resourceType.HasValue)
                {
                    transferredResult = myInventory.DrainAllInto(inventoryToGatherInto, resourceType.Value);
                }else
                {
                    transferredResult = myInventory.DrainAllInto(inventoryToGatherInto, myInventory.GetAllResourceTypes().ToArray());
                }

                var toastString = "";
                foreach (var kvp in transferredResult)
                {
                    toastString += $"Gathered {kvp.Value} of {Enum.GetName(typeof(Resource), kvp.Key)}\n";
                }

                ToastProvider.ShowToast(
                    toastString,
                    gameObject
                    );
            }
            else if(resourceType.HasValue)
            {
                var transferOption = myInventory.TransferResourceInto(resourceType.Value, inventoryToGatherInto, amount);
                transferOption.Execute();
                ToastProvider.ShowToast(
                    $"Gathered {transferOption.info} of {Enum.GetName(typeof(Resource), resourceType.Value)}\n",
                    gameObject
                    );
            }else
            {
                throw new NotImplementedException("Cannot take a set amount without specifying which resource");
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