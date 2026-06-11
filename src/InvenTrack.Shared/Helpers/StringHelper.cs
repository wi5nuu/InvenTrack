using System.Text.RegularExpressions;

namespace InvenTrack.Shared.Helpers;

/// <summary>String validation and manipulation helpers.</summary>
public static partial class StringHelper
{
    public static bool IsValidSku(string sku)
    {
        return SkuRegex().IsMatch(sku);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9\-]{1,50}$")]
    private static partial Regex SkuRegex();
}
