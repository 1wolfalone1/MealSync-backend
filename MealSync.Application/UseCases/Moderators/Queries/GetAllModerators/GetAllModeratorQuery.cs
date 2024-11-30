using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Moderators.Queries.GetAllModerators
{
    public class GetAllModeratorQuery : PaginationRequest, IQuery<Result>
    {
        public string? SearchValue { get; set; }

        public AccountStatus Status { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public long? DormitoryId { get; set; }
    }
}