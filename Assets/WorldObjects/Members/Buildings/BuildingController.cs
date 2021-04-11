using Assets.Behaviors.Errands.Scripts;
using Assets.Scripts.Core;
using Assets.Scripts.ResourceManagement;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Buildings.DOTS;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using Assets.WorldObjects.Members.InteractionInterfaces;
using Assets.WorldObjects.Members.Items;
using Assets.WorldObjects.Members.Storage;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings
{
    [Serializable]
    class BuildableSaveData
    {
        public LimitedResourcePoolSaveObject builtResourceAmount;
    }
    [DisallowMultipleComponent]
    [Obsolete("Use Entities")]
    public class BuildingController : MonoBehaviour,
        IBuildable, ISuppliable, IMemberSaveable,
        IErrandSource<BuildingErrand>, IErrandCompletionReciever<BuildingErrand>,
        IConvertGameObjectToEntity
    {
        public ResourceItemType ItemTypeRequriement;
        public float defaultResourceRequiredAmount;

        public LimitedResourcePool builtAmountPool;

        public BooleanReference hasBeenBuilt;
        public SuppliableType supplyClassification;
        public SuppliableType SuppliableClassification => supplyClassification;

        public ErrandBoard errandBoard;
        public BuildingErrandType buildErrandType;
        public ErrandType ErrandType => buildErrandType;
        public StorageErrandSource storageErrandSource;

        private void Start()
        {
            // Make sure all the errands/suppliables are registered if spawned in via build command
            if (builtAmountPool == null)
            {
                builtAmountPool = new LimitedResourcePool(defaultResourceRequiredAmount, 0f);
            }
            OnResourceAmountChanged();
        }

        public bool Build()
        {
            if (!IsBuildable())
            {
                return false;
            }
            hasBeenBuilt.SetValue(true);
            errandBoard.DeRegisterErrandSource(this);
            ToastProvider.ShowToast(
                "Built",
                gameObject
                );
            return true;
        }
        public bool IsBuildable()
        {
            return !hasBeenBuilt.CurrentValue && builtAmountPool.IsFull();
        }

        #region ISuppliable
        public bool CanClaimSpaceForAny()
        {
            return !hasBeenBuilt.CurrentValue && builtAmountPool.CanAllocateAddition();
        }
        public bool CanClaimSpaceForMoreOf(Resource resource)
        {
            return resource == ItemTypeRequriement.resourceType && CanClaimSpaceForAny();
        }
        public ISet<Resource> ValidSupplyTypes()
        {
            var result = new HashSet<Resource>();
            if (CanClaimSpaceForAny())
            {
                result.Add(ItemTypeRequriement.resourceType);
            }
            return result;
        }
        public ResourceAllocation ClaimAdditionToSuppliable(Resource resourceType, float amount)
        {
            if (resourceType != ItemTypeRequriement.resourceType)
            {
                return null;
            }
            return builtAmountPool.TryAllocateAddition(amount);
        }

        public bool SupplyFrom(
            InventoryHoldingController inventoryToTakeFrom,
            ResourceAllocation amount)
        {
            if (!amount.IsTarget(builtAmountPool))
            {
                Debug.LogError("Attempted to apply allocation to an object which it did not originate from");
                amount.Release();
                return false;
            }

            var toastMsg = new StringBuilder();
            var withdrawlAmt = inventoryToTakeFrom
                .PullItemFromSelf(
                ItemTypeRequriement.resourceType,
                gameObject,
                toastMsg,
                amount);
            if (withdrawlAmt < 1e-5)
            {
                return false;
            }
            OnResourceAmountChanged();

            ToastProvider.ShowToast(
                toastMsg.ToString(),
                gameObject
                );
            return true;
        }

        public bool IsClaimValidForThisSuppliable(ResourceAllocation claim)
        {
            return claim.IsTarget(builtAmountPool);
        }
        #endregion
        private void OnResourceAmountChanged()
        {
            if (!builtAmountPool.CanAllocateAddition())
            {
                storageErrandSource.DeRegisterSuppliable(this);
            }
            else
            {
                storageErrandSource.RegisterSuppliable(this);
            }

            if (IsBuildable())
            {
                errandBoard.RegisterErrandSource(this);
            }
        }


        public string IdentifierInsideMember()
        {
            return "Buildable";
        }

        public object GetSaveObject()
        {
            return new BuildableSaveData
            {
                builtResourceAmount = builtAmountPool.GetSaveObject()
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is BuildableSaveData savedData)
            {
                builtAmountPool = new LimitedResourcePool(savedData.builtResourceAmount);
            }
            else
            {
                // Assume we were generated via map gen
                builtAmountPool = new LimitedResourcePool(defaultResourceRequiredAmount, 0f);
            }
            OnResourceAmountChanged();
        }

        #region Errands

        private bool BuildErrandActive;

        public IErrandSourceNode<BuildingErrand> GetErrand(GameObject errandExecutor)
        {
            if (!IsBuildable())
            {
                Debug.LogError("Errand requested for building controller which should not be registered as a source");
                return null;
            }
            if (BuildErrandActive)
            {
                return null;
            }
            var tilememberActor = errandExecutor.GetComponent<TileMapNavigationMember>();
            var myTileMember = GetComponent<TileMapMember>();
            if (!tilememberActor.IsReachable(myTileMember))
            {
                return null;
            }

            BuildErrandActive = true;
            var errand = new BuildingErrand(
                buildErrandType,
                this,
                errandExecutor);
            return new ImmediateErrandSourceNode<BuildingErrand>(errand);
        }

        public void ErrandCompleted(BuildingErrand errand)
        {
            BuildErrandActive = false;
        }
        public void ErrandAborted(BuildingErrand errand)
        {
            BuildErrandActive = false;
        }

        #endregion
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var parentCoordinatePosition = dstManager.GetComponentData<UniversalCoordinatePositionComponent>(entity);

            var buildingEntity = dstManager.CreateEntity(
                typeof(UniversalCoordinatePositionComponent),
                typeof(IsNotBuiltFlag),
                typeof(ItemAmountsDataComponent),
                typeof(ItemAmountClaimBufferData),
                typeof(SupplyTypeComponent),
                typeof(BuildingChildComponent));

            dstManager.SetComponentData(buildingEntity, parentCoordinatePosition);

            dstManager.SetComponentData(buildingEntity, new ItemAmountsDataComponent
            {
                MaxCapacity = builtAmountPool.MaxCapacity,
                TotalAdditionClaims = 0f,
                LockItemDataBufferTypes = true
            });
            DynamicBuffer<ItemAmountClaimBufferData> itemAmounts = dstManager.GetBuffer<ItemAmountClaimBufferData>(buildingEntity);
            itemAmounts.Add(new ItemAmountClaimBufferData
            {
                Type = ItemTypeRequriement.resourceType,
                Amount = builtAmountPool.CurrentAmount,
                TotalSubtractionClaims = 0f
            });
            dstManager.SetComponentData(buildingEntity, new SupplyTypeComponent
            {
                SupplyTypeFlag = ((uint)1) << SuppliableClassification.myId
            });

            dstManager.SetComponentData(buildingEntity, new BuildingChildComponent
            {
                controllerComponent = entity
            });
            dstManager.AddComponentData(entity, new BuildingParentComponent
            {
                buildingEntity = buildingEntity
            });
        }
    }
}
