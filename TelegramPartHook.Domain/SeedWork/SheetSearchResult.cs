using System.Diagnostics;
using NanoidDotNet;
using static TelegramPartHook.Domain.Constants.Enums;

namespace TelegramPartHook.Domain.SeedWork;

[DebuggerDisplay("Address: {Address}, Source: {Source}")]
public class SheetSearchResult
{
    public string Id { get; set; }
    public string Address { get; set; }
    public string ServerPath { get; set; }
    public string LocalPath { get; private set; }
    public string AdditionalInfo { get; }
    public FileSource Source { get; }

    public KeyboardButtons? Buttons { get; private set; }
    public string Caption { get; private set; }

    public bool Exists => File.Exists(Address) || File.Exists(LocalPath);
    public string LocalWhereExists => File.Exists(Address) ? Address : LocalPath;

    private SheetSearchResult()
    {
        // for serializing purposes
    }

    private SheetSearchResult(FileSource fileSource, string localPath)
    {
        SetLocalPath(localPath);
        Source = fileSource;
    }

    public SheetSearchResult(string address, FileSource source, string serverPath = "", string additionalInfo = "")
    {
        Address = address;
        Source = source;
        ServerPath = serverPath;
        AdditionalInfo = additionalInfo;
    }

    public override string ToString() => $"[{Source}] {Address}";

    public void SetLocalPath(string localPath)
        => LocalPath = localPath;

    public void SetButtons(KeyboardButtons buttons) => Buttons = buttons;
    public void SetCaption(string caption) => Caption = caption;

    public SheetSearchResult FillId()
    {
        if (string.IsNullOrEmpty(Id))
            Id = Nanoid.Generate(size: 5);

        return this;
    }

    public override bool Equals(object obj)
        => obj is SheetSearchResult other && other.LocalPath == LocalPath && other.Address == Address;

    public override int GetHashCode() => HashCode.Combine(LocalPath, Address);

    public static SheetSearchResult CreateLocalFile(string localPath) => new(FileSource.Generated, localPath);
}