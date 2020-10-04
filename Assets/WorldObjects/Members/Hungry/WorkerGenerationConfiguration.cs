using Assets.Scripts.ProceduralGen;
using Assets.UI.Priorities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry
{
    [CreateAssetMenu(fileName = "WorkerGenerator", menuName = "MapGeneration/WorkerGenerator", order = 10)]
    public class WorkerGenerationConfiguration: ScriptableObject
    {
        public NameGen nameGenerator;
        public int prioritySettingCount;
        public int defaultPrioritySetting;
        internal WorkerSaveObject GenerateSaveObject()
        {
            var priorityObject = new SinglePriorityHolder.SerializablePriorityHolder();
            priorityObject.priorities = new int[prioritySettingCount];
            for (int i = 0; i < priorityObject.priorities.Length; i++)
            {
                priorityObject.priorities[i] = defaultPrioritySetting;
            }
            priorityObject.priorityHolderName = nameGenerator.GenerateName();

            return new WorkerSaveObject
            {
                priority = priorityObject
            };
        }
    }
}
