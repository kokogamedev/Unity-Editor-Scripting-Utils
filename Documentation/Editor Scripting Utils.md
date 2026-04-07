# Utility Functions for Editor Scripting and Reflection in Unity

This collection of utilities is designed to streamline custom editor scripting, advanced reflection, and enum manipulation within Unity. These extensions allow developers to dynamically handle serialized properties, type reflection, enum indexing, and more.

The motivation behind the creation of these utilities emerged from the challenges of serializing structs recursively using custom property drawers. Updates to property fields in nested structs, for example, became undependable using SerializedProperty alone. This package allows for updating struct values using reflection and direct assignment to the SerializedProperty's boxedValue using dot-separated paths provided as a string.  

The utilities include:
1. **`EditorExtensions`** - Focused on managing `SerializedProperty` objects and interacting with Unity's serialized system.
2. **`ReflectionExtensions`** - Enables deep reflection, such as accessing nested fields, traversing object hierarchies, and resolving collections.
3. **`EnumExtensions`** - Adds utility methods for working with enum values dynamically using ordered index rather than int-value.


---

## **`EditorExtensions.cs`**

### **Overview:**
The `EditorExtensions` class provides an array of extension methods for `SerializedProperty`, allowing developers to dynamically get and set values, determine type info, and navigate serialized field structures. These methods rely on **Reflection** and modification or extraction from a `SerializedProperty`'s `boxedValue`.

### Key Methods

#### **SetBoxedValue**
*Sets a new value for a serialized property, handling boxing and path-based assignment for potentially nested fields.*

```csharp
void SetBoxedValue(this SerializedProperty property, SerializedProperty valueProp, out object boxedValue);
void SetBoxedValue(this SerializedProperty property, System.Type type, string path, SerializedProperty valueProp, out object boxedValue);
```

**Example Usage:**

_Without using dot-separated path:_
```csharp
SerializedProperty targetProperty = serializedObject.FindProperty("nestedField");
SerializedProperty sourceValueProp = serializedObject.FindProperty("newValue");

object boxedValue;
targetProperty.SetBoxedValue(sourceValueProp, out boxedValue);
Debug.Log($"Value updated to: {boxedValue}");
```

_Using dot-separated path:_ 
```csharp
EditorGUI.BeginChangeCheck();

object currentBoxedValue = valueProp.boxedValue;
var fieldPath = "innerStruct.value.intValue";
var outerType = property.GetSystemType(); //This is a method in this library

EditorGUI.PropertyField(someRect, valueProp, GUIContent.none);

if (EditorGUI.EndChangeCheck())
{
	if (valueProp.serializedObject.ApplyModifiedProperties())
	{
		// --------Sync Object with Serialized State--------
		// Re-assign the entire struct back to the property with the modified value extracted from the value property
		currentBoxedValue = property.GetBoxedValueViaPath(outerType, fieldPath, valueProp);
	}
	
	property.boxedValue = currentBoxedValue;
	property.serializedObject.ApplyModifiedProperties();
}
```

#### **GetValue / SetValue**
Utilizes reflection to set or get the value of a field encapsulated by the `SerializedProperty`.

```csharp
object GetValue(this SerializedProperty property);
void SetValue(this SerializedProperty property, object value);
```

**Example Usage:**
```csharp
var value = property.GetValue();
Debug.Log($"Current value: {value}");

property.SetValue(42);
Debug.Log("Value updated to 42.");
```

#### **GetSystemType**
Utilizes reflection to get the `System.Type` of a field encapsulated by the `SerializedProperty`.

```csharp
System.Type GetSystemType(this SerializedProperty property);
```

**Example Usage:**
```csharp
System.Type type = property.GetSystemType();
Debug.Log($"Property type: {type}");
```

#### **GetFieldViaPath**
Extract the `System.Reflection.FieldInfo` from the field encapsulated by a `SerializedProperty` via its path. 

```csharp
System.Reflection.FieldInfo GetFieldViaPath(this SerializedProperty property);
```

