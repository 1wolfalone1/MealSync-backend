using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Accounts.Commands.ShopRegister;

public class ShopRegisterHandler : ICommandHandler<ShopRegisterCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountRepository _accountRepository;
    private readonly IShopOwnerRepository _shopOwnerRepository;
    private readonly IDormitoryRepository _dormitoryRepository;
    private readonly IShopDormitoryRepository _shopDormitoryRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IOperatingDayRepository _operatingDayRepository;
    private readonly IMapper _mapper;

    public ShopRegisterHandler(IUnitOfWork unitOfWork, IAccountRepository accountRepository, IShopOwnerRepository shopOwnerRepository, IMapper mapper, IDormitoryRepository dormitoryRepository,
        IShopDormitoryRepository shopDormitoryRepository, ISystemResourceRepository systemResourceRepository, IOperatingDayRepository operatingDayRepository)
    {
        _unitOfWork = unitOfWork;
        _accountRepository = accountRepository;
        _shopOwnerRepository = shopOwnerRepository;
        _mapper = mapper;
        _dormitoryRepository = dormitoryRepository;
        _shopDormitoryRepository = shopDormitoryRepository;
        _systemResourceRepository = systemResourceRepository;
        _operatingDayRepository = operatingDayRepository;
    }

    public async Task<Result<Result>> Handle(ShopRegisterCommand request, CancellationToken cancellationToken)
    {
        // Validate Bussiness
        ValidateAccount(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var account = await CreateAccountAsync(request);
            var shopOwner = await CreateShopOwnerAsync(account.Id, request).ConfigureAwait(false);
            var shopDormitories = await CreateShopOwnerDormitoryAsync(shopOwner.Id, request.DormitoryIds).ConfigureAwait(false);
            await CreateOperatingDayAndFrameAsync(shopOwner.Id).ConfigureAwait(false);

            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            Console.WriteLine(e);
            throw;
        }

        return Result.Create(new RegisterResponse()
        {
            Email = request.Email,
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_REGISTER_SUCCESSFULLY.GetDescription()) ?? string.Empty,
        });
    }

    private void ValidateAccount(ShopRegisterCommand register)
    {
        var account = _accountRepository.GetAccountByEmail(register.Email);
        if (account != default)
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription(), HttpStatusCode.Conflict);

        account = _accountRepository.GetAccountByPhoneNumber(register.PhoneNumber);
        if (account != default)
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription(), HttpStatusCode.Conflict);

        foreach (var dormitoryId in register.DormitoryIds)
        {
            var dormitory = _dormitoryRepository.Get(d => d.Id == dormitoryId).SingleOrDefault();
            if (dormitory == default)
                throw new InvalidBusinessException(MessageCode.E_DORMITORY_NOT_FOUND.GetDescription(), HttpStatusCode.Conflict);
        }
    }

    private async Task<Account> CreateAccountAsync(ShopRegisterCommand register)
    {
        var account = new Account()
        {
            PhoneNumber = register.PhoneNumber,
            Email = register.Email,
            Password = BCrypUnitls.Hash(register.Password),
            AvatarUrl = _systemResourceRepository.GetByResourceCode(ResourceCode.ACCOUNT_AVATAR.GetDescription()) ?? string.Empty,
            FullName = register.FullName,
            Genders = register.Gender,
            RoleId = (int)Domain.Enums.Roles.ShopOwner,
            Type = AccountTypes.Local,
            Status = AccountStatus.UnVerify,
            RefreshToken = string.Empty,
        };

        await _accountRepository.AddAsync(account).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        return account;
    }

    private async Task<ShopOwner> CreateShopOwnerAsync(long shopOwnerId, ShopRegisterCommand register)
    {
        var location = new Location()
        {
            Address = register.Address,
            Latitude = register.Latitude,
            Longitude = register.Longitude,
        };

        var shopWallet = new Wallet();

        var shopOwner = new ShopOwner()
        {
            Id = shopOwnerId,
            Name = register.ShopName,
            PhoneNumber = register.PhoneNumber,
            LogoUrl = _systemResourceRepository.GetByResourceCode(ResourceCode.SHOP_LOGO.GetDescription()) ?? string.Empty,
            BannerUrl = _systemResourceRepository.GetByResourceCode(ResourceCode.SHOP_BANNER.GetDescription()) ?? string.Empty,
            Location = location,
            MaxOrderHoursInAdvance = ShopConfigurationConstant.MAX_ORDER_HOURS_IN_ADVANCE,
            MinOrderHoursInAdvance = ShopConfigurationConstant.MIN_ORDER_HOURS_IN_ADVANCE,
            AverageOrderHandleInFrame = ShopConfigurationConstant.AVERAGE_ORDER_HANDLE_IN_FRAME,
            AverageTotalOrderHandleInDay = ShopConfigurationConstant.AVERAGE_TOTAL_ORDER_HANDLE_IN_DAY,
            Wallet = shopWallet,
        };

        await _shopOwnerRepository.AddAsync(shopOwner).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        return shopOwner;
    }

    private async Task<List<ShopDormitory>> CreateShopOwnerDormitoryAsync(long shopId, long[] dormitoryIds)
    {
        var shopDormitories = dormitoryIds.Select(d => new ShopDormitory()
        {
            ShopOwnerId = shopId,
            DormitoryId = d,
        }).ToList();

        await _shopDormitoryRepository.AddRangeAsync(shopDormitories).ConfigureAwait(false);
        return shopDormitories;
    }

    private async Task CreateOperatingDayAndFrameAsync(long shopId)
    {
        var operatingDay = new OperatingDay()
        {
            ShopOwnerId = shopId,
        };

        var operatingDays = operatingDay.CreateListOperatingDayForNewShop();
        await this._operatingDayRepository.AddRangeAsync(operatingDays).ConfigureAwait(false);
    }
}