using System;
using ChronoDesk.Domain.Entities;
using Xunit;

namespace ChronoDesk.Tests.Domain;

public class TimeEntryTests
{
    [Fact]
    public void Duration_ReturnsDifference_WhenEndTimeSet()
    {
        // Arrange
        var start = DateTime.UtcNow.AddHours(-2);
        var end = DateTime.UtcNow;
        var entry = new TimeEntry
        {
            StartTime = start,
            EndTime = end
        };

        // Act
        var duration = entry.Duration;

        // Assert
        Assert.Equal(end - start, duration);
    }

    [Fact]
    public void Duration_ReturnsCurrentDifference_WhenEndTimeNull()
    {
        // Arrange
        var start = DateTime.UtcNow.AddMinutes(-30);
        var entry = new TimeEntry
        {
            StartTime = start,
            EndTime = null
        };

        // Act
        var duration = entry.Duration;

        // Assert
        // Duration should be roughly 30 mins
        Assert.True(duration.TotalMinutes >= 29.9 && duration.TotalMinutes <= 30.1); 
    }
}
