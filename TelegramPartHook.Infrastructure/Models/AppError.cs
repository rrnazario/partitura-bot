using System;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Infrastructure.Models;

public class AppError
    : Entity
{
    public string ErrorDate { get; set; }
    public Exception Content { get; set; }

    public AppError(Exception content)
    {
        Content = content;
    }
}