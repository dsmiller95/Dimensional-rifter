using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry
{
    [Serializable]
    class HungrySaveObject
    {
        public float caloriesPerSecond;
        public float currentCalories;
    }

    [RequireComponent(typeof(TileMapNavigationMember))]
    public class Hungry : MonoBehaviour, IMemberSaveable, IInterestingInfo
    {
        public float caloriesUsedPerSecond = 10f;
        public float currentCalories = 2000;


        private static HungrySaveObject GenerateNewSaveObject(float caloriesUsedPerSecond = 10f)
        {
            return new HungrySaveObject
            {
                currentCalories = 2000f,
                caloriesPerSecond = caloriesUsedPerSecond
            };
        }

        public object GetSaveObject()
        {
            return new HungrySaveObject
            {
                currentCalories = currentCalories,
                caloriesPerSecond = caloriesUsedPerSecond
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
            caloriesUsedPerSecond = saveObject.caloriesPerSecond;
            currentCalories = saveObject.currentCalories;
        }


        private void Update()
        {
            currentCalories -= Time.deltaTime * caloriesUsedPerSecond;
        }

        public string GetCurrentInfo()
        {
            return $"Calories: {currentCalories:F1}Cal\n" +
                $"Metabolism: {caloriesUsedPerSecond:F1}Cal/s\n";
        }

        public string IdentifierInsideMember()
        {
            return "Hungry";
        }
    }
}
