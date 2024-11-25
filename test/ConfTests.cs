namespace Yggdrasil;

using System.IO;

using Hjson;

public class ConfTests {
    [Fact]
    public void LoadsSample() {
        var conf = Conf.Load(new MemoryStream(Sample));
        Assert.NotNull(conf.PrivateKey);
    }
    
    [Fact]
    public void GeneratesDefault() {
        var conf = Conf.GenerateDefault();
        Assert.NotNull(conf.PrivateKey);
    }

    static readonly byte[] Sample = """
                          {
                            # Your private key. DO NOT share this with anyone!
                            PrivateKey: 37ecced9fe7935260fceda65de3fa3382ae6cbccd02813c7d4df19be0981522aa9237ca21748a626b77aea1db975ddfde8fe1050c5f0ae04909f006273499813
                          
                            # List of connection strings for outbound peer connections in URI format,
                            # e.g. tls://a.b.c.d:e or socks://a.b.c.d:e/f.g.h.i:j. These connections
                            # will obey the operating system routing table, therefore you should
                            # use this section when you may connect via different interfaces.
                            Peers: [
                              "tls://ygg.jjolly.dev:3443",
                              "tcp://longseason.1200bps.xyz:13121",
                              "tcp://ygg3.mk16.de:1337?key=000003acdaf2a60e8de2f63c3e63b7e911d02380934f09ee5c83acb758f470c1"
                            ]
                          
                            # List of connection strings for outbound peer connections in URI format,
                            # arranged by source interface, e.g. { "eth0": [ "tls://a.b.c.d:e" ] }.
                            # Note that SOCKS peerings will NOT be affected by this option and should
                            # go in the "Peers" section instead.
                            InterfacePeers: {}
                          
                            # Listen addresses for incoming connections. You will need to add
                            # listeners in order to accept incoming peerings from non-local nodes.
                            # Multicast peer discovery will work regardless of any listeners set
                            # here. Each listener should be specified in URI format as above, e.g.
                            # tls://0.0.0.0:0 or tls://[::]:0 to listen on all interfaces.
                            Listen: []
                          
                            # Configuration for which interfaces multicast peer discovery should be
                            # enabled on. Each entry in the list should be a json object which may
                            # contain Regex, Beacon, Listen, and Port. Regex is a regular expression
                            # which is matched against an interface name, and interfaces use the
                            # first configuration that they match gainst. Beacon configures whether
                            # or not the node should send link-local multicast beacons to advertise
                            # their presence, while listening for incoming connections on Port.
                            # Listen controls whether or not the node listens for multicast beacons
                            # and opens outgoing connections.
                            MulticastInterfaces: [
                              {
                                Regex: .*
                                Beacon: true
                                Listen: true
                                Port: 0
                                Priority: 0
                                Password: ""
                              }
                            ]
                          
                            # List of peer public keys to allow incoming peering connections
                            # from. If left empty/undefined then all connections will be allowed
                            # by default. This does not affect outgoing peerings, nor does it
                            # affect link-local peers discovered via multicast.
                            AllowedPublicKeys: []
                          
                            # Local network interface name for TUN adapter, or "auto" to select
                            # an interface automatically, or "none" to run without TUN.
                            IfName: auto
                          
                            # Maximum Transmission Unit (MTU) size for your local TUN interface.
                            # Default is the largest supported size for your platform. The lowest
                            # possible value is 1280.
                            IfMTU: 65535
                          
                            # By default, nodeinfo contains some defaults including the platform,
                            # architecture and Yggdrasil version. These can help when surveying
                            # the network and diagnosing network routing problems. Enabling
                            # nodeinfo privacy prevents this, so that only items specified in
                            # "NodeInfo" are sent back if specified.
                            NodeInfoPrivacy: false
                          
                            # Optional node info. This must be a { "key": "value", ... } map
                            # or set as null. This is entirely optional but, if set, is visible
                            # to the whole network on request.
                            NodeInfo: {}
                          }


                          """u8.ToArray();
}