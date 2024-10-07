using Newtonsoft.Json.Linq;

namespace MealSync.Infrastructure.Settings;

public class FirebaseConfigLoader
{
    public static void LoadFirebaseCredentials(string jsonFilePath)
    {
        var jsonText = File.ReadAllText(jsonFilePath);
        var jsonObject = JObject.Parse(jsonText);

        Environment.SetEnvironmentVariable("FIREBASE_PROJECT_ID", jsonObject["project_id"]?.ToString());
        Environment.SetEnvironmentVariable("FIREBASE_PRIVATE_KEY_ID", jsonObject["private_key_id"]?.ToString());
        Environment.SetEnvironmentVariable("FIREBASE_PRIVATE_KEY", jsonObject["private_key"]?.ToString());
        Environment.SetEnvironmentVariable("FIREBASE_CLIENT_EMAIL", jsonObject["client_email"]?.ToString());
        Environment.SetEnvironmentVariable("FIREBASE_CLIENT_ID", jsonObject["client_id"]?.ToString());
        Environment.SetEnvironmentVariable("FIREBASE_AUTH_URI", jsonObject["auth_uri"]?.ToString());
        Environment.SetEnvironmentVariable("FIREBASE_TOKEN_URI", jsonObject["token_uri"]?.ToString());
        Environment.SetEnvironmentVariable("FIREBASE_AUTH_PROVIDER_CERT_URL", jsonObject["auth_provider_x509_cert_url"]?.ToString());
        Environment.SetEnvironmentVariable("FIREBASE_CLIENT_CERT_URL", jsonObject["client_x509_cert_url"]?.ToString());
    }
}