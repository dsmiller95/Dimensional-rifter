using Assets.Behaviors.Errands.Scripts;
using UnityEngine;

namespace Assets.UI.Priorities
{
    [CreateAssetMenu(fileName = "PrioritizeErrandsMap", menuName = "Behaviors/Errands/PrioritizeErrandsMap", order = 10)]
    public class PrioritySetToErrandConfiguration : ScriptableObject
    {
        public ErrandType[] errandTypesToSetPrioritiesFor;
    }
}
