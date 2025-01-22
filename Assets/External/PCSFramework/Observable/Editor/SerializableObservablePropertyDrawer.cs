using UnityEngine;
using System.Reflection;
using System;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PCS.Observable.Editor
{
    [CustomPropertyDrawer(typeof(SerializableObservableProperty<>))]
    public class SerializableObservablePropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            var p = property.FindPropertyRelative("_serializedValue");

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
            if (EditorGUI.EndChangeCheck())
            {
                CallForceChange(property);
            }
        }

        private void CallForceChange(SerializedProperty property)
        {
            var paths = property.propertyPath.Split('.');
            var targetObject = property.serializedObject.targetObject;


            var targetProp = (paths.Length == 1)
                ? fieldInfo.GetValue(targetObject)
                : GetValueRecursive(targetObject, 0, paths);

            if (targetProp == null) return;

            property.serializedObject.ApplyModifiedProperties(); // deserialize to field
            var methodInfo = targetProp.GetType().GetMethod("ForceChange", BindingFlags.IgnoreCase | BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo != null)
            {
                methodInfo.Invoke(targetProp, Array.Empty<object>());
            }
        }

        // Code from Unirx
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
            var p = property.FindPropertyRelative("_serializedValue");
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
}