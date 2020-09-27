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

        public GameObject buildWorker;

        private IErrandCompletionReciever<BuildingErrand> completionReciever;
        private bool BehaviorCompleted = false;

        public BuildingErrand(
            BuildingErrandType errandType,
            BuildingController toBeBuilt,
            GameObject buildWorker)
        {
            this.errandType = errandType;
            targetController = toBeBuilt;
            this.buildWorker = buildWorker;
            this.completionReciever = toBeBuilt;

            SetupBehavior();
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        private void SetupBehavior()
        {
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToBakedTarget(
                    buildWorker,
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
                    Debug.Log($"Build behavior completed for {buildWorker.name}");
                    var result = targetController.Build();
                    BehaviorCompleted = true;
                    completionReciever.ErrandCompleted(this);
                    return result ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
                })
            );
        }

        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }
        public void OnReset()
        {
            if (!BehaviorCompleted)
            {
                completionReciever.ErrandAborted(this);
            }
            ErrandBehaviorTreeRoot = null;
        }
    }
}
