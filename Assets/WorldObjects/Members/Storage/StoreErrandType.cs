using Assets.Behaviors.Errands.Scripts;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    [CreateAssetMenu(fileName = "SupplyErrandType", menuName = "Behaviors/Errands/SupplyErrandType", order = 1)]
    public class StoreErrandType : ErrandType
    {
        public StoreErrand CreateErrand()
        {
            throw new NotImplementedException();
        }
    }
}
