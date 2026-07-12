namespace JustTaskTracker.WebUI.Domain.Billing;

public static class PlanPriceFormatting
{
    public static string FormatAmount(long unitAmount, string currency)
    {
        var major = unitAmount / 100m;

        return currency.ToUpperInvariant() switch
        {
            "USD" => $"${major:0.##}",
            "EUR" => $"€{major:0.##}",
            _ => $"{major:0.##} {currency.ToUpperInvariant()}",
        };
    }

    public static string FormatInterval(string interval) =>
        $" / {interval}";

    public static string FormatFreeAmount() => "0";

    public static string FormatFreeInterval() => " / month";
}
