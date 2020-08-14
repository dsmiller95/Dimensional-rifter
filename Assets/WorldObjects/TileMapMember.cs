using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
using UnityEngine;

/// <summary>
/// Provides methods to navigate through a tileMap
///     stores information about the current location in the tileMap
///     
/// </summary>
public class TileMapMember : MonoBehaviour
{
    public TileMapRegionNoCoordinateType homeRegion;

    public SquareCoordinate position;
    //public TriangleCoordinateSystemBehavior coordinateSystemForInspector;
    //public TriangleCoordinate position;

    protected ICoordinate coordinatePosition;
    public ICoordinate CoordinatePosition => coordinatePosition;
    protected object coordinateSystem;

    public void SetPosition(ICoordinate position, ICoordinateSystem coordinateSystem)
    {
        coordinatePosition = position;
        this.coordinateSystem = coordinateSystem;

        transform.position = UniversalToGenericAdaptors.ToRealPosition(position, coordinateSystem);
    }



    // Start is called before the first frame update
    protected virtual void Start()
    {
        SetPosition(position, homeRegion.UntypedCoordianteSystemWorldSpace);
        homeRegion.universalContentTracker.RegisterInTileMap(this);
    }


    // Update is called once per frame
    void Update()
    {
    }


    private void OnDestroy()
    {
        homeRegion.universalContentTracker.DeRegisterInTileMap(this);
    }


}
