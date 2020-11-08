using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Food.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS
{
    public class BuildEntityErrand : IErrand
    {
        private BuildingErrandType errandType;
        public Entity toBeBuilt;
        public ErrandType ErrandType => errandType;

        public GameObject buildingWorker;

        private World entityWorld;

        private IErrandCompletionReciever<BuildEntityErrand> completionReciever;
        private bool BehaviorCompleted = false;

        public BuildEntityErrand(
            World entityWorld,
            BuildingErrandType errandType,
            Entity toBeBuilt,
            GameObject buildingWorker,
            IErrandCompletionReciever<BuildEntityErrand> completionReciever)
        {
            this.entityWorld = entityWorld;
            this.errandType = errandType;
            this.toBeBuilt = toBeBuilt;
            this.buildingWorker = buildingWorker;
            this.completionReciever = completionReciever;

            SetupBehavior();
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        private EntityCommandBufferSystem commandbufferSystem => entityWorld.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        private void SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var position = manager.GetComponentData<UniversalCoordinatePositionComponent>(toBeBuilt);
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToCoordinate(
                    buildingWorker,
                    position.coordinate,
                    "Path",
                    true),
                new NavigateToTarget(
                    buildingWorker,
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
                    var commandbuffer = commandbufferSystem.CreateCommandBuffer();
                    commandbuffer.RemoveComponent<IsNotBuiltFlag>(toBeBuilt);
                    commandbuffer.RemoveComponent<BuildErrandTargetComponent>(toBeBuilt);
                    var growingData = manager.GetComponentData<GrowingThingComponent>(toBeBuilt);
                    var result = growingData.AfterHarvested();
                    manager.SetComponentData(toBeBuilt, growingData);

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
