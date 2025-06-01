using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace PCS.Common.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        private ReorderableList _reorderableList;
        private SerializedProperty _prop;
        private SerializedProperty _kvpsProp;
        private GUIContent _waringIcon;
        private bool _hasAlert;
        private string _status;
        private bool _prevEnabled;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _waringIcon = EditorGUIUtility.IconContent("console.warnicon");
            _hasAlert = false;
            _prevEnabled = true;
            SetupProps(property);
            if (GUI.enabled == false)
            {
                _prevEnabled = false;
                GUI.enabled = true;
            }
            SetupHeader(position, property);
            GUI.enabled = _prevEnabled;
            if (!property.isExpanded) return;
            SetupList(property);

            Rect rect = position;
            rect.y += EditorGUIUtility.singleLineHeight;
            rect.x -= 3;
            _reorderableList.DoList(rect);
            DrawAlertBox();

        }

        private void SetupProps(SerializedProperty prop)
        {
            if (_prop != null) return;

            _prop = prop;
            _kvpsProp = prop.FindPropertyRelative("SerializedKvps");
        }

        private void SetupHeader(Rect rect, SerializedProperty prop)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x -= 3;
            var headerRect = rect;
            headerRect.width = headerRect.width - 50;

            Event currentEvent = Event.current;

            if (headerRect.Contains(currentEvent.mousePosition))
            {
                EditorGUI.DrawRect(headerRect, new Color(1, 1, 1, 0.07f));
            }

            if (GUI.Button(headerRect, "", GUIStyle.none))
            {
                prop.isExpanded = !prop.isExpanded;
            }

            var triangleRect = rect;
            EditorGUI.Foldout(triangleRect, prop.isExpanded, ChangeName(prop.name));

            var countRect = headerRect;
            countRect.width = 50;
            countRect.x = headerRect.width;
            countRect.x -= 3;
            _kvpsProp.arraySize = EditorGUI.DelayedIntField(countRect, _kvpsProp.arraySize);
        }

        private string ChangeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if(name.StartsWith("_")) name = name.Substring(1);
            if(name.Length > 0 && char.IsLower(name[0])) name = char.ToUpper(name[0]) + name.Substring(1);
            return name;
        }

        private void SetupList(SerializedProperty prop)
        {
            if (_reorderableList != null) return;

            _reorderableList = new ReorderableList(_kvpsProp.serializedObject, _kvpsProp, true, false, true, true);
            _reorderableList.drawElementCallback = DrawElement;
            _reorderableList.elementHeightCallback = GetElementHeight;
            _reorderableList.drawNoneElementCallback = DrawDictionaryIsEmpty;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect keyRect;
            Rect valueRect;
            Rect alertRect;
            var kvpProp = _kvpsProp.GetArrayElementAtIndex(index);
            var keyProp = kvpProp.FindPropertyRelative("Key");
            var valueProp = kvpProp.FindPropertyRelative("Value");


            float spacing = 1.6f;
            keyRect = new Rect(rect.x, rect.y + spacing, rect.width * 0.2f, rect.height);
            alertRect = new Rect(rect.x + rect.width * 0.22f, rect.y + spacing, rect.width * 0.06f, rect.height);
            valueRect = new Rect(rect.x + rect.width * 0.3f, rect.y + spacing, rect.width * 0.7f, rect.height);

            DrawProp(keyRect, keyProp);
            DrawProp(valueRect, valueProp);
            DrawAlertIcon(alertRect, kvpProp);
        }

        private float GetElementHeight(int index)
        {
            var kvpProp = _kvpsProp.GetArrayElementAtIndex(index);
            var keyProp = kvpProp.FindPropertyRelative("Key");
            var valueProp = kvpProp.FindPropertyRelative("Value"); 

            float keyHeight = EditorGUI.GetPropertyHeight(keyProp, true);
            float valueHeight = EditorGUI.GetPropertyHeight(valueProp, true); 

            return Mathf.Max(keyHeight, valueHeight) + EditorGUIUtility.standardVerticalSpacing;
        }
        private float GetPropHeight(SerializedProperty prop)
        {
            var height = EditorGUI.GetPropertyHeight(prop);
            if (!IsSingleLine(prop) && prop.type != "EventReference")
                height -= 10;
            return height;
        }

        private void DrawProp(Rect rect, SerializedProperty prop)
        {
            if (IsSingleLine(prop))
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, prop, GUIContent.none);
                return;
            }
            
            GUI.BeginGroup(rect);

            if (prop.type == "EventReference")
            {
                EditorGUIUtility.labelWidth = 1;
                rect.x = 0;
                rect.y = 0;
                EditorGUI.PropertyField(rect, prop, GUIContent.none);
                EditorGUIUtility.labelWidth = 0;
            }
            else
            {
                rect.x = 20;
                rect.y = 0;
                EditorGUI.PropertyField(rect, prop, true);
            }

            GUI.EndGroup();
        }

        private void DrawAlertIcon(Rect rect, SerializedProperty kvpProp)
        {
            var isKeyNull = kvpProp.FindPropertyRelative("IsKeyNull").boolValue;
            var isKeyRepeated = kvpProp.FindPropertyRelative("IsKeyRepeated").boolValue;

            if (isKeyNull || isKeyRepeated)
            {
                _status = isKeyNull ? "Null Key is not allowed. The element will not be added." : "Duplicate Key is not allowed. The element will not be added.";
                GUI.Label(rect, _waringIcon);
                _hasAlert = true;
            }
        }

        private void DrawAlertBox()
        {
            if (_hasAlert)
                EditorGUILayout.HelpBox(_status, MessageType.Warning);
        }

        private void DrawDictionaryIsEmpty(Rect rect) => GUI.Label(rect, "Dictionary is empty");
        private bool IsSingleLine(SerializedProperty prop) => prop.propertyType != SerializedPropertyType.Generic || !prop.hasVisibleChildren;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SetupProps(property);
            var height = EditorGUIUtility.singleLineHeight; //기본 한줄
            if (property.isExpanded)
            {
                SetupList(property);
                height += _reorderableList.GetHeight();
            }

            return height;

        }
    }
#endif
}
