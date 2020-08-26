﻿using Assets.Behaviors.Scripts;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry
{
    [Serializable]
    class HungrySaveObject
    {
        public float hungeringRate;
        public float currentHunger;
    }

    [RequireComponent(typeof(TileMapNavigationMember))]
    public class Hungry : MonoBehaviour, IMemberSaveable, IInterestingInfo
    {
        public float hungeringRate = .1f;
        public float currentHunger = 0;


        private static HungrySaveObject GenerateNewSaveObject(float hungeringRate = .1f)
        {
            return new HungrySaveObject
            {
                currentHunger = 0f,
                hungeringRate = hungeringRate
            };
        }

        public object GetSaveObject()
        {
            return new HungrySaveObject
            {
                currentHunger = currentHunger,
                hungeringRate = hungeringRate
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
        }


        private void Update()
        {
            currentHunger += Time.deltaTime * hungeringRate;
        }

        public string GetCurrentInfo()
        {
            return $"Hunger: {currentHunger:F1}\n" +
                $"Hungering Rate: {hungeringRate:F1}\n";
        }

        public string IdentifierInsideMember()
        {
            return "Hungry";
        }
    }
}
