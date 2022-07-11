namespace Concorde.Extensions;

public static class CancellationTokenExtensions
{
    public static async Task AsTask(this CancellationToken cancellationToken, TimeSpan timeout)
    {
        try
        {
            await Task.Delay(timeout, cancellationToken);
        }
        catch
        {
            // ignored
        }
    }
    
    public static async Task AsTask(this CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(-1, cancellationToken);
        }
        catch
        {
            // ignored
        }
    }
}