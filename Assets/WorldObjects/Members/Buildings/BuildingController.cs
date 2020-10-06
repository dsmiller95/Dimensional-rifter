using Assets.Behaviors.Errands.Scripts;
using Assets.Scripts.Core;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using Assets.WorldObjects.Members.InteractionInterfaces;
using Assets.WorldObjects.Members.Items;
using Assets.WorldObjects.Members.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings
{
    [Serializable]
    class BuildableSaveData
    {
        public float remainingResourceRequirement;
    }
    [DisallowMultipleComponent]
    public class BuildingController : MonoBehaviour,
        IBuildable, ISuppliable, IMemberSaveable,
        IErrandSource<BuildingErrand>, IErrandCompletionReciever<BuildingErrand>
    {
        public ResourceItemType ItemTypeRequriement;
        public float remainingResourceRequirement;
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
            SetRemainingResourceRequirement(remainingResourceRequirement);
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
            return !hasBeenBuilt.CurrentValue && remainingResourceRequirement <= 1e-5;
        }

        public bool CanRecieveSupply()
        {
            return !hasBeenBuilt.CurrentValue && remainingResourceRequirement > 0;
        }
        public bool CanClaimSpaceForMoreOf(Resource resource)
        {
            return resource == ItemTypeRequriement.resourceType && CanRecieveSupply();
        }

        public bool SupplyAllFrom(InventoryHoldingController inventoryToTakeFrom)
        {
            return SupplyFrom(inventoryToTakeFrom, ItemTypeRequriement.resourceType);
        }
        public bool SupplyFrom(InventoryHoldingController inventoryToTakeFrom, Resource resourceType, float amount = -1)
        {
            if (resourceType != ItemTypeRequriement.resourceType)
            {
                return false;
            }
            if (remainingResourceRequirement <= 1e-5)
            {
                return false;
            }
            var amountToTransfer = remainingResourceRequirement;
            if(amount >= 0)
            {
                amountToTransfer = Math.Min(amountToTransfer, amount);
            }

            var toastMsg = new StringBuilder();
            var withdrawlAmt = inventoryToTakeFrom
                .PullItemFromSelf(
                resourceType,
                gameObject,
                toastMsg,
                amountToTransfer);
            if (withdrawlAmt < 1e-5)
            {
                return false;
            }
            SetRemainingResourceRequirement(remainingResourceRequirement - withdrawlAmt);

            ToastProvider.ShowToast(
                toastMsg.ToString(),
                gameObject
                );
            return true;
        }

        private void SetRemainingResourceRequirement(float remaining)
        {
            remainingResourceRequirement = remaining;
            if (remainingResourceRequirement <= 1e-5)
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

        public ISet<Resource> ValidSupplyTypes()
        {
            var result = new HashSet<Resource>();
            result.Add(ItemTypeRequriement.resourceType);
            return result;
        }

        public string IdentifierInsideMember()
        {
            return "Buildable";
        }

        public object GetSaveObject()
        {
            return new BuildableSaveData
            {
                remainingResourceRequirement = remainingResourceRequirement
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is BuildableSaveData savedData)
            {
                SetRemainingResourceRequirement(savedData.remainingResourceRequirement);
            }
            else if (save == null)
            {
                // Assume we were generated via map gen
                SetRemainingResourceRequirement(0);
            }
            else
            {
                Debug.LogError("Error: save data is improperly formatted");
            }
        }

        #region Errands

        private bool BuildErrandActive;

        public BuildingErrand GetErrand(GameObject errandExecutor)
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
            return new BuildingErrand(
                buildErrandType,
                this,
                errandExecutor);
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
    }
}
