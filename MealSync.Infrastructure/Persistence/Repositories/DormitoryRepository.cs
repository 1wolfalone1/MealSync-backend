using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class DormitoryRepository : BaseRepository<Dormitory>, IDormitoryRepository
{
    public DormitoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public List<Dormitory> GetAll()
    {
        return DbSet.Include(d => d.Location).ToList();
    }

    public bool CheckExistedById(long id)
    {
        return DbSet.All(d => d.Id == id);
    }
}