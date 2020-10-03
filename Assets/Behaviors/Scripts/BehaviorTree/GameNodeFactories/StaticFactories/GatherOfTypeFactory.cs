using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    public static class GatherOfTypeFactory
    {
        public static BehaviorNode GatherResourceOfType(
           GameObject target,
           ItemSourceType[] validItemSources,
           Resource resource,
           string tempPathProp = "itemSourcePath",
           string targetReachedProp = "itemSourceObject")
        {
            return
            new Sequence(
                new FindItemSourceTarget(
                    target,
                    validItemSources,
                    resource,
                    tempPathProp
                ),
                new NavigateToTarget(
                    target,
                    tempPathProp,
                    targetReachedProp),
                Grab.GrabWithAnimation(
                    target,
                    targetReachedProp,
                    resource)
            );
        }
    }
}