**Limitations:**
The field info extracted here is instance-independent, meaning that the actual instance cannot be extracted form the returned FieldInfo.

---

## **`ReflectionExtensions.cs`**

### **Overview:**
`ReflectionExtensions` expands on Unity's and .NET's reflection capabilities, making it easier to access and manipulate fields, traverse object hierarchies, and interact with collections. In particular, these extensions support the ability to obtain or assign information and values from fields using the starting object, its type, the value object to be assigned, and a dot-separated field path provided by string.

**Limitations**:
Although index-accessed collections are supported in the dot-separated path, the associated collection must be an array or implement IList, and the format for access must be "collection[i]"

### Key Methods and Classes:

#### **IListMemberPointer**
Encapsulates information about a member of an `IList` via its `FieldInfo`, index in the collection, and the collection object with which it is associated. 

**Methods**:
- `object GetValue()`: Fetches the value from the list/field.
- `void SetValue(object newValue)`: Sets the value for the field/list element at the contained `ArrayIndex` of the `Container` collection to the newly passed in value.
- `IList GetCollection()`: Returns the collection as an `IList`.

#### **GetFieldViaPath**
*Dynamically resolves a `FieldInfo` for a dot-separated field path.*

```csharp
System.Reflection.FieldInfo GetFieldViaPath(this System.Type type, string path);
```

**Example Usage:**
```csharp
FieldInfo field = typeof(MyClass).GetFieldViaPath("nestedField.innerProperty");
```

**Limitations:**
Although index-accessed collections are supported in the path, the associated collection must be an array or implement IList, and the format for access must be "collection[i]." Furthermore, this method returns the field info apart from its instance, so the "i'th" element is never accessed.

#### **TryParseCollectionPart**
Parses a string-provided collection access (e.g. "someCollection[3]") and returns the collection name, as well as the index accessed, as long as the format is correct.
```csharp
bool TryParseCollectionAccess(this string accessPath, out string fieldName, out int index);
```

**Example Usage:**
```csharp
string fieldPart = "myList[2]";
if (fieldPart.TryParseCollectionAccess(out string fieldName, out int index))
{
    Debug.Log($"Field: {fieldName}, Index: {index}");
}
```

#### **GetFieldMapsViaPath**
*Maps a dot-separated field path to corresponding type and object stacks along with field names. It traverses a type hierarchy, resolving the path and storing object instances, types, and field names encountered. Supports arrays and nested fields.*

```csharp
bool GetFieldMapsViaPath(this object target, System.Type type, string path,
    ref object[] objectStack, ref System.Type[] typeStack,
    ref string[] fieldNames, bool reversed = false);
```

**Parameters**:
- `object target`: The root object instance from which the field traversal starts.
- `System.Type type`: The root type to begin resolving the field path.
- `string path`: A dot-separated string representing the hierarchy of fields to traverse (e.g., "Field1.SubField.ArrayField[0]").
- `ref object[] objectStack`: An output parameter storing resolved object instances encountered during traversal in sequence.
- `ref System.Type[] typeStack`: An output parameter storing resolved type instances encountered during traversal in sequence.
- `ref string[] fieldNames`: An output parameter storing resolved field names in the traversal sequence.
- `bool reversed`: A boolean value indicating the traversal direction. If true, the collections store fields in reverse order

**Returns**:
- A boolean value indicating whether the field mapping was successful. Returns false if input parameters are invalid or a field in the path cannot be resolved.

**Example Usage:**
```csharp
object[] objectStack = null;
System.Type[] typeStack = null;
string[] fieldNames = null;

if (myObject.GetFieldMapsViaPath(typeof(MyClass), "nestedField.innerProperty",
    ref objectStack, ref typeStack, ref fieldNames)) {
    Debug.Log($"Field map successful! Field at path: {fieldNames[fieldNames.Length - 1]}");
}
```

