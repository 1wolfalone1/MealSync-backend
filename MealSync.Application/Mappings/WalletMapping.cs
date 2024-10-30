using AutoMapper;
using MealSync.Application.UseCases.Wallets.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class WalletMapping : Profile
{
    public WalletMapping()
    {
        CreateMap<Wallet, WalletSummaryResponse>()
            .ForMember(dest => dest.IsAllowedRequestWithdrawal, opt => opt.MapFrom(src => src.AvailableAmount > 0));
    }
}