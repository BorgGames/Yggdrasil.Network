namespace Yggdrasil;

using Hjson;

class PeerDiscoveryInterfaceCollection(JsonArray json): Collection<YggdrasilPeerDiscoveryInterface>(json) {
    protected override YggdrasilPeerDiscoveryInterface Parse(JsonValue arg) => new((JsonObject)arg);

    protected override JsonValue Serialize(YggdrasilPeerDiscoveryInterface arg) => arg.Json;
}