using FluentValidation;
using ClinicBackend.Models.DTOs;

namespace ClinicBackend.Validators;

public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Must(PatientNameValidator.IsValid).WithMessage("Name must contain at least first and last name (two words) with no digits.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleFor(x => x.TajNumber)
            .NotEmpty().WithMessage("TAJ number is required.")
            .Must(TajNumberValidator.IsValid).WithMessage("Invalid TAJ number format. Expected: XXX-XX-XXX");

        RuleFor(x => x.Complaint)
            .NotEmpty().WithMessage("Complaint is required.");

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("Specialty is required.");
    }
}