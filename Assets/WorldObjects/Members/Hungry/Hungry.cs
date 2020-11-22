using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry
{
    [Serializable]
    class HungrySaveObject
    {
        public float caloriesPerSecond;
        public float currentCalories;
        public float maximumCalories;
    }

    [RequireComponent(typeof(TileMapNavigationMember))]
    public class Hungry : MonoBehaviour, IMemberSaveable, IInterestingInfo
    {
        public float caloriesUsedPerSecond = 10f;
        public float currentCalories = 2000;
        public float maximumCalories = 5000;

        private static readonly float CALORIES_PER_FOOD = 250;

        public void EatAmount(Resource resourceEaten, float amountEaten)
        {
            //TODO: system to map out calories per resource?
            if (resourceEaten == Resource.FOOD)
            {
                currentCalories += amountEaten * CALORIES_PER_FOOD;
                currentCalories = Mathf.Min(currentCalories, maximumCalories);
            }
        }

        public float MaxAmountCanBeEatenOfResource(Resource foodtype)
        {
            if (foodtype == Resource.FOOD)
            {
                return (maximumCalories - currentCalories) / CALORIES_PER_FOOD;
            }
            return 0;
        }


        private static HungrySaveObject GenerateNewSaveObject(float caloriesUsedPerSecond = 10f)
        {
            return new HungrySaveObject
            {
                currentCalories = 2000f,
                caloriesPerSecond = caloriesUsedPerSecond,
                maximumCalories = 5000f
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
            return $"Calories: {currentCalories:F0}Cal\n" +
                $"Metabolism: {caloriesUsedPerSecond:F1}Cal/s\n";
        }

        public string IdentifierInsideMember()
        {
            return "Hungry";
        }
    }
}
