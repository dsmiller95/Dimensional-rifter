using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "Eat", menuName = "Behaviors/Actions/Eat", order = 10)]
    public class EatFactory : NodeFactory
    {
        public GenericSelector<IInventory<Resource>> inventoryToEatFrom;
        public float caloriesPerFood;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new Eat(
                target,
                inventoryToEatFrom,
                caloriesPerFood);
        }
    }
}
