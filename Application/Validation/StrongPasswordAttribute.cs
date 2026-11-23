using System.ComponentModel.DataAnnotations;

namespace Application.Validation
{
    /// <summary>Password policy: 8-64 chars with at least one lowercase, one uppercase and one digit.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class StrongPasswordAttribute : ValidationAttribute
    {
        public const int MinLength = 8;
        public const int MaxLength = 64;

        public StrongPasswordAttribute()
            : base($"Password must be {MinLength}-{MaxLength} characters and contain an uppercase letter, a lowercase letter and a digit.") { }

        public override bool IsValid(object? value)
        {
            if (value is null) return true;               // use [Required] for presence
            if (value is not string s) return false;
            return Check(s);
        }

        public static bool Check(string s)
            => s.Length is >= MinLength and <= MaxLength
               && s.Any(char.IsLower) && s.Any(char.IsUpper) && s.Any(char.IsDigit);
    }
}
