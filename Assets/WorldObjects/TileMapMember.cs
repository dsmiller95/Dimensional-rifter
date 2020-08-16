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
    public TileMapRegionNoCoordinateType homeRegion;
    public MemberType memberType;


    protected ICoordinate coordinatePosition;
    public ICoordinate CoordinatePosition => coordinatePosition;
    protected object coordinateSystem;

    public void SetPosition(ICoordinate position, ICoordinateSystem coordinateSystem)
    {
        coordinatePosition = position;
        this.coordinateSystem = coordinateSystem;

        transform.position = UniversalToGenericAdaptors.ToRealPosition(position, coordinateSystem);
    }

    protected virtual void Awake()
    {
        if (homeRegion == null)
        {
            homeRegion = GetComponentInParent<TileMapRegionNoCoordinateType>();
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
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
