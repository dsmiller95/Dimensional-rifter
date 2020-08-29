using Assets.Scripts.Core;
using Assets.Scripts.Core.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Assets.Scripts.ObjectVariables.Editor
{
    [CustomPropertyDrawer(typeof(InventoryReference))]
    public class InventoryReferenceDrawer : GenericReferenceDrawer
    {
        protected override List<string> GetValidNamePaths(VariableInstantiator instantiator)
        {
            return instantiator.inventoryStateConfig.Select(x => x.IdentifierInInstantiator).ToList();
        }
    }
}
