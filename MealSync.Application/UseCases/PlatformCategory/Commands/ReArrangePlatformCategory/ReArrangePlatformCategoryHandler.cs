using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.ReArrangePlatformCategory;

public class ReArrangePlatformCategoryHandler : ICommandHandler<ReArrangePlatformCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlatformCategoryRepository _platformCategoryRepository;
    private readonly ILogger<ReArrangePlatformCategoryHandler> _logger;
    private readonly IMapper _mapper;

    public ReArrangePlatformCategoryHandler(IUnitOfWork unitOfWork, IPlatformCategoryRepository platformCategoryRepository, ILogger<ReArrangePlatformCategoryHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _platformCategoryRepository = platformCategoryRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ReArrangePlatformCategoryCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await Validate(request).ConfigureAwait(false);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var platforms = await _platformCategoryRepository.GetAll().ConfigureAwait(false);
            var displayOrder = 1;
            foreach (var platformCategory in platforms)
            {
                platformCategory.DisplayOrder = displayOrder++;
            }

            _platformCategoryRepository.UpdateRange(platforms);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        var platformsResponse = await _platformCategoryRepository.GetAll().ConfigureAwait(false);
        return Result.Success(_mapper.Map<List<FoodDetailResponse.PlatformCategoryResponse>>(platformsResponse));
    }

    private async Task Validate(ReArrangePlatformCategoryCommand request)
    {
        var platformCategory = await _platformCategoryRepository.GetAll().ConfigureAwait(false);
        if (platformCategory.Count() != request.Ids.Length)
            throw new InvalidBusinessException(MessageCode.E_PLATFORM_CATEGORY_NOT_ENOUGH_ID_TO_RE_ARRANGE.GetDescription());

        foreach (var id in request.Ids)
        {
            if (_platformCategoryRepository.GetById(id) == null)
            {
                throw new InvalidBusinessException(MessageCode.E_PLATFORM_CATEGORY_NOT_FOUND.GetDescription(), new object[] { id });
            }
        }
    }
}