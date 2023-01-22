using Aniwari.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniwari.Managers;

public interface IToastManager
{
    void RegisterToaster(Toaster toaster);
    void Show(ToastType type, string text, string? heading = null);
}

public class ToastManager : IToastManager
{
    private readonly ILogger<ToastManager> _logger;
    private Toaster? _toaster;

    public ToastManager(ILogger<ToastManager> logger)
    {
        _logger = logger;
    }

    public void RegisterToaster(Toaster toaster)
    {
        if (toaster == null)
            throw new Exception("Cannot register null reference.");

        _toaster = toaster;
    }

    public void Show(ToastType type, string text, string? heading = null)
    {
        if (_toaster == null)
            throw new Exception("Toaster instance is not registered.");

        _logger.LogDebug("{}: {}", type.ToString(), text);

        _toaster.AddMessage(new ToastMessage(Guid.NewGuid().ToString(), type, text, heading));
    }
}

public record ToastMessage(string Guid, ToastType Type, string Text, string? Heading);

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}