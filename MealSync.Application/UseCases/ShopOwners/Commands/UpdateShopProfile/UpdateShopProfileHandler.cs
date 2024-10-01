using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopOwners.Models;
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

    public UpdateShopProfileHandler(IUnitOfWork unitOfWork, IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService, ILogger<UpdateShopProfileHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(UpdateShopProfileCommand request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request).ConfigureAwait(false);

        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            shop.Name = request.Name;
            shop.PhoneNumber = request.PhoneNumber;
            shop.Description = request.Description;
            shop.LogoUrl = request.LogoUrl;
            shop.BannerUrl = request.BannerUrl;
            shop.IsAcceptingOrderNextDay = request.IsAcceptingOrderNextDay;
            shop.MaxOrderHoursInAdvance = request.MaxOrderHoursInAdvance;
            shop.MinOrderHoursInAdvance = request.MinOrderHoursInAdvance;
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

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
    }
}