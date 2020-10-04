using Assets.WorldObjects.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UI.Priorities
{
    [CreateAssetMenu(fileName = "SinglePriorityHolder", menuName = "Behaviors/Errands/SinglePriorityHolder", order = 10)]
    public class SinglePriorityHolder: ScriptableObject
    {
        public string priorityHolderName = "bob";
        public int[] priorities;
    }
}
