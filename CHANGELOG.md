## [2.5.2] - 2026-05-24

### Added
- Paramater `orderResponseModel` to the `onPurchaseFailed` callback, to access full order details on failed purchases.
- Error codes: 1007, 2001, 2002, 2003, 2004, 3006, 3007, 8001, 9001.

### Updated
- Android platform version to v1.6.0.
- iOS platform version to v1.8.0.
- Bundled Android foreground service type from `dataSync` to `shortService`.
- Error codes with clearer, more specific errors that better match current SDK behavior.

### Removed
- Exclude Add Framework Search Paths entitlement (SPM migration; the iOS XCFramework is now already included).
- Error codes: 1002, 1006, 3001, 4001, 4003, 9000.

## [2.5.1] - 2026-04-19

### Updated
- Dummy release tester A.

## [2.5.0] - 2026-04-19

### Updated
- Dummy release tester.

## [2.4.0] - 2026-02-26
### Changed
- Sample Scene overhaul.

### Updated
- iOS Native SDK 1.5.0.
- Android Native SDK 1.7.0.
- Improved Android automatic post-process script.

### Added
- Added Configuration support for Checkout Foreground Service.
