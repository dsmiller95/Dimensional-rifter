using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using BehaviorTree.Nodes;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
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

        private EntityCommandBufferSystem commandBufferSystem => entityWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private void SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var position = manager.GetComponentData<UniversalCoordinatePositionComponent>(toBeBuilt);
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToCoordinate(
                    buildingWorker,
                    position.Value,
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
                    var commandbuffer = commandBufferSystem.CreateCommandBuffer();

                    SetComponentToBuild(toBeBuilt, manager, commandbuffer);

                    BehaviorCompleted = true;
                    completionReciever.ErrandCompleted(this);
                    return NodeStatus.SUCCESS;
                })
            );
        }

        private static void SetComponentToBuild(Entity buildingComponent, EntityManager manager, EntityCommandBuffer commandBuffer)
        {
            manager.RemoveChunkComponent<ChunkWorldRenderBounds>(buildingComponent);
            manager.RemoveComponent<RenderBounds>(buildingComponent);

            // remove all rendering and transform data from the building component. turn it into a data container
            var componentsToRemoveFromBuildingBuffer = new ComponentTypes(new ComponentType[] {
                typeof(IsNotBuiltFlag),
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
