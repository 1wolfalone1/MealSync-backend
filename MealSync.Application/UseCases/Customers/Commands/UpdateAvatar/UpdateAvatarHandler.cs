using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Customers.Queries.GetCustomerInfo;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Customers.Commands.UpdateAvatar;

public class UpdateAvatarHandler : ICommandHandler<UpdateAvatarCommand, Result>
{
    private readonly IStorageService _storageService;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAvatarHandler> _logger;
    private readonly IMediator _mediator;

    public UpdateAvatarHandler(
        IStorageService storageService, IAccountRepository accountRepository,
        ICurrentPrincipalService currentPrincipalService, IUnitOfWork unitOfWork,
        ILogger<UpdateAvatarHandler> logger, IMediator mediator
    )
    {
        _storageService = storageService;
        _accountRepository = accountRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<Result<Result>> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var url = await _storageService.UploadFileAsync(request.File).ConfigureAwait(false);
        var account = _accountRepository.GetById(accountId);
        account!.AvatarUrl = url;

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            _accountRepository.Update(account);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
        var result = await _mediator.Send(new GetCustomerInfoQuery()).ConfigureAwait(false);
        return Result.Success(result.Value);
    }
}