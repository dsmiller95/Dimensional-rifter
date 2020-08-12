using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping.Square;
using Assets.Tiling.Tilemapping.Triangle;
using Assets.Tiling.TriangleCoords;
using Assets.WorldObjects;
using Extensions;
using System.Linq;
using UnityEngine;


/// <summary>
/// Provides methods to navigate through a tileMap
///     stores information about the current location in the tileMap
///     
/// </summary>
public class TileMapMemeber : MonoBehaviour
{
    //public SquareCoordinateSystemBehavior coordinateSystemForInspector;
    public TriangleCoordinateSystemBehavior coordinateSystemForInspector;
    public TriangleCoordinate position;

    private ICoordinate coordinatePosition;
    private object coordinateSystem;

    public void SetPosition<T>(T position, ICoordinateSystem<T> coordinateSystem) where T : ICoordinate
    {
        coordinatePosition = position;
        this.coordinateSystem = coordinateSystem;

        transform.position = coordinateSystem.ToRealPosition(position);
    }


    // Start is called before the first frame update
    void Start()
    {
        SetPosition(position, coordinateSystemForInspector.BaseCoordinateSystem);
        lastMove = Time.time;
    }

    public float movementSpeed = .5f;
    private float lastMove;

    // Update is called once per frame
    void Update()
    {
        if (lastMove + movementSpeed < Time.time)
        {
            lastMove = Time.time;

            var position = (TriangleCoordinate)coordinatePosition;
            var system = coordinateSystem as ICoordinateSystem<TriangleCoordinate>;

            var nextOptions = system.Neighbors(position).ToArray();
            var nextPosition = nextOptions[Random.Range(0, nextOptions.Length)];

            SetPosition(nextPosition, system);
            UpdatePath(nextPosition, system);
        }
    }

    private Vector2[] path;

    private void OnDrawGizmos()
    {
        if (path == null)
        {
            return;
        }
        Gizmos.color = Color.magenta;
        foreach (var pair in path.RollingWindow(2))
        {
            Gizmos.DrawLine(pair[0], pair[1]);
        }
    }

    private void UpdatePath(TriangleCoordinate position, ICoordinateSystem<TriangleCoordinate> coordinateSystem)
    {
        var pather = new Pathfinder<TriangleCoordinate>(position, coordinateSystem);
        var destination = coordinateSystem.DefaultCoordinate();
        path = pather.ShortestPathTo(destination)
            .Concat(new[] { position })
            .Select(coord => coordinateSystem.ToRealPosition(coord))
            .ToArray();
    }
}
