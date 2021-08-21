using System;
using System.Linq;
using FluentValidation;
using YoutubeMusicBot.Application.Models;

namespace YoutubeMusicBot.Application.Validation
{
    public class UserMessageValidator : AbstractValidator<MessageModel>
    {
        private static readonly string[] AllowedSchemes = { "http", "https", "ftp" };

        public UserMessageValidator()
        {
            RuleFor(r => r.Text)
                .NotEmpty()
                .WithMessage("Message must be not empty.")
                .DependentRules(
                    () =>
                    {
                        RuleFor(r => r.Text)
                            .Must(
                                m => Uri.TryCreate(m, UriKind.Absolute, out var uri)
                                    && AllowedSchemes.Contains(uri.Scheme))
                            .WithMessage("Message must be valid URL.");
                    });
        }
    }
}
