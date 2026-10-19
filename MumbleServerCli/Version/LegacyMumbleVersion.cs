namespace MumbleServerCli.Version;

public class LegacyMumbleVersion
{
    public LegacyMumbleVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public LegacyMumbleVersion(int value)
    {
        Major = (value >> 16) & 0xFFFF;
        Minor = (value >> 8) & 0xFF;
        Patch = value & 0xFF;
    }

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
