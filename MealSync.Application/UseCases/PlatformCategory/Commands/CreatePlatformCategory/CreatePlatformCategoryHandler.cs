using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.PlatformCategory.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.CreatePlatformCategory;

public class CreatePlatformCategoryHandler : ICommandHandler<CreatePlatformCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlatformCategoryRepository _platformCategoryRepository;
    private readonly ILogger<CreatePlatformCategoryHandler> _logger;
    private readonly IMapper _mapper;

    public CreatePlatformCategoryHandler(IUnitOfWork unitOfWork, IPlatformCategoryRepository platformCategoryRepository, ILogger<CreatePlatformCategoryHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _platformCategoryRepository = platformCategoryRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(CreatePlatformCategoryCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var platformData = new Domain.Entities.PlatformCategory();
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var maxDisplayOrder = _platformCategoryRepository.GetMaxDisplayOrder();
            var platform = new Domain.Entities.PlatformCategory()
            {
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                Description = request.Description,
                DisplayOrder = ++maxDisplayOrder,
            };

            await _platformCategoryRepository.AddAsync(platform).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            platformData = platform;
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success(_mapper.Map<PlatformCategoryResponse>(platformData));
    }

    private void Validate(CreatePlatformCategoryCommand request)
    {
        if (_platformCategoryRepository.CheckExsitName(request.Name))
            throw new InvalidBusinessException(MessageCode.E_PLATFORM_CATEGORY_DOUBLE_NAME.GetDescription(), new object[] { request.Name });
    }
}