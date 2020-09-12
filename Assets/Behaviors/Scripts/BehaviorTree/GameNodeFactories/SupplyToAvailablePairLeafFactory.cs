using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "SupplyToAvailablePair", menuName = "Behaviors/Actions/SupplyToAvailablePair", order = 10)]
    public class SupplyToAvailablePairLeafFactory : NodeFactory
    {
        public ItemSourceType[] validItemSources;
        public SuppliableType validSupplyTarget;
        public GenericSelector<IInventory<Resource>> selfInventoryToUse;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            // TODO: check if I can supply from my current inventory, if I have anything?
            return
            new Sequence(
                new EnsureAvailableResourceSupplyPair(
                    target,
                    validItemSources,
                    validSupplyTarget,
                    "supplyType"
                ),
                GatherOfTypeFactory.GatherResourceOfType(
                    target,
                    validItemSources,
                    selfInventoryToUse,
                    "supplyType"
                ),
                SupplyToSupplyTypeFactory.SupplyAnyResourceFromSelfToSuppliable(
                    target,
                    validSupplyTarget,
                    selfInventoryToUse
                )
            );
        }
    }
}
