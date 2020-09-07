using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite;
using Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.StaticFactories
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
