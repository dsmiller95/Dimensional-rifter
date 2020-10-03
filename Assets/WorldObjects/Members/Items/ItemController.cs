using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using Assets.WorldObjects.Members.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Members.Items
{
    [Serializable]
    class ItemSaveObject
    {
        //only saving values that are not set in the prefab in the inspector
        public float remainingResourceAmount;
    }

    [DisallowMultipleComponent]
    public class ItemController : MonoBehaviour, IItemSource, IMemberSaveable
    {
        public ResourceItemType resource;
        [Tooltip("This amount must be set when spawning an item")]
        public float resourceAmount;
        public ItemSourceType SourceType;

        public ItemSourceType ItemSourceType => SourceType;
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

        public IEnumerable<Resource> AvailableTypes()
        {
            yield return resource.resourceType;
        }

        public void GatherAllInto(InventoryHoldingController inventoryToGatherInto)
        {
            this.GatherInto(inventoryToGatherInto, resource.resourceType);
        }
        public void GatherInto(InventoryHoldingController inventoryToGatherInto, Resource resourceType, float amount = -1)
        {
            var myType = resource.resourceType;
            if (resourceType != myType)
            {
                return;
            }
            if (amount == -1)
            {
                amount = resourceAmount;
            }
            var toastMessage = new StringBuilder();
            var gatheredAmt = inventoryToGatherInto.GrabItemIntoSelf(
                resourceType,
                gameObject,
                toastMessage,
                amount);
            if(gatheredAmt <= 1e-5)
            {
                return;
            }
            resourceAmount -= gatheredAmt;

            ToastProvider.ShowToast(
                toastMessage.ToString(),
                gameObject
                );

            if (resourceAmount <= 1e-5)
            {
                OnItemCompletelyGathered();
            }
        }

        public bool HasResource(Resource resource)
        {
            return resource == this.resource.resourceType;
        }

        public string IdentifierInsideMember()
        {
            return "Item";
        }

        public object GetSaveObject()
        {
            return new ItemSaveObject
            {
                remainingResourceAmount = resourceAmount
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is ItemSaveObject saveObject)
            {
                resourceAmount = saveObject.remainingResourceAmount;
            }
        }
    }
}
