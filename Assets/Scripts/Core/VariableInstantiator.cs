using Assets.WorldObjects.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Core
{
    [Serializable]  
    class ValueSaveObject<T>
    {
        public string dataID;
        public T savedValue;
    }
    [Serializable]
    class VariableInstantiatorSaveObject
    {
        public ValueSaveObject<object>[] boolValues;
        public ValueSaveObject<object>[] floatValues;
    }

    public class VariableInstantiator : MonoBehaviour, IMemberSaveable
    {
        public BooleanState[] booleanStateConfig;
        public FloatState[] floatStateConfig;

        protected IDictionary<string, GenericVariable<bool>> instancedBooleans;
        protected IDictionary<string, GenericVariable<float>> instancedFloats;

        private void Awake()
        {
            if (instancedBooleans == null || instancedFloats == null)
            {
                InstantiateVariables();
            }
        }

        private void InstantiateVariables()
        {
            instancedBooleans = booleanStateConfig.ToDictionary(x => x.IdentifierInInstantiator, x => x.GenerateNewVariable());
            instancedFloats = floatStateConfig.ToDictionary(x => x.IdentifierInInstantiator, x => x.GenerateNewVariable());
        }

        public GenericVariable<bool> GetBooleanValue(string name)
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
        public GenericVariable<float> GetFloatValue(string name)
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

        public static string ConstantIdentifier()
        {
            return "Instantiator";
        }
        public string IdentifierInsideMember()
        {
            return ConstantIdentifier();
        }

        public object GetSaveObject()
        {
            return new VariableInstantiatorSaveObject
            {
                boolValues = SaveValues(booleanStateConfig, instancedBooleans),
                floatValues = SaveValues(floatStateConfig, instancedFloats),
            };
        }

        private ValueSaveObject<object>[] SaveValues<T>(GenericState<T>[] stateConfig, IDictionary<string, GenericVariable<T>> variables)
        {
            return stateConfig.Select(config =>
            {
                var variable = variables[config.IdentifierInInstantiator];
                return new ValueSaveObject<object>
                {
                    dataID = config.IdentifierInInstantiator,
                    savedValue = config.GetSaveObjectFromVariable(variable)
                };
            }).ToArray();
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
                LoadValues(booleanStateConfig, instancedBooleans, saveData.boolValues);
                LoadValues(floatStateConfig, instancedFloats, saveData.floatValues);
            }
        }
        private void LoadValues<T>(GenericState<T>[] stateConfig, IDictionary<string, GenericVariable<T>> variables, ValueSaveObject<object>[] savedValues)
        {
            var stateConfigDictionary = stateConfig.ToDictionary(x => x.IdentifierInInstantiator);
            foreach (var value in savedValues)
            {
                if (variables.TryGetValue(value.dataID, out var variable) && stateConfigDictionary.TryGetValue(value.dataID, out var state))
                {
                    state.SetSaveObjectIntoVariable(variable, value.savedValue);
                }
            }
        }
    }
}
