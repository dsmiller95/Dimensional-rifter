using Assets.Behaviors.Scripts;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.DOTSMembers.MemberPrefabs;
using Assets.WorldObjects.Members.Buildings.DOTS;
using Assets.WorldObjects.Members.Buildings.DOTS.Anchor;
using Unity.Entities;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    public class WaitForConfirm : IGenericStateHandler<TriangleTileMapPlacementManipulator>
    {
        public WaitForConfirm(bool ImmediateCancel = false)
        {
            Canceled = ImmediateCancel;
        }
        private bool Confirmed;
        private bool Canceled;

        public IGenericStateHandler<TriangleTileMapPlacementManipulator> HandleState(TriangleTileMapPlacementManipulator data)
        {
            if (Confirmed)
            {
                Debug.Log("confirm");
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var newRegionIndex = data.previewer.ReplaceWithRealRegion();
                var anchorPrefab = this.GetAnchorPrefab(data, entityManager);
                foreach (var anchor in data.anchorPreviewers)
                {
                    var newAnchor = entityManager.Instantiate(anchorPrefab);// GameObject.Instantiate(data.anchorBuildPrefab, CombinationTileMapManager.instance.transform);
                    entityManager.SetComponentData(newAnchor, new UniversalCoordinatePositionComponent
                    {
                        Value = anchor.CoordinatePosition
                    });
                    var builder = entityManager.GetComponentData<BuildingParentComponent>(newAnchor).buildingEntity;
                    entityManager.SetComponentData(builder, new UniversalCoordinatePositionComponent
                    {
                        Value = anchor.CoordinatePosition
                    });
                    entityManager.SetComponentData(newAnchor, new TilemapAnchorComponent
                    {
                        AnchoredTileMap = newRegionIndex
                    });
                    GameObject.Destroy(anchor.gameObject);
                }
                return new DragStartDetectState();
            }
            if (Canceled)
            {
                Debug.Log("cancel");
                CombinationTileMapManager.instance.ClosePreviewRegion(data.previewer);
                foreach (var anchor in data.anchorPreviewers)
                {
                    GameObject.Destroy(anchor.gameObject);
                }
                return new DragStartDetectState();
            }
            return this;
        }

        private Entity GetAnchorPrefab(TriangleTileMapPlacementManipulator data, EntityManager entityManager)
        {
            var prefabs = data.memberPrefabQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

            foreach (var prefab in prefabs)
            {
                var memberType = entityManager.GetComponentData<MemberPrefabIDComponent>(prefab);
                if (memberType.prefabID == data.anchorMemberType.memberID)
                {
                    var prefabData = entityManager.GetComponentData<MemberPrefabComponent>(prefab);
                    return prefabData.prefab;
                }
            }
            return Entity.Null;
        }

        public void TransitionIntoState(TriangleTileMapPlacementManipulator data)
        {
            var previewBehavior = data.previewer;
            previewBehavior.ConfirmButton.onClick.AddListener(OnConfirm);
            previewBehavior.CancelButton.onClick.AddListener(OnCancel);
        }
        public void TransitionOutOfState(TriangleTileMapPlacementManipulator data)
        {
            var previewBehavior = data.previewer;
            previewBehavior?.ConfirmButton.onClick.RemoveListener(OnConfirm);
            previewBehavior?.CancelButton.onClick.RemoveListener(OnCancel);
        }

        private void OnConfirm()
        {
            Confirmed = true;
        }
        private void OnCancel()
        {
            Canceled = true;
        }

    }
}