**Remarks**:
1. This method supports nested types, arrays, and non-public fields during field resolution. 
2. The traversal order can be controlled by the reversed parameter. If reversed is true, the stacks are populated in reverse field order. 
3. If an invalid path or target is encountered, the output parameters are initialized but left empty, and false is returned.
4. In the event a collection that implements `IList` (including an array) is encountered in the field path, the collection's object is stored in the `objectStack` as well as its type, but the element contained is wrapped in an `IListMemberPointer` instance linking it to that collection. From this wrapper, the instance of that element can be reassigned to the collection in post-processing. This is essential, as the objects stored in the stack may not necessarily reference one another, including the element extracted from the collection.

**Limitations:**
 Although index-accessed collections are supported in the path, the associated collection must be an array or implement IList, and the format for access must be "collection[i]"


#### **SetValueViaPath**
*Sets the value of a field within a type or its nested fields, based on a dot-separated path.
The method resolves the field hierarchy, including nested fields or indexed collections, and assigns the specified value.*

```csharp
bool SetValueViaPath(this object target, System.Type type, string path, object value);
```

**Parameters**:
- `object target`: The object instance on which the field value will be set.
- `System.Type type`: The type of the target object that contains the "topmost" field (field at the beginning of the path).
- `string path`: The dot-separated path to the field representing the hierarchy of fields to traverse (e.g., "Field1.SubField.ArrayField[0]").
- `object value`: The value to set on the resolved field.

**Returns**:
- Returns `true` if the value was successfully set; otherwise, `false` if the path is invalid, the field cannot be resolved, or the value assignment fails.

**Example Usage:**
```csharp
var boxedObject = property.boxedValue;
var path = "innerStruct.booleanValue";
//valueProp is derived from a PropertyField in a custom property drawer drawing a boolean
//Update the innerStruct.booleanValue field of our boxed object and reassigne it to its associated SerializedProperty
boxedObj.SetValueViaPath(type, path, valueProp.boolValue);
property.boxedValue = boxedObject; 
```

**Limitations**:
Although index-accessed collections are supported in the path, the associated collection must be an array or implement IList, and the format for access must be "collection[i]

---

## **`EnumExtensions.cs`**

### **Overview:**
`EnumExtensions` provides specialized methods for handling enums.

### Key Methods

#### **TryGetEnumByIndex**
*Safely retrieves an enum value by index rather than int-value.*

```csharp
bool TryGetEnumByIndex<T>(this int index, out T enumValue) where T : Enum;
```

**Example Usage:**
```csharp
if (3.TryGetEnumByIndex(out MyEnum result)) {
    Debug.Log($"Retrieved enum: {result}");
} else {
    Debug.Log("Index out of bounds.");
}
```

#### **Field-based Enum Retrieval**
Attempts to retrieve an enum value from the object containing the instance of that enum from the index (by order) of that value within the enum and the name of the field-instance of that enum in the container. 

```csharp
bool TryGetEnumByIndex(this object container, string fieldName, int index, [CanBeNull] out object result);
```

**Example Use Case**:
In custom property drawers designed to target multiple structures without any inheritance- or interface-based connections, this allows inspection of enums within those structures requiring only that their instances share the same name, and that the structure's individual enum-type "members" be related by order. The type of the enums need not be the same, and the structures may be structs, which cannot support inheritance or interface-implementation. 

**Example Code**:
```csharp
// 1. Get the specific struct instance
var anyTypeStruct = property.boxedValue;

//2. Try and get the enum equivalent value for the current type (if defined in the correct format)
if (!anyTypeStruct.TryGetEnumByIndex("type", currentIndex, out var castedValueType))
{
	Debug.LogErrorFormat("{0} does not possess a properly formatted value type enum", property.displayName);
	return false;
}
```

---

## How These Work Together:

1. **Reflection + SerializedProperty**:
	- Use `ReflectionExtensions` to dynamic access/manipulation of nested fields and collections.
	- Use `EditorExtensions` for Unity’s serialization integration.

2. **Enumeration Utilities**:
	- Use `EnumExtensions` for dynamically resolving enums at runtime.

3. **Custom Unity Editors**:
	- Leverage these tools in custom inspectors and drawers to interact with serialized data and complex hierarchies effectively.