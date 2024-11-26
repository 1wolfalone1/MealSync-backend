using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.PlatformCategory.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.UpdatePlatformCategory;

public class UpdatePlatformCategoryHandler : ICommandHandler<UpdatePlatformCategoryCommand, Result>
{
    private readonly IPlatformCategoryRepository _platformCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<UpdatePlatformCategoryHandler> _logger;
    private readonly IMapper _mapper;

    public UpdatePlatformCategoryHandler(IPlatformCategoryRepository platformCategoryRepository, IUnitOfWork unitOfWork, ISystemResourceRepository systemResourceRepository, ILogger<UpdatePlatformCategoryHandler> logger,
        IFoodRepository foodRepository, IMapper mapper)
    {
        _platformCategoryRepository = platformCategoryRepository;
        _unitOfWork = unitOfWork;
        _systemResourceRepository = systemResourceRepository;
        _logger = logger;
        _foodRepository = foodRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(UpdatePlatformCategoryCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Warning
        if (!request.IsConfirm)
        {
            var foods = _foodRepository.Get(f => f.PlatformCategoryId == request.Id).ToList();
            if (foods != null && foods.Count > 0)
            {
                var platformOrigin = _platformCategoryRepository.GetById(request.Id);
                return Result.Warning(new
                {
                    Code = MessageCode.E_PLATFORM_CATEGORY_HAVE_FOOD_LINKED.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.E_PLATFORM_CATEGORY_HAVE_FOOD_LINKED.GetDescription(), new object[] { platformOrigin.Name, foods.Count }),
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var platform = _platformCategoryRepository.GetById(request.Id);
            platform.Name = request.Name;
            platform.Description = request.Description;
            platform.ImageUrl = request.ImageUrl;
            _platformCategoryRepository.Update(platform);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        var platformData = _platformCategoryRepository.GetById(request.Id);
        return Result.Success(_mapper.Map<PlatformCategoryResponse>(platformData));
    }

    private void Validate(UpdatePlatformCategoryCommand request)
    {
        if (_platformCategoryRepository.GetById(request.Id) == null)
        {
            throw new InvalidBusinessException(MessageCode.E_PLATFORM_CATEGORY_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }

        if (_platformCategoryRepository.CheckExsitUpdateName(request.Name, request.Id))
            throw new InvalidBusinessException(MessageCode.E_PLATFORM_CATEGORY_DOUBLE_NAME.GetDescription(), new object[] { request.Name });
    }
}