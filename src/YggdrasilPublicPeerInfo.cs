namespace Yggdrasil;

using HtmlAgilityPack;

public class YggdrasilPublicPeerInfo {
    public Uri Uri { get; }
    public int Reliability { get; }

    public override string ToString() => Invariant($"{this.Reliability}% {this.Uri}");

    public YggdrasilPublicPeerInfo(Uri uri, int reliability) {
        this.Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        if (reliability is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(reliability), actualValue: reliability,
                                                  "Reliability must be between 0 and 100");
        this.Reliability = reliability;
    }

    public static async Task<YggdrasilPublicPeerInfo[]> GetPublicPeers(
        CancellationToken cancel = default) {
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync("https://publicpeers.neilalexander.dev/", cancel)
                           .ConfigureAwait(false);

        var peers = new List<YggdrasilPublicPeerInfo>();
        var rows = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'status')]");

        if (rows is null)
            throw new NotSupportedException(
                "Public peer list format changed. Unable to find entries.");

        foreach (var row in rows) {
            var addressNode = row.SelectSingleNode(".//td[@id='address']");
            var reliabilityNode = row.SelectSingleNode(".//td[@id='reliability']");

            if (addressNode is null || reliabilityNode is null)
                continue;

            string address = addressNode.InnerText.Trim();
            string reliabilityText = reliabilityNode.InnerText.Trim().TrimEnd('%');

            if (Uri.TryCreate(address, UriKind.Absolute, out Uri? uri) &&
                int.TryParse(reliabilityText, out int reliability)) {
                peers.Add(new(uri, reliability));
            } else {
                throw new NotSupportedException(
                    "Public peer list format changed. Unable to parse row.");
            }
        }

        return peers.ToArray();
    }
}