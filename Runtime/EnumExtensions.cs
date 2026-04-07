using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace PsigenVision.Utilities
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Tries to retrieve an enum value of type <typeparamref name="T"/> corresponding to the specified index.
        /// </summary>
        /// <typeparam name="T">The enum type to retrieve the value from. Must be of type <see cref="Enum"/>.</typeparam>
        /// <param name="index">The zero-based index of the enum value to retrieve.</param>
        /// <param name="enumValue">
        /// When this method returns, contains the enum value of type <typeparamref name="T"/> at the specified index,
        /// if the operation was successful; otherwise, the default value of <typeparamref name="T"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the operation is successful and an enum value is retrieved; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryGetEnumByIndex<T>(this int index, out T enumValue) where T : Enum
        {
            // GetValues returns an array of the enum members in order
            var values = System.Enum.GetValues(typeof(T));
    
            // Bounds check to avoid index out of range errors
            if (index >= 0 && index < values.Length) {
                enumValue = (T)values.GetValue(index);
                return true;
            }
    
            enumValue = default;
            return false;
        }

        /// <summary>
        /// Tries to retrieve an enum value from the specified field of the given object based on the provided index.
        /// </summary>
        /// <param name="container">The object containing the field with the enum type.</param>
        /// <param name="fieldName">The name of the field holding the enum type.</param>
        /// <param name="index">The zero-based index of the enum value to retrieve.</param>
        /// <param name="result">
        /// When this method returns, contains the enum value at the specified index from the enum field,
        /// if the operation was successful; otherwise, the value is null.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if the operation is successful and an enum value is retrieved; otherwise, false.
        /// </returns>
        public static bool TryGetEnumByIndex(this object container, string fieldName, int index,
            [CanBeNull] out object result)
        {
            result = null;
            if (container == null) return false;

            // 1. Find the field named after the field name provided
            FieldInfo? fieldInfo = container.GetType().GetField(fieldName);
            if (fieldInfo == null || !fieldInfo.FieldType.IsEnum) return false;

            // 2. Get all values in the order defined
            var enumValues = Enum.GetValues(fieldInfo.FieldType);

            // 3. Validate index and return
            if (index >= 0 && index < enumValues.Length) {
                result = enumValues.GetValue(index);
                return true;
            }

            return false;
        }

    }
}