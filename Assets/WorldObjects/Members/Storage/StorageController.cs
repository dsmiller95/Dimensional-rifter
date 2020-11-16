using Assets.Scripts.ResourceManagement;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    [Serializable]
    public class StorageSaveData
    {
        public LimitedMultiResourcePoolSaveObject resourcePoolData;
    }

    [DisallowMultipleComponent]
    [System.Obsolete("Use Entities")]
    public class StorageController : MonoBehaviour,
        ISuppliable, IItemSource,
        IMemberSaveable, IInterestingInfo,
        IConvertGameObjectToEntity
    {
        public SuppliableType supplyClassification;
        public SuppliableType SuppliableClassification => supplyClassification;

        public ItemSourceType sourceClassification;
        public ItemSourceType ItemSourceType => sourceClassification;

        public int defaultInventoryCapacity = 10;
        private LimitedMultiResourcePool myInventory;


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
                resourcePoolData = myInventory.GetSaveObject()
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is StorageSaveData typedSaveData)
            {
                myInventory = new LimitedMultiResourcePool(typedSaveData.resourcePoolData);
            }
            else
            {
                myInventory = new LimitedMultiResourcePool(defaultInventoryCapacity);
            }
        }
        #endregion

        #region ISuppliable
        public bool CanClaimSpaceForAny()
        {
            return myInventory.CanAllocateAddition();
        }

        public ISet<Resource> ValidSupplyTypes()
        {
            return new HashSet<Resource>(myInventory.GetResourcesWithAllocatableSpace());
        }

        public bool CanClaimSpaceForMoreOf(Resource resource)
        {
            return myInventory.CanAllocateAddition();
        }
        public ResourceAllocation ClaimAdditionToSuppliable(Resource resourceType, float amount)
        {
            return myInventory.TryAllocateAddition(resourceType, amount);
        }

        public bool SupplyFrom(
            InventoryHoldingController inventoryToTakeFrom,
            ResourceAllocation amount)
        {
            if (!amount.IsTarget(myInventory) || !(amount is LimitedMultiResourcePool.AdditionAllocation typedAllocation))
            {
                Debug.LogError("Attempted to apply allocation to an object which it did not originate from");
                amount.Release();
                return false;
            }
            var toastString = new StringBuilder();
            var takenAmount = inventoryToTakeFrom.PullItemFromSelf(
                typedAllocation.type,
                gameObject,
                toastString,
                amount);
            if (takenAmount <= 1e-5)
            {
                return false;
            }

            ToastProvider.ShowToast(
                toastString.ToString(),
                gameObject
                );
            return true;
        }

        public bool IsClaimValidForThisSuppliable(ResourceAllocation claim)
        {
            return claim.IsTarget(myInventory);
        }
        #endregion

        #region IItemSource
        public IEnumerable<Resource> ClaimableTypes()
        {
            return myInventory.GetResourcesWithAllocatableSubtraction();
        }

        public bool HasClaimableResource(Resource resource)
        {
            return myInventory.CanAllocateSubtraction(resource);
        }

        public ResourceAllocation ClaimSubtractionFromSource(Resource resourceType, float amount = -1)
        {
            return myInventory.TryAllocateSubtraction(resourceType, amount);
        }

        public void GatherInto(
            InventoryHoldingController inventoryToGatherInto,
            ResourceAllocation amount)
        {
            if (!amount.IsTarget(myInventory) || !(amount is LimitedMultiResourcePool.SubtractionAllocation typedAllocation))
            {
                Debug.LogError("Attempted to apply allocation to an object which it did not originate from");
                amount.Release();
                return;
            }
            var toastMessage = new StringBuilder();
            var actualGrabbedAmount = inventoryToGatherInto.GrabItemIntoSelf(
                typedAllocation.type,
                gameObject,
                toastMessage,
                amount);
            if (actualGrabbedAmount <= 1e-5)
            {
                return;
            }
            ToastProvider.ShowToast(
                toastMessage.ToString(),
                gameObject
                );
        }

        #endregion

        #region IInterestingInfo
        public string GetCurrentInfo()
        {
            return "Storage: \n" + myInventory.ToString();
        }
        #endregion

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new ItemAmountsDataComponent
            {
                MaxCapacity = myInventory.maxCapacity,
                TotalAdditionClaims = 0f
            });

            DynamicBuffer<ItemAmountClaimBufferData> itemAmounts = dstManager.AddBuffer<ItemAmountClaimBufferData>(entity);

            foreach (var itemData in myInventory.GetResourceAmountThenAllocatedSubtracts())
            {
                itemAmounts.Add(new ItemAmountClaimBufferData
                {
                    Type = itemData.Item1,
                    Amount = itemData.Item2,
                    TotalSubtractionClaims = itemData.Item3
                });
            }

            dstManager.AddComponentData(entity, new ItemSourceTypeComponent
            {
                SourceTypeFlag = ((uint)1) << ItemSourceType.ID
            });
            dstManager.AddComponentData(entity, new SupplyTypeComponent
            {
                SupplyTypeFlag = ((uint)1) << SuppliableClassification.ID
            });
        }
    }
}