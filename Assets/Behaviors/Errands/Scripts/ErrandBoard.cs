using Assets.WorldObjects.SaveObjects.SaveManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    [CreateAssetMenu(fileName = "ErrandBoard", menuName = "Behaviors/Errands/ErrandBoard", order = 1)]
    public class ErrandBoard : ScriptableObject
    {
        public ISet<IErrandSource<IErrand>>[] ErrandSourcesByErrandTypeID;
        public ErrandTypeRegistry ErrandRegistry;


        public void Init()
        {
            SaveSystemHooks.Instance.PreLoad += ClearErrandSources;
        }

        private void ClearErrandSources()
        {
            if (ErrandSourcesByErrandTypeID == null) return;
            Debug.Log("Clearing all errands");
            foreach (var sourceSet in ErrandSourcesByErrandTypeID)
            {
                sourceSet.Clear();
            }
        }

        /// <summary>
        /// Attempt to claim any errand matching <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="claimer"></param>
        /// <returns>Errand if any found, otherwise null</returns>
        public IErrand AttemptClaimAnyErrandOfType(ErrandType type, GameObject claimer)
        {
            var errandIndex = type.uniqueID;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);
            return ErrandSourcesByErrandTypeID[errandIndex]
                .Select(source => source.GetErrand(claimer))
                .FirstOrDefault(errand => errand != null);
        }

        public bool DeRegisterErrandSource(IErrandSource<IErrand> errandSource)
        {
            var errandType = errandSource.ErrandType;
            var errandIndex = errandType.uniqueID;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);
            Debug.Log($"Deregistered errand source of type: {errandType.name}");
            return ErrandSourcesByErrandTypeID[errandIndex].Remove(errandSource);
        }

        public void RegisterErrandSource(IErrandSource<IErrand> source)
        {
            var errandIndex = source.ErrandType.uniqueID;
            ExtendErrandMappingToLengthIfNeeded(errandIndex);

            ErrandSourcesByErrandTypeID[errandIndex].Add(source);
            Debug.Log($"Registered errand source of type: {source.ErrandType.name}");
        }

        private void ExtendErrandMappingToLengthIfNeeded(int errandIndex)
        {
            if (ErrandSourcesByErrandTypeID != null && errandIndex < ErrandSourcesByErrandTypeID.Length)
            {
                return;
            }
            var newArray = new ISet<IErrandSource<IErrand>>[errandIndex + 1];
            int i = 0;
            if (ErrandSourcesByErrandTypeID != null)
            {
                for (; i < ErrandSourcesByErrandTypeID.Length; i++)
                {
                    newArray[i] = ErrandSourcesByErrandTypeID[i];
                }
            }
            for (; i < newArray.Length; i++)
            {
                newArray[i] = new HashSet<IErrandSource<IErrand>>();
            }

            ErrandSourcesByErrandTypeID = newArray;
        }
    }
}
