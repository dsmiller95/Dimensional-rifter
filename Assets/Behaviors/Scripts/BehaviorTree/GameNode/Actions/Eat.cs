using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry;
using BehaviorTree.Nodes;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Give items to a target object. if resource type pointer is provided only gives items of
    ///     that type. otherwise tried to give everything.
    /// </summary>
    public class Eat : ComponentMemberLeaf<Hungry>
    {
        private GenericSelector<IInventory<Resource>> inventoryToEatFrom;
        private VariableInstantiator variableInstantiator;
        private float caloriesPerFood;

        public Eat(
            GameObject gameObject,
            GenericSelector<IInventory<Resource>> inventoryToEatFrom,
            float caloriesPerFood = 200
            ) : base(gameObject)
        {
            this.inventoryToEatFrom = inventoryToEatFrom;
            variableInstantiator = gameObject.GetComponent<VariableInstantiator>();
            this.caloriesPerFood = caloriesPerFood;
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            var inv = inventoryToEatFrom.GetCurrentValue(variableInstantiator);
            var maximumEatAmount = (componentValue.maximumCalories - componentValue.currentCalories) / caloriesPerFood;
            var consume = inv.Consume(Resource.FOOD, maximumEatAmount);
            
            componentValue.currentCalories += consume.info * caloriesPerFood;
            consume.Execute();

            return consume.info > 1e-5 ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
