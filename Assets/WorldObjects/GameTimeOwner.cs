using Assets.WorldObjects.Members;
using System;
using UnityEngine;

namespace Assets.WorldObjects
{
    public class GameTimeOwner : MonoBehaviour, IInterestingInfo
    {
        public GameTime timeProvider;

        // Update is called once per frame
        void Update()
        {
            timeProvider.TickTime(Time.deltaTime);
            //sunAnimator?.SetFloat("Motion", currentTime);
        }

        public string GetCurrentInfo()
        {
            var timezoneDescription = Enum.GetName(typeof(Timezone), timeProvider.GetTimezone());
            return $"Time: {timeProvider.currentTime * timeProvider.dayLength:F1}\tPhase: {timezoneDescription}";
        }
    }

}