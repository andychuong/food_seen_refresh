using FluentValidation;
using FoodSeen.API.Models.Requests;

namespace FoodSeen.API.Validators;

/// <summary>
/// Validator for CreatePostRequest ensuring all required fields are present and valid.
/// Runs automatically via FluentValidation middleware before the controller action executes.
/// Returns 400 Bad Request with validation errors if any rules fail.
/// </summary>
public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        // Title validation - required field with reasonable length limit
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be less than 200 characters");

        // Description validation - must provide meaningful content
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters");

        // Address validation - required for location display
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(300).WithMessage("Address must be less than 300 characters");

        // Latitude validation - WGS84 coordinate range (-90 to 90 degrees)
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        // Longitude validation - WGS84 coordinate range (-180 to 180 degrees)
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        // Event date validation - must be a future date (no past events)
        RuleFor(x => x.EventDate)
            .NotEmpty().WithMessage("Event date is required")
            .GreaterThan(DateTime.UtcNow).WithMessage("Event date must be in the future");

        // Optional end date validation - if provided, must be after start date
        RuleFor(x => x.EventEndDate)
            .GreaterThan(x => x.EventDate)
            .When(x => x.EventEndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}

/// <summary>
/// Validator for UpdatePostRequest with partial update support.
/// All fields are optional (null = no change), but if provided, must be valid.
/// Uses conditional validation (.When) to only validate non-null fields.
/// </summary>
public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        // Title validation - only if provided (partial update)
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must be less than 200 characters")
            .When(x => x.Title != null);

        // Description validation - minimum length ensures meaningful content
        RuleFor(x => x.Description)
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .When(x => x.Description != null);

        // Address validation - reasonable length limit
        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("Address must be less than 300 characters")
            .When(x => x.Address != null);

        // Latitude validation - WGS84 range check when updating location
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);

        // Longitude validation - WGS84 range check when updating location
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);

        // Event date validation - if updating, must still be in the future
        RuleFor(x => x.EventDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Event date must be in the future")
            .When(x => x.EventDate.HasValue);
    }
}
