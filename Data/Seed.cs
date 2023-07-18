using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyJob.Entities;

namespace MyJob.Data;

public class Seed
{
    public static async Task SeedUsers(DataContext context)
    {
        // Load and add users
        if (!await context.Users.AnyAsync())
        {
            var userData = await File.ReadAllTextAsync("data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToString();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("p"));
                user.PasswordSalt = hmac.Key;

                context.Users.Add(user);
            }
        }


        await context.SaveChangesAsync();
    }
}