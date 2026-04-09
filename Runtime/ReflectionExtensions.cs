using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace PsigenVision.Utilities
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Retrieves a FieldInfo object for a field within a type or its nested fields, based on a dot-separated path.
        /// Handles both regular fields and fields within array or generic container types.
        /// </summary>
        /// <param name="type">The type from which the field will be resolved.</param>
        /// <param name="path">A dot-separated string representing the hierarchy of fields to traverse (e.g., "Field1.SubField.ArrayField[0]").</param>
        /// <returns>A FieldInfo object representing the resolved field, or null if the field cannot be found or the path is invalid.</returns>
        /// <remarks>Although index-accessed collections are supported in the path, the associated collection must be an array or implement IList,
        /// and the format for access must be "collection[i]." Furthermore, this method returns the field info apart from its instance, so the "i'th" element is never accessed.</remarks>
        public static System.Reflection.FieldInfo GetFieldViaPath(this System.Type type, string path)
        {
            //Ensure that both public and non-public instance fields are searched.
            System.Reflection.BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            
            System.Type containingObjectType = type;
            System.Reflection.FieldInfo fieldInfo = containingObjectType.GetField(path, flags);
            
            //The `path` is split into individual field names using `Split('.')`. These field names guide the traversal.
            string[] pathsPerDot = path.Split('.');

            //Fields are resolved one by one along the hierarchy
            foreach (var part in pathsPerDot)
            {
                string fieldName = part;
                
                // Detect and handle array/generic collection notation (e.g., "arrayVariable[3]")
                //NOTE: No runtime access of collection via indices is performed as this would require an instance
                bool isCollection = part.Contains("[") && part.EndsWith("]");
                
                //Process the collection-type part, and output the field name without index notation as well as the index
                if (isCollection && !part.TryParseCollectionAccess(out fieldName, out int collectionIndex))
                    //If the collection-type part is in an invalid format, return null (as an improperly formatted path has been supplied)
                    return null; 
                
                //Resolve the field using reflection - get the field info of the current field along the dot hierarchy
                fieldInfo = containingObjectType.GetField(fieldName, flags); 
                
                //Attempt to handle null field info by assuming inheritance, and attempting to resolve it in the base type
                /*
                 * If the field cannot be found in the current type, the method attempts to resolve it in the **base type**.
                 * This allows for field retrievals in inherited classes. (via recursion)
                 */
                if (fieldInfo == null)
                    return type.BaseType != null ? GetFieldViaPath(containingObjectType.BaseType, path) : null;
                
                //If we are not dealing with a collection (or a valid collection), return the field type of the current field info directly
                if (!isCollection)
                {
                    containingObjectType = fieldInfo.FieldType;
                    continue;
                }

                //There are only two container field types that can be serialized: Array and List<T> - check for both
                //Handle Arrays
                if (fieldInfo.FieldType.IsArray)
                    //If the resolved field is an `Array`, its **element type** (`GetElementType()`) is used for further traversal
                    containingObjectType  = fieldInfo.FieldType.GetElementType();
                //Handle Generics
                else if (fieldInfo.FieldType.IsGenericType)
                {
                    //Ensure this is a collection type (like List<T>)
                    if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        Debug.LogError($"Field '{fieldName}' is not a compatible collection but was accessed as one.");
                        return null;
                    }
                    
                    //If the resolved field is **generic** (e.g., `List<T>`), the contained type (`T`) is used for the next traversal step
                    containingObjectType = fieldInfo.FieldType.GetGenericArguments()[0];
                }
            }
            
            return fieldInfo;
        }

        /// <summary>
        /// Sets the value of a field within a type or its nested fields, based on a dot-separated path.
        /// The method resolves the field hierarchy, including nested fields or indexed collections, and assigns the specified value.
        /// </summary>
        /// <param name="target">The object instance on which the field value will be set.</param>
        /// <param name="type">The type of the target object that contains the "topmost" field (field at the beginning of the path).</param>
        /// <param name="path">A dot-separated string representing the hierarchy of fields to traverse (e.g., "Field1.SubField.ArrayField[0]").</param>
        /// <param name="value">The value to set on the resolved field.</param>
        /// <returns>True if the value was successfully set; otherwise, false if the path is invalid, the field cannot be resolved, or the value assignment fails.</returns>
        /// <remarks>Although index-accessed collections are supported in the path, the associated collection must be an array or implement IList,
        /// and the format for access must be "collection[i]"</remarks>
        public static bool TrySetValueViaPath(this object target, System.Type type, string path, object value)
        {
            if (target == null || string.IsNullOrEmpty(path)) return false;

            var objectStack = Array.Empty<object>();
            var typeStack = Array.Empty<Type>();
            var fieldNames = Array.Empty<string>();

            if (!target.GetFieldMapsViaPath(type, path, ref objectStack, ref typeStack, ref fieldNames, true))
            {
                Debug.LogError($"Field maps cannot be derived from path {path} for object {target} of type {type}");
                return false;
            }

            int currentObjectIndex = 0;
            // Now walk back up and set each level
            for (int i = 0; i < fieldNames.Length; i++)
            {
                //Get current child field name
                var childFieldName = fieldNames[i];
                var containingType = (i == fieldNames.Length - 1) ? type : typeStack[currentObjectIndex + 1]; //If we have reached the root of the fields, assign the last object to the target; otherwise chain-assign the object to the next parent field 

                //Handle any potential collections
                if (((i == fieldNames.Length - 1) ? target : objectStack[currentObjectIndex + 1]) //If we have reached the root of the fields, assign the last object to the target; otherwise chain-assign the object to the next parent field  
                    is IListMemberPointer memberPointer)
                {
                    //Set the upcoming member in the object stack to be an element of the current collection 
                    //Handle nested collections
                    if (objectStack[currentObjectIndex] is IListMemberPointer memberPointer2)
                    {
                        memberPointer.SetValue(memberPointer2.Container);
                        currentObjectIndex++; //The element at the current object index has been handled via setting it to the member collection
                        
                        //Defer to the next loop for assigning field to the nested container
                    }
                    else
                    {
                        memberPointer.SetValue(
                            (i == 0) ? value //Set the element's value to be the passed in value if this is the first child (the last field in the dot-separated path)
                            : objectStack[currentObjectIndex]); //Set the element's value to be current object instance if this is the any of the parent fields in the dot-separated path
                        currentObjectIndex++; //The element at the current object index has been handled via setting it to the member collection
                    
                        //Set the current parent's field to be the collection, not the element
                        //Set child field name to correct value (collection container)
                        SetNextObjectInStack(i, containingType, ref childFieldName, memberPointer.Container);
                    }
                }
                else//Handle normal members
                {
                    //Set child field to correct value (the value passed in to be assigned if the first child, which is the last in the dot-separated list, and the current object instance otherwise)
                    SetNextObjectInStack(i, containingType, ref childFieldName, (i == 0) ? value : objectStack[currentObjectIndex]);
                }

                currentObjectIndex++;
            }
            return true;
            
            void SetNextObjectInStack(int iterationIndex, Type containingType, ref string childFieldName, object valueToAssign) => 
                containingType. //The next object/type up is the parent type of the current field
                GetField(childFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)//Get the child field of the upcoming type (off the parent object instance)
                .SetValue((iterationIndex == fieldNames.Length - 1) ? target : objectStack[currentObjectIndex+1], //If we have reached the root of the fields, assign the last object to the target; otherwise chain-assign the object to the next parent field
                    valueToAssign); 
        }

        /// <summary>
        /// Maps a dot-separated field path to corresponding type and object stacks along with field names.
        /// It traverses a type hierarchy, resolving the path and storing object instances, types, and field names encountered.
        /// Supports arrays and nested fields.
        /// </summary>
        /// <param name="target">The root object instance from which the field traversal starts.</param>
        /// <param name="type">The root type to begin resolving the field path.</param>
        /// <param name="path">A dot-separated string representing the hierarchy of fields to traverse (e.g., "Field1.SubField.ArrayField[0]").</param>
        /// <param name="objectStack">An output parameter storing resolved object instances encountered during traversal in sequence.</param>
        /// <param name="typeStack">An output parameter storing resolved type instances encountered during traversal in sequence.</param>
        /// <param name="fieldNames">An output parameter storing resolved field names in the traversal sequence.</param>
        /// <param name="reversed">A boolean value indicating the traversal direction. If true, the collections store fields in reverse order.</param>
        /// <returns>A boolean value indicating whether the field mapping was successful. Returns false if input parameters are invalid or a field in the path cannot be resolved.</returns>
        /// <remarks>
        /// This method supports nested types, arrays, and non-public fields during field resolution.
        /// The traversal order can be controlled by the reversed parameter. If reversed is true, the stacks are populated in reverse field order.
        /// If an invalid path or target is encountered, the output parameters are initialized but left empty, and false is returned.
        /// Although index-accessed collections are supported in the path, the associated collection must be an array or implement IList,
        /// and the format for access must be "collection[i]"
        /// </remarks>
        public static bool GetFieldMapsViaPath(this object target, System.Type type, string path,
            ref object[] objectStack, ref System.Type[] typeStack,
            ref string[] fieldNames, bool reversed = false)
        {
            //Return a falsy result in the case of invalid inputs
            if (target == null || string.IsNullOrEmpty(path)) return Falsy(out objectStack, out typeStack, out fieldNames);
            
            //Ensure that both public and non-public instance fields are searched.
            System.Reflection.BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            
            //Split the path into parts based on dot-separation (this includes potential array notation). These parts guide traversal
            var pathsPerDot = path.Split('.');

            //Initialize arrays based on how many parts are in the path
            objectStack = new object[pathsPerDot.Length];
            typeStack = new Type[pathsPerDot.Length];
            fieldNames = new string[pathsPerDot.Length];
            
            object currentObjInstance = target;
            Type containingObjectType = type;
            FieldInfo fieldInfo = containingObjectType.GetField(path, flags);

            var length = pathsPerDot.Length;
            var currentIndex = (reversed) ? length - 1 : 0;
            var currentNameIndex = currentIndex; //In the case of arrays, the currentIndex (for object ant type stacks) will shift by 1, whereas the name index will not
            var nameLength = length;
            
            /*//Get first containing type and object
            currentObjInstance = fieldInfo.GetValue(currentObjInstance);
            containingObjectType = fieldInfo.GetType();
            
            //Initialize object and type collections with the pre-obtained object and type (the top/bottom of the stacks)
            objectStack[currentIndex] = currentObjInstance;
            typeStack[currentIndex] = containingObjectType;*/
            
            for (int i = 0; i < nameLength; i++)
            {
                var part = pathsPerDot[i];
                string fieldName = part;
                
                //Initialize object and type collections with the previously derived object and type
                if (i > 0)//Only do this if this is not the first iteration
                {
                    objectStack[currentIndex] = currentObjInstance;
                    typeStack[currentIndex] = containingObjectType;
                    
                    //Update index for next spot in the loop
                    Step(ref currentIndex);
                    Step(ref currentNameIndex);
                }
            
                // Detect and handle array/generic collection notation (e.g., "arrayVariable[3]")
                //NOTE: No runtime access of collection via indices is performed as this would require an instance
                bool isCollection = part.Contains("[") && part.EndsWith("]");
                int collectionIndex = 0; //initialize collection index in case this is a collection
                
                //Process the collection-type part, and output the field name without index notation as well as the index
                if (isCollection && !part.TryParseCollectionAccess(out fieldName, out collectionIndex))
                    //If the collection-type part is in an invalid format, return null (as an improperly formatted path has been supplied)
                    return Falsy(out objectStack, out typeStack, out fieldNames); 
                
                //Store the currently processed field name
                fieldNames[currentNameIndex] = fieldName;
                
                //Resolve the field using reflection - get the field info of the current field along the dot hierarchy
                fieldInfo = containingObjectType.GetField(fieldName, flags); 
                
                //Attempt to handle null field info by assuming inheritance, and attempting to resolve it in the base type
                /*
                 * If the field cannot be found in the current type, the method attempts to resolve it in the **base type**.
                 * This allows for field retrievals in inherited classes. (via recursion)
                 */
                if (fieldInfo == null)
                    return type.BaseType != null 
                        ? GetFieldMapsViaPath(target, containingObjectType.BaseType, path,
                        ref objectStack, ref typeStack, ref fieldNames, reversed) 
                        : Falsy(out objectStack, out typeStack, out fieldNames);

                //If we are not dealing with a collection (or a valid collection), cache the field type of the current field info directly, as well as its associated object instance
                if (!isCollection)
                {
                    currentObjInstance = fieldInfo.GetValue(currentObjInstance);
                    containingObjectType = fieldInfo.FieldType;
                    continue;
                }
                //Handle that a dot-separated part for a collection (e.g. "arrayA[4]") is really two objects, and we must lengthen the stacks
                ShiftStacks(ref objectStack, ref typeStack, ref length, ref currentIndex, reversed);
                
                //Assign an instance of a member pointer class rather than the collection itself, as the likelihood of it being connected to its member in this process is remote, and this wrapper is needed to reconnect them
                var collectionObject = new IListMemberPointer(fieldInfo.GetValue(currentObjInstance), fieldInfo, collectionIndex);
                //Add the collection to the object and type stacks at the current index
                objectStack[currentIndex] = currentObjInstance = collectionObject;
                typeStack[currentIndex] = fieldInfo.FieldType;
                    
                //Step the current index to insert the element
                Step(ref currentIndex);
                //currentIndex = Index(currentIndex + 1, length);

                if (fieldInfo.FieldType != typeof(IList))
                {
                    Debug.LogError($"Field path containing indexer [{collectionIndex}] is not an implementer of IList");
                    return Falsy(out objectStack, out typeStack, out fieldNames);
                }
                
                //There are only two container field types that can be serialized: Array and List<T> - check for both
                //Handle Arrays
                if (fieldInfo.FieldType.IsArray)
                {
                    //If the resolved field is an `Array`, its **element type** (`GetElementType()`) is used for further traversal
                    //Get elementtype for caching in next loop
                    containingObjectType = fieldInfo.FieldType.GetElementType();
                }
                //Handle Generics
                else if (fieldInfo.FieldType.IsGenericType)
                {
                    //Ensure this is a collection type (like List<T>)
                    if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        Debug.LogError($"Field '{fieldName}' is not a compatible collection but was accessed as one.");
                        return Falsy(out objectStack, out typeStack, out fieldNames);
                    }

                    //If the resolved field is **generic** (e.g., `List<T>`), the contained type (`T`) is used for the next traversal step
                    containingObjectType = fieldInfo.FieldType.GetGenericArguments()[0];

                }
                else continue;
                
                currentObjInstance = collectionObject.GetValue();

                //In the event of element retrieval failure
                if (currentObjInstance == null) return Falsy(out objectStack, out typeStack, out fieldNames);
            }
            
            //Add lingering object and type to the current stacks (loop exits before the last can be added)
            objectStack[currentIndex] = currentObjInstance;
            typeStack[currentIndex] = containingObjectType;

            return true; //Indicates successful completion of the algorithm


            bool Falsy(out object[] objectStack, out System.Type[] typeStack,
                out string[] fieldNames)
            {
                objectStack = null; typeStack = null; fieldNames = null;
                return false;
            }
            
            void ShiftStacks(ref object[] objectStack, ref Type[] typeStack, ref int length,
                ref int currentIndex, bool reversed)
            {
                //Handle that a dot-separated part for a collection (e.g. "arrayA[4]") is really two objects, and we must lengthen the stacks
                //Shift the currentIndex; Increase the stack length
                Shift(ref currentIndex, ref length);
                //Shift the stacks
                var shiftedObjectStack = new object[length];
                var shiftedTypeStack = new Type[length];
                //if reversed, the end of the last collection must be at the end of the new one, which is lengthed by 1 (so the original array must be inserted at index 1 of the shifted array)
                objectStack.CopyTo(shiftedObjectStack, reversed ? 1 : 0); 
                typeStack.CopyTo(shiftedTypeStack, reversed ? 1 : 0);
                //Reassign the shifted stacks to the source stacks 
                objectStack = shiftedObjectStack;
                typeStack = shiftedTypeStack;
            }
            
            void Step(ref int index) => index = reversed ? index - 1 : index + 1;
            void Shift(ref int index, ref int length)
            {
                index = reversed ? index + 1 : index - 1;
                length++;
            }
        }


        /// <summary>
        /// Parses a string representing an indexed collection (e.g., "array[3]") to extract the field name and the index value.
        /// </summary>
        /// <param name="accessPath">The string to parse, which may represent a collection field with an index (e.g., "fieldName[index]").</param>
        /// <param name="fieldName">Outputs the field name parsed from the string, excluding any index notation.</param>
        /// <param name="index">Outputs the index value if successfully parsed; otherwise, default if parsing fails.</param>
        /// <returns>True if the string is successfully parsed into a field name and index, false if the format is invalid.</returns>
        /// <remarks>The format for the collection access path must be "collection[i]".</remarks>
        public static bool TryParseCollectionAccess(this string accessPath, out string fieldName, out int index)
        {
            // Extract the field name before the "[" and the index within [ ]
            int indexStart = accessPath.IndexOf('[');
            fieldName = accessPath.Substring(0, indexStart); // Everything before "["
            string indexPart = accessPath.Substring(indexStart + 1, accessPath.Length - indexStart - 2); // Extract value inside "[ ]"

            if (!int.TryParse(indexPart, out int parsedIndex))
            {
                index = parsedIndex; // Successfully parsed the index
                return true;
            }
            Debug.LogError($"Invalid array index format: '{accessPath}'");
            index = default;
            return false;
        }

        /// <summary>
        /// Represents a container or a field that includes metadata to access its value, allowing operations such as retrieval or modification
        /// of data within collections or complex types by accounting for indexers and nested member paths.
        /// </summary>
        /// <remarks>The associated collection container must be an array or implement IList
        /// </remarks>
        public struct IListMemberPointer
        {
            /// <summary>
            /// Represents an encapsulating IList or Array object to which the stored field element "points" or "is a member of."
            /// </summary>
            /// <remarks>The associated collection container must be an array or implement IList
            /// </remarks>
            /// <remarks>
            /// The <c>Container</c> property holds a reference to the encapsulated parent object
            /// that contains the field being accessed or set via its information.
            /// </remarks>
            /// <value>
            /// An <see cref="System.Object"/> that represents the parent container object of the
            /// field being manipulated. This is typically used in conjunction with FieldInfo
            /// to dynamically access or modify fields or members of the container object.
            /// </value>
            /// <seealso cref="FieldInfo"/>
            /// <seealso cref="System.Collections.IList"/>
            public object Container { get; set; }

            /// <summary>
            /// Gets the FieldInfo object associated with the container member, representing metadata
            /// and functionality to access or manipulate the field's value.
            /// </summary>
            /// <remarks>
            /// This field provides access to the metadata and value of a field within a container object.
            /// It supports operations such as retrieving or setting field values and accommodates use cases
            /// where fields may belong to arrays or indexed collections.
            /// </remarks>
            public FieldInfo Field { get; private set; }

            /// <summary>
            /// Gets the index of the array or collection represented by <exception cref="Container"></exception> that implements <see cref="System.Collections.IList"/> or is an Array.
            /// </summary>
            /// <remarks>
            /// This property is used to specify the index of an element within a collection or array being accessed.
            /// It is nullable to account for cases where no specific index is required, such as accessing the entire collection.
            /// </remarks>
            public int? ArrayIndex { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the <see cref="IListMemberPointer"/> instance is valid.
            /// </summary>
            /// <remarks>
            /// The property determines if the container and field associated with the <see cref="IListMemberPointer"/>
            /// have been correctly initialized and if the optional array index, if present, is valid. If the property
            /// evaluates to false, operations like `GetValue`, `SetValue`, or `GetCollection` may not behave as expected.
            /// </remarks>
            public bool Valid { get; private set; }

            /// <summary>
            /// Retrieves a value from a specified field or indexed collection within a container object.
            /// </summary>
            /// <returns>
            /// The value of the field or the indexed element in the collection if applicable.
            /// Returns null if the collection is invalid or the field cannot be resolved.
            /// </returns>
            /// <remarks>
            /// The method handles fields that are collections such as arrays or types implementing IList.
            /// If an index is provided and the collection supports indexing, the indexed value is returned.
            /// For invalid collections, an error is logged and null is returned.
            /// </remarks>
            public object GetValue() {
                if (!Valid)
                {
                    Debug.LogError("Invalid collection: does not implement IList and/or is not an array");
                    return null;
                }
                
                object collection = Field.GetValue(Container);

                // Handle anything with an indexer (Array, List<T>, etc.)
                if (ArrayIndex.HasValue && collection is IList list) {
                    return list[ArrayIndex.Value];
                }
                return collection;
            }

            /// <summary>
            /// Sets a new value for a field or an indexed element in a field's collection.
            /// Handles updating the value for regular fields, array elements, and elements in collections
            /// that implement IList, based on the provided information from the ContainerPointer instance.
            /// </summary>
            /// <param name="newValue">The new value to be assigned to the field or collection element.</param>
            public void SetValue(object newValue) {
                if (!Valid)
                {
                    Debug.LogError("Invalid collection: does not implement IList and/or is not an array");
                    return;
                }
                
                object collection = Field.GetValue(Container);

                if (ArrayIndex.HasValue && collection is IList list) {
                    // This works for Arrays and List<T> perfectly
                    list[ArrayIndex.Value] = newValue;
                } else {
                    Field.SetValue(Container, newValue);
                }
            }

            /// <summary>
            /// Updates the array index for an IListMemberPointer if the provided index is valid, ensuring it falls within the bounds
            /// of the associated IList or array. This allows setting/getting operations to target a new element location within the stored Container collection.
            /// </summary>
            /// <param name="newIndex">The new index to attempt to set for array or IList access.</param>
            /// <returns>True if the index was successfully updated; false if the index is out of bounds or the collection is invalid.</returns>
            public bool OverrideArrayIndex(int newIndex)
            {
                if (!Valid)
                {
                    Debug.LogError("Invalid collection: does not implement IList and/or is not an array");
                    return false;
                }

                if (newIndex >= 0 && Field.GetValue(Container) is IList list && newIndex < list.Count)
                {
                    ArrayIndex = newIndex;
                    return true;
                }
                
                Debug.LogError($"Invalid array index: '{newIndex}'");
                return false;
            }
            

            // Implicit conversion to Array
            /// <summary>
            /// Defines a custom implementation of an operator for a specific type or to enable specific functionality, such as operator overloading.
            /// Allows defining or modifying the behavior of standard operators (e.g., +, -, ==, etc.) for user-defined types.
            /// </summary>
            /// <param name="leftOperand">The left-hand operand participating in the operation.</param>
            /// <param name="rightOperand">The right-hand operand participating in the operation.</param>
            /// <returns>The result of the operation, of the appropriate type as defined by the operator.</returns>
            /// <remarks>Custom operators must follow established operator resolution and overloading rules in C#. Only certain operators can be overloaded, and they must follow symmetry and mathematical consistency to ensure logical behavior.</remarks>
            public static implicit operator System.Array(IListMemberPointer pointer)
            {
                return pointer.Container as System.Array;
            }

            /// <summary>
            /// Retrieves a collection from the container, either as an array or an IList, based on the container's underlying type.
            /// </summary>
            /// <returns>An IList representation of the collection if the container is a valid array or IList; otherwise, null if the container does not meet these criteria.</returns>
            public IList GetCollection()
            {
                if (!Valid)
                {
                    Debug.LogError("Invalid collection: does not implement IList and/or is not an array");
                    return null;
                }
                
                if (Container is Array array)
                    // Handle Array collection
                    return array;
                
                //Otherwise handle as a general IList
                return Container as IList;
            }

            /// <summary>
            /// Attempts to retrieve a collection as an IList of a specified type, casting it to the targeted generic IList if possible.
            /// </summary>
            /// <param name="collection">
            /// An output parameter where the result is stored. It will contain the casted IList if the operation is successful or null if the cast fails or the collection is invalid.
            /// </param>
            /// <typeparam name="T">The target type that must implement IList to which the collection should be cast.</typeparam>
            /// <remarks>
            /// The container must either implement IList or be an array. If the container cannot be cast to the specified type, an error message will be logged, and the output parameter will be set to null.
            /// </remarks>
            public void GetCollectionAsIList<T>(out T collection) where T:class,IList
            {
                if (!Valid)
                {
                    Debug.LogError("Invalid collection: does not implement IList and/or is not an array");
                    collection = null;
                }
                var col = Container as IList;
                if (col != null)
                    collection = col as T;
                else
                {
                    Debug.LogError($"Cannot cast colleciton to IList");
                    collection = null;
                }
            }

            /// <summary>
            /// Retrieves the collection contained within the specified container as an array, if the container is a valid array or implements IList.
            /// </summary>
            /// <param name="collection">The output array reference that will hold the collection as an array if the conversion is successful; otherwise, null if the container is invalid or does not support conversion to an array.</param>
            public void GetCollectionAsArray(out Array collection)
            {
                if (!Valid)
                {
                    Debug.LogError("Invalid collection: does not implement IList and/or is not an array");
                    collection = null;
                }
                var col = Container as Array;
                if (col != null) collection = col;
                else
                {
                    Debug.LogError($"Invalid array format: '{Container}'");
                    collection = null;
                }
            }

            /// <summary>
            /// Represents a pointer to a container, encapsulating information about the container object, its field, and an optional array index.
            /// </summary>
            /// <remarks>The associated collection container must be an array or implement IList
            /// </remarks>
            public IListMemberPointer(object container, FieldInfo field, int? arrayIndex)
            {
                Valid = container is IList || container.GetType().IsArray;
                Container = container;
                Field = field;
                ArrayIndex = arrayIndex;
            }
        }
    }
}