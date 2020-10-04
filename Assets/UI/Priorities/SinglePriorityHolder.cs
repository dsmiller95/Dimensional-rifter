using System;
using UnityEngine;

namespace Assets.UI.Priorities
{
    [CreateAssetMenu(fileName = "SinglePriorityHolder", menuName = "Behaviors/Errands/SinglePriorityHolder", order = 10)]
    public class SinglePriorityHolder : ScriptableObject
    {
        public string priorityHolderName = "bob";
        public int[] priorities;

        public SerializablePriorityHolder GetSerlizable()
        {
            return new SerializablePriorityHolder
            {
                priorities = priorities,
                priorityHolderName = priorityHolderName
            };
        }

        public static SinglePriorityHolder FromSerializable(SerializablePriorityHolder serializable)
        {
            var newInst = ScriptableObject.CreateInstance<SinglePriorityHolder>();
            newInst.priorityHolderName = serializable.priorityHolderName;
            newInst.priorities = serializable.priorities;
            return newInst;
        }

        [Serializable]
        public class SerializablePriorityHolder
        {
            public string priorityHolderName;
            public int[] priorities;
        }
    }

}
