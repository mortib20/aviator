namespace Aviator.Main.Services;

public static class TimeRange
{
    public static bool TryParse(string? input, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = input.Trim().ToLowerInvariant();
        if (s.Length < 2) return false;

        var unit = s[^1];
        if (!int.TryParse(s[..^1], out var value) || value <= 0) return false;

        result = unit switch
        {
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            'w' => TimeSpan.FromDays(value * 7),
            _ => TimeSpan.Zero
        };

        return result != TimeSpan.Zero;
    }
}
