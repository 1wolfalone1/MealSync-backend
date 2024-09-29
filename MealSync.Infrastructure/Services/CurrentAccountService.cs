using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Services;

public class CurrentAccountService : ICurrentAccountService, IBaseService
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IAccountRepository _accountRepository;

    public CurrentAccountService(ICurrentPrincipalService currentPrincipalService, IAccountRepository accountRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _accountRepository = accountRepository;
    }

    public Account GetCurrentAccount()
    {
        var curerntAccountEmail = _currentPrincipalService.CurrentPrincipal;
        return _accountRepository.GetAccountByEmail(curerntAccountEmail);
    }
}