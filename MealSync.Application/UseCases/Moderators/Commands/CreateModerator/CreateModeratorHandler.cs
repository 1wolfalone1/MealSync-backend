using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Moderators.Commands.CreateModerator;

public class CreateModeratorHandler : ICommandHandler<CreateModeratorCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IModeratorRepository _moderatorRepository;
    private readonly ILogger<CreateModeratorHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IDormitoryRepository _dormitoryRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IEmailService _emailService;

    public CreateModeratorHandler(IUnitOfWork unitOfWork, IModeratorRepository moderatorRepository, ILogger<CreateModeratorHandler> logger, IAccountRepository accountRepository, IModeratorDormitoryRepository moderatorDormitoryRepository, ISystemResourceRepository systemResourceRepository, IEmailService emailService, IDormitoryRepository dormitoryRepository)
    {
        _unitOfWork = unitOfWork;
        _moderatorRepository = moderatorRepository;
        _logger = logger;
        _accountRepository = accountRepository;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _systemResourceRepository = systemResourceRepository;
        _emailService = emailService;
        _dormitoryRepository = dormitoryRepository;
    }

    public async Task<Result<Result>> Handle(CreateModeratorCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var password = GeneratePassword();

        var accountTemp = _accountRepository.GetAccountByEmail(request.Email);

        // If account exist
        if (accountTemp != default && accountTemp.Status == AccountStatus.UnVerify && accountTemp.RoleId == (int)Domain.Enums.Roles.Moderator)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                accountTemp.PhoneNumber = request.PhoneNumber;
                accountTemp.Password = password;
                accountTemp.Status = request.Status;
                var moderatorDormitories = new List<ModeratorDormitory>();
                foreach (var id in request.DormitoryIds)
                {
                    moderatorDormitories.Add(new ModeratorDormitory()
                    {
                        DormitoryId = id,
                    });
                }

                var moderator = _moderatorRepository.Get(md => md.Id == accountTemp.Id)
                    .Include(md => md.ModeratorDormitories).SingleOrDefault();
                if (moderator != null)
                {
                    moderator.ModeratorDormitories = moderatorDormitories;
                }

                _accountRepository.Update(accountTemp);
                _moderatorRepository.Update(moderator);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw;
            }

            _emailService.SendCreatedAccountModerator(request.Email, request.FullName, request.Email, password);
            return Result.Success(new
            {
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.E_ACCOUNT_MODERATOR_CREATED.GetDescription()),
                Code = MessageCode.E_ACCOUNT_MODERATOR_CREATED.GetDescription(),
            });
        }
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

            var moderator = new Moderator();
            moderator.ModeratorDormitories = moderatorDormitories;
            var account = new Account()
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Password = BCrypUnitls.Hash(password),
                Genders = Genders.UnKnown,
                AvatarUrl = _systemResourceRepository.GetByResourceCode(ResourceCode.ACCOUNT_AVATAR.GetDescription()),
                Type = AccountTypes.Local,
                Status = request.Status,
                RoleId = (int)Domain.Enums.Roles.Moderator,
            };
            account.Moderator = moderator;

            await _accountRepository.AddAsync(account).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        _emailService.SendCreatedAccountModerator(request.Email, request.FullName, request.Email, password);
        return Result.Success(new
        {
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.E_ACCOUNT_MODERATOR_CREATED.GetDescription()),
            Code = MessageCode.E_ACCOUNT_MODERATOR_CREATED.GetDescription(),
        });
    }

    public static string GeneratePassword(int length = 8)
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialCharacters = "!@#$%^&*()-_=+[]{}|;:,.<>?";

        if (length < 8)
            throw new ArgumentException("Password length must be at least 8 characters.");

        var password = new char[length];

        Random _random = new Random();
        // Ensure at least one character from each category
        password[0] = uppercase[_random.Next(uppercase.Length)];
        password[1] = lowercase[_random.Next(lowercase.Length)];
        password[2] = digits[_random.Next(digits.Length)];
        password[3] = specialCharacters[_random.Next(specialCharacters.Length)];

        // Fill the rest of the password with a mix of all characters
        const string allCharacters = uppercase + lowercase + digits + specialCharacters;
        for (int i = 4; i < length; i++)
        {
            password[i] = allCharacters[_random.Next(allCharacters.Length)];
        }

        // Shuffle the password to randomize character positions
        return new string(password.OrderBy(_ => _random.Next()).ToArray());
    }

    private void Validate(CreateModeratorCommand request)
    {
        foreach (var id in request.DormitoryIds)
        {
            if (_dormitoryRepository.GetById(id) == null)
                throw new InvalidBusinessException(MessageCode.E_DORMITORY_NOT_FOUND.GetDescription(), new object[] { id });
        }

        var account = _accountRepository.GetAccountByEmail(request.Email);
        if (account != default && account.Status != AccountStatus.UnVerify)
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription(), HttpStatusCode.Conflict);

        if (account != default && account.Status == AccountStatus.UnVerify && account.RoleId != (int)Domain.Enums.Roles.Moderator)
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST_IN_ORTHER_ROLE.GetDescription(), HttpStatusCode.Conflict);

        if (account != default && account.PhoneNumber != request.PhoneNumber && _accountRepository.CheckExistByPhoneNumber(request.PhoneNumber))
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription(), HttpStatusCode.Conflict);
    }
}