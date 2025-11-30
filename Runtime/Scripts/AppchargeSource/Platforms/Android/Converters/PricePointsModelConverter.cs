using System;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android {
	public static class PricePointsModelConverter
	{
		public static PricePointsModel ToPricePointsModel(AndroidJavaObject javaPricePoints)
		{
			if (javaPricePoints == null) return null;
			return new PricePointsModel
			{
				pricingPoints = GetPricingPoints(javaPricePoints.Get<AndroidJavaObject>("pricingPoints")),
				pricingPointData = GetPricePointsData(javaPricePoints.Get<AndroidJavaObject>("pricingPointData"))
			};
		}

		private static PricingPointsModel[] GetPricingPoints(AndroidJavaObject javaPricingPointsList)
		{
			if (javaPricingPointsList == null) return new PricingPointsModel[0];
			int size = javaPricingPointsList.Call<int>("size");
			var pricingPoints = new PricingPointsModel[size];
			for (int i = 0; i < size; i++)
			{
				var javaPricingPoint = javaPricingPointsList.Call<AndroidJavaObject>("get", i);
				pricingPoints[i] = new PricingPointsModel
				{
					basePriceInUSD = GetSafeString(javaPricingPoint, "basePriceInUSD"),
					localizedPrice = GetSafeString(javaPricingPoint, "localizedPrice"),
					formattedPrice = GetSafeString(javaPricingPoint, "formattedPrice")
				};
			}
			return pricingPoints;
		}

		private static PricePointsDataModel GetPricePointsData(AndroidJavaObject javaPricePointsData)
		{
			if (javaPricePointsData == null) return null;
			return new PricePointsDataModel
			{
				currencyCode = GetSafeString(javaPricePointsData, "currencyCode"),
				currencySymbol = GetSafeString(javaPricePointsData, "currencySymbol"),
				fractionalSeparator = GetSafeString(javaPricePointsData, "fractionalSeparator"),
				milSeparator = GetSafeString(javaPricePointsData, "milSeparator"),
				symbolPosition = GetSafeString(javaPricePointsData, "symbolPosition"),
				spacing = GetSafeBool(javaPricePointsData, "spacing") ?? false,
				multiplier = GetSafeInt(javaPricePointsData, "multiplier") ?? 0
			};
		}

		private static string GetSafeString(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<string>(fieldName); } catch (Exception) { return null; }
		}

		private static bool? GetSafeBool(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<bool>(fieldName); } catch (Exception) { return null; }
		}

		private static int? GetSafeInt(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<int>(fieldName); } catch (Exception) { return null; }
		}
	}
}