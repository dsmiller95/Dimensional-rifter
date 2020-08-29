using Assets.Scripts.ObjectVariables;
using Assets.WorldObjects;
using Assets.WorldObjects.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
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
        public ValueSaveObject<object>[] inventoryValues;
    }

    public class VariableInstantiator : MonoBehaviour, IMemberSaveable
    {
        public BooleanState[] booleanStateConfig;
        public FloatState[] floatStateConfig;
        public InventoryState[] inventoryStateConfig;

        protected IDictionary<string, GenericVariable<bool>> instancedBooleans;
        protected IDictionary<string, GenericVariable<float>> instancedFloats;
        protected IDictionary<string, GenericVariable<IInventory<Resource>>> instancedInventories;

        private void Awake()
        {
            EnsureInstanced();
        }

        private void EnsureInstanced()
        {
            if (instancedBooleans == null || instancedFloats == null || instancedInventories == null)
            {
                InstantiateVariables();
            }
        }

        private void InstantiateVariables()
        {
            instancedBooleans = booleanStateConfig.ToDictionary(x => x.IdentifierInInstantiator, x => x.GenerateNewVariable());
            instancedFloats = floatStateConfig.ToDictionary(x => x.IdentifierInInstantiator, x => x.GenerateNewVariable());
            instancedInventories = inventoryStateConfig.ToDictionary(x => x.IdentifierInInstantiator, x => x.GenerateNewVariable());
        }

        public GenericVariable<bool> GetBooleanValue(string name)
        {
            EnsureInstanced();
            return GetValue(name, instancedBooleans);
        }
        public GenericVariable<float> GetFloatValue(string name)
        {
            EnsureInstanced();
            return GetValue(name, instancedFloats);
        }
        public GenericVariable<IInventory<Resource>> GetInventoryValue(string name)
        {
            EnsureInstanced();
            return GetValue(name, instancedInventories);
        }

        private GenericVariable<T> GetValue<T>(string path, IDictionary<string, GenericVariable<T>> instancedValues)
        {
            if (instancedValues.TryGetValue(path, out var variable))
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
                inventoryValues = SaveValues(inventoryStateConfig, instancedInventories),
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
            EnsureInstanced();

            var saveData = save as VariableInstantiatorSaveObject;
            if (saveData != null)
            {
                LoadValues(booleanStateConfig, instancedBooleans, saveData.boolValues);
                LoadValues(floatStateConfig, instancedFloats, saveData.floatValues);
                LoadValues(inventoryStateConfig, instancedInventories, saveData.inventoryValues);
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
