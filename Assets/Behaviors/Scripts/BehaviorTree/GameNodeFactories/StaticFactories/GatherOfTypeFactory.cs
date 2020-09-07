using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    public static class GatherOfTypeFactory
    {
        /// <summary>
        /// generates nodes which will gather a resource of a given type.
        ///   TODO: add a node which checks to see if the inventory already has an item of this type?
        /// </summary>
        /// <param name="target">the object to attach the behavior nodes to</param>
        /// <param name="validItemSources"></param>
        /// <param name="inventoryToGatherInto"></param>
        /// <param name="blackboardResourceProperty"></param>
        /// <param name="tempPathProp"></param>
        /// <param name="targetReachedProp"></param>
        /// <returns></returns>
        public static Node GatherResourceOfType(
            GameObject target,
            ItemSourceType[] validItemSources,
            GenericSelector<IInventory<Resource>> inventoryToGatherInto,
            string blackboardResourceProperty,
            string tempPathProp = "itemSourcePath",
            string targetReachedProp = "itemSourceObject")
        {
            return
            new Sequence(
                new FindItemSourceTarget(
                    target,
                    validItemSources,
                    blackboardResourceProperty,
                    tempPathProp
                ),
                new NavigateToTarget(
                    target,
                    tempPathProp,
                    targetReachedProp),
                new Grab(
                    target,
                    targetReachedProp,
                    inventoryToGatherInto,
                    blackboardResourceProperty)
            );
        }
    }
}
