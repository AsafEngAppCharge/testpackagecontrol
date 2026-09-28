## [3.0.0] - 2026-09-27

### Added
- Unity Thread Dispatcher option.
- `OnPurchaseCanceled` callback on the purchase interface.
- Restore order information on failed or canceled purchases when returning via deeplink.

### Updated
- Updated Android SDK to v2.0.0.
- Updated iOS SDK to v2.0.0.
- Improved WebGL support.
- Improved deep-link performance.
- Improved Android Post-Process support.

### Changed
- Initialization no longer relies on `customerId`.
- `customerId` is now mandatory when opening the checkout.
- Removed requirement for `buildConfig = true` on Android.

### Removed
- Price points SDK calls.