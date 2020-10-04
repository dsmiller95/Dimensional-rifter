using Assets.UI.Priorities;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry
{
    [Serializable]
    class WorkerSaveObject
    {
        public SinglePriorityHolder.SerializablePriorityHolder priority;
    }

    [DisallowMultipleComponent]
    public class WorkerController : MonoBehaviour, IMemberSaveable, IInterestingInfo
    {
        public SinglePriorityHolder myPriorities;
        public PriorityHolderObjectSet priorityObjectSet;
        public WorkerGenerationConfiguration selfGenerator;
        private void Start()
        {
            priorityObjectSet.AddItem(myPriorities);
        }

        private void OnDestroy()
        {
            priorityObjectSet.RemoveItem(myPriorities);
        }

        public object GetSaveObject()
        {
            return new WorkerSaveObject
            {
                priority = myPriorities.GetSerlizable()
            };
        }

        public string IdentifierInsideMember()
        {
            return "workerController";
        }

        public void SetupFromSaveObject(object save)
        {
            WorkerSaveObject saveObject = save as WorkerSaveObject;
            if (saveObject == null)
            {
                saveObject = selfGenerator.GenerateSaveObject();
            }
            myPriorities = SinglePriorityHolder.FromSerializable(saveObject.priority);
        }

        public string GetCurrentInfo()
        {
            return $"Name: {myPriorities.priorityHolderName}";
        }
    }
}
