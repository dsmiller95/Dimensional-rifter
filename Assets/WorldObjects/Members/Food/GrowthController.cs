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

        public float changePerSecond;
        public float stoppingPoint;

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
            var newValue = ToIncrease.CurrentValue + changePerSecond * Time.deltaTime;
            if (changePerSecond > 0 ? newValue >= stoppingPoint : newValue <= stoppingPoint)
            {
                newValue = stoppingPoint;
                IsGrown.SetValue(true);
            }

            ToIncrease.SetValue(newValue);
        }
    }
}
