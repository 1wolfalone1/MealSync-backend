namespace MealSync.Application.UseCases.Reviews.Models;

public class ReviewOverviewDto
{
    public int TotalReview { get; set; }

    public double RatingAverage { get; set; }

    public int TotalOneStar { get; set; }

    public int TotalTwoStar { get; set; }

    public int TotalThreeStar { get; set; }

    public int TotalFourStar { get; set; }

    public int TotalFiveStar { get; set; }
}