using System;
using System.Linq;
using FluentValidation;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot
{
    public class MessageContextValidator : AbstractValidator<MessageContext>
    {
        private static readonly string[] AllowedSchemes = { "http", "https", "ftp" };

        public MessageContextValidator()
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
