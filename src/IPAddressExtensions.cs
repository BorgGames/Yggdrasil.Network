namespace Yggdrasil;

using System.Net;
using System.Net.Sockets;

public static class IPAddressExtensions {
    const byte YGGDRASIL_NETWORK_BYTE = 0x02;
    const byte YGGDRASIL_PREFIX_MASK = 0xFE;

    public static bool IsYggdrasil(this IPAddress ip) {
        if (ip is null) throw new ArgumentNullException(nameof(ip));

        return ip.AddressFamily == AddressFamily.InterNetworkV6
            && (ip.GetAddressBytes()[0] & YGGDRASIL_PREFIX_MASK) == YGGDRASIL_NETWORK_BYTE;
    }
}
