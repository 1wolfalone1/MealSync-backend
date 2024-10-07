using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("account_flag")]
public class AccountFlag : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long AccountId { get; set; }

    public AccountActionTypes ActionType { get; set; }

    public AccountTargetTypes TargetType { get; set; }

    public string TargetId { get; set; }

    [Column(TypeName = "text")]
    public string Description { get; set; }

    public virtual Account Account { get; set; }

    public AccountFlag()
    {
    }

    public AccountFlag(AccountActionTypes actionType, long accountId, object? data = null)
    {
        AccountId = accountId;
        ActionType = actionType;

        switch (actionType)
        {
            case AccountActionTypes.CancelConfirmOrder:
            {
                TargetId = string.Empty;
                Description = "Bạn bị đánh cờ vì đạt 5 lần cảnh báo từ sàn";
                break;
            }

            default:
            {
                Description = ActionType.ToString();
                break;
            }
        }
    }
}