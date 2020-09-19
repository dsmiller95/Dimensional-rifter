using UnityEditor;
using UnityEngine;

namespace Assets.WorldObjects.Members.Editor
{
    [CustomEditor(typeof(MembersRegistry))]
    public class MemberRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Reassign unique IDs"))
            {
                var registry = serializedObject.targetObject as MembersRegistry;
                for (var i = 0; i < registry.allTypes.Length; i++)
                {
                    registry.allTypes[i].uniqueData.uniqueId = i;
                }
                Debug.Log("Successfully reset all member IDs. All save files may be invalid.");
            }
        }
    }
}