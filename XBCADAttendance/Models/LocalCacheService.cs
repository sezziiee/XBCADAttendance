using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using static XBCADAttendance.Controllers.HomeController;

public static class LocalCacheService
{
    private static readonly string CacheFilePath = "localUserAuthCache.json";

    // Save UserAuth object to local cache
    public static void SaveUserToCache(UserAuth userAuth, string sessionToken)
    {
        var cachedData = GetAllCachedUsers();
        var existingUser = cachedData.FirstOrDefault(u => u.SessionToken == sessionToken);

        if (existingUser != null)
        {
            cachedData.Remove(existingUser);
        }

        cachedData.Add(new CachedUserAuth
        {
            SessionToken = sessionToken,
            UserAuth = userAuth
        });

        File.WriteAllText(CacheFilePath, JsonSerializer.Serialize(cachedData));
    }

    // Retrieve UserAuth from local cache by session token
    public static UserAuth GetUserByToken(string sessionToken)
    {
        var cachedData = GetAllCachedUsers();
        var cachedUser = cachedData.FirstOrDefault(u => u.SessionToken == sessionToken);
        return cachedUser?.UserAuth;
    }

    // Retrieve all cached users
    public static List<CachedUserAuth> GetAllCachedUsers()
    {
        if (File.Exists(CacheFilePath))
        {
            var jsonData = File.ReadAllText(CacheFilePath);
            return JsonSerializer.Deserialize<List<CachedUserAuth>>(jsonData) ?? new List<CachedUserAuth>();
        }

        return new List<CachedUserAuth>();
    }
}

// Helper class to cache UserAuth with its session token
public class CachedUserAuth
{
    public string SessionToken { get; set; }
    public UserAuth UserAuth { get; set; }
}
