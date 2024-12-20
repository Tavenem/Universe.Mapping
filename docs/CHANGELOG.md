# Changelog

## 1.3
### Changed
- Update to .NET 9
### Updated
- Update dependencies

## 1.2
### Updated
- Update dependencies

## 1.1
### Updated
- Update dependencies

## 1.0
### Added
- Initial production release

## 0.7.0-preview
### Added
- Source generated (de)serialization support
### Changed
- Update to .NET 8 preview
### Removed
- Support for non-JSON serialization

## 0.6.2-preview
### Changed
- Update to .NET 7

## 0.6.1-preview
### Fixed
- Weather mapping

## 0.6.0-preview
### Changed
- Update to .NET 7 preview

## 0.5.1-preview - 0.5.2-preview
### Updated
- Update dependencies

## 0.5.0-preview
### Changed
- Update to .NET 6 preview
- Update to C# 10 preview

## 0.4.1-preview
### Updated
- Update dependencies

## 0.4.0-preview
### Added
- Projection-agnostic methods for `SurfaceRegionMap`
### Removed
- Projection-specific methods from `SurfaceMap` and `SurfaceRegionMap`

## 0.3.0-preview
### Added
- Projection-agnostic latitude-longitude method for `SurfaceRegionMap`
- Cartesian coordinate temperature method for `SurfaceMapImage`
### Changed
- Made some internal temperature methods public
### Removed
- Projection-specific extension methods from `SurfaceRegionMap`

## 0.2.0-preview
### Added
- Make some internal methods public
### Changed
- Move some extension methods with no reference to the extended class to the `SurfaceMapImage`
  static class

## 0.1.0-preview
### Added
- Initial preview release