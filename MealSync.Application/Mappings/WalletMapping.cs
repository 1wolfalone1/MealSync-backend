using AutoMapper;
using MealSync.Application.Common.Services.Notifications.Models;
using MealSync.Application.UseCases.Wallets.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Mappings;

public class WalletMapping : Profile
{
    public WalletMapping()
    {
        CreateMap<Wallet, WalletSummaryResponse>()
            .ForMember(dest => dest.IsAllowedRequestWithdrawal, opt => opt.MapFrom(src => src.AvailableAmount > 0 && !src.WithdrawalRequests.Any(w => w.Status == WithdrawalRequestStatus.Pending)));

        CreateMap<Wallet, WalletNotification>();
    }
}