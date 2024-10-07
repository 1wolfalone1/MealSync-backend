using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.PlatformCategory.Models;

namespace MealSync.Application.UseCases.PlatformCategory.Queries.GetAll;

public class GetAllPlatformCategoryHandler : IQueryHandler<GetAllPlatformCategoryQuery, Result>
{
    private readonly IPlatformCategoryRepository _platformCategoryRepository;
    private readonly IMapper _mapper;

    public GetAllPlatformCategoryHandler(IPlatformCategoryRepository platformCategoryRepository, IMapper mapper)
    {
        _platformCategoryRepository = platformCategoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetAllPlatformCategoryQuery request, CancellationToken cancellationToken)
    {
        return Result.Success(_mapper.Map<IEnumerable<PlatformCategoryResponse>>(await _platformCategoryRepository.GetAll().ConfigureAwait(false)));
    }
}