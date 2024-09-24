namespace MealSync.API.Shared;

public static class Endpoints
{
    public const string BASE = "/api/v1";
    public const string LOGIN_USERNAME_PASS = "auth/login";
    public const string ALL_DORMITORY = "dormitory/all";
    public const string GET_BUILDING_BY_DORMITORY = "dormitory/{id}/building";
    public const string REGISTER_CUSTOMER = "auth/customer-register";
}