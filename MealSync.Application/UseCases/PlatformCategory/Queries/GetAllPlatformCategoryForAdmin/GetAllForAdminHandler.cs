using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.PlatformCategory.Models;

namespace MealSync.Application.UseCases.PlatformCategory.Queries.GetAllPlatformCategoryForAdmin;

public class GetAllForAdminHandler : IQueryHandler<GetAllForAdminQuery, Result>
{
    private readonly IPlatformCategoryRepository _platformCategoryRepository;
    private readonly IMapper _mapper;

    public GetAllForAdminHandler(IPlatformCategoryRepository platformCategoryRepository, IMapper mapper)
    {
        _platformCategoryRepository = platformCategoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetAllForAdminQuery request, CancellationToken cancellationToken)
    {
        var platformPaging = _platformCategoryRepository.GetPlatformCategoryForAdmin(request.SearchValue, request.DateFrom, request.DateTo, request.PageIndex, request.PageSize);
        var response = _mapper.Map<List<PlatformCategoryResponse>>(platformPaging.Result);
        return Result.Success(new PaginationResponse<PlatformCategoryResponse>(response, platformPaging.TotalCount, request.PageIndex, request.PageSize));
    }
}