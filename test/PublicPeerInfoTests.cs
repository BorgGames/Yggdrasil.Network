namespace Yggdrasil;

using System.Threading.Tasks;

public class PublicPeerInfoTests {
    [Fact]
    public async Task CanGetPublicPeers() {
        var peers = await YggdrasilPublicPeerInfo.GetPublicPeers();
        Assert.NotEmpty(peers);
    }
}