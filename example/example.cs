using Yggdrasil;

var candidates = await YggdrasilPublicPeerInfo.GetPublicPeers();
var peers = await YggdrasilPeerSelector.GetLowestLatency(candidates
                                                         .Where(c => c.Reliability > 90)
                                                         .Select(c => c.Uri));
foreach (var peer in peers) {
    Console.WriteLine(peer);
}