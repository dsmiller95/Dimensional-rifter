using Assets.Tiling.Tilemapping;
using Assets.WorldObjects;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tiling.TileAutomata
{
    public class AutomataSystem<T> : MonoBehaviour where T : ICoordinate
    {
        public AutomataRule<T>[] rules;

        public async void ExecuteAutomataStep(ICoordinateRange<T> coordinates, CoordinateSystemMembers<T> tileMemebers)
        {
            await Task.Run(() =>
            {
                foreach (var coordinate in coordinates)
                {
                    foreach (var rule in rules)
                    {
                        if (rule.TryMatch(coordinate, tileMemebers))
                        {
                            break;
                        }
                    }
                }
            });
        }

        private ICoordinateRange<T> CoordRange => GetComponent<TileMapRegion<T>>().CoordinateRange;
        private CoordinateSystemMembers<T> Members => GetComponent<CoordinateSystemMembers<T>>();


        public float updateDelay = 1f;
        private float lastUpdate;

        private void Update()
        {
            if (lastUpdate + updateDelay >= Time.time)
            {
                return;
            }
            lastUpdate = Time.time;

            ExecuteAutomataStep(CoordRange, Members);
        }
    }
}
