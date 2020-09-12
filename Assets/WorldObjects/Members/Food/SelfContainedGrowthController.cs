using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    public class SelfContainedGrowthController : MonoBehaviour
    {
        public FloatReference ToIncrease;
        public BooleanReference CanGrow;

        public float changePerSecond;
        public float stoppingPoint;

        private void Awake()
        {
        }

        private bool IsPastStoppingPoint(float currentValue)
        {
            return changePerSecond > 0 ? currentValue >= stoppingPoint : currentValue <= stoppingPoint;
        }

        private void Update()
        {
            var currentValue = ToIncrease.CurrentValue;
            if (!CanGrow.CurrentValue || IsPastStoppingPoint(currentValue))
            {
                return;
            }
            var newValue = currentValue + changePerSecond * Time.deltaTime;
            if (IsPastStoppingPoint(newValue))
            {
                newValue = stoppingPoint;
            }

            ToIncrease.SetValue(newValue);
        }
    }
}
