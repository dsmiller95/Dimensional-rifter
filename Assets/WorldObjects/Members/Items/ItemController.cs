using Assets.Scripts.ResourceManagement;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage;
using Assets.WorldObjects.Members.Storage.DOTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Items
{
    [Serializable]
    public class ItemSaveObject
    {
        //only saving values that are not set in the prefab in the inspector
        public LimitedResourcePoolSaveObject remainingResourceAmount;
    }

    [DisallowMultipleComponent]
    public class ItemController : MonoBehaviour, IItemSource, IMemberSaveable, IInterestingInfo, IConvertGameObjectToEntity
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

        private bool AddAmountIntoSelf(float extraAmount)
        {
            var additionAllocation = resourceAmount.TryAllocateAddition(extraAmount);
            if (additionAllocation == null)
            {
                return false;
            }
            return additionAllocation.Execute();
        }

        private void OnItemCompletelyGathered()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            storingCleanupErrandSource.DeRegisterItemSource(this);
        }

        /// <summary>
        /// Sets the size of this item. Only meant to be used when creating a new item,
        ///     this will clear any claims on the item.
        /// The position of the item should be set before this is called
        /// </summary>
        /// <param name="newAmount"></param>
        public void InitializeItemToAmount(float newAmount)
        {
            resourceAmount = new LimitedResourcePool(ItemMaxCapacity, newAmount);
            AttemptToMergeWithOtherOnTile();
        }

        private bool AttemptToMergeWithOtherOnTile()
        {
            var tilemember = GetComponent<TileMapMember>();
            var otherItem = tilemember.bigManager.everyMember.GetMembersOnTile(tilemember.CoordinatePosition)
                .Where(x => x != tilemember)
                .Select(x => x.GetComponent<ItemController>())
                .Where(x => x != null && x.resource == resource)
                .FirstOrDefault();
            if (otherItem != default && otherItem.AddAmountIntoSelf(resourceAmount.CurrentAmount))
            {
                Debug.Log("Merged item into another, destroying self");
                Destroy(gameObject);
                return true;
            }
            return false;
        }

        #region IItemSource
        public ItemSourceType ItemSourceType => SourceType;
        public IEnumerable<Resource> ClaimableTypes()
        {
            if (resourceAmount.CanAllocateSubtraction())
            {
                yield return resource.resourceType;
            }
        }
        public bool HasClaimableResource(Resource resource)
        {
            return resource == this.resource.resourceType
                && resourceAmount.CanAllocateSubtraction();
        }
        public ResourceAllocation ClaimSubtractionFromSource(Resource resource, float amount = -1)
        {
            if (resource != this.resource.resourceType)
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
                AttemptToMergeWithOtherOnTile();
            }
        }
        #endregion

        #region IInterestingInfo
        public string GetCurrentInfo()
        {
            return $"Item: {Enum.GetName(typeof(Resource), resource.resourceType)}\n" +
                resourceAmount.ToString();
        }
        #endregion

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new ItemAmountsDataComponent
            {
                MaxCapacity = resourceAmount.MaxCapacity,
                TotalAdditionClaims = 0f,
                LockItemDataBufferTypes = true
            });

            DynamicBuffer<ItemAmountClaimBufferData> itemAmounts = dstManager.AddBuffer<ItemAmountClaimBufferData>(entity);
            itemAmounts.Add(new ItemAmountClaimBufferData
            {
                Type = resource.resourceType,
                Amount = resourceAmount.CurrentAmount,
                TotalSubtractionClaims = 0f
            });

            dstManager.AddComponentData(entity, new LooseItemFlagComponent());

            dstManager.AddComponentData(entity, new ItemSourceTypeComponent
            {
                SourceTypeFlag = ((uint)1) << ItemSourceType.ID
            });
        }
    }
}
