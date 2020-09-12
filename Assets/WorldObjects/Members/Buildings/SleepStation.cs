using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.WorldObjects.Members.Building
{
    [RequireComponent(typeof(TileMapMember))]
    public class SleepStation : MonoBehaviour
    {
        public BooleanReference isBuiltAndActive;
        public BooleanReference isOccupied;
        public GameObject occupier;

        /// <summary>
        /// If the the sleep station is currently occupied
        /// </summary>
        /// <returns>True if the station is occupied</returns>
        public bool CanBeOccupied()
        {
            return !isOccupied.CurrentValue && isBuiltAndActive.CurrentValue;
        }

        public bool OccupyStation(GameObject occupier)
        {
            if (!CanBeOccupied())
            {
                return false;
            }
            this.occupier = occupier;
            isOccupied.SetValue(true);
            return true;
        }

        public bool ExitStation(GameObject exiter)
        {
            if (occupier == exiter && isOccupied.CurrentValue)
            {
                isOccupied.SetValue(false);
                return true;
            }
            return false;
        }
    }
}
