using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects.SaveObjects;
using Assets.WorldObjects.SaveObjects.SaveManager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.WorldObjects.WorldGen
{
    [Serializable]
    public class LevelGenerationCompleteEvent : UnityEvent { }

    public class WorldGenerator : MonoBehaviour
    {
        public string GeneratedWorldSave = "newSave";

        public MapGenerationConfiguration mapGenerationConfiguration;

        public LevelGenerationCompleteEvent levelGenComplete;

        public void GenerateAndSaveWorld()
        {
            var world = new WorldSaveObject();
            world.regions = new List<TileRegionSaveObject>();

            var rootCoord = new SquareCoordinate(-mapGenerationConfiguration.baseMapSize.y / 2, -mapGenerationConfiguration.baseMapSize.x / 2);
            var mapSize = RectCoordinateRange.FromCoordsLargestExclusive(
                rootCoord,
                -rootCoord);
            var baseRegion = new RegionGenerator(
                UniversalCoordinateRange.From(mapSize),
                0,
                mapGenerationConfiguration);

            world.members = new UniversalTileMembersSaveObject
            {
                tileKeys = new UniversalCoordinate[0],
                tileValues = new int[0],
                tileTypeInfoByIndex = new Tiling.Tilemapping.TileConfiguration.TileTypeInfo[0],
                members = new List<TileMemberSaveObject>(),
                defaultTile = mapGenerationConfiguration.defaultTile
            };
            world.regions.Add(baseRegion.GenerateSaveObject(world.members));

            WorldSaveManager.ClearECSWorldSaveFiles();
            SerializationManager.Save(WorldSaveManager.GAMEOBJECT_WORLD_ROOT, GeneratedWorldSave, world);
            SaveContext.instance.saveName = GeneratedWorldSave;

            levelGenComplete.Invoke();
        }

    }
}
