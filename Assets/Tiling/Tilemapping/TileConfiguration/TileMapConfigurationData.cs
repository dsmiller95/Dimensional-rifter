using Assets.Tiling.TileAutomata;
using Assets.Tiling.Tilemapping.TileConfiguration;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    /// <summary>
    /// contains data used to display and configure tilemaps by coordinate type. should contain nothing
    ///     that gets modified per-plane, ver global
    /// </summary>
    [CreateAssetMenu(fileName = "TileMapConfigData", menuName = "TileMap/ConfigDataObject", order = 1)]
    public class TileMapConfigurationData : ScriptableObject
    {
        public CoordinateType coordinateType;
        public TileSet tileSet;
        public AutomataSystem atomataSystem;
        public Material tileMaterial;
    }
}
