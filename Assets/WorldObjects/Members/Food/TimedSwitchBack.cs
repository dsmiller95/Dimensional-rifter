using Assets.Scripts.Core;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    public class TimedSwitchBack : MonoBehaviour
    {
        public BooleanReference boolToSwitch;
        public float timeToSwitchBack;

        private void Awake()
        {
            boolToSwitch.ValueChanges.TakeUntilDisable(this)
                .Where(value => !value)
                .Subscribe(newValue =>
                {
                    StartSetToTrueTimer();
                }).AddTo(this);
        }

        private float nextTriggerTime;
        private void StartSetToTrueTimer()
        {
            nextTriggerTime = Time.time + timeToSwitchBack;
        }

        private void SetToTrue()
        {
            nextTriggerTime = -1;
            boolToSwitch.SetValue(true);
        }

        private void Update()
        {
            if (nextTriggerTime > 0 && nextTriggerTime < Time.time)
            {
                SetToTrue();
            }
        }
    }
}
