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
    List<ToastMessage> Messages { get; }
    void RegisterToaster(Toaster toaster);
    void Show(ToastType type, string text, string? heading = null);
}

public class ToastManager : IToastManager
{
    private readonly ILogger<ToastManager> _logger;
    private Toaster? _toaster;

    public List<ToastMessage> Messages { get; private set; } = new();

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
        _logger.LogDebug("{}: {}", type.ToString(), text);

        Messages.Add(new ToastMessage(Guid.NewGuid().ToString(), type, text, heading));

        if (_toaster != null)
            _toaster.Update();
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