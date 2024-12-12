using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Map;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Ocsp;

namespace MealSync.Application.UseCases.Accounts.Commands.ShopRegister;

public class ShopRegisterHandler : ICommandHandler<ShopRegisterCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountRepository _accountRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IDormitoryRepository _dormitoryRepository;
    private readonly IShopDormitoryRepository _shopDormitoryRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly ILogger<ShopRegisterHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly IMapApiService _mapApiService;

    public ShopRegisterHandler(IUnitOfWork unitOfWork, IAccountRepository accountRepository, IShopRepository shopRepository
        , IMapper mapper, IDormitoryRepository dormitoryRepository,
        IShopDormitoryRepository shopDormitoryRepository, ISystemResourceRepository systemResourceRepository, IOperatingSlotRepository operatingSlotRepository, ILogger<ShopRegisterHandler> logger, IEmailService emailService,
        ICacheService cacheService, IMapApiService mapApiService)
    {
        _unitOfWork = unitOfWork;
        _accountRepository = accountRepository;
        _shopRepository = shopRepository;
        _mapper = mapper;
        _dormitoryRepository = dormitoryRepository;
        _shopDormitoryRepository = shopDormitoryRepository;
        _systemResourceRepository = systemResourceRepository;
        _operatingSlotRepository = operatingSlotRepository;
        _logger = logger;
        _emailService = emailService;
        _cacheService = cacheService;
        _mapApiService = mapApiService;
    }

    public async Task<Result<Result>> Handle(ShopRegisterCommand request, CancellationToken cancellationToken)
    {
        // Validate Business
        Validate(request);
        // Todo: Validate 3km radius

        var accountTemp = _accountRepository.GetAccountByEmail(request.Email);

        // If account exist
        if (accountTemp != default && accountTemp.Status == AccountStatus.UnVerify && accountTemp.RoleId == (int)Domain.Enums.Roles.ShopOwner)
        {
            var shop = _shopRepository.Get(shop => shop.Id == accountTemp.Id)
                .Include(shop => shop.ShopDormitories)
                .Include(shop => shop.Location).Single();

            shop.Location.Address = request.Address;
            shop.Location.Latitude = request.Latitude;
            shop.Location.Longitude = request.Longitude;
            shop.Name = request.ShopName;
            shop.PhoneNumber = request.PhoneNumber;

            // Call api to get and duration and infor
            var shopDormitores = new List<ShopDormitory>();
            foreach (var id in request.DormitoryIds)
            {
                var dormitoryLocation = _dormitoryRepository.GetLocationByDormitoryId(id);
                var distanceOfMap = await _mapApiService.GetDistanceOneDestinationAsync(shop.Location, dormitoryLocation, VehicleMaps.Car).ConfigureAwait(false);
                shopDormitores.Add(new ShopDormitory()
                {
                    DormitoryId = id,
                    Distance = distanceOfMap.Distance.Value / 1000,
                    Duration = distanceOfMap.Duration.Value / 60,
                });
            }
            shop.ShopDormitories = shopDormitores;

            accountTemp.PhoneNumber = request.PhoneNumber;
            accountTemp.Password = BCrypUnitls.Hash(request.Password);
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                _accountRepository.Update(accountTemp);
                _shopRepository.Update(shop);
                SendAndSaveVerificationCode(request.Email);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                return Result.Create(new RegisterResponse()
                {
                    Email = request.Email,
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_REGISTER_SUCCESSFULLY.GetDescription()) ?? string.Empty,
                });
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw;
            }
        }
        else
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                var account = await CreateAccountAsync(request);
                var shop = await CreateShopAsync(account.Id, request).ConfigureAwait(false);
                var shopDormitories = await CreateShopDormitoryAsync(shop.Id, request.DormitoryIds, shop.Location).ConfigureAwait(false);
                await CreateOperatingDayAndFrameAsync(shop.Id).ConfigureAwait(false);
                SendAndSaveVerificationCode(request.Email);

                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                return Result.Create(new RegisterResponse()
                {
                    Email = request.Email,
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_REGISTER_SUCCESSFULLY.GetDescription()) ?? string.Empty,
                });
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw;
            }
        }
    }

    private void Validate(ShopRegisterCommand register)
    {
        var account = _accountRepository.GetAccountByEmail(register.Email);
        if (account != default && account.Status != AccountStatus.UnVerify)
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription(), HttpStatusCode.Conflict);

        if (account != default && account.Status == AccountStatus.UnVerify && account.RoleId != (int)Domain.Enums.Roles.ShopOwner)
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST_IN_ORTHER_ROLE.GetDescription(), HttpStatusCode.Conflict);

        if (account != default && account.PhoneNumber != register.PhoneNumber && _accountRepository.CheckExistByPhoneNumber(register.PhoneNumber))
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription(), HttpStatusCode.Conflict);

        if (_accountRepository.CheckExistPhoneNumberInOtherEmailAccount(register.Email, register.PhoneNumber))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription(), HttpStatusCode.Conflict);
        }

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

    private async Task<Shop> CreateShopAsync(long shopId, ShopRegisterCommand register)
    {
        var location = new Location()
        {
            Address = register.Address,
            Latitude = register.Latitude,
            Longitude = register.Longitude,
        };

        var shopWallet = new Wallet();

        var shop = new Shop()
        {
            Id = shopId,
            Name = register.ShopName,
            PhoneNumber = register.PhoneNumber,
            LogoUrl = _systemResourceRepository.GetByResourceCode(ResourceCode.SHOP_LOGO.GetDescription()) ?? string.Empty,
            BannerUrl = _systemResourceRepository.GetByResourceCode(ResourceCode.SHOP_BANNER.GetDescription()) ?? string.Empty,
            Location = location,
            MaxOrderHoursInAdvance = ShopConfigurationConstant.MAX_ORDER_HOURS_IN_ADVANCE,
            MinOrderHoursInAdvance = ShopConfigurationConstant.MIN_ORDER_HOURS_IN_ADVANCE,
            Wallet = shopWallet,
        };

        await _shopRepository.AddAsync(shop).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        return shop;
    }

    private async Task<List<ShopDormitory>> CreateShopDormitoryAsync(long shopId, long[] dormitoryIds, Location shopLocation)
    {
        // Call api to get and duration and infor
        var shopDormitores = new List<ShopDormitory>();
        foreach (var id in dormitoryIds)
        {
            var dormitoryLocation = _dormitoryRepository.GetLocationByDormitoryId(id);
            var distanceOfMap = await _mapApiService.GetDistanceOneDestinationAsync(shopLocation, dormitoryLocation, VehicleMaps.Car).ConfigureAwait(false);
            shopDormitores.Add(new ShopDormitory()
            {
                ShopId = shopId,
                DormitoryId = id,
                Distance = distanceOfMap.Distance.Value / 1000,
                Duration = distanceOfMap.Duration.Value / 60,
            });
        }

        await _shopDormitoryRepository.AddRangeAsync(shopDormitores).ConfigureAwait(false);
        return shopDormitores;
    }

    private async Task CreateOperatingDayAndFrameAsync(long shopId)
    {
        var operatingDay = new OperatingSlot()
        {
            ShopId = shopId,
        };

        // TODO: Remove operating frame
        // var operatingDays = operatingDay.CreateListOperatingDayForNewShop();
        // await _operatingDayRepository.AddRangeAsync(operatingDays).ConfigureAwait(false);
    }

    private void SendAndSaveVerificationCode(string email)
    {
        var code = new Random().Next(1000, 10000).ToString();
        _cacheService.SetCacheResponseAsync(
            GenerateCacheKey(VerificationCodeTypes.Register, email),
            code, TimeSpan.FromSeconds(RedisConstant.TIME_VERIFY_CODE_LIVE));

        var isSendMail = _emailService.SendVerificationCodeRegister(email, code);
        if (!isSendMail)
        {
            throw new Exception("Internal Server Error");
        }
    }

    private string GenerateCacheKey(VerificationCodeTypes verificationCodeType, string email)
    {
        return verificationCodeType.GetDescription() + "-" + email;
    }
}