using AutoMapper;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class AccountInforInChatRepsonseMapping : Profile
{
    public AccountInforInChatRepsonseMapping()
    {
        CreateMap<Account, AccountInforInChatRepsonse>();
    }
}