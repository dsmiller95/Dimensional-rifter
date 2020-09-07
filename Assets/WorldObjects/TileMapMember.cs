using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects;
using Assets.WorldObjects.Members;
using Assets.WorldObjects.SaveObjects;
using System.Linq;
using UnityEngine;

/// <summary>
/// Provides methods to navigate through a tileMap
///     stores information about the current location in the tileMap
///     
/// </summary>
public class TileMapMember : MonoBehaviour, ISaveable<TileMemberData>, IInterestingInfo
{
    public TileMapRegionNoCoordinateType currentRegion;

    [Tooltip("Used to load the memberType on save load")]
    public MembersRegistry membersScriptRegistry;

    /// <summary>
    /// This member type is used to determine what this object saves as
    ///     The source of truth for this member time originally comes from the saveObject -- this controls which prefab is instantiated
    ///     If this value is set in a prefab it only determines what it saves as if the prefab is instantiated during game-time
    ///     It will be overwritten on-load
    /// </summary>
    [Tooltip("Member Type is overwritten on save load. This value set in a prefab only effects the very first save after being created during a running game. If set to null, the object will not save")]
    public MemberType memberType;

    public TileMemberOrderingLayer orderingLayer;

    protected ICoordinate coordinatePosition;
    public ICoordinate CoordinatePosition => coordinatePosition;
    protected ICoordinateSystem coordinateSystem => currentRegion.UntypedCoordianteSystemWorldSpace;

    public void SetPosition(TileMapMember otherMember)
    {
        SetPosition(otherMember.coordinatePosition, otherMember.currentRegion);
    }

    public void SetPosition(ICoordinate position, TileMapRegionNoCoordinateType region)
    {
        currentRegion?.universalContentTracker.DeRegisterInTileMap(this);

        coordinatePosition = position;
        currentRegion = region;
        var newPosition = UniversalToGenericAdaptors.ToRealPosition(coordinatePosition, coordinateSystem);
        transform.position = orderingLayer.ApplyPositionInOrderingLayer(newPosition);

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
        var saveAbles = GetComponents<IMemberSaveable>();
        var saveableData = saveAbles.Select(member => new InMemberObjectData
        {
            data = member.GetSaveObject(),
            identifierInMember = member.IdentifierInsideMember()
        });
        var saveAble = GetComponent<IMemberSaveable>();
        return new TileMemberData
        {
            memberType = memberType.uniqueData,
            objectDatas = saveableData.ToArray()
        };
    }

    public void SetupFromSaveObject(TileMemberData save)
    {
        memberType = membersScriptRegistry.GetMemberFromUniqueInfo(save.memberType);

        var saveables = GetComponents<IMemberSaveable>();
        var saveObjects = save.objectDatas.ToDictionary(x => x.identifierInMember, x => x.data);
        foreach (var saveable in saveables)
        {
            if(saveObjects.TryGetValue(saveable.IdentifierInsideMember(), out var objectData))
            {
                saveable.SetupFromSaveObject(objectData);
            }else
            {
                saveable.SetupFromSaveObject(null);
            }
        }
    }

    public string GetCurrentInfo()
    {
        return $"Type: {memberType.name}\n";
    }
}
