using System;

namespace KylaPcRental.Client.Services;

public sealed class RentalSession
{
    public const decimal HourlyRate = 40m;
    public TimeSpan Remaining { get; private set; }
    public bool IsActive => Remaining > TimeSpan.Zero;
    public bool IsExpired => Remaining <= TimeSpan.Zero;

    public void Start(int hours)
    {
        if (hours is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(hours));
        Remaining = TimeSpan.FromHours(hours);
    }

    public void AddHours(int hours)
    {
        if (hours is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(hours));
        Remaining += TimeSpan.FromHours(hours);
    }

    public void Tick(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero) return;
        Remaining = Remaining - elapsed;
        if (Remaining < TimeSpan.Zero) Remaining = TimeSpan.Zero;
    }

    public static decimal PriceForHours(int hours) => hours * HourlyRate;
}
