using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects.Members;
using Assets.WorldObjects.SaveObjects;
using System;
using UnityEngine;

/// <summary>
/// Provides methods to navigate through a tileMap
///     stores information about the current location in the tileMap
///     
/// </summary>
public class TileMapMember : MonoBehaviour, ISaveable<TileMemberData>
{
    public TileMapRegionNoCoordinateType currentRegion;
    public MemberType memberType;


    protected ICoordinate coordinatePosition;
    public ICoordinate CoordinatePosition => coordinatePosition;
    protected ICoordinateSystem coordinateSystem => currentRegion.UntypedCoordianteSystemWorldSpace;

    public void SetPosition(ICoordinate position, TileMapRegionNoCoordinateType region)
    {
        currentRegion?.universalContentTracker.DeRegisterInTileMap(this);

        coordinatePosition = position;
        this.currentRegion = region;
        transform.position = UniversalToGenericAdaptors.ToRealPosition(coordinatePosition, coordinateSystem);

        currentRegion.universalContentTracker.RegisterInTileMap(this);
    }

    protected virtual void Awake()
    {
        if (currentRegion == null)
        {
            currentRegion = GetComponentInParent<TileMapRegionNoCoordinateType>();
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
    }


    private void OnDestroy()
    {
        currentRegion?.universalContentTracker.DeRegisterInTileMap(this);
    }

    public TileMemberData GetSaveObject()
    {
        var saveAble = GetComponent<IMemberSaveable>();
        return new TileMemberData
        {
            memberType = saveAble.GetMemberType(),
            memberData = saveAble.GetSaveObject()
        };
    }

    public void SetupFromSaveObject(TileMemberData save)
    {
        var saveable = GetComponent<IMemberSaveable>();
        if (saveable == null || saveable.GetMemberType() != save.memberType)
        {
            throw new Exception("Member types do not match! likely a malformed or misplaced prefab");
        }
        saveable.SetupFromSaveObject(save.memberData);
    }
}
