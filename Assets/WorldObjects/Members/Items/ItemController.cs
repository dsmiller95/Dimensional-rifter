using Assets.WorldObjects.Inventories;
using System;
using System.Collections.Generic;
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
        public float resourceAmount;
        public ItemSourceType SourceType;

        public ItemSourceType ItemSourceType => SourceType;

        private void OnItemCompletelyGathered()
        {
            Destroy(gameObject);
        }

        public IEnumerable<Resource> AvailableTypes()
        {
            yield return resource.resourceType;
        }

        public void GatherInto(IInventory<Resource> inventoryToGatherInto, Resource? resourceType = null, float amount = -1)
        {
            var myType = resource.resourceType;
            if (resourceType.HasValue && resourceType.Value != myType)
            {
                return;
            }
            if (amount == -1)
            {
                amount = resourceAmount;
            }
            var addOption = inventoryToGatherInto.Add(myType, amount);
            resourceAmount -= addOption.info;
            addOption.Execute();

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
