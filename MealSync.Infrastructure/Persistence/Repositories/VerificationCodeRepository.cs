using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class VerificationCodeRepository : BaseRepository<VerificationCode>, IVerificationCodeRepository
{
    public VerificationCodeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}