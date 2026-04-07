# Unity Utility Extensions

A powerful set of utilities for custom editor scripting, reflection, and enum manipulation in Unity. This package simplifies working with nested serialized fields, dynamic reflection, and enum values for complex Unity workflows.

## **Motivation**

Working with nested structs and serialized fields in Unity can be challenging, especially when updating properties inside deeply nested structures. Unity's `SerializedProperty` system lacks robust tools for recursive struct serialization and direct field manipulation. This library bridges that gap by:
- Leveraging reflection for dynamic field access (via dot-separated paths).
- Adding support for boxed value manipulation.
- Providing enum handling utilities based on order indices instead of raw integer values.

---

## **Features Overview**

### `EditorExtensions`
- **Dynamic Value Retrieval/Assignment:** Provides methods to get or set values in serialized properties.
- **Type Information:** Retrieve `System.Type` of serialized fields.
- **Boxed Value Handling:** Update and manipulate `SerializedProperty.boxedValue` for nested struct serialization.

### `ReflectionExtensions`
- **Advanced Field Reflection:** Access nested fields or array elements using dot-separated paths.
- **Field Mapping:** Retrieve field stacks for dynamic traversal.
- **Collection Parsing:** Handle array or IList elements for reflection.
- **Direct Value Assignment:** Set values inside structs or arrays with path-based navigation.

### `EnumExtensions`
- **Index-based Enums:** Retrieve enums dynamically using their order index.
- **Unified Enum Handling:** Resolve enums for fields across different structures.

---

## Installation & Usage

1. Add the package via Git URL or local folder.
2. `using PsigenVision.Utilities;` and/or `using PsigenVision.Utilities.Editor`
3. Start using extensions as seen in examples!


---

## **How It Works**

### 1. Reflection + SerializedProperty
Easily manipulate or update fields using dot-separated field paths and reflection.

```csharp
// Example: Updating fields using boxedValue
object boxedObj = property.boxedValue;
string path = "innerStruct.booleanValue";

// Set the value via path
boxedObj.SetValueViaPath(property.GetSystemType(), path, true);

// Assign back to the property
property.boxedValue = boxedObj;
property.serializedObject.ApplyModifiedProperties();
```

### 2. Field Mapping for Collections
Use reflection to map nested field stacks, supporting fields like arrays or lists.

```csharp
object[] objectStack = null;
System.Type[] typeStack = null;
string[] fieldNames = null;

if (myObject.GetFieldMapsViaPath(typeof(MyClass), "nestedField.listField[3].value",
    ref objectStack, ref typeStack, ref fieldNames)) {
    Debug.Log($"Resolved field: {fieldNames.Last()}");
}
```

### 3. Enum Handling by Index
Retrieve enums dynamically using order-based indices instead of raw integer values.

```csharp
int index = 2;
if (index.TryGetEnumByIndex(out MyEnum result)) {
    Debug.Log($"Enum found: {result}");
}
```

---

## **Key Methods**

### `EditorExtensions`
- `SetBoxedValue(SerializedProperty property, ...)` - Update nested values in serialized properties.
- `GetValue()` / `SetValue(object value)` - Read/write values using reflection.
- `GetSystemType()` - Get the type of a serialized property.

### `ReflectionExtensions`
- `GetFieldViaPath(string path)` - Resolve `FieldInfo` dynamically.
- `SetValueViaPath(string path, object value)` - Set values in nested fields and collections.
- `GetFieldMapsViaPath(...)` - Traverse object hierarchies for advanced scenarios.

### `EnumExtensions`
- `TryGetEnumByIndex<T>(int index)` - Get an enum value by its index in the definition order.

---

## **Use Cases**

1. **Custom Property Drawers:** Simplify serialization and property field updates for nested structs or dynamic objects using reflection and boxed values.
2. **Dynamic Enums:** Dynamically handle enums in property fields based on user selection indices.
3. **Complex Data Hierarchies:** Easily traverse, debug, or modify deeply nested serialized data structures.

---

## **Limitations**
- Array/List access in reflection requires implementing `IList`.
- Field paths must follow the dot-separated format (supports collections like `array[3]`).

---

## Contributing
Feel free to submit issues or propose improvements. Contributions are welcome!

---

## License
This package is distributed under the MIT license. See `LICENSE` for details.