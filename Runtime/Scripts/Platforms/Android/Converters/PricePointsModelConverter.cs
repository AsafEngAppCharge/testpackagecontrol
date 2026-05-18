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
					basePriceInUSD = ConvertUtility.GetSafeString(javaPricingPoint, "basePriceInUSD"),
					localizedPrice = ConvertUtility.GetSafeString(javaPricingPoint, "localizedPrice"),
					formattedPrice = ConvertUtility.GetSafeString(javaPricingPoint, "formattedPrice")
				};
			}
			return pricingPoints;
		}

		private static PricePointsDataModel GetPricePointsData(AndroidJavaObject javaPricePointsData)
		{
			if (javaPricePointsData == null) return null;
			return new PricePointsDataModel
			{
				currencyCode = ConvertUtility.GetSafeString(javaPricePointsData, "currencyCode"),
				currencySymbol = ConvertUtility.GetSafeString(javaPricePointsData, "currencySymbol"),
				fractionalSeparator = ConvertUtility.GetSafeString(javaPricePointsData, "fractionalSeparator"),
				milSeparator = ConvertUtility.GetSafeString(javaPricePointsData, "milSeparator"),
				symbolPosition = ConvertUtility.GetSafeString(javaPricePointsData, "symbolPosition"),
				spacing = ConvertUtility.GetSafeBool(javaPricePointsData, "spacing") ?? false,
				multiplier = ConvertUtility.GetSafeInt(javaPricePointsData, "multiplier") ?? 0
			};
		}
	}
}
