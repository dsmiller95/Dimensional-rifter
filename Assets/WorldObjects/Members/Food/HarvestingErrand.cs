using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    [System.Obsolete("Use Entities")]
    public class HarvestingErrand : IErrand
    {
        private HarvestErrandType errandType;
        public GrowingThingController targetController;
        public ErrandType ErrandType => errandType;

        public GameObject gatheringWorker;

        private IErrandCompletionReciever<HarvestingErrand> completionReciever;
        private bool BehaviorCompleted = false;

        public HarvestingErrand(
            HarvestErrandType errandType,
            GrowingThingController toBeBuilt,
            GameObject gatheringWorker)
        {
            this.errandType = errandType;
            targetController = toBeBuilt;
            this.gatheringWorker = gatheringWorker;
            completionReciever = toBeBuilt;

            SetupBehavior();
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        private void SetupBehavior()
        {
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToBakedTarget(
                    gatheringWorker,
                    targetController.gameObject,
                    "Path",
                    true),
                new NavigateToTarget(
                    gatheringWorker,
                    "Path",
                    "target"),
                new LabmdaLeaf(blackboard =>
                {
                    return NodeStatus.SUCCESS;
                }),
                new Wait(1),
                new LabmdaLeaf(blackboard =>
                {
                    Debug.Log($"Gather behavior completed for {gatheringWorker.name}");
                    var result = targetController.DoHarvest();
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
