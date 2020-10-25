using Assets.Scripts.Core;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Building
{
    [Serializable]
    public class SleepStationSaveData
    {
        public bool IsClaimed;
    }

    [RequireComponent(typeof(TileMapMember))]
    [DisallowMultipleComponent]
    public class SleepStation : MonoBehaviour, IMemberSaveable
    {
        public BooleanReference isBuiltAndActive;
        public BooleanReference isOccupied;
        public GameObject occupierClaimer;

        public bool IsClaimed = false;

        /// <summary>
        /// If the the sleep station is currently occupied
        /// </summary>
        /// <returns>True if the station is occupied</returns>
        public bool CanBeClaimed()
        {
            return !IsClaimed && !isOccupied.CurrentValue && isBuiltAndActive.CurrentValue;
        }

        public bool ClaimStation(GameObject claimer)
        {
            if (IsClaimed)
            {
                return false;
            }
            IsClaimed = true;
            occupierClaimer = claimer;
            return true;
        }

        public void ReleaseStationClaim(GameObject releaser)
        {
            if (occupierClaimer == releaser)
            {
                IsClaimed = false;
                occupierClaimer = null;
            }
        }

        private bool CanBeOccupiedBy(GameObject occupier)
        {
            return IsClaimed == true &&
                !isOccupied.CurrentValue &&
                isBuiltAndActive.CurrentValue &&
                occupier == occupierClaimer;
        }

        public bool OccupyStation(GameObject occupier)
        {
            if (!CanBeOccupiedBy(occupier))
            {
                return false;
            }
            isOccupied.SetValue(true);
            return true;
        }

        public bool ExitStation(GameObject exiter)
        {
            if (occupierClaimer == exiter && isOccupied.CurrentValue)
            {
                isOccupied.SetValue(false);
                IsClaimed = false;
                occupierClaimer = null;
                return true;
            }
            return false;
        }

        #region Member saveable
        public string IdentifierInsideMember()
        {
            return "sleepStation";
        }

        public object GetSaveObject()
        {
            return new SleepStationSaveData
            {
                IsClaimed = IsClaimed
            };
        }

        public void SetupFromSaveObject(object save)
        {
            IsClaimed = false;
            isOccupied.SetValue(false);
        }
        #endregion
    }
}
