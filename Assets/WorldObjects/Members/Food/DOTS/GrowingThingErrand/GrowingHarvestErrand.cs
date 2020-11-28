using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.DOTS.ErrandClaims;
using Assets.UI.ThingSelection;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using BehaviorTree.Nodes;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food.DOTS.GrowingThingErrand
{
    public class GrowingHarvestErrand :
        BasicErrand<GrowingHarvestErrandResultComponent, GrowingHarvestErrand>
    {
        private LooseItemSpawnSystem itemSpawnSystem => entityWorld.GetOrCreateSystem<LooseItemSpawnSystem>();
        public GrowingHarvestErrand(
            GrowingHarvestErrandResultComponent errandResult,
            World entityWorld,
            GameObject actor,
            IErrandCompletionReciever<GrowingHarvestErrand> completionReciever)
            : base(errandResult, entityWorld, actor, completionReciever)
        {
        }

        protected override BehaviorNode SetupBehavior()
        {
            var toBeHarvest = errandResult.harvestTarget;
            var manager = entityWorld.EntityManager;
            var position = manager.GetComponentData<UniversalCoordinatePositionComponent>(toBeHarvest);
            return
            new Sequence(
                new FindPathToCoordinate(
                    actor,
                    position.Value,
                    "Path",
                    true),
                new NavigateToTarget(
                    actor,
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

                    var growingData = manager.GetComponentData<GrowingThingComponent>(toBeHarvest);
                    var result = growingData.AfterHarvested();
                    commandbuffer.SetComponent(toBeHarvest, growingData);

                    if (result)
                    {
                        var growthData = manager.GetComponentData<GrowthProductComponent>(toBeHarvest);
                        itemSpawnSystem.SpawnLooseItem(position.Value, growthData, commandbuffer);
                    }

                    commandbuffer.RemoveComponent<ErrandClaimComponent>(toBeHarvest);
                    errandClaimCleared = true;

                    BehaviorCompleted = true;
                    completionReciever.ErrandCompleted(this);
                    return NodeStatus.SUCCESS;
                })
            );
        }

        private bool errandClaimCleared = false;

        private void ClearErrandClaim(EntityCommandBuffer commandBuffer, EntityManager manager)
        {
            if (errandClaimCleared || !manager.Exists(errandResult.harvestTarget))
            {
                return;
            }
            errandClaimCleared = true;
            commandBuffer.SetComponent(errandResult.harvestTarget, new ErrandClaimComponent
            {
                Claimed = false
            });
        }
        public override void OnErrandFailToComplete()
        {
            ClearErrandClaim(commandBufferSystem.CreateCommandBuffer(), entityWorld.EntityManager);
        }
    }
}
