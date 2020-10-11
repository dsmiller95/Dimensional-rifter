using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [FactoryGraphNode("Leaf/GetFoodToEat", "GetFoodToEat", 0)]
    public class GetFoodToEatFactory : LeafFactory
    {
        public float targetCalories;
        public float caloriesPerFood;
        public string foodBitsTempPath = "FoodBits";
        public ItemSourceType[] validItemSources;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            var hungry = target.GetComponent<Hungry>();

            return new Sequence(
                new LabmdaLeaf(blackboard =>
                {
                    var calories = hungry.currentCalories;
                    var calorieDeficit = targetCalories - hungry.currentCalories;
                    if (calorieDeficit <= 0)
                    {
                        return NodeStatus.FAILURE;
                    }
                    var desiredFoodBits = calorieDeficit / caloriesPerFood;
                    blackboard.SetValue(foodBitsTempPath, desiredFoodBits);

                    return NodeStatus.SUCCESS;
                }),
                GatherOfTypeFactory.GatherResourceOfType(
                    target,
                    validItemSources,
                    WorldObjects.Resource.FOOD,
                    foodBitsTempPath)
                );
        }
    }
}
