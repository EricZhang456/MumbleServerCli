namespace MumbleServerCli.Version;

public class MumbleVersion
{
    public MumbleVersion(int major, int minor, int patch, int build)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Build = build;
    }

    public MumbleVersion(long value)
    {
        Major = (int)(value >> 48) & 0xFFFF;
        Minor = (int)(value >> 32) & 0xFFFF;
        Patch = (int)(value >> 16) & 0xFFFF;
        Build = (int)value & 0xFFFF;
    }

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public int Build { get; }

    public override string ToString() => Build > 0 ? $"{Major}.{Minor}.{Patch}.{Build}" : $"{Major}.{Minor}.{Patch}";
}
