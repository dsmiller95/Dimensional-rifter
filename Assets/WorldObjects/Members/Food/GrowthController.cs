using Assets.Scripts.Core;
using UniRx;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    public class GrowthController : MonoBehaviour
    {
        public FloatReference ToIncrease;
        public BooleanReference CanGrow;
        public BooleanReference IsGrown;

        public float increasePerSecond;
        public float completeAmount;

        private void Awake()
        {
            IsGrown.ValueChanges.TakeUntilDisable(this)
                .Subscribe(isGrownNewValue =>
                {
                    if (!isGrownNewValue)
                    {
                        ToIncrease.SetValue(0);
                    }
                }).AddTo(this);

        }

        private void Update()
        {
            if (!CanGrow.CurrentValue || IsGrown.CurrentValue)
            {
                return;
            }
            var newValue = ToIncrease.CurrentValue + increasePerSecond * Time.deltaTime;
            if (newValue >= completeAmount)
            {
                newValue = completeAmount;
                IsGrown.SetValue(true);
            }

            ToIncrease.SetValue(newValue);
        }
    }
}
