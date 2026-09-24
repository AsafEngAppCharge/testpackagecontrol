namespace Appcharge.PaymentLinks.Platforms.WebGL
{
    internal static class ParsedUrlSessionTokenExtractor
    {
        public static string Extract(string parsedUrl)
        {
            if (string.IsNullOrEmpty(parsedUrl))
            {
                return string.Empty;
            }

            int bootFragmentIndex = parsedUrl.IndexOf("#boot");
            if (bootFragmentIndex == -1)
            {
                int lastSlashIndex = parsedUrl.LastIndexOf('/');
                if (lastSlashIndex == -1 || lastSlashIndex == parsedUrl.Length - 1)
                {
                    return string.Empty;
                }

                int queryOrFragmentIndex = parsedUrl.IndexOfAny(new char[] { '?', '#' }, lastSlashIndex + 1);
                if (queryOrFragmentIndex == -1)
                {
                    return parsedUrl.Substring(lastSlashIndex + 1);
                }

                return parsedUrl.Substring(lastSlashIndex + 1, queryOrFragmentIndex - lastSlashIndex - 1);
            }

            int lastSlashBeforeBoot = parsedUrl.LastIndexOf('/', bootFragmentIndex);
            if (lastSlashBeforeBoot == -1 || lastSlashBeforeBoot >= bootFragmentIndex - 1)
            {
                return string.Empty;
            }

            return parsedUrl.Substring(lastSlashBeforeBoot + 1, bootFragmentIndex - lastSlashBeforeBoot - 1);
        }
    }
}
