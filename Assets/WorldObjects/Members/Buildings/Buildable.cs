using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.WorldObjects.Members.Building
{
    [RequireComponent(typeof(TileMapMember))]
    public class Buildable : MonoBehaviour
    {
        public BooleanReference isBuilt;
        public BooleanReference isBuildSupplyFull;

        private void Awake()
        {
        }

        private bool IsSelfSetup()
        {
            return !isBuilt.CurrentValue;
        }


        /// <summary>
        /// If the buildable can transition into the built state, given enough resources
        /// </summary>
        /// <returns>True if the buildable is set up and is not already built</returns>
        public bool CanBeBuilt()
        {
            return IsSelfSetup();
        }

        /// <summary>
        /// If the buildable is ready to transition into the build state
        /// </summary>
        /// <returns>True if the buildable CanBeBuilt, and also has the resources required to be built</returns>
        public bool CanBuild()
        {
            if (!CanBeBuilt())
            {
                return false;
            }
            return isBuildSupplyFull.CurrentValue;
        }
        public bool BuildIfPossible()
        {
            if (!CanBuild())
            {
                return false;
            }

            isBuilt.SetValue(true);
            return true;
        }
    }
}
