using Assets.Behaviors;
using Assets.WorldObjects.Members.Hungry.HungryStates;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry
{
    [Serializable]
    class HungrySaveObject
    {
        public float hungeringRate;
        public float currentHunger;
        public ResourceInventorySaveData inventory;
    }

    [RequireComponent(typeof(TileMapNavigationMember))]
    public class Hungry : MonoBehaviour, IMemberSaveable, IInterestingInfo
    {
        public float hungeringRate = .1f;
        public float currentHunger = 0;

        StateMachine<Hungry> stateMachine;

        private static HungrySaveObject GenerateNewSaveObject(float hungeringRate = .1f)
        {
            return new HungrySaveObject
            {
                currentHunger = 0f,
                hungeringRate = hungeringRate,
                inventory = ResourceInventory.GenerateEmptySaveObject()
            };
        }

        public object GetSaveObject()
        {
            return new HungrySaveObject
            {
                currentHunger = currentHunger,
                hungeringRate = hungeringRate,
                inventory = GetComponent<ResourceInventory>().GetSaveObject()
            };
        }

        public void SetupFromSaveObject(object save)
        {
            Debug.Log("Setting up hungry");
            var saveObject = save as HungrySaveObject;
            if (saveObject == null)
            {
                saveObject = Hungry.GenerateNewSaveObject();
            }
            hungeringRate = saveObject.hungeringRate;
            currentHunger = saveObject.currentHunger;
            stateMachine.ForceSetState(new HungryDecider(), this);

            GetComponent<ResourceInventory>().SetupFromSaveObject(saveObject.inventory);
        }

        private void Awake()
        {
            stateMachine = new StateMachine<Hungry>(new Foraging());
        }

        private void Update()
        {
            stateMachine.update(this);
            currentHunger += Time.deltaTime * hungeringRate;
        }

        public string GetCurrentInfo()
        {
            return $"Hunger: {currentHunger:F1}\n" +
                $"Hungering Rate: {hungeringRate:F1}";
        }
    }
}
