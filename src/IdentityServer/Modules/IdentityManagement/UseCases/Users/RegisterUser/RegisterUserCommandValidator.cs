using FluentValidation;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

// TODO: Is there a way we can translate these and use our specific codes and messages?

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.FirstName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);

        When(x => x.IsExternalLogin, () =>
        {
            RuleFor(x => x.ExternalProvider)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.ExternalUserId)
                .NotNull()
                .NotEmpty();
        }).Otherwise(() =>
        {
            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty();
        });
    }
}