namespace XboxEOS.EventArgs;

public class OSCMessageSentEventArgs
{
    public string Address { get; init; }

    public string[] Data { get; init; }

    public override string ToString()
    {
        return $"{Address} {string.Join(',', Data)}";
    }
}
