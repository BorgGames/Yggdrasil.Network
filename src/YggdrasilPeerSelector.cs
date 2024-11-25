namespace Yggdrasil;

using System.Diagnostics;
using System.Net.Sockets;

public class YggdrasilPeerSelector {
    public static int RecommendedPeerCount => 5;

    public static async Task<(Uri, TimeSpan)[]> GetLowestLatency(
        IEnumerable<Uri> candidates, int? count = null, CancellationToken cancel = default) {
        if (candidates is null) throw new ArgumentNullException(nameof(candidates));
        count ??= RecommendedPeerCount;
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), actualValue: count,
                                                  "Count must be non-negative");

        using var raceCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        var raceCancellationToken = raceCancellation.Token;
        var result = new List<(Uri, TimeSpan)>();

        var alreadyAddedHosts = new HashSet<string>();

        var stopwatch = Stopwatch.StartNew();
        var tasks = candidates.Select(uri => TryPing(uri, raceCancellationToken)).ToList();
        while (result.Count < count && tasks.Count > 0) {
            var reply = await Task.WhenAny(tasks).ConfigureAwait(false);
            tasks.Remove(reply);
            if (reply is { Status: TaskStatus.RanToCompletion, Result: { } uri }
             && alreadyAddedHosts.Add(uri.Host))
                result.Add((uri, stopwatch.Elapsed));
        }

        raceCancellation.Cancel();
        return result.ToArray();
    }

    static async Task<Uri?> TryPing(Uri uri, CancellationToken cancel) {
        try {
            var client = new TcpClient();
            int port = uri.Scheme switch {
                "tcp" => 0,
                "tls" or "wss" => 443,
                "ws" => 80,
                _ => throw new NotSupportedException("Unsupported URI scheme: " + uri.Scheme),
            };
            port = PortFallback(uri.Port, port);
#if NET5_0_OR_GREATER
            await client.ConnectAsync(uri.Host, port, cancel).ConfigureAwait(false);
#else
            await client.ConnectAsync(uri.Host, port).ConfigureAwait(false);
#endif
        } catch (Exception e) {
            return null;
        }

        return uri;
    }

    static int PortFallback(int port, int fallback) => port == 0 ? fallback : port;
}