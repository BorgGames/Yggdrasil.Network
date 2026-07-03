namespace Yggdrasil;

using System.Globalization;
using System.Net;

public class KeyTests {
    [Fact]
    public void Roundtrip() {
        var key = YggdrasilKey.Generate();
        var key2 = YggdrasilKey.Parse(key.ToString());
        Assert.Equal(key, key2);
    }

    [Fact]
    public void GetIPMatchesReferenceDerivation() {
        for (int attempt = 0; attempt < 64; attempt++) {
            var key = YggdrasilKey.Generate();
            Assert.Equal(ReferenceAddress(PublicKeyOf(key)), key.GetIP());
        }
    }

    [Fact]
    public void GetIPKnownVector() {
        var key = YggdrasilKey.Parse(KNOWN_KEY);
        Assert.Equal(IPAddress.Parse(KNOWN_ADDRESS), key.GetIP());
    }

    // verified against `yggdrasil -useconffile <conf> -address` (yggdrasil-go 0.5.14);
    // the key has 3 leading 1-bits in the inverted public key, which v0.0.5 GetIP
    // double-counted, reporting 206:... instead of 203:...
    const string KNOWN_KEY =
        "477b88343c3ec63b42c9adfea212cca0a5e0e7611fd232074721c2259f21907f"
      + "1e7502f8a6081eccc5c95a18339426d1d4e0a9c7d0eb4ba4e19e9b0d883b7b76";
    const string KNOWN_ADDRESS = "203:18af:d075:9f7e:1333:a36a:5e7c:c6bd";

    static byte[] PublicKeyOf(YggdrasilKey key) {
        // ToString() is the config format: 64-byte expanded ed25519 private key
        // in hex, whose second half is the public key
        string hex = key.ToString();
        byte[] pub = new byte[32];
        for (int i = 0; i < 32; i++)
            pub[i] = byte.Parse(hex.Substring(64 + i * 2, 2), NumberStyles.HexNumber);
        return pub;
    }

    // independent implementation of the spec: 0x02, then the count of leading
    // 1-bits of ~publicKey, then the bits that follow the first 0-bit
    static IPAddress ReferenceAddress(byte[] publicKey) {
        int[] bits = new int[256];
        for (int i = 0; i < 256; i++)
            bits[i] = (~publicKey[i / 8] >> (7 - i % 8)) & 1;

        int ones = 0;
        while (bits[ones] == 1) ones++;

        byte[] addr = new byte[16];
        addr[0] = 0x02;
        addr[1] = (byte)ones;
        for (int bit = 0; bit < 112; bit++)
            addr[2 + bit / 8] = (byte)((addr[2 + bit / 8] << 1) | bits[ones + 1 + bit]);
        return new IPAddress(addr);
    }
}
