using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PCS.Observable.Editor
{
    [CustomPropertyDrawer(typeof(SerializableObservableList<>))]
    public class SerializableObservableListDrawer : PropertyDrawer
    {
        private Dictionary<string, ReorderableList> _reorderableLists = new Dictionary<string, ReorderableList>();

        private ReorderableList GetReorderableList(SerializedProperty property)
        {
            string key = property.propertyPath;
            if (!_reorderableLists.ContainsKey(key))
            {
                var itemsProperty = property.FindPropertyRelative("_items");
                var list = new ReorderableList(property.serializedObject, itemsProperty, true, true, true, true);

                list.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, property.displayName);
                };

                list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = itemsProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, new GUIContent($"Element {index}"), true);
                };

                list.elementHeightCallback = (int index) =>
                {
                    var element = itemsProperty.GetArrayElementAtIndex(index);
                    return EditorGUI.GetPropertyHeight(element, true);
                };

                list.onAddCallback = (ReorderableList reorderableList) =>
                {
                    var targetObject = property.serializedObject.targetObject;
                    var serializableList = GetValueRecursive(targetObject, 0, property.propertyPath.Split('.'));
                    var listType = serializableList.GetType();
                    var elementType = listType.GetGenericArguments()[0];

                    // 새로운 인스턴스 생성 로직
                    object defaultValue;
                    try
                    {
                        // 기본 생성자를 통해 인스턴스 생성 시도
                        defaultValue = Activator.CreateInstance(elementType);
                    }
                    catch (MissingMethodException)
                    {
                        // 기본 생성자가 없는 경우 처리
                        Debug.LogError($"Type {elementType.Name} must have a parameterless constructor to be used in SerializableList");
                        return;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to create instance of {elementType.Name}: {e.Message}");
                        return;
                    }

                    var addMethod = listType.GetMethod("Add");
                    addMethod.Invoke(serializableList, new[] { defaultValue });
                    property.serializedObject.Update();
                };

                list.onRemoveCallback = (ReorderableList reorderableList) =>
                {
                    var targetObject = property.serializedObject.targetObject;
                    var serializableList = GetValueRecursive(targetObject, 0, property.propertyPath.Split('.'));
                    var removeAtMethod = serializableList.GetType().GetMethod("RemoveAt");
                    removeAtMethod.Invoke(serializableList, new object[] { reorderableList.index });

                    property.serializedObject.Update();
                };

                list.onReorderCallbackWithDetails = (ReorderableList reorderableList, int oldIndex, int newIndex) =>
                {
                    var targetObject = property.serializedObject.targetObject;
                    var serializableList = GetValueRecursive(targetObject, 0, property.propertyPath.Split("."));
                    var itemsField = serializableList.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
                    var items = itemsField.GetValue(serializableList) as System.Collections.IList;

                    var observableField = serializableList.GetType().GetField("_onValueChangeObservable",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var observable = observableField.GetValue(serializableList);
                    var notifyMethod = observable.GetType().GetMethod("Notify");
                    var elementType = serializableList.GetType().GetGenericArguments()[0];
                    var observeType = typeof(CollectionObserve<>).MakeGenericType(elementType);

                    if (oldIndex < newIndex)
                    {
                        // 위에서 아래로 이동
                        for (int i = oldIndex; i <= newIndex; i++)
                        {
                            object currentValue = items[i];
                            object previousValue = (i == newIndex) ? items[oldIndex] : items[i + 1];

                            var args = Activator.CreateInstance(observeType,
                                new object[] { i, previousValue, currentValue });
                            notifyMethod.Invoke(observable, new[] { args });
                        }
                    }
                    else
                    {
                        // 아래에서 위로 이동
                        for (int i = oldIndex; i >= newIndex; i--)
                        {
                            object currentValue = items[i];
                            object previousValue = (i == newIndex) ? items[oldIndex] : items[i - 1];

                            var args = Activator.CreateInstance(observeType,
                                new object[] { i, previousValue, currentValue });
                            notifyMethod.Invoke(observable, new[] { args });
                        }
                    }
                };

                _reorderableLists[key] = list;
            }

            return _reorderableLists[key];
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
                throw new Exception("Can't decode path." + string.Join(", ", paths));
            }

            var v = fldInfo.GetValue(obj);
            if (index < paths.Length - 1)
            {
                return GetValueRecursive(v, ++index, paths);
            }

            return v;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            GetReorderableList(property).DoList(position);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetReorderableList(property).GetHeight();
        }
    }
}