using Assets.Behaviors.Errands.Scripts;
using Assets.Scripts.Core;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.InteractionInterfaces;
using Assets.WorldObjects.Members.Items;
using System;
using System.Collections.Generic;
using TradeModeling.Inventories;
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

        public bool Build()
        {
            if (!IsBuildable())
            {
                return false;
            }
            hasBeenBuilt.SetValue(true);
            errandBoard.DeRegisterErrandSource(this);
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
        public bool IsResourceSupplyable(Resource resource)
        {
            return resource == ItemTypeRequriement.resourceType && CanRecieveSupply();
        }

        public bool SupplyInto(IInventory<Resource> inventoryToTakeFrom, Resource? resourceType = null)
        {
            if (resourceType.HasValue && resourceType.Value != ItemTypeRequriement.resourceType)
            {
                return false;
            }
            if (remainingResourceRequirement <= 1e-5)
            {
                return false;
            }

            var withdrawl = inventoryToTakeFrom.Consume(ItemTypeRequriement.resourceType, remainingResourceRequirement);
            SetRemainingResourceRequirement(remainingResourceRequirement - withdrawl.info);
            withdrawl.Execute();
            return true;
        }

        private void SetRemainingResourceRequirement(float remaining)
        {
            remainingResourceRequirement = remaining;

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
            else if (save != null)
            {
                Debug.LogError("Error: save data is improperly formatted");
            }
        }

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
    }
}
