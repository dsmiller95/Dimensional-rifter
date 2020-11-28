using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.DOTS.ErrandClaims;
using Assets.UI.ThingSelection;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using BehaviorTree.Nodes;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.BuildErrand
{
    public class BuildEntityErrand :
        BasicErrand<BuildErrandResultComponent, BuildEntityErrand>
    {
        public BuildEntityErrand(
            BuildErrandResultComponent errandResult,
            World entityWorld,
            GameObject actor,
            IErrandCompletionReciever<BuildEntityErrand> completionReciever)
            : base(errandResult, entityWorld, actor, completionReciever)
        {
        }

        protected override BehaviorNode SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var toBeBuilt = errandResult.constructTarget;
            var position = manager.GetComponentData<UniversalCoordinatePositionComponent>(toBeBuilt);
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

                    BuildTarget(manager, commandbuffer);
                    this.errandClaimCleared = true;

                    BehaviorCompleted = true;
                    completionReciever.ErrandCompleted(this);
                    return NodeStatus.SUCCESS;
                })
            );
        }

        private bool errandClaimCleared = false;
        private void BuildTarget(EntityManager manager, EntityCommandBuffer commandBuffer)
        {
            var buildingComponent = this.errandResult.constructTarget;
            manager.RemoveChunkComponent<ChunkWorldRenderBounds>(buildingComponent);
            manager.RemoveComponent<RenderBounds>(buildingComponent);

            // remove all rendering and transform data from the building component. turn it into a data container
            var componentsToRemoveFromBuildingBuffer = new ComponentTypes(new ComponentType[] {
                typeof(IsNotBuiltFlag),
                typeof(ErrandClaimComponent),
                typeof(SelectableFlagComponent),
                typeof(SupplyTypeComponent),
                typeof(UniversalCoordinatePositionComponent),
                typeof(OffsetFromCoordinatePositionComponent),
                typeof(OffsetLayerComponent),
                typeof(RenderMesh),

                typeof(LocalToWorld),
                typeof(Rotation),
                typeof(Translation),
                typeof(NonUniformScale),
                typeof(WorldRenderBounds),
                typeof(PerInstanceCullingTag)
                });
            commandBuffer.RemoveComponent(buildingComponent, componentsToRemoveFromBuildingBuffer);

            var buildingChildData = manager.GetComponentData<BuildingChildComponent>(buildingComponent);
            var parentController = manager.GetComponentData<BuildingParentComponent>(buildingChildData.controllerComponent);
            parentController.isBuilt = true;
            commandBuffer.SetComponent(buildingChildData.controllerComponent, parentController);

            commandBuffer.RemoveComponent<Disabled>(buildingChildData.controllerComponent);
        }

        private void ClearErrandClaim(EntityCommandBuffer commandBuffer, EntityManager manager)
        {
            if (errandClaimCleared || !manager.Exists(errandResult.constructTarget))
            {
                return;
            }
            errandClaimCleared = true;
            commandBuffer.SetComponent(errandResult.constructTarget, new ErrandClaimComponent
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
