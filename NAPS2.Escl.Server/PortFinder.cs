namespace NAPS2.Escl.Server;

internal static class PortFinder
{
    private const int MAX_PORT_TRIES = 5;
    private const int RANDOM_PORT_MIN = 10001;
    private const int RANDOM_PORT_MAX = 19999;

    public static async Task RunWithSpecifiedOrRandomPort(int defaultPort, Func<int, Task> portTaskFunc,
        CancellationToken cancelToken)
    {
        int port = defaultPort;
        int retries = 0;
        var random = new Random();
        if (port == 0)
        {
            port = RandomPort(random);
        }
        while (true)
        {
            try
            {
                await portTaskFunc(port);
                break;
            }
            catch (Exception)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }
                retries++;
                port = RandomPort(random);
                if (retries > MAX_PORT_TRIES)
                {
                    throw;
                }
            }
        }
    }

    private static int RandomPort(Random random) => random.Next(RANDOM_PORT_MIN, RANDOM_PORT_MAX + 1);
}