namespace Yggdrasil;

using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Hjson;

public class Conf {
    readonly JsonObject json;

    public YggdrasilKey PrivateKey {
        get => YggdrasilKey.Parse(this.json["PrivateKey"].Qs());
        set => this.json["PrivateKey"] = value.ToString();
    }
    /// <summary>
    /// List of connection strings for outbound peer connections in URI format,
    /// e.g. tls://a.b.c.d:e or socks://a.b.c.d:e/f.g.h.i:j. These connections
    /// will obey the operating system routing table, therefore you should
    /// use this section when you may connect via different interfaces.
    /// </summary>
    public ICollection<Uri> Peers => new UriCollection(this.json["Peers"].Qa());

    /// <summary>
    /// List of connection strings for outbound peer connections in URI format,
    /// arranged by source interface, e.g. { "eth0": [ "tls://a.b.c.d:e" ] }.
    /// Note that SOCKS peerings will NOT be affected by this option and should
    /// go in the "Peers" section instead.
    /// </summary>
    public Dictionary<string, Uri[]> InterfacePeers => throw new NotImplementedException();

    /// <summary>
    /// Listen addresses for incoming connections. You will need to add
    /// listeners in order to accept incoming peerings from non-local nodes.
    /// Multicast peer discovery will work regardless of any listeners set
    /// here. Each listener should be specified in URI format as above, e.g.
    /// tls://0.0.0.0:0 or tls://[::]:0 to listen on all interfaces.
    /// </summary>
    public ICollection<Uri> Listen => new UriCollection(this.json["Listen"].Qa());
    
    /// <summary>
    /// Listen address for admin connections.
    /// Default is to listen for local connections either on TCP/9001 or a UNIX socket
    /// depending on your platform.
    ///
    /// To disable the admin socket, use the value <see cref="ADMIN_LISTEN_NONE"/> instead.
    /// </summary>
    public string AdminListen {
        get => this.json["AdminListen"].Qs();
        set => this.json["AdminListen"] = value;
    }
    
    public const string ADMIN_LISTEN_NONE = "none";

    /// <summary>
    /// Configuration for which interfaces multicast peer discovery should be
    /// enabled on. Each entry in the list should be a json object which may
    /// contain Regex, Beacon, Listen, and Port. Regex is a regular expression
    /// which is matched against an interface name, and interfaces use the
    /// first configuration that they match gainst. Beacon configures whether
    /// or not the node should send link-local multicast beacons to advertise
    /// their presence, while listening for incoming connections on Port.
    /// Listen controls whether or not the node listens for multicast beacons
    /// and opens outgoing connections.
    /// </summary>
    public ICollection<YggdrasilPeerDiscoveryInterface> MulticastInterfaces
        => new PeerDiscoveryInterfaceCollection(this.json["MulticastInterfaces"].Qa());

    /// <summary>
    /// List of peer public keys to allow incoming peering connections
    /// from. If left empty/undefined then all connections will be allowed
    /// by default. This does not affect outgoing peerings, nor does it
    /// affect link-local peers discovered via multicast.
    /// </summary>
    public ICollection<string> AllowedPublicKeys
        => new StringCollection(this.json["AllowedPublicKeys"].Qa());

    /// <summary>
    /// Local network interface name for TUN adapter, or "auto" to select
    /// an interface automatically, or "none" to run without TUN.
    /// </summary>
    public string IfName {
        get => this.json["IfName"].Qs();
        set => this.json["IfName"] = value;
    }

    /// <summary>
    /// Maximum Transmission Unit (MTU) size for your local TUN interface.
    /// Default is the largest supported size for your platform. The lowest
    /// possible value is 1280.
    /// </summary>
    [DefaultValue(65535)]
    public int IfMTU {
        get => this.json["IfMTU"].Qi();
        set => this.json["IfMTU"] = value;
    }


    /// <summary>
    /// By default, nodeinfo contains some defaults including the platform,
    /// architecture and Yggdrasil version. These can help when surveying
    /// the network and diagnosing network routing problems. Enabling
    /// nodeinfo privacy prevents this, so that only items specified in
    /// <see cref="NodeInfo"/> are sent back if specified.
    /// </summary>
    public bool NodeInfoPrivacy {
        get => this.json["NodeInfoPrivacy"].Qb();
        set => this.json["NodeInfoPrivacy"] = value;
    }

    /// <summary>
    /// Optional node info. This is entirely optional but, if set, is visible
    /// to the whole network on request.
    /// </summary>
    public Dictionary<string, string> NodeInfo => throw new NotImplementedException();

    Conf(JsonObject json) {
        this.json = json ?? throw new ArgumentNullException(nameof(json));
    }

    public static Conf Load(Stream stream) {
        if (stream is null) throw new ArgumentNullException(nameof(stream));

        return new((JsonObject)HjsonValue.Load(stream));
    }

    public static Conf Load(string value) {
        if (value is null) throw new ArgumentNullException(nameof(value));

        return new((JsonObject)HjsonValue.Parse(value));
    }

    public void SaveSystem() {
        string path = SystemConfigPath();
        File.WriteAllText(path, this.json.ToString(Stringify.Hjson), Encoding.UTF8);
    }

    public static Conf LoadSystem() {
        string path = SystemConfigPath();
        return Load(File.ReadAllText(path, Encoding.UTF8));
    }

    public static string SystemConfigPath() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            string programData =
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return Path.Combine(programData, "Yggdrasil", "yggdrasil.conf");
        }

        throw new PlatformNotSupportedException();
    }

    public static Conf GenerateDefault()
        => new(new JsonObject {
            ["Peers"] = new JsonArray(),
            ["InterfacePeers"] = new JsonObject(),
            ["Listen"] = new JsonArray(),
            ["MulticastInterfaces"] = new JsonArray { },
            ["AllowedPublicKeys"] = new JsonArray(),
            ["NodeInfo"] = new JsonObject(),
        }) {
            AdminListen = ADMIN_LISTEN_NONE,
            PrivateKey = YggdrasilKey.Generate(),
            MulticastInterfaces = {
                new(new()) {
                    Regex = ".*",
                    Beacon = true,
                    Listen = true,
                    Port = 0,
                    Priority = 0,
                    Password = "",
                },
            },
            IfName = "auto",
            IfMTU = 65535,
        };

    public override string ToString() => this.json.ToString(Stringify.Hjson);
}