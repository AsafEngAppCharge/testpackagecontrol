namespace Appcharge.PaymentLinks.Platforms.WebGL
{
    /// <summary>
    /// Builds the WebGL core script URL. Host env segment and core version are the only varying parts.
    /// </summary>
    internal static class WebGLCoreData
    {
        /// <summary>Core script path version (independent of Unity SDK version).</summary>
        private const string CoreVersion = "3-0-0";

        private const string UrlTemplate =
            "https://sdk-configurations.{0}.appcharge-int.com/checkout/webgl/{1}/sdk/unity-webgl-core.js";

        public static string Resolve(string environment)
        {
            return string.Format(UrlTemplate, ResolveHostSegment(environment), CoreVersion);
        }

        private static string ResolveHostSegment(string environment)
        {
            if (string.IsNullOrEmpty(environment))
            {
                return "sandbox";
            }

            switch (environment.Trim().ToLowerInvariant())
            {
                case "production":
                case "prod":
                    return "prod";
                case "staging":
                    return "staging";
                case "sandbox":
                default:
                    return "sandbox";
            }
        }
    }
}
