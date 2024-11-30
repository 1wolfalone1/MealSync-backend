using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Moderators.Models;
using MealSync.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Moderators.Queries.GetAllModerators;

public class GetAllModeratorHandler : IQueryHandler<GetAllModeratorQuery, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IModeratorRepository _moderatorRepository;
    private readonly ILogger<GetAllModeratorHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllModeratorHandler(IUnitOfWork unitOfWork, IModeratorRepository moderatorRepository, ILogger<GetAllModeratorHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _moderatorRepository = moderatorRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetAllModeratorQuery request, CancellationToken cancellationToken)
    {
        var moderatorPaging = _moderatorRepository.GetAllModerator(request.SearchValue, request.DormitoryId, (int) request.Status, request.DateFrom, request.DateTo, request.PageIndex, request.PageSize);
        var moderators = _mapper.Map<List<ModeratorResponse>>(moderatorPaging.Moderators);
        return Result.Success(new PaginationResponse<ModeratorResponse>(moderators, moderatorPaging.TotalCount, request.PageIndex, request.PageSize));
    }
}