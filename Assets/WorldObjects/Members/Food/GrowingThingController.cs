using Assets.Behaviors.Errands.Scripts;
using Assets.Scripts.Core;
using Assets.WorldObjects.Members.Buildings;
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
    public class GrowingThingController : MonoBehaviour,
        IHarvestable, IMemberSaveable,
        IErrandSource<GatheringErrand>, IErrandCompletionReciever<GatheringErrand>
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


        public ErrandBoard errandBoard;
        public GatheringErrandType gatheringErrandType;
        public ErrandType ErrandType => gatheringErrandType;

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
            errandBoard.DeRegisterErrandSource(this);

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
                errandBoard.RegisterErrandSource(this);
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


        #region Errands
        private bool ErrandActive;

        public GatheringErrand GetErrand(GameObject errandExecutor)
        {
            if (!HarvestReady())
            {
                Debug.LogError("Errand requested for building controller which should not be registered as a source");
                return null;
            }
            if (ErrandActive)
            {
                return null;
            }
            var tilememberActor = errandExecutor.GetComponent<TileMapNavigationMember>();
            var myTileMember = GetComponent<TileMapMember>();
            if (!tilememberActor.IsReachable(myTileMember))
            {
                return null;
            }
            ErrandActive = true;
            return new GatheringErrand(
                gatheringErrandType,
                this,
                errandExecutor);
        }

        public void ErrandCompleted(GatheringErrand errand)
        {
            ErrandActive = false;
        }
        public void ErrandAborted(GatheringErrand errand)
        {
            ErrandActive = false;
        }
        #endregion
    }
}
