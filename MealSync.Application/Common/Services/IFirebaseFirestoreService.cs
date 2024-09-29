namespace MealSync.Application.Common.Services;

public interface IFirebaseFirestoreService
{
    Task<bool> AddNewNotifyCollectionToUser(string email, string type, int status, string message);

    Task<bool> AddNewNotifyCollectionToShop(string email, string type, int status, string message);
}