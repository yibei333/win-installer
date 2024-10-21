namespace WinInstaller.Setup.Engine;

public class ProgressModel
{
    internal ProgressModel(long total, long handled, double progress, string speed)
    {
        Total = total;
        Handled = handled;
        Progress = progress;
        Speed = speed;
    }

    public long Total { get; }
    public long Handled { get; }
    public double Progress { get; }
    public string Speed { get; }
}
