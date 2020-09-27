using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    [CreateAssetMenu(fileName = "ErrandBoard", menuName = "Behaviors/ErrandBoard", order = 1)]
    public class ErrandBoard : ScriptableObject
    {
        public IList<ErrandHandler>[] ErrandsByErrandTypeUniqueID;
        public ErrandTypeRegistry ErrandRegistry;

        /// <summary>
        /// Attempt to claim any errand matching <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="claimer"></param>
        /// <returns>Errand if any found, otherwise null</returns>
        public ErrandHandler AttemptClaimAnyErrandOfType(ErrandType type, GameObject claimer)
        {
            var errandIndex = type.uniqueID;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);
            return ErrandsByErrandTypeUniqueID[errandIndex]
                .FirstOrDefault(errand => errand.Available && errand.TryClaim(claimer));
        }

        public bool DeRegisterErrand(ErrandHandler errandHandler)
        {
            var errandIndex = errandHandler.errand.ErrandType.uniqueID;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);
            Debug.Log($"Deregistered errand: {errandHandler.errand.ErrandType.name}");
            return ErrandsByErrandTypeUniqueID[errandIndex].Remove(errandHandler);
        }

        public void RegisterErrand(IErrand errand)
        {
            var errandIndex = errand.ErrandType.uniqueID;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);

            var errandHandler = new ErrandHandler(errand, this);
            ErrandsByErrandTypeUniqueID[errandIndex].Add(errandHandler);
            Debug.Log($"Registered errand: {errand.ErrandType.name}");
        }

        private void ExtendErrandMappingToLengthIfNeeded(int errandIndex)
        {
            if (ErrandsByErrandTypeUniqueID != null && errandIndex < ErrandsByErrandTypeUniqueID.Length)
            {
                return;
            }
            var newArray = new IList<ErrandHandler>[errandIndex + 1];
            int i = 0;
            if (ErrandsByErrandTypeUniqueID != null)
            {
                for (; i < ErrandsByErrandTypeUniqueID.Length; i++)
                {
                    newArray[i] = ErrandsByErrandTypeUniqueID[i];
                }
            }
            for (; i < newArray.Length; i++)
            {
                newArray[i] = new List<ErrandHandler>();
            }

            ErrandsByErrandTypeUniqueID = newArray;
        }
    }
}
