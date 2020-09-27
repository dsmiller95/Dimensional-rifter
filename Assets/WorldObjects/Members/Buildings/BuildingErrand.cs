using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings
{
    public class BuildingErrand : IErrand
    {
        private BuildingErrandType errandType;
        public BuildingController targetController;
        public ErrandType ErrandType => errandType;

        public BuildingErrand(BuildingErrandType errandType, BuildingController builder)
        {
            this.errandType = errandType;
            targetController = builder;
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        public void ClaimedBy(GameObject claimer)
        {
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToBakedTarget(
                    claimer,
                    targetController.gameObject,
                    "Path",
                    true),
                new LabmdaLeaf(blackboard =>
                {
                    Debug.Log("Reached buildable");
                    return NodeStatus.SUCCESS;
                }),
                new Wait(1),
                new LabmdaLeaf(blackboard =>
                {
                    Debug.Log($"Build behavior completed for {claimer.name}");

                    return targetController.Build() ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
                })
            );
        }

        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }
    }
}
