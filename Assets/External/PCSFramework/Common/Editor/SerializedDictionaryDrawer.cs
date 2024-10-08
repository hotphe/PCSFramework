using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
public class SerializedDictionaryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUILayout.Space(-EditorGUIUtility.singleLineHeight); //Dictionary 안의 SerializedKvps를 생성하므로 한줄이 내려가 한줄 위로 올림.
        GUILayout.BeginHorizontal();
        var serializedKvpsProp = property.FindPropertyRelative("SerializedKvps");
        GUILayout.Space(-3); //Dictionay 안 SerializedKvps이므로 2중 foldout 구조라 왼쪽으로 이동시킴. indentLevel-- 사용시 너무 많이 이동함.
        EditorGUILayout.PropertyField(serializedKvpsProp, label);
        GUILayout.EndHorizontal();

        for(int i=0; i < serializedKvpsProp.arraySize; i++)
        {
            var element = serializedKvpsProp.GetArrayElementAtIndex(i);
            var isKeyNullProp = element.FindPropertyRelative("IsKeyNull");
            var isKeyRepeatedProp = element.FindPropertyRelative("IsKeyRepeated");

            if(isKeyNullProp.boolValue)
            {
                EditorGUILayout.HelpBox($"The null key is not allowed at {i}. The element will not be added to the dictionary.", MessageType.Warning);
                break;
            }

            if(isKeyRepeatedProp.boolValue)
            {
                EditorGUILayout.HelpBox($"The key is duplicated at {i}. The element will not be added to the dictionary.", MessageType.Warning);
                break;
            }
        }
        EditorGUI.EndProperty();
    }
}
