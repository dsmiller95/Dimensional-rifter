using Assets.Scripts.Core;
using Assets.WorldObjects;
using System;
using TradeModeling.Inventories;

namespace Assets.Scripts.ObjectVariables
{
    [Serializable]
    public class InventoryReference : GenericReference<IInventory<Resource>>
    {
        public InventoryReference(IInventory<Resource> value) : base(value)
        {
            DataSource = ReferenceDataSource.INSTANCER;
        }

        public override GenericVariable<IInventory<Resource>> GetFromInstancer(VariableInstantiator Instancer, string NamePath)
        {
            return Instancer.GetInventoryValue(NamePath);
        }
    }
}
