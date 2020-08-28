using Assets.WorldObjects.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Core
{
    [Serializable]
    public struct BooleanInstanceConfig
    {
        public string name;
        public bool defaultValue;
    }
    [Serializable]
    public struct FloatInstanceConfig
    {
        public string name;
        public float defaultValue;
    }

    [Serializable]  
    class ValueSaveObject<T>
    {
        public string dataID;
        public T savedValue;
    }
    [Serializable]
    class VariableInstantiatorSaveObject
    {
        public ValueSaveObject<bool>[] boolValues;
        public ValueSaveObject<float>[] floatValues;
    }

    public class VariableInstantiator : MonoBehaviour, IMemberSaveable
    {
        public BooleanInstanceConfig[] booleanInstancingConfig;
        public FloatInstanceConfig[] floatInstancingConfig;

        protected IDictionary<string, BooleanVariable> instancedBooleans;
        protected IDictionary<string, FloatVariable> instancedFloats;

        private void Awake()
        {
            if (instancedBooleans == null || instancedFloats == null)
            {
                InstantiateVariables();
            }
        }

        private void InstantiateVariables()
        {
            instancedBooleans = booleanInstancingConfig.ToDictionary(x => x.name, x =>
            {
                var instanced = ScriptableObject.CreateInstance<BooleanVariable>();
                instanced.SetValue(x.defaultValue);
                return instanced;
            });
            instancedFloats = floatInstancingConfig.ToDictionary(x => x.name, x =>
            {
                var instanced = ScriptableObject.CreateInstance<FloatVariable>();
                instanced.SetValue(x.defaultValue);
                return instanced;
            });
        }

        public BooleanVariable GetBooleanValue(string name)
        {
            if (instancedBooleans == null)
            {
                InstantiateVariables();
            }
            if (instancedBooleans.TryGetValue(name, out var variable))
            {
                return variable;
            }
            return null;
        }
        public FloatVariable GetFloatValue(string name)
        {
            if (instancedFloats == null)
            {
                InstantiateVariables();
            }
            if (instancedFloats.TryGetValue(name, out var variable))
            {
                return variable;
            }
            return null;
        }

        public string IdentifierInsideMember()
        {
            return "Instantiator";
        }

        public object GetSaveObject()
        {
            return new VariableInstantiatorSaveObject
            {
                boolValues = instancedBooleans.Select(value => new ValueSaveObject<bool>
                {
                    dataID = value.Key,
                    savedValue = value.Value.CurrentValue
                }).ToArray(),
                floatValues = instancedFloats.Select(value => new ValueSaveObject<float>
                {
                    dataID = value.Key,
                    savedValue = value.Value.CurrentValue
                }).ToArray()
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (instancedBooleans == null)
            {
                InstantiateVariables();
            }

            var saveData = save as VariableInstantiatorSaveObject;
            if(saveData != null)
            {
                foreach (var value in saveData.boolValues)
                {
                    if (instancedBooleans.TryGetValue(value.dataID, out var booleanVariable))
                    {
                        booleanVariable.SetValue(value.savedValue);
                    }
                }
                foreach (var value in saveData.floatValues)
                {
                    if (instancedFloats.TryGetValue(value.dataID, out var floatVariable))
                    {
                        floatVariable.SetValue(value.savedValue);
                    }
                }
            }
        }
    }
}
