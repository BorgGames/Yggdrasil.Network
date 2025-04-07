namespace Yggdrasil;

using System.Net;

public class IPTests {
    static readonly IPAddress PublicV6 = IPAddress.Parse("2607:f8b0:400a:801::200e");
    static readonly IPAddress LinkLocalV6 = IPAddress.Parse("fe80::9408:9e72:f01b:3f03%31");
    static readonly IPAddress YggdrasilV6_1 = IPAddress.Parse("324:71e:281a:9ed3::41");
    static readonly IPAddress YggdrasilV6_2 = IPAddress.Parse("200:6f99:2afe:41fd:fc3b:b1f7:af9d:f3a0");

    [Fact]
    public void IsYggdrasil() {
        Assert.False(PublicV6.IsYggdrasil());
        Assert.False(LinkLocalV6.IsYggdrasil());

        Assert.False(IPAddress.IPv6Any.IsYggdrasil());
        Assert.False(IPAddress.IPv6Loopback.IsYggdrasil());
        Assert.False(IPAddress.IPv6None.IsYggdrasil());

        Assert.True(YggdrasilV6_1.IsYggdrasil());
        Assert.True(YggdrasilV6_2.IsYggdrasil());
    }
}
