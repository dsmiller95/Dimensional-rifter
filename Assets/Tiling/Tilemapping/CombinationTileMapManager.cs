﻿using Assets.WorldObjects.SaveObjects;
using Assets.WorldObjects.SaveObjects.SaveManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public class CombinationTileMapManager : MonoBehaviour, ISaveable<WorldSaveObject>
    {
        /// <summary>
        /// used when loading tileMaps from save
        /// </summary>
        public TileMapConfigurationData[] tileMapConfig;
        private Dictionary<CoordinateType, TileMapConfigurationData> _configDataDict;
        private Dictionary<CoordinateType, TileMapConfigurationData> ConfigDataDict
        {
            get
            {
                if (_configDataDict == null)
                {
                    _configDataDict = tileMapConfig.ToDictionary(x => x.coordinateType);
                }
                return _configDataDict;
            }
        }

        public TileMapPreviewsByCoordinateRangeType previewPrefabIndex;

        /// <summary>
        /// the most "On Top" region is at index N, all regions at a lower index will avoid overlapping with it
        /// </summary>
        [Tooltip("Generated by map gen, loaded from save")]
        public TileMapRegionData[] allRegions;
        private TileMapRegionRenderer[] regionBehaviors;

        public UniversalCoordinateSystemMembers everyMember;

        public TileMapRegion regionBehaviorPrefab;

        #region Singleton management
        public static CombinationTileMapManager instance;
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Instance already registered, combination tile map manager is singleton-y");
            }
            instance = this;
            SaveSystemHooks.Instance.PreLoad += ClearInstance;
        }

        private void ClearInstance()
        {
            instance = null;
        }
        #endregion

        private void OnRegionPlaneDataChanged()
        {
            SetPlaneIDs();
            if (regionBehaviors != null)
                foreach (var region in regionBehaviors)
                {
                    Destroy(region.gameObject);
                }

            regionBehaviors = allRegions.Select(x =>
            {
                var region = Instantiate(regionBehaviorPrefab, transform);
                var configData = ConfigDataDict[x.baseRange.CoordinateType];
                region.InitializeForTopologyBake(configData, everyMember);

                return region;
            }).ToArray();
            // TODO: figure out how to load build previews
            previewBehaviors = new List<TileMapRegionPreview>();

            for (short i = 0; i < allRegions.Length; i++)
            {
                var configData = ConfigDataDict[allRegions[i].baseRange.CoordinateType];
                configData.atomataSystem.ExecuteOnRegion(this, i);
            }

            BakeAllTileMapMeshes();
        }

        #region Previewing

        private List<TileMapRegionData> previewData;
        private List<TileMapRegionPreview> previewBehaviors;

        private ushort GetFreePreviewIndex()
        {
            for (ushort i = 0; i < previewData.Count; i++)
            {
                if (previewData[i] == null)
                {
                    return i;
                }
            }
            previewData.Add(null);
            previewBehaviors.Add(null);
            return (ushort)(previewData.Count - 1);
        }

        public ushort BeginPreviewRegion(Matrix4x4 initialTransform, UniversalCoordinateRange initialRange)
        {
            var nextPlanePreviewIndex = GetFreePreviewIndex();

            var newData = new TileMapRegionData
            {
                coordinateTransform = initialTransform,
                planeIDIndex = nextPlanePreviewIndex,
                baseRange = initialRange,
                preview = true
            };
            if (!previewPrefabIndex.keyedPrefabs.TryGetValue(initialRange.rangeType, out var prefabSource))
            {
                throw new System.Exception("prefab for range type not found");
            }
            var newPreview = Instantiate(prefabSource.prefab, transform);

            previewData[nextPlanePreviewIndex] = newData;
            previewBehaviors[nextPlanePreviewIndex] = newPreview;

            var configData = ConfigDataDict[newData.baseRange.CoordinateType];

            newPreview.InitializeForTopologyBake(configData, everyMember);
            newPreview.BakeTopology(newData, new TileMapRegionRenderer[0]);
            newPreview.SetupBoundingCollider(newData);

            SetPreviewColors();
            return nextPlanePreviewIndex;
        }

        public void SetPreviewRegionData(Matrix4x4 regionTransform, UniversalCoordinateRange newRange, ushort previewRegionID)
        {
            var previewer = previewBehaviors[previewRegionID];
            var regionData = previewData[previewRegionID];
            regionData.coordinateTransform = regionTransform;
            regionData.baseRange = newRange;

            previewer.BakeTopology(regionData, new TileMapRegionRenderer[0]);
            previewer.SetupBoundingCollider(regionData);

            SetPreviewColors();
        }
        public void ClosePreviewRegion(ushort previewRegionID)
        {
            Destroy(previewBehaviors[previewRegionID]);
            previewData[previewRegionID] = null;
            previewBehaviors[previewRegionID] = null;
            if (previewRegionID == previewData.Count - 1)
            {
                for (var i = previewData.Count - 1; i >= 0; i--)
                {
                    if (previewData[i] == null)
                    {
                        previewData.RemoveAt(i);
                        previewBehaviors.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            SetPreviewColors();
        }
        private void SetPreviewColors()
        {
            if (previewBehaviors.Count <= 0)
            {
                for (var i = 0; i < allRegions.Length; i++)
                {
                    if (regionBehaviors[i] is TileMapRegion region)
                    {
                        region.SetNoPreviews(allRegions[i]);
                    }
                }
            }
            else
            {
                for (var i = 0; i < allRegions.Length; i++)
                {
                    if (regionBehaviors[i] is TileMapRegion region)
                    {
                        region.SetPreviewOnCollidesWith(allRegions[i], previewBehaviors);
                    }
                }
            }
        }
        #endregion

        private void SetPlaneIDs()
        {
            for (ushort planeID = 0; planeID < allRegions.Length; planeID++)
            {
                allRegions[planeID].planeIDIndex = planeID;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
            SaveSystemHooks.Instance.PreLoad -= ClearInstance;
        }

        private void BakeAllTileMapMeshes(int startFromIndex = -1)
        {
            if (startFromIndex == -1)
            {
                startFromIndex = allRegions.Length - 1;
            }
            for (var regionIndex = startFromIndex; regionIndex >= 0; regionIndex--)
            {
                var regionData = allRegions[regionIndex];
                if (regionData.preview)
                {
                    return;
                }
                var regionBehavior = regionBehaviors[regionIndex];

                regionBehavior.BakeTopology(regionData, regionBehaviors.Skip(regionIndex + 1));

                regionBehavior.SetupBoundingCollider(regionData);
            }
        }


        #region Coordinate Access
        public Vector2 PositionInRealWorld(UniversalCoordinate coordinate)
        {
            var planeData = allRegions[coordinate.CoordinatePlaneID];
            return planeData.coordinateTransform.MultiplyPoint3x4((Vector2)coordinate.ToPositionInPlane());
        }

        public bool ValidCoordinateInOwnPlane(UniversalCoordinate coordinate)
        {
            if (coordinate.CoordinatePlaneID >= allRegions.Length)
            {
                return false;
            }
            return TileMapRegion.IsValidInThisPlane(coordinate, allRegions[coordinate.CoordinatePlaneID]);
        }

        public UniversalCoordinate? GetValidCoordinateFromWorldPosIfExists(Vector2 worldPosition)
        {
            for (short planeID = 0; planeID < allRegions.Length; planeID++)
            {
                var planeData = allRegions[planeID];
                var coordOpt = TileMapRegion.GetCoordinateFromRealPositionIffValid(worldPosition, planeData);
                if (coordOpt.HasValue)
                {
                    return coordOpt.Value;
                }
            }
            return null;
        }

        public UniversalCoordinate GetCoordinateOnSamePlane(Vector2 worldPosition, UniversalCoordinate otherCoordinate)
        {
            var planeID = otherCoordinate.CoordinatePlaneID;
            var planeData = allRegions[planeID];
            return TileMapRegion.GetCoordinateFromRealPosition(worldPosition, planeData);
        }
        #endregion

        public WorldSaveObject GetSaveObject()
        {
            var allRegionSaveData = allRegions.Select((data, planeId) =>
            {
                return data.preview ? null : new TileRegionSaveObject
                {
                    matrixSerialized = new Scripts.Utilities.SerializableMatrix4x4(data.coordinateTransform),
                    range = data.baseRange
                };
            }).Where(x => x != null);

            return new WorldSaveObject
            {
                regions = allRegionSaveData.ToList(),
                members = everyMember.GetSaveObject()
                //regions = allRegions.Select(x => x.GetSaveObject()).ToList()
            };
        }

        public void SetupFromSaveObject(WorldSaveObject save)
        {
            allRegions = save.regions.Select((saved, index) =>
            {
                return new TileMapRegionData
                {
                    coordinateTransform = GetTransformForPlane(saved),
                    planeIDIndex = (ushort)index,
                    baseRange = saved.range
                };
            }).ToArray();
            previewData = new List<TileMapRegionData>();
            everyMember.SetupFromSaveObject(save.members);


            OnRegionPlaneDataChanged();
        }

        private Matrix4x4 GetTransformForPlane(TileRegionSaveObject regionPlane)
        {
            return regionPlane?.matrixSerialized?.GetMatrix() ?? Matrix4x4.identity;
        }

        private void Update()
        {
            //var currentMousePos = MyUtilities.GetMousePos2D();
            //var mouseDelta = currentMousePos - lastMousePos;
            //lastMousePos = currentMousePos;

            //if (Input.GetKeyDown(KeyCode.A) && !isPlacingTileMap)
            //{
            //    var newTileMap = Instantiate(tileMapPrefab, transform).GetComponent<TileMapRegionNoCoordinateType>();
            //    newTileMap.BakeTopologyAvoidingColliders(null);
            //    tileMapToMove = newTileMap;
            //    BeginMovingTileMap(tileMapToMove);
            //}
            //else if (Input.GetMouseButtonDown(0) && isPlacingTileMap)
            //{
            //    FinishMovingTileMap();
            //}

            //if (isPlacingTileMap)
            //{
            //    tileMapToMove.transform.position += (Vector3)(mouseDelta);
            //    UpdateTileMapsBelow(tileMapToMove);
            //}
        }
    }
}
