using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.GetByIds;

public class GetByIdsForCartQuery : IQuery<Result>
{
    public long ShopId { get; set; }

    public List<DetailFoodQuery> Foods { get; set; }

    public long OperatingSlotId { get; set; }

    public class DetailFoodQuery
    {
        public string Id { get; set; }

        public List<OptionGroupRadioQuery>? OptionGroupRadio { get; set; }

        public List<OptionGroupCheckboxQuery>? OptionGroupCheckbox { get; set; }
    }

    public class OptionGroupRadioQuery
    {
        public long Id { get; set; }

        public long OptionId { get; set; }
    }

    public class OptionGroupCheckboxQuery
    {
        public long Id { get; set; }

        public long[] OptionIds { get; set; }
    }
}