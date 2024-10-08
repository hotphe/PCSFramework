using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
public class SerializedDictionaryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUILayout.Space(-EditorGUIUtility.singleLineHeight); //Dictionary ���� SerializedKvps�� �����ϹǷ� ������ ������ ���� ���� �ø�.
        GUILayout.BeginHorizontal();
        var serializedKvpsProp = property.FindPropertyRelative("SerializedKvps");
        GUILayout.Space(-3); //Dictionay �� SerializedKvps�̹Ƿ� 2�� foldout ������ �������� �̵���Ŵ. indentLevel-- ���� �ʹ� ���� �̵���.
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
