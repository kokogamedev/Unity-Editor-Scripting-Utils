# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-04-07

### This is the first release of *Unity Utility Extensions*.

### Added
- **EditorExtensions**:
    - `SetBoxedValue`: Allows updates of nested `SerializedProperty` values using a dot-separated path.
    - `GetValue` & `SetValue`: Reflection-based methods for retrieving and assigning serialized property values.
    - `GetSystemType`: Provides the `System.Type` for serialized fields.
    - `GetFieldViaPath`: Accesses `FieldInfo` for a field based on the serialized property's path.
- **ReflectionExtensions**:
    - `SetValueViaPath`: Updates nested object fields using a string-based dot-separated path.
    - `GetFieldMapsViaPath`: Resolves object and type stacks, supporting indexed collections.
    - `GetFieldViaPath`: Retrieves `FieldInfo` for nested fields using reflection.
    - `TryParseCollectionPart`: Parses and resolves indexed collection paths (e.g., "list[3]").
    - `IListMemberPointer`: Introspects and modifies list or array items for reflection operations.
- **EnumExtensions**:
    - `TryGetEnumByIndex<T>`: Dynamically retrieves an enum value based on its index order.
    - `TryGetEnumByIndex`: Resolves enum values dynamically using the field name and its definition order.

### Notes
- This package is designed to simplify Unity editor scripting workflows by providing robust reflection-based utilities.
- The **first release** offers foundational support for serialized property handling, advanced reflection techniques, and dynamic enum manipulation.
- The package includes documentation for all key features.

---