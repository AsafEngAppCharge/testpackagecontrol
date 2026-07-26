//
//  ACBridgeAPI_Bridge.h
//  ACPaymentLinks
//

#ifndef ACBridgeAPI_Bridge_h
#define ACBridgeAPI_Bridge_h

#pragma once

#ifdef __cplusplus
extern "C" {
#endif

typedef enum ac_browser_mode_t {
    ac_browser_mode_default = -1,
    ac_browser_mode_internal = 0,
    ac_browser_mode_external = 1
} ac_browser_mode_t;

/// ---------- DELEGATE CALLBACKS ----------
/// All const char* arguments are UTF-8.
/// They are only valid during the callback. Copy on the managed side if needed.

// Called when SDK initialized successfully.
typedef void(*ac_on_initialized_cb)();

// General error callback type for failures (initialize).
typedef void(*ac_on_error_cb)(const char* errorMessage);

// Success payloads as strings (you can treat as JSON or description).
typedef void(*ac_on_purchase_success_cb)(const char* order);
typedef void(*ac_on_purchase_failed_cb)(const char* errorMessage, const char* order);
typedef void(*ac_on_purchase_canceled_cb)(const char* errorMessage, const char* order);

/// Initializes the SDK.
/// @note Call this before any other SDK function.
void ac_initialize(const char* publicKey,
                   const char* environment,
                   const char* redirectUrl,
                   ac_on_initialized_cb            onInitialized,
                   ac_on_error_cb                  onInitializeFailed,
                   ac_on_purchase_success_cb       onPurchaseSuccess,
                   ac_on_purchase_failed_cb        onPurchaseFailed,
                   ac_on_purchase_canceled_cb      onPurchaseCanceled);

/// Sets browser mode for checkout flow.
/// mode: ac_browser_mode_default / ac_browser_mode_sfsvc / ac_browser_mode_external
void ac_set_browser_mode(ac_browser_mode_t mode);

/// Sets portraitOrientationLock (nullable bool).
/// value: -1 = nil, 0 = false, 1 = true
void ac_set_portrait_orientation_lock(int value);

/// Sets isDebugModeEnabled (nullable bool).
/// value: -1 = nil, 0 = false, 1 = true
void ac_set_debug_mode_enabled(int value);

/// Opens the checkout flow.
void ac_open_checkout(const char* purchaseId,
                      const char* parsedUrl,
                      const char* customerId);

/// Handles a deep link URL string.
void ac_handle_deep_link(const char* url);

/// Returns a heap-allocated UTF-8 string with the SDK version.
/// MUST be freed by calling ac_free_string().
const char* ac_get_sdk_version(void);

/// Frees strings returned by this bridge (e.g., ac_get_sdk_version()).
void ac_free_string(const char* str);

/// Registers callbacks that mirror ACPaymentLinksDelegate.
/// Any of the function pointers may be NULL if you don't care about that event.
///
/// Call ac_set_delegate(...) before ac_initialize(...) if you want to receive
/// initialization callbacks as well.
void ac_set_delegate(ac_on_initialized_cb onInitialized,
                     ac_on_error_cb onInitializeFailed,
                     ac_on_purchase_success_cb onPurchaseSuccess,
                     ac_on_purchase_failed_cb onPurchaseFailed,
                     ac_on_purchase_canceled_cb onPurchaseCanceled);


#ifdef __cplusplus
} // extern "C"
#endif

#endif /* ACBridgeAPI_Bridge_h */
