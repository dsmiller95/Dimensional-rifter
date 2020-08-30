using Assets.Scripts.Core;
using Assets.WorldObjects;
using System;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Scripts.ObjectVariables
{
    [CreateAssetMenu(fileName = "SingleInventorySelector", menuName = "State/Selectors/SingleInventorySelector", order = 1)]
    public class InventoryDirectSelector : GenericSelector<IInventory<Resource>>
    {
        public InventoryState StateToSelectFrom;

        public override IInventory<Resource> GetCurrentValue(VariableInstantiator instancer)
        {
            return instancer.GetInventoryValue(StateToSelectFrom.IdentifierInInstantiator).CurrentValue;
        }

        public override IObservable<IInventory<Resource>> ValueChanges(VariableInstantiator instancer)
        {
            return instancer.GetInventoryValue(StateToSelectFrom.IdentifierInInstantiator).Value;
        }
    }
}
