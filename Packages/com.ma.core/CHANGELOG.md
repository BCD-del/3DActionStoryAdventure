# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.13] - 2024-07-04
### Fixed
- This version is compatible with Flora 2.0.13.

## [2.0.12] - 2024-07-03
### Fixed
- Fixed SetBitEnumerator not enumerating set bits correctly.

## [2.0.11] - 2024-06-29
- This version is compatible with Flora 2.0.11.

## [2.0.10] - 2024-06-27
- This version is compatible with Flora 2.0.10.

## [2.0.9] - 2024-06-26
### Changed
- Remove `com.unity.collections` and `com.unity.burst` dependencies. These are still required, but are now installed with the wizard.

## [2.0.8] - 2024-06-25
- This version is compatible with Flora 2.0.8.

## [2.0.7] - 2024-06-24
- This version is compatible with Flora 2.0.7.

## [2.0.6] - 2024-06-21
- This version is compatible with Flora 2.0.6.

## [2.0.5] - 2024-06-20
### Added
- Add internal bridge `TerrainDataBridge`.
- Add legacy bridge for jobs in 2021.3 of `ByRef` job scheduling.

## [2.0.4] - 2024-06-19
### Fixed
- Fixed `ResizeArray` extension methods for `UnsafeArray<T>` and `NativeArray<T>`.
- Fixed `UnsafeBitList` realloc methods, and the `IsCreated` property able to return `true` when the array is not created.

## [2.0.3] - 2024-06-18
- This version is compatible with Flora 2.0.3.

## [2.0.2] - 2024-06-17
- This version is compatible with Flora 2.0.2.

## [2.0.1] - 2024-06-14
- This version is compatible with Flora 2.0.1.

## [2.0.0] - 2024-06-13
- This version is compatible with Flora 2.0.0.

## [1.1.9] - 2024-05-09
- This version is compatible with Flora 1.1.9.

## [1.1.8] - 2024-05-07
- This version is compatible with Flora 1.1.8.

## [1.1.7] - 2024-05-06
- This version is compatible with Flora 1.1.7.

## [1.1.6] - 2024-05-03
- This version is compatible with Flora 1.1.6.

## [1.1.5] - 2024-03-20
- This version is compatible with Flora 1.1.5.

## [1.1.3] - 2024-1-26
### Added
- Add `ColorUtility` class.

## [1.0.7] - 2023-09-16
### Added
- Add `UnsafeArray<T>` native container.
- Add `PinnedArrayView<T>` native container.

## [1.0.6] - 2023-08-28
### Added
- Add `UnsafeBitList` container.
### Changed
- Rename `ValueList<T>` -> `LeanList<T>`.

## [1.0.5] - 2023-08-23
### Fixed~~~~~~~~
- `ValueList<T>` `RemoveAtSwapBack` fix and other minor fixes.
### Changed
- Rename `Transform` -> `LocalTransform`.

## [1.0.4] - 2023-08-22
### Added
- Add `UIEditorUtility` for modifying `UnityEditor.UIElements` elements.
- Add `ArrayView<T>` struct.
- Add `ValueList<T>` class.
- Add `IVersionable` and Migration classes.
### Changed
- Rename namespace `MAEditor.Core` to `Ma.Core.Editor`.
- `UnsafeIndiectList` Optimizations.

## [1.0.3] - 2023-08-11
### Changed
- Rename `RuntimeGlobalObjectId` to `SerializableGlobalObjectId`.

## [1.0.2] - 2023-08-07
### Added
- Add `IReadOnlyList<T>` extensions for converting to `ReadOnlySpan<T>`.
- Add `NativeArrayView` extensions for converting to `IReadOnlyList<T>`.

## [1.0.1] - 2023-07-23
### Fixed
- Fix compiler errors when `Unity.Collections` version is >`2.0.0`.

## [1.0.0] - 2023-07-21
- Initial release
