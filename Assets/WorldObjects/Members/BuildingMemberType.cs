using Assets.Scripts.Core;
using Assets.WorldObjects.SaveObjects;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members
{
    [Serializable]
    public struct ResourceRequirement
    {
        public float amount;
        public Resource type;
    }
    [CreateAssetMenu(fileName = "BuildingType", menuName = "Members/MemberType/BuildingType", order = 3)]
    public class BuildingMemberType : MemberType
    {
        public ResourceRequirement resourceCost;

        //TODO: come up with some generic way to handle different default values when generated vs built
        public string NamePathOfIsBuilt;
        public override InMemberObjectData[] InstantiateNewSaveObject()
        {
            if(NamePathOfIsBuilt == null || NamePathOfIsBuilt.Length <= 0)
            {
                return new InMemberObjectData[0];
            }else
            {
                return new InMemberObjectData[] {
                    new InMemberObjectData
                    {
                        identifierInMember = VariableInstantiator.ConstantIdentifier(),
                        data = new VariableInstantiatorSaveObject
                        {
                            boolValues = new ValueSaveObject<object>[]
                            {
                                new ValueSaveObject<object>
                                {
                                    dataID = NamePathOfIsBuilt,
                                    savedValue = true
                                }
                            },
                            floatValues = new ValueSaveObject<object>[0]
                        }
                    }
                };
            }
        }
    }
}
