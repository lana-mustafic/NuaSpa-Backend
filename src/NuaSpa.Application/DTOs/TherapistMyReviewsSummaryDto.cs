namespace NuaSpa.Application.DTOs;

/// <summary>All-time review stats for the logged-in therapist.</summary>
public class TherapistMyReviewsSummaryDto
{
    public int TotalCount { get; set; }
    public double AverageRating { get; set; }
    public string? MostReviewedServiceName { get; set; }
    public TherapistReviewRowDto? LatestReview { get; set; }
}
