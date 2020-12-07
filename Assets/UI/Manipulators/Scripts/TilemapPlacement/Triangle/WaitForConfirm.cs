using Assets.Behaviors.Scripts;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    public class DragEnd : IGenericStateHandler<TriangleTileMapPlacementManipulator>
    {
        public IGenericStateHandler<TriangleTileMapPlacementManipulator> HandleState(TriangleTileMapPlacementManipulator data)
        {
            //TOdO:L do things

            Debug.Log("drag ended");

            return new DragStartDetectState();
        }

        public void TransitionIntoState(TriangleTileMapPlacementManipulator data)
        {

        }

        public void TransitionOutOfState(TriangleTileMapPlacementManipulator data)
        {
        }
    }
}
