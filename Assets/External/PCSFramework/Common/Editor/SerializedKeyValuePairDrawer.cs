using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SerializedDictionary<,>.SerializedKeyValuePair<,>))]
public class SerializedKeyValuePairDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        GUIContent waringIcon = EditorGUIUtility.IconContent("console.warnicon");

        var keyProp = property.FindPropertyRelative("Key");
        var valueProp = property.FindPropertyRelative("Value");
        var isKeyNullProp = property.FindPropertyRelative("IsKeyNull");
        var isKeyRepeatedProp = property.FindPropertyRelative("IsKeyRepeated");

        float height = position.height - 1.6f; // spacing 1.6f

        Rect keyRect = new Rect(position.x, position.y, position.width * 0.33f, height);
        Rect alertImageRect = new Rect(position.x + position.width * 0.35f, position.y, height, height);
        Rect alertRect = new Rect(position.x + position.width * 0.35f+height, position.y, position.width * 0.13f, height);
        Rect valueRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, height);

        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
        EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

        // 상태 표시
        if (isKeyNullProp.boolValue || isKeyRepeatedProp.boolValue)
        {
            string status = isKeyNullProp.boolValue ? "Null Key" : "Duplicate";
            GUI.Label(alertImageRect, waringIcon);
            EditorGUI.LabelField(alertRect, status, EditorStyles.miniLabel);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
