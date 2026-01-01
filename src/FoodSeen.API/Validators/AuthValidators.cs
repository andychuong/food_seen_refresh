using FluentValidation;
using FoodSeen.API.Models.Requests;

namespace FoodSeen.API.Validators;

/// <summary>
/// Validator for user registration requests.
/// Enforces password strength requirements and username format rules.
/// These validations run before the AuthController.Register action.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // Email validation - must be valid email format
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        // Password validation - enforces security requirements
        // Requires: 8+ chars, uppercase, lowercase, and number
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number");

        // Username validation - alphanumeric and underscores only
        // Used for display and @mentions, must be URL-safe
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(100).WithMessage("Username must be less than 100 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        // Optional first name - reasonable length limit
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must be less than 100 characters")
            .When(x => x.FirstName != null);

        // Optional last name - reasonable length limit
        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must be less than 100 characters")
            .When(x => x.LastName != null);
    }
}

/// <summary>
/// Validator for login requests.
/// Performs basic input validation before authentication attempt.
/// Note: Does not validate credentials - that's the AuthService's job.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // Email validation - basic format check
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        // Password validation - just check it's not empty
        // Actual password verification happens in AuthService
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

/// <summary>
/// Validator for refresh token requests.
/// Ensures the refresh token string is provided.
/// Token validity is verified by AuthService, not this validator.
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        // Refresh token validation - must be provided (validity checked by service)
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
