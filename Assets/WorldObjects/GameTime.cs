using System;
using UnityEngine;

namespace Assets.WorldObjects
{
    public enum Timezone
    {
        Day,
        Evening,
        Night
    }
    [Serializable]
    public struct TimezoneConfig
    {
        public Timezone zone;
        public float startTime;
    }
    [CreateAssetMenu(fileName = "GameTime", menuName = "GameTime", order = 10)]
    public class GameTime : ScriptableObject
    {
        public float dayLength = 10f;
        public TimezoneConfig[] timezones;
        /// <summary>
        /// the current time in the day; represented as a value between 0 inclusive and 1 exclusive
        /// </summary>
        public float currentTime;

        public void TickTime(float extraTime)
        {
            currentTime += extraTime / dayLength;
            currentTime %= 1;
        }

        public Timezone GetTimezone()
        {
            foreach (var timezone in timezones)
            {
                if (currentTime >= timezone.startTime)
                {
                    return timezone.zone;
                }
            }
            throw new Exception("incorrectly formatted time zone indexes");
        }
    }
}
