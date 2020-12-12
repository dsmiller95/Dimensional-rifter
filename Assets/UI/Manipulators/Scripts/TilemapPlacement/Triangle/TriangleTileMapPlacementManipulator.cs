using Assets.Behaviors.Scripts;
using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    [CreateAssetMenu(fileName = "TriangleTileMapPlacement", menuName = "TileMap/Manipulators/TriangleTileMapPlacement", order = 2)]
    public class TriangleTileMapPlacementManipulator : MapManipulator
    {
        public GameObject tilemapPlacingPreviewPrefab;
        public TileMapMember anchorMemberBuildingPreviewPrefab;
        public float zLayer;
        // TODO: accomidate multi-tile-type placement
        public TileMapMember anchorBuildPrefab;

        private StateMachine<TriangleTileMapPlacementManipulator> dragEditStateMachine;

        [NonSerialized]
        public UniversalCoordinate regionRootCoordinate;
        [NonSerialized]
        public TileMapRegionPreview previewer;
        [NonSerialized]
        public List<TileMapMember> anchorPreviewers;
        public bool placementValid;

        public override void OnOpen(ManipulatorController controller)
        {
            dragEditStateMachine = new StateMachine<TriangleTileMapPlacementManipulator>(new DragStartDetectState());
        }

        public override void OnClose()
        {
            dragEditStateMachine.ForceSetState(new DragStartDetectState(), this);
            dragEditStateMachine = null;
        }

        public override void OnUpdate()
        {
            dragEditStateMachine.update(this);
        }

        public void PositionAnchors()
        {
            var meshRenderer = previewer.GetComponent<MeshRenderer>();
            placementValid = TryPositionAllAnchors();
            if (placementValid)
            {
                foreach (var anchor in anchorPreviewers)
                {
                    anchor.GetComponent<MeshRenderer>().material.color = Color.white;
                }
                meshRenderer.material.color = Color.white;
            }
            else
            {
                foreach (var anchor in anchorPreviewers)
                {
                    anchor.GetComponent<MeshRenderer>().material.color = Color.red;
                }
                meshRenderer.material.color = Color.red;
            }
        }

        private bool TryPositionAllAnchors()
        {
            var boundingPoints = previewer.ownRegionData.BoundingPoints().ToArray();
            var allValid = true;
            for (var i = 0; i < anchorPreviewers.Count; i++)
            {
                var boundingPoint = boundingPoints[i];
                var anchor = anchorPreviewers[i];
                var previewCoordinate = CombinationTileMapManager.instance.ClosestNonPreviewValidCoordinate(boundingPoint);

                allValid &= IsPositionValid(previewCoordinate);
                anchor.SetPosition(previewCoordinate);
            }
            return allValid;
        }

        private bool IsPositionValid(UniversalCoordinate previewCoordinate)
        {
            if (!previewCoordinate.IsValid())
            {
                return false;
            }
            var propertiesAt = CombinationTileMapManager.instance.everyMember.TilePropertiesAt(previewCoordinate);
            if (!propertiesAt.isPassable)
            {
                return false;
            }
            return true;
        }
    }
}
