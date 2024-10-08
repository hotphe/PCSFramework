using UnityEngine;
using UnityEditor;

namespace PCS.Common.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionAttribute))]
    public class ConditionAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionAttribute conditionAttribute = (ConditionAttribute)attribute;
            bool boolValue = GetConditionAttributeResult(conditionAttribute, property);
            if (boolValue == conditionAttribute.CanVisible)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionAttribute conditionAttribute = (ConditionAttribute)attribute;
            bool boolValue = GetConditionAttributeResult(conditionAttribute, property);

            if (conditionAttribute.CanVisible == boolValue)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
        private bool GetConditionAttributeResult(ConditionAttribute condAttr, SerializedProperty property)
        {
            bool enabled = true;
            string propertyPath = property.propertyPath;
            string conditionBooleanPath = propertyPath.Replace(property.name, condAttr.BooleanName);
            SerializedProperty sourceProperty = property.serializedObject.FindProperty(conditionBooleanPath);

            if (sourceProperty != null)
            {
                enabled = sourceProperty.boolValue;
            }
            else
            {
                Debug.LogWarning($"Target ({condAttr.BooleanName}) is not bool type.");
            }

            return enabled;
        }
    }
#endif
}