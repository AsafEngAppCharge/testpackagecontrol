using System;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android
{
	public static class OrderResponseModelConverter
	{
		public static OrderResponseModel ToOrderResponseModel(AndroidJavaObject javaOrderResponse)
		{
			if (javaOrderResponse == null) return null;

			long date = ConvertUtility.GetSafeLong(javaOrderResponse, "date") ?? 0L;

			int price = ConvertUtility.GetSafeInt(javaOrderResponse, "price") ?? 0;

			return new OrderResponseModel
			{
				date = date,
				sessionToken = ConvertUtility.GetSafeString(javaOrderResponse, "sessionToken") ?? string.Empty,
				offerName = ConvertUtility.GetSafeString(javaOrderResponse, "offerName") ?? string.Empty,
				offerSku = ConvertUtility.GetSafeString(javaOrderResponse, "offerSku") ?? string.Empty,
				items = GetItems(javaOrderResponse.Get<AndroidJavaObject>("items")),
				price = price,
				currency = ConvertUtility.GetSafeString(javaOrderResponse, "currency") ?? string.Empty,
				customerId = ConvertUtility.GetSafeString(javaOrderResponse, "customerId") ?? string.Empty,
				customerCountry = ConvertUtility.GetSafeString(javaOrderResponse, "customerCountry") ?? string.Empty,
				paymentMethodName = ConvertUtility.GetSafeString(javaOrderResponse, "paymentMethodName") ?? string.Empty,
				orderId = ConvertUtility.GetSafeString(javaOrderResponse, "orderId") ?? string.Empty,
				purchaseId = ConvertUtility.GetSafeString(javaOrderResponse, "purchaseId") ?? string.Empty
			};
		}

		private static ProductModel[] GetItems(AndroidJavaObject javaItemsList)
		{
			if (javaItemsList == null) return Array.Empty<ProductModel>();

			int size = 0;
			try { size = javaItemsList.Call<int>("size"); }
			catch { return Array.Empty<ProductModel>(); }

			var items = new ProductModel[size];
			for (int i = 0; i < size; i++)
			{
				AndroidJavaObject javaProduct = null;
				try { javaProduct = javaItemsList.Call<AndroidJavaObject>("get", i); }
				catch { }

				if (javaProduct == null)
				{
					items[i] = new ProductModel { name = string.Empty, sku = string.Empty, amount = 0.ToString() };
					continue;
				}

				var name = ConvertUtility.GetSafeString(javaProduct, "name") ?? string.Empty;
				var sku = ConvertUtility.GetSafeString(javaProduct, "sku") ?? string.Empty;
				int quantity = ConvertUtility.GetSafeInt(javaProduct, "quantity") ?? 0;

				items[i] = new ProductModel
				{
					name = name,
					sku = sku,
					amount = quantity.ToString()
				};
			}

			return items;
		}
	}
}
