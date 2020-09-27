using Assets.Behaviors.Errands.Scripts;
using System;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts.GameErrands
{
    [CreateAssetMenu(fileName = "SupplyErrandType", menuName = "Behaviors/Errands/SupplyErrandType", order = 1)]
    public class SupplyErrandType : ErrandType
    {
        public SupplyErrand CreateErrand()
        {
            throw new NotImplementedException();
        }
    }
}
