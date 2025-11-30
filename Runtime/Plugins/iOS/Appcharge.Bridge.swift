import Foundation
import ACCheckoutSDK

private let UNITY_CALLBACK_HANDLER = "ACCallbackHandler"
private var delegateRef: ACCheckoutPurchaseDelegate?

@_silgen_name("UnitySendMessage")
func UnitySendMessage(_ obj: UnsafePointer<CChar>, _ method: UnsafePointer<CChar>, _ msg: UnsafePointer<CChar>)

@_cdecl("acbridge_initialize")
public func acbridge_initialize(configJsonCString: UnsafePointer<CChar>, customerIdCString: UnsafePointer<CChar>) {
    let configJson = String(cString: configJsonCString)
    let customerId = String(cString: customerIdCString)

    do {
        let config = try JSONDecoder().decode(ACConfigModel.self, from: Data(configJson.utf8))
        ACBridgeAPI.initialize(configModel: config, customerId: customerId)
        let unityDelegate = ACUnityCheckoutDelegate()
        delegateRef = unityDelegate
        ACBridgeAPI.delegate = unityDelegate
    
    } catch {
        print("Failed to decode config JSON: \(error)")
    }
}

@_cdecl("acbridge_openCheckout")
public func acbridge_openCheckout(sessionTokenCString: UnsafePointer<CChar>, purchaseIdCString: UnsafePointer<CChar>, urlCString: UnsafePointer<CChar>) {
    let token = String(cString: sessionTokenCString)
    let purchaseId = String(cString: purchaseIdCString)
    let url = String(cString: urlCString)
    ACBridgeAPI.openCheckout(sessionToken: token, purchaseId: purchaseId, url: url)
}

@_cdecl("acbridge_handleDeepLink")
public func acbridge_handleDeepLink(urlCString: UnsafePointer<CChar>) {
    let urlString = String(cString: urlCString)
    if let url = URL(string: urlString) {
        ACBridgeAPI.handleDeepLink(url)
    }
}

@_cdecl("acbridge_getSdkVersion")
public func acbridge_getSdkVersion() -> UnsafeMutablePointer<CChar>? {
    return strdup(ACBridgeAPI.getSdkVersion())
}

@_cdecl("acbridge_freeCString")
public func acbridge_freeCString(ptr: UnsafeMutablePointer<CChar>?) {
    free(ptr)
}

@_cdecl("acbridge_setUseExternalBrowser")
public func acbridge_setUseExternalBrowser(useExternal: Bool) {
    ACBridgeAPI.useExternalBrowser = useExternal
}

@_cdecl("acbridge_setPortraitOrientationLock")
public func acbridge_setPortraitOrientationLock(portraitOrientationLock: Bool) {
    ACBridgeAPI.portraitOrientationLock = portraitOrientationLock
}

@_cdecl("acbridge_getPricePoints")
public func acbridge_getPricePoints() {
    ACBridgeAPI.getPricePoints()
}

@_cdecl("acbridge_openSubscriptionManager")
public func acbridge_openSubscriptionManager(urlCString: UnsafePointer<CChar>) {
    let url = String(cString: urlCString)
    ACBridgeAPI.openSubscriptionManager(url: url)
}

class ACUnityCheckoutDelegate: ACCheckoutPurchaseDelegate {
    func onInitialized() {
        UnitySendMessage(UNITY_CALLBACK_HANDLER, "OnInitialized", "")
    }

    func onInitializeFailed(error: ACErrorMessage) {
        do {
            let jsonData = try JSONEncoder().encode(error)
            if let jsonString = String(data: jsonData, encoding: .utf8) {
                UnitySendMessage(UNITY_CALLBACK_HANDLER, "OnInitializeFailed", jsonString)
            }
        } catch {
            print("[UnityBridge] Failed to encode error: \(error)")
        }
    }

    func onPurchaseSuccess(order: GameOrderResponse) {
        let jsonData = try! JSONEncoder().encode(order)
        let jsonString = String(data: jsonData, encoding: .utf8)!
        UnitySendMessage(UNITY_CALLBACK_HANDLER, "OnPurchaseSuccess", jsonString)
    }

    func onPurchaseFailed(error: ACErrorMessage) {
        let jsonData = try! JSONEncoder().encode(error)
        let jsonString = String(data: jsonData, encoding: .utf8)!
        UnitySendMessage(UNITY_CALLBACK_HANDLER, "OnPurchaseFailed", jsonString)
    }

    func onPricePointsSuccess(pricePoints: PricePoints) {
        do {
            let jsonData = try JSONEncoder().encode(pricePoints)
            if let jsonString = String(data: jsonData, encoding: .utf8) {
                UnitySendMessage(UNITY_CALLBACK_HANDLER, "OnPricePointsSuccess", jsonString)
            }
        } catch {
            print("[UnityBridge] Failed to encode pricePoints: \(error)")
        }
    }

    func onPricePointsFailed(errorMessage: ACErrorMessage) {
        do {
            let jsonData = try JSONEncoder().encode(errorMessage)
            if let jsonString = String(data: jsonData, encoding: .utf8) {
                UnitySendMessage(UNITY_CALLBACK_HANDLER, "OnPricePointsFail", jsonString)
            }
        } catch {
            print("[UnityBridge] Failed to encode error message: \(errorMessage.message)")
        }
    }

    func onAward(success: Bool) {
    }

    func onNoneAwardedSuccess(orderIds: [String]) {
    }

    func onNoneAwardedFailed(error: ACErrorMessage) {
    }
}
