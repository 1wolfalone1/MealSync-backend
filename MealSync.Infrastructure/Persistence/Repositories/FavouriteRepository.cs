using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FavouriteRepository : BaseRepository<Favourite>, IFavouriteRepository
{
    public FavouriteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}