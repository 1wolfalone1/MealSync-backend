using System.Text.Json.Serialization;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reviews.Models;

public class ReviewForShopWebResponse
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; }

    public string ImageUrl { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string Entity { get; set; }

    public long ShopId { get; set; }

    public bool IsAllowShopReply { get; set; }

    public bool IsShopReplied { get; set; }

    [JsonIgnore]
    public int TotalCount { get; set; }

    public CustomerInReview Customer { get; set; }

    public class CustomerInReview
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string AvatarUrl { get; set; }
    }
}