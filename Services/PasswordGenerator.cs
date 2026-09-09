using System;
using System.Collections.Generic;
using System.Linq;

namespace Alumni_Management_System.Services;

public static class PasswordGenerator
{
    // Satisfies the app's password policy (upper/lower/digit/symbol, 8+
    // chars) without ambiguous-looking characters (0/O, 1/l). Shared by
    // UsersController.ResetPassword (admin-triggered) and
    // AccountController.ForgotPassword (self-service).
    public static string GenerateTempPassword()
    {
        const string upper = "ABCDEFGHJKMNPQRSTUVWXYZ";
        const string lower = "abcdefghjkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%?";
        var rng = System.Security.Cryptography.RandomNumberGenerator.Create();

        char Pick(string chars)
        {
            var bytes = new byte[1];
            rng.GetBytes(bytes);
            return chars[bytes[0] % chars.Length];
        }

        var chars = new List<char> { Pick(upper), Pick(lower), Pick(digits), Pick(symbols) };
        for (int i = 0; i < 6; i++)
        {
            var pool = upper + lower + digits;
            chars.Add(Pick(pool));
        }

        // Shuffle so the fixed-category characters aren't always first.
        return new string(chars.OrderBy(_ => Guid.NewGuid()).ToArray());
    }
}
