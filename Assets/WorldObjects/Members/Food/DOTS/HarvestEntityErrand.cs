using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Food.DOTS;
using Assets.WorldObjects.Members.Items.DOTS;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    public class HarvestEntityErrand : IErrand
    {
        public Entity targetEntity;

        public GameObject gatheringWorker;

        private World entityWorld;
        private EntityCommandBufferSystem commandBufferSystem => entityWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private LooseItemSpawnSystem itemSpawnSystem => entityWorld.GetOrCreateSystem<LooseItemSpawnSystem>();

        private IErrandCompletionReciever<HarvestEntityErrand> completionReciever;
        private bool BehaviorCompleted = false;

        public HarvestEntityErrand(
            World entityWorld,
            HarvestErrandType errandType,
            Entity toBeBuilt,
            GameObject gatheringWorker,
            IErrandCompletionReciever<HarvestEntityErrand> completionReciever)
        {
            this.entityWorld = entityWorld;
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
                    position.Value,
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
                    var commandbuffer = commandBufferSystem.CreateCommandBuffer();
                    var growingData = manager.GetComponentData<GrowingThingComponent>(targetEntity);
                    var result = growingData.AfterHarvested();
                    commandbuffer.SetComponent(targetEntity, growingData);
                    var growthData = manager.GetComponentData<GrowthProductComponent>(targetEntity);
                    itemSpawnSystem.SpawnLooseItem(position.Value, growthData, commandbuffer);

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
