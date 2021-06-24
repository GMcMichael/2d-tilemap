using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ColorGradient))]//https://www.youtube.com/watch?v=8_ZAlEoAQiA 7:51
public class GradientDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        ColorGradient gradient = (ColorGradient)fieldInfo.GetValue(property.serializedObject.targetObject);
        GUI.DrawTexture(position, gradient.GetTexture((int) position.height));
        GUI.Label(position, label);
    }
}
