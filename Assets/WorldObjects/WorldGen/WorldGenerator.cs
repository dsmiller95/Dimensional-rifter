using Assets.Tiling.SquareCoords;
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

            var mapSize = new SquareCoordinateRange();

            mapSize.coord0 = new SquareCoordinate(-mapGenerationConfiguration.baseMapSize.y / 2, -mapGenerationConfiguration.baseMapSize.x / 2);
            mapSize.coord1 = -mapSize.coord0;
            var baseRegion = new RegionGenerator<SquareCoordinate>(Tiling.CoordinateSystemType.SQUARE, mapSize, mapGenerationConfiguration);
            world.regions.Add(baseRegion.GenerateSaveObject());

            SerializationManager.Save(GeneratedWorldSave, world);
            SaveContext.instance.saveFile = GeneratedWorldSave;

            levelGenComplete.Invoke();
        }

    }
}
