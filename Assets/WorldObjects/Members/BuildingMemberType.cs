using Assets.Scripts.Core;
using Assets.WorldObjects.SaveObjects;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects.Members
{

    [CreateAssetMenu(fileName = "BuildingType", menuName = "Members/MemberType/BuildingType", order = 3)]
    public class BuildingMemberType : MemberType
    {
        [Serializable]
        public struct DefaultBoolValue
        {
            public bool defaultValue;
            public BooleanState stateToSet;
        }
        [Header("Defaults used when generated as part of a map")]
        public DefaultBoolValue[] boolDefaults;

        public override InMemberObjectData[] InstantiateNewSaveObject()
        {
            return new InMemberObjectData[] {
                new InMemberObjectData
                {
                    identifierInMember = VariableInstantiator.ConstantIdentifier(),
                    data = new VariableInstantiatorSaveObject
                    {
                        boolValues = boolDefaults.Select(def => new ValueSaveObject<object>{
                            dataID = def.stateToSet.IdentifierInInstantiator,
                            savedValue = def.defaultValue
                            }).ToArray(),
                        floatValues = new ValueSaveObject<object>[0],
                        inventoryValues = new ValueSaveObject<object>[0],
                    }
                }
            };
        }
    }
}
