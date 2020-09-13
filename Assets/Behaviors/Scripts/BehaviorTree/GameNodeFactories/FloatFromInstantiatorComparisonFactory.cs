using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    public enum ComparisonType
    {
        LESS_THAN,
        GREATER_THAN
    }

    [CreateAssetMenu(fileName = "FloatInstatiatorComparison", menuName = "Behaviors/Actions/FloatInstatiatorComparison", order = 10)]
    public class FloatFromInstantiatorComparisonFactory : LeafFactory
    {
        public float threshold;
        [Tooltip("instantiated state (Less than/Greater Than) threshold")]
        public ComparisonType comparison;
        public FloatState stateToCompareAgainst;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            Func<float, bool> comparisonFunction;
            if (comparison == ComparisonType.GREATER_THAN)
            {
                comparisonFunction = stateValue => stateValue > threshold;
            }
            else
            {
                comparisonFunction = stateValue => stateValue < threshold;
            }
            return
                new FloatFromInstantiatorComparison(
                    target,
                    stateToCompareAgainst,
                    comparisonFunction
                );
        }
    }
}
