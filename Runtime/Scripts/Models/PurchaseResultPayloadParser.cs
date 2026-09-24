using System;
using UnityEngine;

namespace Appcharge.PaymentLinks.Models
{
    [Serializable]
    public class PurchaseResultPayload
    {
        public string errorJson;
        public string orderJson;
    }

    public static class PurchaseResultPayloadParser
    {
        public static (ErrorMessage error, OrderResponseModel order) Parse(string payloadJson)
        {
            if (TryParseLegacyErrorCode(payloadJson, out int legacyCode))
            {
                return (CreateLegacyError(legacyCode), null);
            }

            var payload = JsonUtility.FromJson<PurchaseResultPayload>(payloadJson);
            if (payload == null || string.IsNullOrEmpty(payload.errorJson))
            {
                return (new ErrorMessage { code = 0, message = "Invalid purchase result payload" }, null);
            }

            var error = JsonUtility.FromJson<ErrorMessage>(payload.errorJson);
            OrderResponseModel order = null;
            if (!string.IsNullOrEmpty(payload.orderJson) && payload.orderJson != "null")
            {
                order = JsonUtility.FromJson<OrderResponseModel>(payload.orderJson);
            }

            return (error, order);
        }

        public static bool TryParseLegacyErrorCode(string payloadJson, out int code)
        {
            code = 0;
            if (string.IsNullOrEmpty(payloadJson))
            {
                return false;
            }

            var trimmed = payloadJson.Trim();
            if (trimmed.StartsWith("{"))
            {
                return false;
            }

            return int.TryParse(trimmed, out code);
        }

        private static ErrorMessage CreateLegacyError(int code)
        {
            return new ErrorMessage
            {
                code = code,
                message = "OnPurchaseFailed"
            };
        }
    }
}
