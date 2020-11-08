using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Food.DOTS;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS
{
    public class BuildEntityErrand : IErrand
    {
        private BuildingErrandType errandType;
        public Entity targetEntity;
        public ErrandType ErrandType => errandType;

        public GameObject gatheringWorker;

        private World entityWorld;

        private IErrandCompletionReciever<BuildEntityErrand> completionReciever;
        private bool BehaviorCompleted = false;

        public BuildEntityErrand(
            World entityWorld,
            BuildingErrandType errandType,
            Entity toBeBuilt,
            GameObject gatheringWorker,
            IErrandCompletionReciever<BuildEntityErrand> completionReciever)
        {
            this.entityWorld = entityWorld;
            this.errandType = errandType;
            targetEntity = toBeBuilt;
            this.gatheringWorker = gatheringWorker;
            this.completionReciever = completionReciever;

            SetupBehavior();
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        private void SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var position = manager.GetComponentData<UniversalCoordinatePositionComponent>(targetEntity);
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToCoordinate(
                    gatheringWorker,
                    position.coordinate,
                    "Path",
                    true),
                new NavigateToTarget(
                    gatheringWorker,
                    "Path",
                    "target",
                    false),
                new LabmdaLeaf(blackboard =>
                {
                    return NodeStatus.SUCCESS;
                }),
                new Wait(1),
                new LabmdaLeaf(blackboard =>
                {
                    var growingData = manager.GetComponentData<GrowingThingComponent>(targetEntity);
                    var result = growingData.AfterHarvested();
                    manager.SetComponentData(targetEntity, growingData);

                    // TODO: trigger the other side effects of harvest, like creating a new items

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
