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
            if (!this.TryPositionAllAnchors())
            {
                foreach (var anchor in anchorPreviewers)
                {
                    anchor.gameObject.SetActive(false);
                }
            }
        }
        
        private bool TryPositionAllAnchors()
        {
            var boundingPoints = previewer.ownRegionData.BoundingPoints().ToArray();
            for(var i = 0; i < anchorPreviewers.Count; i++)
            {
                var boundingPoint = boundingPoints[i];
                var anchor = anchorPreviewers[i];
                var previewCoordinate = CombinationTileMapManager.instance.ClosestNonPreviewValidCoordinate(boundingPoint);
                if (!previewCoordinate.IsValid())
                {
                    return false;
                }
                anchor.gameObject.SetActive(true);
                anchor.SetPosition(previewCoordinate);
            }
            return true;
        }
    }
}
