using System;

namespace Aniwari.Platforms;

public interface IFolderPicker
{
    Task<string?> PickFolder();
}
