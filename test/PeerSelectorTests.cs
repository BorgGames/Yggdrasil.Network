namespace Yggdrasil;

using System.Threading.Tasks;

public class PeerSelectorTests {
    [Fact]
    public async Task CanGetPublicPeers() {
        var candidates = await YggdrasilPublicPeerInfo.GetPublicPeers();
        var peers = await YggdrasilPeerSelector.GetLowestLatency(candidates
                                                                 .Where(c => c.Reliability > 90)
                                                                 .Select(c => c.Uri));
        Assert.Equal(YggdrasilPeerSelector.RecommendedPeerCount, peers.Length);
    }
}