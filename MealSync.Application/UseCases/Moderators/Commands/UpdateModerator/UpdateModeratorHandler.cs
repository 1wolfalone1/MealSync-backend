using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Moderators.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Moderators.Commands.UpdateModerator;

public class UpdateModeratorHandler : ICommandHandler<UpdateModeratorCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateModeratorHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IDormitoryRepository _dormitoryRepository;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public UpdateModeratorHandler(IUnitOfWork unitOfWork, ILogger<UpdateModeratorHandler> logger, IAccountRepository accountRepository, IDormitoryRepository dormitoryRepository, IMapper mapper, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _accountRepository = accountRepository;
        _dormitoryRepository = dormitoryRepository;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<Result<Result>> Handle(UpdateModeratorCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var account = _accountRepository.Get(a => a.Id == request.Id)
            .Include(a => a.Moderator)
            .ThenInclude(a => a.ModeratorDormitories).Single();
        bool isChangeEmail = request.Email != account.Email;
        string oldEmail = account.Email;
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            var moderatorDormitories = new List<ModeratorDormitory>();
            foreach (var id in request.DormitoryIds)
            {
                moderatorDormitories.Add(new ModeratorDormitory()
                {
                    DormitoryId = id,
                });
            }

            account.Moderator.ModeratorDormitories = moderatorDormitories;
            account.Email = request.Email;
            account.PhoneNumber = request.PhoneNumber;
            account.FullName = request.FullName;
            account.Status = request.Status;

            _accountRepository.Update(account);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        if (isChangeEmail)
        {
            _emailService.SendChangeEmailAccountModerator(oldEmail, request.Email, account.FullName);
        }

        return Result.Success(_mapper.Map<ModeratorResponse>(account.Moderator));
    }

    private void Validate(UpdateModeratorCommand request)
    {
        var account = _accountRepository.GetById(request.Id);
        if (account == default || (account != default && account.RoleId != (int) Domain.Enums.Roles.Moderator))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_NOT_FOUND.GetDescription());
        }

        foreach (var id in request.DormitoryIds)
        {
            if (_dormitoryRepository.GetById(id) == null)
                throw new InvalidBusinessException(MessageCode.E_DORMITORY_NOT_FOUND.GetDescription(), new object[] { id });
        }

        if (account.Status == AccountStatus.UnVerify && account.RoleId != (int)Domain.Enums.Roles.Moderator)
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST_IN_ORTHER_ROLE.GetDescription(), HttpStatusCode.Conflict);

        if ( account.PhoneNumber != request.PhoneNumber && _accountRepository.CheckExistPhoneNumberInOtherEmailAccount(account.Email, request.PhoneNumber))
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription(), HttpStatusCode.Conflict);

        if (account.Email != request.Email)
        {
            if (_accountRepository.CheckExistEmailWhenUpdate(account.Id, request.Email))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription());
            }
        }
    }
}