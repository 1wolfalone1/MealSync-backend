using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.PlatformCategory.Models;

namespace MealSync.Application.UseCases.PlatformCategory.Queries.GetPlatformCategoryDetail;

public class GetPlatformCategoryDetailHandler : IQueryHandler<GetPlatformCategoryDetailQuery, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlatformCategoryRepository _platformCategoryRepository;
    private readonly IMapper _mapper;

    public GetPlatformCategoryDetailHandler(IUnitOfWork unitOfWork, IPlatformCategoryRepository platformCategoryRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _platformCategoryRepository = platformCategoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetPlatformCategoryDetailQuery request, CancellationToken cancellationToken)
    {
        var platformCategory = _platformCategoryRepository.GetById(request.Id);
        return Result.Success(_mapper.Map<PlatformCategoryResponse>(platformCategory));
    }
}