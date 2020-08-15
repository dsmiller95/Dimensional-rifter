using Assets.Behaviors;
using Assets.Behaviors.HungryStates;
using Assets.WorldObjects.Members;
using System;
using UnityEngine;

namespace Assets.WorldObjects
{
    [Serializable]
    class HungrySaveObject
    {
        public float hungeringRate;
        public float currentHunger;
        public ResourceInventorySaveData inventory;
    }

    [RequireComponent(typeof(TileMapNavigationMember))]
    public class Hungry : MonoBehaviour, IMemberSaveable
    {
        public float hungeringRate = .1f;
        public float currentHunger = 0;

        StateMachine<Hungry> stateMachine;

        public MemberType GetMemberType()
        {
            return MemberType.HUNGRY;
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
    }
}
