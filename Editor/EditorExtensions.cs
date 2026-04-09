using UnityEditor;
using UnityEngine;

namespace PsigenVision.Utilities.Editor
{
    public static class EditorExtensions
    {
        /// <summary>
        /// Attempts to assign a boxed value to a specified field within a SerializedProperty hierarchy based on the provided field path.
        /// </summary>
        /// <param name="property">The SerializedProperty representing the root object for the field path.</param>
        /// <param name="type">The type that declares the field hierarchy corresponding to the specified path.</param>
        /// <param name="path">The dot-separated string indicating the field hierarchy within the serialized object (e.g., "Outer.Inner.Field").</param>
        /// <param name="valueProp">The SerializedProperty containing the value to be used for updating the specified field.</param>
        /// <param name="modifiedObject">The resulting updated object after attempting to set the boxed value along the specified field path.</param>
        /// <returns>True if the boxed value was successfully set; otherwise, false.</returns>
        public static bool TrySetBoxedValueViaPath(this SerializedProperty property, System.Type type, string path,
            SerializedProperty valueProp, out object modifiedObject)
        {
            modifiedObject = property.boxedValue;
            bool success;
            switch (valueProp.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.boolValue);
                    //Debug.Log($"Value property changed to {valueProp.boolValue}");
                    break;
                case SerializedPropertyType.Integer:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.intValue);
                    //Debug.Log($"Value property changed to {valueProp.intValue}");
                    break;
                case SerializedPropertyType.Float:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.floatValue);
                    //Debug.Log($"Value property changed to {valueProp.floatValue}");
                    break;
                case SerializedPropertyType.String:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.stringValue);
                    //Debug.Log($"Value property changed to {valueProp.stringValue}");
                    break;
                case SerializedPropertyType.Color:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.colorValue);
                    //Debug.Log($"Value property changed to {valueProp.colorValue}");
                    break;
                case SerializedPropertyType.Vector2:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.vector2Value);
                    //Debug.Log($"Value property changed to {valueProp.vector2Value}");
                    break;
                case SerializedPropertyType.Vector3:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.vector3Value);
                    //Debug.Log($"Value property changed to {valueProp.vector3Value}");
                    break;
                case SerializedPropertyType.Quaternion:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.quaternionValue);
                    //Debug.Log($"Value property changed to {valueProp.quaternionValue}");
                    break;
                case SerializedPropertyType.Generic:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.objectReferenceValue);
                    break;
                case SerializedPropertyType.ObjectReference:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.objectReferenceValue);
                    break;
                case SerializedPropertyType.LayerMask:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.intValue);
                    break;
                case SerializedPropertyType.Enum:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.enumValueFlag);
                    break;
                case SerializedPropertyType.Vector4:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.vector4Value);
                    break;
                case SerializedPropertyType.Rect:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.rectValue);
                    break;
                case SerializedPropertyType.ArraySize:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.arraySize);
                    break;
                case SerializedPropertyType.Character:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.stringValue);
                    break;
                case SerializedPropertyType.AnimationCurve:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.animationCurveValue);
                    break;
                case SerializedPropertyType.Bounds:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.boundsValue);
                    break;
                case SerializedPropertyType.Gradient:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.gradientValue);
                    break;
                case SerializedPropertyType.ExposedReference:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.objectReferenceValue);
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.fixedBufferSize);
                    break;
                case SerializedPropertyType.Vector2Int:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.vector2IntValue);
                    break;
                case SerializedPropertyType.Vector3Int:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.vector3IntValue);
                    break;
                case SerializedPropertyType.RectInt:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.rectIntValue);
                    break;
                case SerializedPropertyType.BoundsInt:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.boundsIntValue);
                    break;
                case SerializedPropertyType.ManagedReference:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.managedReferenceValue);
                    break;
                case SerializedPropertyType.Hash128:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.hash128Value);
                    break;
                case SerializedPropertyType.RenderingLayerMask:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.intValue);
                    break;
                case SerializedPropertyType.EntityId:
                    success = modifiedObject.TrySetValueViaPath(type, path, valueProp.entityIdValue);
                    break;
                default:
                    success = false;
                    break;
            }

            if (!success) Debug.LogError($"Failed to set boxed value via path: {path} as {property.propertyType}");
            else property.boxedValue = modifiedObject;
            return success;
        }

        /// <summary>
        /// Assigns a boxed object to the specified field within the passed-in SerializedProperty using the value contained in the provided valueProp
        /// </summary>
        /// <param name="property">The SerializedProperty to which the boxed value will be assigned.</param>
        /// <param name="type">The type containing the property hierarchy where the boxed value will be set.</param>
        /// <param name="path">The dot-separated string indicating the field hierarchy where the boxed value should be assigned (e.g., "Outer.Inner.Field").</param>
        /// <param name="valueProp">The SerializedProperty from which the value to be boxed will be resolved.</param>
        /// <param name="boxedValue">The resulting boxed object that represents the value assigned to the specified property and field path.</param>
        public static void SetBoxedValue(this SerializedProperty property, SerializedProperty valueProp,
            out object boxedValue) =>
            boxedValue = property.GetBoxedValue(valueProp);

        /// <summary>
        /// Retrieves the boxed object contained in the specified SerializedProperty updated with the value of the specified value property.
        /// </summary>
        /// <param name="property">The SerializedProperty from which to extract the boxed value.</param>
        /// <param name="valueProp">The SerializedProperty providing the value to update the boxed object with.</param>
        /// <returns>The boxed object with the resolved value, or null if the extraction or update process fails.</returns>
        public static object GetBoxedValue(this SerializedProperty property, SerializedProperty valueProp)
        {
            switch (valueProp.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    property.SetValue(valueProp.boolValue);
                    //Debug.Log($"Value property changed to {valueProp.boolValue}");
                    break;
                case SerializedPropertyType.Integer:
                    property.SetValue(valueProp.intValue);
                    //Debug.Log($"Value property changed to {valueProp.intValue}");
                    break;
                case SerializedPropertyType.Float:
                    property.SetValue(valueProp.floatValue);
                    //Debug.Log($"Value property changed to {valueProp.floatValue}");
                    break;
                case SerializedPropertyType.String:
                    property.SetValue(valueProp.stringValue);
                    //Debug.Log($"Value property changed to {valueProp.stringValue}");
                    break;
                case SerializedPropertyType.Color:
                    property.SetValue(valueProp.colorValue);
                    //Debug.Log($"Value property changed to {valueProp.colorValue}");
                    break;
                case SerializedPropertyType.Vector2:
                    property.SetValue(valueProp.vector2Value);
                    //Debug.Log($"Value property changed to {valueProp.vector2Value}");
                    break;
                case SerializedPropertyType.Vector3:
                    property.SetValue(valueProp.vector3Value);
                    //Debug.Log($"Value property changed to {valueProp.vector3Value}");
                    break;
                case SerializedPropertyType.Quaternion:
                    property.SetValue(valueProp.quaternionValue);
                    //Debug.Log($"Value property changed to {valueProp.quaternionValue}");
                    break;
                case SerializedPropertyType.Generic:
                    property.SetValue(valueProp.objectReferenceValue);
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.SetValue(valueProp.objectReferenceValue);
                    break;
                case SerializedPropertyType.LayerMask:
                    property.SetValue(valueProp.intValue);
                    break;
                case SerializedPropertyType.Enum:
                    property.SetValue(valueProp.enumValueFlag);
                    break;
                case SerializedPropertyType.Vector4:
                    property.SetValue(valueProp.vector4Value);
                    break;
                case SerializedPropertyType.Rect:
                    property.SetValue(valueProp.rectValue);
                    break;
                case SerializedPropertyType.ArraySize:
                    property.SetValue(valueProp.arraySize);
                    break;
                case SerializedPropertyType.Character:
                    property.SetValue(valueProp.stringValue);
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.SetValue(valueProp.animationCurveValue);
                    break;
                case SerializedPropertyType.Bounds:
                    property.SetValue(valueProp.boundsValue);
                    break;
                case SerializedPropertyType.Gradient:
                    property.SetValue(valueProp.gradientValue);
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.SetValue(valueProp.objectReferenceValue);
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    property.SetValue(valueProp.fixedBufferSize);
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.SetValue(valueProp.vector2IntValue);
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.SetValue(valueProp.vector3IntValue);
                    break;
                case SerializedPropertyType.RectInt:
                    property.SetValue(valueProp.rectIntValue);
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.SetValue(valueProp.boundsIntValue);
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.SetValue(valueProp.managedReferenceValue);
                    break;
                case SerializedPropertyType.Hash128:
                    property.SetValue(valueProp.hash128Value);
                    break;
                case SerializedPropertyType.RenderingLayerMask:
                    property.SetValue(valueProp.intValue);
                    break;
                case SerializedPropertyType.EntityId:
                    property.SetValue(valueProp.entityIdValue);
                    break;
                default: break;
            }
            return property.boxedValue;
        }
        
        /// <summary>
        /// Retrieves a FieldInfo object for a field within a type, including fields in nested types,
        /// by traversing the specified dot-separated path of fields.
        /// </summary>
        /// <param name="type">The type from which the field will be resolved.</param>
        /// <param name="path">Dot-separated string representing the field hierarchy (e.g., "Outer.Inner.Field").</param>
        /// <returns>A FieldInfo object representing the requested field, or null if the field cannot be found.</returns>
        /// <remarks> The field info extracted here is instance-independent, meaning that the actual instance cannot be extracted form the returned FieldInfo.
        /// </remarks>
        public static System.Reflection.FieldInfo GetFieldViaPath(this SerializedProperty property)
        {
            //**Obtain the Parent Type**: The type of the object containing the serialized property is determined:
            System.Type parentType = property.serializedObject.targetObject.GetType();
            //Using the `GetFieldViaPath` method, resolve the field's `FieldInfo` based on the dot-separated path of the serialized property:
            return parentType.GetFieldViaPath(property.propertyPath);
        }
        
        /// <summary>
        /// Retrieves the System.Type of the field backing a SerializedProperty instance.
        /// </summary>
        /// <param name="property">The SerializedProperty whose FieldInfo type needs to be determined.</param>
        /// <returns>The System.Type of the field backing the SerializedProperty, or null if unable to resolve the type.</returns>
        public static System.Type GetSystemType(this SerializedProperty property) =>
            property.GetFieldViaPath().FieldType;

        /// <summary>
        /// Retrieves the value of the field represented by the specified SerializedProperty
        /// from its associated object.
        /// </summary>
        /// <param name="property">The SerializedProperty object representing the field for which the value will be retrieved.</param>
        /// <returns>The value of the field as an object, or null if the field cannot be resolved.</returns>
        public static object GetValue(this SerializedProperty property) =>
            property.GetFieldViaPath().GetValue(property.serializedObject.targetObject);

        /// <summary>
        /// Sets the value of the field represented by the SerializedProperty to the specified value.
        /// </summary>
        /// <param name="property">The serialized property whose field value will be modified.</param>
        /// <param name="value">The value to assign to the field represented by the serialized property.</param>
        public static void SetValue(this SerializedProperty property, object value) =>
            property.GetFieldViaPath().SetValue(property.serializedObject.targetObject, value);
    }
}