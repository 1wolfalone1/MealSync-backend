using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Map;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;

public class UpdateShopProfileHandler : ICommandHandler<UpdateShopProfileCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<UpdateShopProfileHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IDormitoryRepository _dormitoryRepository;
    private readonly IShopDormitoryRepository _shopDormitoryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMapApiService _mapApiService;

    public UpdateShopProfileHandler(IUnitOfWork unitOfWork, IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService, ILogger<UpdateShopProfileHandler> logger, IMapper mapper, IDormitoryRepository dormitoryRepository, IShopDormitoryRepository shopDormitoryRepository, IAccountRepository accountRepository, IMapApiService mapApiService)
    {
        _unitOfWork = unitOfWork;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
        _mapper = mapper;
        _dormitoryRepository = dormitoryRepository;
        _shopDormitoryRepository = shopDormitoryRepository;
        _accountRepository = accountRepository;
        _mapApiService = mapApiService;
    }

    public async Task<Result<Result>> Handle(UpdateShopProfileCommand request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request).ConfigureAwait(false);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await UpdateShopInfo(request).ConfigureAwait(false);
            await UpdateShopAccount(request).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            var shop = _shopRepository.Get(shop => shop.Id == _currentPrincipalService.CurrentPrincipalId)
                .Include(shop => shop.ShopDormitories)
                .ThenInclude(shopDor => shopDor.Dormitory)
                .Include(shop => shop.Location)
                .Include(shop => shop.Account)
                .SingleOrDefault();
            var response = _mapper.Map<ShopProfileResponse>(shop);
            return Result.Success(response);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task ValidateAsync(UpdateShopProfileCommand request)
    {
        var shop = await _shopRepository.Get(s => s.PhoneNumber == request.PhoneNumber &&
                                                  s.Id != _currentPrincipalService.CurrentPrincipalId).SingleOrDefaultAsync().ConfigureAwait(false);
        if (shop != default)
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription(), HttpStatusCode.Conflict);

        foreach (var dormitoryId in request.DormitoryIds)
        {
            var dormitory = _dormitoryRepository.Get(d => d.Id == dormitoryId).SingleOrDefault();
            if (dormitory == default)
                throw new InvalidBusinessException(MessageCode.E_DORMITORY_NOT_FOUND.GetDescription(), HttpStatusCode.Conflict);
        }
    }

    private async Task UpdateShopInfo(UpdateShopProfileCommand request)
    {
        var shop = _shopRepository.Get(shop => shop.Id == _currentPrincipalService.CurrentPrincipalId)
            .Include(shop => shop.ShopDormitories)
            .Include(shop => shop.Location)
            .SingleOrDefault();
        shop.Name = request.ShopName;
        shop.PhoneNumber = request.PhoneNumber;
        shop.Description = request.Description;
        shop.LogoUrl = request.LogoUrl;
        shop.BannerUrl = request.BannerUrl;

        // Update shop location
        shop.Location.Address = request.Location.Address;
        shop.Location.Latitude = request.Location.Latitude;
        shop.Location.Longitude = request.Location.Longitude;

        // Update shop dormitory
        var shopDormitories = new List<ShopDormitory>();
        foreach (var id in request.DormitoryIds)
        {
            var dormitoryLocation = _dormitoryRepository.GetLocationByDormitoryId(id);
            var distanceOfMap = await _mapApiService.GetDistanceOneDestinationAsync(shop.Location, dormitoryLocation, VehicleMaps.Car).ConfigureAwait(false);
            shopDormitories.Add(new ShopDormitory()
            {
                DormitoryId = id,
                Distance = (double)distanceOfMap.Distance.Value / 1000,
                Duration = distanceOfMap.Duration.Value / 60,
            });
        }
        shop.ShopDormitories = shopDormitories;

        _shopRepository.Update(shop);
    }

    private Task UpdateShopAccount(UpdateShopProfileCommand request)
    {
        var account = _accountRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        account.FullName = request.ShopOwnerName;
        _accountRepository.Update(account);
        return Task.CompletedTask;
    }
}