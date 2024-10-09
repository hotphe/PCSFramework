using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using System;
using PCS.UI;
using PCS.Common;

[CustomEditor(typeof(UIAdjuster), true)]
public class FoldoutEditor : Editor
{
    private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
    private string currentFold = string.Empty;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            var foldAttr = GetAttribute<FoldAttribute>(iterator);
            var endFoldAttr = GetAttribute<EndFoldAttribute>(iterator);

            if (foldAttr != null)
            {
                if (!foldoutStates.ContainsKey(foldAttr.name))
                    foldoutStates[foldAttr.name] = true;

                foldoutStates[foldAttr.name] = EditorGUILayout.Foldout(foldoutStates[foldAttr.name], foldAttr.name);
                currentFold = foldAttr.name;

                if (foldoutStates[foldAttr.name])
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
                continue;
            }

            if (endFoldAttr != null)
            {
                currentFold = string.Empty;
                
                EditorGUILayout.PropertyField(iterator, true);
                continue;
            }
            if(currentFold == string.Empty)
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            if ( currentFold != string.Empty)
            {
                if (foldoutStates[currentFold])
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }

        }

        serializedObject.ApplyModifiedProperties();
        currentFold = string.Empty;
    }

    private T GetAttribute<T>(SerializedProperty property) where T : Attribute
    {
        var fieldInfo = GetFieldInfo(property);
        if (fieldInfo == null) return null;
        return fieldInfo.GetCustomAttribute<T>();
    }

    private FieldInfo GetFieldInfo(SerializedProperty property)
    {
        var targetObject = property.serializedObject.targetObject;
        var targetType = targetObject.GetType();

        var field = targetType.GetField(property.propertyPath,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        return field;
    }
}