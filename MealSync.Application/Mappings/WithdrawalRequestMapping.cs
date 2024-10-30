using AutoMapper;
using MealSync.Application.Common.Services.Notifications.Models;
using MealSync.Application.UseCases.Wallets.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class WithdrawalRequestMapping : Profile
{
    public WithdrawalRequestMapping()
    {
        CreateMap<WithdrawalRequest, WithdrawalRequestNotification>();

        CreateMap<WithdrawalRequest, WithdrawalRequestHistoryResponse>()
            .ForMember(dest => dest.WalletHistory, opt => opt.MapFrom(src => src));

        CreateMap<WithdrawalRequest, WithdrawalRequestHistoryResponse.WalletHistoryResponse>()
            .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.WalletId))
            .ForMember(dest => dest.AvaiableAmountBefore, opt => opt.MapFrom(src => src.WalletTransaction!.AvaiableAmountBefore))
            .ForMember(dest => dest.IncomingAmountBefore, opt => opt.MapFrom(src => src.WalletTransaction!.IncomingAmountBefore))
            .ForMember(dest => dest.ReportingAmountBefore, opt => opt.MapFrom(src => src.WalletTransaction!.ReportingAmountBefore));
    }
}