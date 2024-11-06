using AutoMapper;
using MealSync.Application.UseCases.Wallets.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class WalletTransactionMapping : Profile
{
    public WalletTransactionMapping()
    {
        CreateMap<WalletTransaction, WalletTransactionResponse>()
            .ForMember(dest => dest.NameOfWalletOwnerFrom, opt => opt.MapFrom(src => NameOfWalletOwnerFrom(src)))
            .ForMember(dest => dest.NameOfWalletOwnerTo, opt => opt.MapFrom(src => NameOfWalletOwnerTo(src)));

    }

    private string NameOfWalletOwnerFrom(WalletTransaction wallet)
    {
        if (wallet.PaymentId.HasValue)
        {
            return $"Tiền từ đơn hàng";
        }

        if (wallet.WithdrawalRequestId.HasValue)
        {
            return "Trừ tiền từ yêu cầu rút tiền";
        }

        return string.Empty;
    }

    private string NameOfWalletOwnerTo(WalletTransaction wallet)
    {
        return "Bạn";
    }
}