using Appcharge.PaymentLinks.Models;

namespace Appcharge.PaymentLinks.Platforms.WebGL
{
    public class WebGLCheckoutData
    {
        public string PurchaseId { get; private set; } = string.Empty;
        public string ParsedUrl { get; private set; } = string.Empty;
        public string CustomerId { get; private set; } = string.Empty;

        public void Set(string purchaseId, string parsedUrl, string customerId)
        {
            PurchaseId = purchaseId ?? string.Empty;
            ParsedUrl = parsedUrl ?? string.Empty;
            CustomerId = customerId ?? string.Empty;
        }

        public OrderResponseModel Enrich(OrderResponseModel order)
        {
            if (order == null)
            {
                order = new OrderResponseModel();
            }

            if (string.IsNullOrEmpty(order.purchaseId) && !string.IsNullOrEmpty(PurchaseId))
            {
                order.purchaseId = PurchaseId;
            }

            if (string.IsNullOrEmpty(order.customerId) && !string.IsNullOrEmpty(CustomerId))
            {
                order.customerId = CustomerId;
            }

            if (string.IsNullOrEmpty(order.sessionToken) && !string.IsNullOrEmpty(ParsedUrl))
            {
                order.sessionToken = ParsedUrlSessionTokenExtractor.Extract(ParsedUrl);
            }

            return order;
        }
    }
}
