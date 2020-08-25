using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Core.Editor
{
    [CustomPropertyDrawer(typeof(BooleanReference))]
    public class BooleanReferenceDrawer : PropertyDrawer
    {

        //const int dataPathHeight = 14;
        const int dataPathHeight = 18;
        const int dropdownWidth = 100;

        // Here you must define the height of your property drawer. Called by Unity.
        public override float GetPropertyHeight(SerializedProperty prop,
                                                 GUIContent label)
        {
            BooleanReferenceDataSource dataSource = DataSource(prop);
            if (dataSource == BooleanReferenceDataSource.INSTANCER)
                return base.GetPropertyHeight(prop, label) + dataPathHeight;
            else
                return base.GetPropertyHeight(prop, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BooleanReferenceDataSource dataSource = DataSource(property);
            if (dataSource != BooleanReferenceDataSource.INSTANCER)
            {
                var topRowPosition = new Rect(position);

                label = EditorGUI.BeginProperty(topRowPosition, label, property);

                DrawObjectSelection(topRowPosition, property, label);
            }
            else
            {
                var topRowPosition = new Rect(position);
                topRowPosition.yMax -= dataPathHeight;

                label = EditorGUI.BeginProperty(topRowPosition, label, property);

                DrawObjectSelection(topRowPosition, property, label);

                var bottomRowPosition = new Rect(position);
                bottomRowPosition.yMin = bottomRowPosition.yMax - dataPathHeight;
                DrawPathSelection(bottomRowPosition, property);
            }
            EditorGUI.EndProperty();
        }

        private void DrawPathSelection(Rect position, SerializedProperty property)
        {

            SerializedProperty instancer = property.FindPropertyRelative("Instancer");
            var instancerObj = instancer.objectReferenceValue as VariableInstantiator;
            if (instancerObj == null)
            {
                EditorGUI.HelpBox(position, "Set the instantiator", MessageType.Error);
                return;
            }

            SerializedProperty namePath = property.FindPropertyRelative("NamePath");

            EditorGUI.BeginChangeCheck();
            var selectionOptions = instancerObj.variableInstancingConfig.Select(x => x.name).ToList();
            var currentPathIndex = selectionOptions.IndexOf(namePath.stringValue);

            int newPathIndex = EditorGUI.Popup(position, currentPathIndex, selectionOptions.ToArray());
            // var newpath = EditorGUI.TextField(position, namePath.stringValue);

            if (newPathIndex < 0)// && !instancerObj.variableInstancingConfig.Any(x => x.name == newpath))
            {
                namePath.stringValue = "";
            }
            else
            {
                namePath.stringValue = selectionOptions[newPathIndex];
            }
            EditorGUI.EndChangeCheck();
        }

        /// <summary>
        ///  draw the first row
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        private void DrawObjectSelection(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            // Get properties
            SerializedProperty dataSource = property.FindPropertyRelative("DataSource");
            SerializedProperty constantValue = property.FindPropertyRelative("ConstantValue");
            SerializedProperty variable = property.FindPropertyRelative("Variable");
            SerializedProperty instancer = property.FindPropertyRelative("Instancer");

            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.width = dropdownWidth;
            position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            BooleanReferenceDataSource dataSourceEnum = (BooleanReferenceDataSource)dataSource.enumValueIndex;
            dataSourceEnum = (BooleanReferenceDataSource)EditorGUI.EnumPopup(buttonRect, dataSourceEnum);

            //int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);

            dataSource.enumValueIndex = (int)dataSourceEnum;

            switch (dataSourceEnum)
            {
                case BooleanReferenceDataSource.CONSTANT:
                    EditorGUI.PropertyField(position,
                        constantValue,
                        GUIContent.none);
                    break;
                case BooleanReferenceDataSource.SINGLETON_VARIABLE:
                    EditorGUI.PropertyField(position,
                        variable,
                        GUIContent.none);
                    break;
                case BooleanReferenceDataSource.INSTANCER:
                    EditorGUI.PropertyField(position,
                        instancer,
                        GUIContent.none);
                    break;
                default:
                    break;
            }

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
        }



        private BooleanReferenceDataSource DataSource(SerializedProperty prop)
        {
            SerializedProperty dataSource = prop.FindPropertyRelative("DataSource");
            return (BooleanReferenceDataSource)dataSource.enumValueIndex;
        }
    }
}
