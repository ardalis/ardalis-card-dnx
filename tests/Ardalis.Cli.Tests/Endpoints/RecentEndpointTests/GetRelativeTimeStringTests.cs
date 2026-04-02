using System;
using System.Reflection;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.RecentEndpointTests;

/// <summary>
/// Tests for RecentEndpoint.RecentActivity.GetRelativeTimeString() — CRAP 210,
/// cyclomatic complexity 14. RecentActivity is a private sealed class, so we access
/// it via reflection following the pattern used in ExtractVideoIdTests. The method
/// converts a DateTime to a human-readable relative string used in the table display.
/// </summary>
public class GetRelativeTimeStringTests
{
    private static readonly Type RecentActivityType =
        typeof(RecentEndpoint)
            .GetNestedType("RecentActivity", BindingFlags.NonPublic)
        ?? throw new MissingMemberException("RecentEndpoint", "RecentActivity");

    private static readonly MethodInfo GetRelativeTimeStringMethod =
        RecentActivityType.GetMethod("GetRelativeTimeString", BindingFlags.Public | BindingFlags.Instance)
        ?? throw new MissingMethodException("RecentActivity", "GetRelativeTimeString");

    private static readonly PropertyInfo DateProperty =
        RecentActivityType.GetProperty("Date", BindingFlags.Public | BindingFlags.Instance)
        ?? throw new MissingMemberException("RecentActivity", "Date");

    private static string Invoke(DateTime date)
    {
        object activity = Activator.CreateInstance(RecentActivityType)!;
        DateProperty.SetValue(activity, date);
        return (string)GetRelativeTimeStringMethod.Invoke(activity, null)!;
    }

    [Test]
    public async Task ReturnsFormattedDate_WhenMoreThanTwoDaysOld()
    {
        DateTime oldDate = DateTime.UtcNow.AddDays(-7);

        string result = Invoke(oldDate);

        // Should match "d MMM yyyy" pattern (e.g., "25 Mar 2026")
        await Assert.That(result).IsNotEmpty();
        await Assert.That(result).Contains(oldDate.Year.ToString());
    }

    [Test]
    public async Task ReturnsJustNow_WhenUnderOneMinuteOld()
    {
        DateTime now = DateTime.UtcNow;

        string result = Invoke(now);

        await Assert.That(result).IsEqualTo("just now");
    }

    [Test]
    public async Task ReturnsMinutesAgo_WhenBetweenOneAndSixtyMinutesOld()
    {
        DateTime thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);

        string result = Invoke(thirtyMinutesAgo);

        await Assert.That(result).IsEqualTo("30 min ago");
    }

    [Test]
    public async Task ReturnsOneHourAgo_WhenOneHourOld()
    {
        DateTime oneHourAgo = DateTime.UtcNow.AddHours(-1);

        string result = Invoke(oneHourAgo);

        await Assert.That(result).IsEqualTo("1 hour ago");
    }

    [Test]
    public async Task ReturnsHoursAgo_WhenSeveralHoursOld()
    {
        DateTime fiveHoursAgo = DateTime.UtcNow.AddHours(-5);

        string result = Invoke(fiveHoursAgo);

        await Assert.That(result).IsEqualTo("5 hours ago");
    }

    [Test]
    public async Task ReturnsOneDayAgo_WhenOneDayOld()
    {
        // Use 26 hours to stay safely within the "1 day" bucket
        DateTime yesterday = DateTime.UtcNow.AddHours(-26);

        string result = Invoke(yesterday);

        await Assert.That(result).IsEqualTo("1 day ago");
    }

    [Test]
    public async Task ConvertesLocalTimeToUtc_BeforeComparison()
    {
        // Date with Local kind — the branch converts via ToUniversalTime()
        DateTime localTime = DateTime.Now.AddMinutes(-45);

        string result = Invoke(localTime);

        // 45 min ago regardless of timezone
        await Assert.That(result).IsEqualTo("45 min ago");
    }
}
