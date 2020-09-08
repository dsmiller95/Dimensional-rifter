using Assets.WorldObjects;
using System;
using TradeModeling.Inventories;
using UnityEditor;

namespace Assets.Scripts.Core.Editor
{
    [CustomEditor(typeof(VariableInstantiator))]
    public class VariableInstantiatorInspector : UnityEditor.Editor
    {
        SerializedProperty booleanStates;
        SerializedProperty floatStates;
        SerializedProperty inventoryStates;

        void OnEnable()
        {
            booleanStates = serializedObject.FindProperty("booleanStateConfig");
            floatStates = serializedObject.FindProperty("floatStateConfig");
            inventoryStates = serializedObject.FindProperty("inventoryStateConfig");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(booleanStates);
            EditorGUILayout.PropertyField(floatStates);
            EditorGUILayout.PropertyField(inventoryStates);
            serializedObject.ApplyModifiedProperties();

            var instantiator = serializedObject.targetObject as VariableInstantiator;
            if (instantiator == null)
            {
                return;
            }
            if (instantiator.instancedBooleans != null)
            {
                foreach (var kvp in instantiator.instancedBooleans)
                {
                    ShowBooleanVariable(kvp.Key, kvp.Value);
                }
            }
            if (instantiator.instancedFloats != null)
            {
                foreach (var kvp in instantiator.instancedFloats)
                {
                    ShowFloatVariable(kvp.Key, kvp.Value);
                }
            }
            if (instantiator.instancedInventories != null)
            {
                foreach (var kvp in instantiator.instancedInventories)
                {
                    ShowInventoryVariable(kvp.Key, kvp.Value);
                }
            }
        }

        private void ShowBooleanVariable(string variableName, GenericVariable<bool> variable)
        {
            EditorGUILayout.LabelField(variableName, variable.CurrentValue ? "true" : "false");
        }
        private void ShowFloatVariable(string variableName, GenericVariable<float> variable)
        {
            var formattedFloat = $"{variable.CurrentValue:F1}";
            EditorGUILayout.LabelField(variableName, formattedFloat);
        }
        private void ShowInventoryVariable(string variableName, GenericVariable<IInventory<Resource>> variable)
        {
            var info = "";
            var myInv = variable.CurrentValue;
            foreach (var resource in myInv.GetCurrentResourceAmounts())
            {
                info += $"{Enum.GetName(typeof(Resource), resource.Key)}: {resource.Value:F1}\n";
            }
            EditorGUILayout.LabelField(variableName, info);
        }
    }
}
