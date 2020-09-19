using Assets.Scripts.Core;
using Assets.WorldObjects.Members.InteractionInterfaces;
using Assets.WorldObjects.Members.Items;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    /// <summary>
    /// only saving the things which will change as a result of gameplay,
    ///     not stuff set once in the inspector
    /// </summary>
    [Serializable]
    class GrowingThingSaveObject
    {
        public float currentGrowth;
    }

    // can't have multiple on same component due to saveObject tag being constant
    [DisallowMultipleComponent]
    public class GrowingThingController : MonoBehaviour, IHarvestable, IMemberSaveable
    {
        public BooleanReference IsBuiltAndGrowing;

        public float currentGrowthAmount;
        [Tooltip("Must be positive")]
        public float growthPerSecond;
        public float finalGrowthAmount;

        private bool IsGrown;
        public Sprite[] growingSprites;
        public Sprite grownSprite;

        public ResourceItemType resourceToGrow;
        public float amountToGrow;

        public bool DoHarvest()
        {
            if (!HarvestReady())
            {
                return false;
            }
            var thingToSpawn = resourceToGrow.memberPrefab;
            var myMember = GetComponent<TileMapMember>();

            var spawnedTileMemeber = Instantiate(thingToSpawn, transform.parent).GetComponent<TileMapMember>();
            spawnedTileMemeber.SetPosition(myMember);

            var spawnedItem = spawnedTileMemeber.GetComponent<ItemController>();
            spawnedItem.resourceAmount = amountToGrow;

            SetGrownAmount(0f);

            return true;
        }

        public bool HarvestReady()
        {
            return IsGrown;
        }

        private void Awake()
        {
            if (growthPerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(growthPerSecond)} must be above 0");
            }
            SetGrownAmount(currentGrowthAmount);
        }

        private void SetGrownAmount(float grownAmount)
        {
            currentGrowthAmount = grownAmount;
            if (currentGrowthAmount >= finalGrowthAmount)
            {
                currentGrowthAmount = finalGrowthAmount;
                IsGrown = true;
                GetComponent<SpriteRenderer>().sprite = grownSprite;
            }
            else
            {
                IsGrown = false;
                var growthSpriteID = Mathf.FloorToInt((grownAmount / finalGrowthAmount) * growingSprites.Length);
                GetComponent<SpriteRenderer>().sprite = growingSprites[growthSpriteID];
            }
        }

        private void Update()
        {
            if (IsGrown || !IsBuiltAndGrowing.CurrentValue)
            {
                return;
            }
            SetGrownAmount(currentGrowthAmount + (growthPerSecond * Time.deltaTime));
        }

        public string IdentifierInsideMember()
        {
            return "GrowingController";
        }

        public object GetSaveObject()
        {
            return new GrowingThingSaveObject
            {
                currentGrowth = currentGrowthAmount,
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is GrowingThingSaveObject saveObject)
            {
                SetGrownAmount(saveObject.currentGrowth);
            }
        }
    }
}
