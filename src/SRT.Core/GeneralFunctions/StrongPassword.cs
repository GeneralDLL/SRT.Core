// Ignore Spelling: DTO SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SRT.Core.GeneralFunctions
{
    /// <summary>
    /// سیاست رمز قوی برای تعیین/تغییر رمز (نه لاگین).
    /// قابل ذخیره از سطح نارنجی: عدد + حرف + علامت و حداقل ۶ کاراکتر.
    /// </summary>
    public static class StrongPassword
    {
        public const int MinLength = 6;
        public const int MaxLength = 128;

        public sealed class ValidationResult
        {
            public bool IsValid { get; init; }
            public string? ErrorCode { get; init; }
            public string? Message { get; init; }

            public static ValidationResult Ok() => new() { IsValid = true };

            public static ValidationResult Fail(string errorCode, string message) =>
                new() { IsValid = false, ErrorCode = errorCode, Message = message };
        }

        private static readonly HashSet<string> Blocklist = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "password1", "password12", "password123",
            "12345678", "123456789", "1234567890", "01234567",
            "qwerty", "qwertyui", "qwerty123", "asdfghjk",
            "admin", "admin123", "administrator",
            "mci", "mci123", "mci1234", "mci12345",
            "welcome", "welcome1", "letmein", "changeme",
            "secret", "default", "passw0rd", "test1234",
            "abcdefgh", "abc12345", "11111111", "00000000",
            "iloveyou", "sunshine", "football", "baseball",
            "dragon", "master", "monkey", "shadow",
            "superman", "trustno1", "whatever", "computer",
        };

        public static ValidationResult Validate(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return ValidationResult.Fail("Empty", "رمز عبور را وارد کنید.");

            var hasDigit = Regex.IsMatch(password, @"\d");
            var hasLetter = Regex.IsMatch(password, @"[A-Za-z]");
            var hasSpecial = Regex.IsMatch(password, @"[^A-Za-z0-9]");

            if (!hasDigit || !hasLetter || !hasSpecial)
                return ValidationResult.Fail("Composition", "رمز باید شامل عدد، کاراکتر خاص و حروف باشد.");

            if (password.Length < MinLength)
                return ValidationResult.Fail("TooShort", "رمز عبور باید حداقل ۶ کاراکتر باشد.");

            if (password.Length > MaxLength)
                return ValidationResult.Fail("TooLong", "رمز عبور نباید بیش از ۱۲۸ کاراکتر باشد.");

            if (Blocklist.Contains(password.Trim()))
                return ValidationResult.Fail("Common", "این رمز عبور بسیار رایج است؛ رمز قوی‌تری انتخاب کنید.");

            if (password.All(c => c == password[0]))
                return ValidationResult.Fail("Common", "این رمز عبور بسیار رایج است؛ رمز قوی‌تری انتخاب کنید.");

            return ValidationResult.Ok();
        }

        public static string? ValidateOrError(string? password)
        {
            var result = Validate(password);
            return result.IsValid ? null : result.Message;
        }
    }
}