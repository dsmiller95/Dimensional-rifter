using Assets.Scripts.Utilities;
using UnityEngine;

namespace Assets.UI.Priorities
{
    [CreateAssetMenu(fileName = "PriorityHolderObjectSet", menuName = "Behaviors/Errands/PriorityHolderObjectSet", order = 10)]
    public class PriorityHolderObjectSet : LiveObjectSet<SinglePriorityHolder>
    {
    }
}
