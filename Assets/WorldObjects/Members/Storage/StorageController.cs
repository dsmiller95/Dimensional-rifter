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
            if (resourceType.HasValue)
            {
                if(amount < 0)
                {
                    myInventory.DrainAllInto(inventoryToGatherInto, resourceType.Value);
                }else
                {
                    myInventory.TransferResourceInto(resourceType.Value, inventoryToGatherInto, amount).Execute();
                }
            }else
            {
                if(amount > 0)
                {
                    Debug.LogError("Setting an amount but no resource makes no sense, ignoring amount");
                }
                myInventory.DrainAllInto(inventoryToGatherInto, myInventory.GetAllResourceTypes().ToArray());
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