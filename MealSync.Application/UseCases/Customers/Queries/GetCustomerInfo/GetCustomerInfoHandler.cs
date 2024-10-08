using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Customers.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Customers.Queries.GetCustomerInfo;

public class GetCustomerInfoHandler : IQueryHandler<GetCustomerInfoQuery, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetCustomerInfoHandler(IAccountRepository accountRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetCustomerInfoQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var customer = _accountRepository.GetCustomerById(accountId);
        if (customer == default)
        {
            throw new InvalidBusinessException(MessageCode.E_CUSTOMER_NOT_FOUND.GetDescription(), new object[] { accountId });
        }
        else
        {
            return Result.Success(_mapper.Map<CustomerInfoResponse>(customer));
        }
    }
}