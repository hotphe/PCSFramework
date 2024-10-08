using System;
using UnityEngine;

namespace PCS.Common
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ConditionAttribute : PropertyAttribute
    {
        public string BooleanName = "";
        public bool CanVisible = true;
        /// <summary>
        /// Attribute to show property when the value of booleanName is equal to the value of canVisible.
        /// </summary>
        /// <param name="booleanName"></param>
        /// <param name="canVisible"></param>
        public ConditionAttribute(string booleanName, bool canVisible = true)
        {
            BooleanName = booleanName;
            CanVisible = canVisible;
        }
    }
}