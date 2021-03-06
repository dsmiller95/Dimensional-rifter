﻿using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members;
using Assets.WorldObjects.SaveObjects;
using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects
{
    /// <summary>
    /// Provides methods to navigate through a tileMap
    ///     stores information about the current location in the tileMap
    ///     
    /// </summary>
    public class TileMapMember : MonoBehaviour, ISaveable<TileMemberData>, IInterestingInfo, IConvertGameObjectToEntity
    {
        [NonSerialized] // grabbed from object heirarchy
        public CombinationTileMapManager bigManager;

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

        protected UniversalCoordinate coordinatePosition;
        public UniversalCoordinate CoordinatePosition => coordinatePosition;

        public void SetPosition(TileMapMember otherMember)
        {
            SetPosition(otherMember.coordinatePosition);
        }

        public void SetPosition(UniversalCoordinate position)
        {
            if (this != null)
                bigManager?.everyMember.DeRegisterInTileMap(this);

            coordinatePosition = position;
            var newPosition = bigManager?.PositionInRealWorld(coordinatePosition);
            if (newPosition.HasValue)
                transform.position = orderingLayer.ApplyPositionInOrderingLayer(newPosition.Value);

            var offset = GetComponent<OffsetFromCoordinatePositionComponentAuthoring>();
            if (offset != null)
            {
                transform.position += (Vector3)(Vector2)offset.Offset;
            }

            if (this != null)
                bigManager?.everyMember.RegisterInTileMap(this);
        }

        protected void InterplateFromCurrentTo(UniversalCoordinate target, float t)
        {
            var sourcePos = bigManager.PositionInRealWorld(coordinatePosition);
            var targetPos = bigManager.PositionInRealWorld(target);
            var lerped = Vector3.Lerp(sourcePos, targetPos, t);
            lerped.z = transform.position.z;
            transform.position = lerped;

        }

        protected virtual void Awake()
        {
            if (bigManager == null)
            {
                bigManager = GetComponentInParent<CombinationTileMapManager>();
            }
            if (bigManager == null)
            {
                // SINGLETON TIME BAYBEE
                bigManager = GameObject.FindObjectOfType<CombinationTileMapManager>();
            }
        }


        private void OnDestroy()
        {
            bigManager.everyMember.DeRegisterInTileMap(this);
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
                memberID = memberType.myId,
                objectDatas = saveableData.ToArray()
            };
        }

        public void SetupFromSaveObject(TileMemberData save)
        {
            memberType = membersScriptRegistry.GetUniqueObjectFromID(save.memberID);

            var saveables = GetComponents<IMemberSaveable>();
            var saveObjects = save.objectDatas.ToDictionary(x => x.identifierInMember, x => x.data);
            foreach (var saveable in saveables)
            {
                if (saveObjects.TryGetValue(saveable.IdentifierInsideMember(), out var objectData))
                {
                    saveable.SetupFromSaveObject(objectData);
                }
                else
                {
                    saveable.SetupFromSaveObject(null);
                }
            }
        }

        public string GetCurrentInfo()
        {
            return $"Type: {memberType.name}\n";
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            UniversalCoordinatePositionComponent position = new UniversalCoordinatePositionComponent
            {
                Value = coordinatePosition
            };
            dstManager.AddComponentData(entity, position);

            // TODO: move the connectivity system over into DOTS
            dstManager.AddComponentData(entity, new ReachabilityFlagComponent
            {
                RegionBitMask = ulong.MaxValue - 1
            });
            dstManager.AddComponentData(entity, orderingLayer.ToECSComponent());
        }
    }
}