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
    class BoolValueSaveObject
    {
        public string dataID;
        public bool savedValue;
    }
    [Serializable]
    class VariableInstantiatorSaveObject
    {
        public BoolValueSaveObject[] boolValues;
    }

    public class VariableInstantiator : MonoBehaviour, IMemberSaveable
    {
        public BooleanInstanceConfig[] variableInstancingConfig;

        public IDictionary<string, BooleanVariable> instancedVariables;

        private void Awake()
        {
            if (instancedVariables == null)
            {
                instantiateVariables();
            }
        }

        private void instantiateVariables()
        {
            instancedVariables = variableInstancingConfig.ToDictionary(x => x.name, x =>
            {
                var instanced = ScriptableObject.CreateInstance<BooleanVariable>();
                instanced.SetValue(x.defaultValue);
                return instanced;
            });
        }

        public BooleanVariable GetValue(string name)
        {
            if (instancedVariables == null)
            {
                instantiateVariables();
            }
            if (instancedVariables.TryGetValue(name, out var variable))
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
                boolValues = instancedVariables.Select(value => new BoolValueSaveObject
                {
                    dataID = value.Key,
                    savedValue = value.Value.CurrentValue
                }).ToArray()
            };
        }

        public void SetupFromSaveObject(object save)
        {
            if (instancedVariables == null)
            {
                instantiateVariables();
            }

            var saveData = save as VariableInstantiatorSaveObject;
            if(saveData != null)
            {
                foreach (var value in saveData.boolValues)
                {
                    if(instancedVariables.TryGetValue(value.dataID, out var booleanVariable))
                    {
                        booleanVariable.SetValue(value.savedValue);
                    }
                }
            }
        }
    }
}
