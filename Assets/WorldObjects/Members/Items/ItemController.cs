using Assets.Scripts.ResourceManagement;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using Assets.WorldObjects.Members.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Assets.Scripts.ResourceManagement.LimitedResourcePool;

namespace Assets.WorldObjects.Members.Items
{
    [Serializable]
    public class ItemSaveObject
    {
        //only saving values that are not set in the prefab in the inspector
        public LimitedResourcePoolSaveObject remainingResourceAmount;
    }

    [DisallowMultipleComponent]
    public class ItemController : MonoBehaviour, IItemSource, IMemberSaveable
    {
        public static readonly float ItemMaxCapacity = 100;
        public static readonly string SaveDataIndentifier = "Item";

        public ResourceItemType resource;
        public ItemSourceType SourceType;

        private LimitedResourcePool resourceAmount;

        public StorageErrandSource storingCleanupErrandSource;

        private void Awake()
        {
            storingCleanupErrandSource.RegisterItemSource(this);
        }

        private void OnItemCompletelyGathered()
        {
            storingCleanupErrandSource.DeRegisterItemSource(this);
            Destroy(gameObject);
        }

        /// <summary>
        /// Sets the size of this item. Only meant to be used when creating a new item,
        ///     this will clear any claims on the item
        /// </summary>
        /// <param name="newAmount"></param>
        public void SetAmountInItem(float newAmount)
        {
            this.resourceAmount = new LimitedResourcePool(ItemMaxCapacity, newAmount);
        }

        #region IItemSource
        public ItemSourceType ItemSourceType => SourceType;
        public IEnumerable<Resource> ClaimableTypes()
        {
            yield return resource.resourceType;
        }
        public bool HasClaimableResource(Resource resource)
        {
            return resource == this.resource.resourceType
                && this.resourceAmount.CanAllocateSubtraction();
        }
        public ResourceAllocation ClaimSubtractionFromSource(Resource resource, float amount = -1)
        {
            if(resource != this.resource.resourceType)
            {
                return null;
            }
            return resourceAmount.TryAllocateSubtraction(amount);
        }

        public void GatherInto(
            InventoryHoldingController inventoryToGatherInto,
            ResourceAllocation amount)
        {
            if (!amount.IsTarget(resourceAmount))
            {
                Debug.LogError("Attempted to apply allocation to an object which it did not originate from");
                amount.Release();
                return;
            }

            var toastMessage = new StringBuilder();
            var gatheredAmt = inventoryToGatherInto.GrabItemIntoSelf(
                resource.resourceType,
                gameObject,
                toastMessage,
                amount);
            if (gatheredAmt <= 1e-5)
            {
                return;
            }

            ToastProvider.ShowToast(
                toastMessage.ToString(),
                gameObject
                );

            if (!resourceAmount.CanAllocateSubtraction())
            {
                OnItemCompletelyGathered();
            }
        }

        #endregion

        #region IMemberSaveable
        public string IdentifierInsideMember()
        {
            return SaveDataIndentifier;
        }

        public object GetSaveObject()
        {
            return new ItemSaveObject
            {
                remainingResourceAmount = resourceAmount.GetSaveObject()
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is ItemSaveObject saveObject)
            {
                resourceAmount = new LimitedResourcePool(saveObject.remainingResourceAmount);
            }
        }
        #endregion
    }
}
