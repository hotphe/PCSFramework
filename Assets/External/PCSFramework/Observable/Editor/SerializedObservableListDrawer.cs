using System;
using System.Reflection;
using UnityEngine;
using System.Text.RegularExpressions;
using PCS.Observable;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CustomPropertyDrawer(typeof(SerializedObservableList<>.ValueMap))]
public class ValueMapDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty valueProperty = property.FindPropertyRelative("Value");
        EditorGUI.PropertyField(position, valueProperty, label, true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty valueProperty = property.FindPropertyRelative("Value");
        return EditorGUI.GetPropertyHeight(valueProperty, label, true);
    }
}

[CustomPropertyDrawer(typeof(SerializedObservableList<>))]
public class SerializedObservableListDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var p = property.FindPropertyRelative("_newItems");

        EditorGUI.BeginChangeCheck();

        if (p.propertyType == SerializedPropertyType.Quaternion)
        {
            label.text += "(EulerAngles)";
            EditorGUI.PropertyField(position, p, label, true);
        }
        else
        {
            EditorGUI.PropertyField(position, p, label, true);
        }

        var prevSize = property.FindPropertyRelative("_prevSize").intValue;
        if(p.isArray)
        {
            if(p.arraySize > prevSize)
            {
                var counterField = property.FindPropertyRelative("_counter");
                if (counterField != null)
                {
                    int counter = counterField.intValue;
                    for(int i = prevSize; i < p.arraySize; i++)
                    {
                        var element = p.GetArrayElementAtIndex(i);
                        var id = element.FindPropertyRelative("Id");
                        id.intValue = counter;
                        counter++;
                        counterField.intValue = counter;
                    }
                }
            }
        }

        if(p.isArray)
        {
            int size = p.arraySize;
            prevSize = size;
        }

        if (EditorGUI.EndChangeCheck())
        {
            var paths = property.propertyPath.Split('.');
            var attachedComponent = property.serializedObject.targetObject;

            var targetProp = (paths.Length == 1)
                ? fieldInfo.GetValue(attachedComponent)
                : GetValueRecursive(attachedComponent, 0, paths);

            if (targetProp == null) return;

            property.serializedObject.ApplyModifiedProperties(); // deserialize to field
            var methodInfo = targetProp.GetType().GetMethod("ForceChange", BindingFlags.IgnoreCase | BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo != null)
            {
                methodInfo.Invoke(targetProp, Array.Empty<object>());
            }
        }
    }

    object GetValueRecursive(object obj, int index, string[] paths)
    {
        var path = paths[index];

        FieldInfo fldInfo = null;
        var type = obj.GetType();
        while (fldInfo == null)
        {
            // attempt to get information about the field
            fldInfo = type.GetField(path, BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (fldInfo != null ||
                type.BaseType == null ||
                type.BaseType.IsSubclassOf(typeof(ObservableProperty<>))) break;

            // if the field information is missing, it may be in the base class
            type = type.BaseType;
        }

        // If array, path = Array.data[index]
        if (fldInfo == null && path == "Array")
        {
            try
            {
                path = paths[++index];
                var m = Regex.Match(path, @"(.+)\[([0-9]+)*\]");
                var arrayIndex = int.Parse(m.Groups[2].Value);
                var arrayValue = (obj as System.Collections.IList)[arrayIndex];
                if (index < paths.Length - 1)
                {
                    return GetValueRecursive(arrayValue, ++index, paths);
                }
                else
                {
                    return arrayValue;
                }
            }
            catch
            {
                Debug.Log("InspectorDisplayDrawer Exception, objType:" + obj.GetType().Name + " path:" + string.Join(", ", paths));
                throw;
            }
        }
        else if (fldInfo == null)
        {
            throw new Exception("Can't decode path, please report to UniRx's GitHub issues:" + string.Join(", ", paths));
        }

        var v = fldInfo.GetValue(obj);
        if (index < paths.Length - 1)
        {
            return GetValueRecursive(v, ++index, paths);
        }

        return v;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        
        var p = property.FindPropertyRelative("_newItems");
        if (p.propertyType == SerializedPropertyType.Quaternion)
        {
            // Quaternion is Vector3(EulerAngles)
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3, label);
        }
        else
        {
            return EditorGUI.GetPropertyHeight(p);
        }
    }
}
