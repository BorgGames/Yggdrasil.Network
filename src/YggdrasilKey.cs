namespace Yggdrasil;

using System.Globalization;

using Rebex.Security.Cryptography;

public sealed class YggdrasilKey: IEquatable<YggdrasilKey> {
    readonly Ed25519 key;

    public static YggdrasilKey Generate() {
        return new(new Ed25519());
    }

    YggdrasilKey(Ed25519 key) {
        this.key = key;
    }

    public static YggdrasilKey Parse(string configValue) {
        if (configValue is null) throw new ArgumentNullException(nameof(configValue));
        if (configValue.Length != 128)
            throw new ArgumentException("Expanded ed25519 keys in hex are 128 characters long",
                                        nameof(configValue));
        byte[] bytes = new byte[64];
        for (int i = 0; i < 64; i++)
            bytes[i] = byte.Parse(configValue.Substring(i * 2, 2), NumberStyles.HexNumber);
        var key = new Ed25519();
        key.FromPrivateKey(bytes);
        return new(key);
    }

    public IPAddress GetIP() {
        // Derive Yggdrasil IPv6 address from the public key.
        byte[] pub = this.key.GetPublicKey();

        Span<byte> inv = stackalloc byte[32];
        for (int i = 0; i < 32; i++) inv[i] = (byte)~pub[i];

        int ones = CountLeadingOnes(inv);
        Span<byte> payload = stackalloc byte[32];
        int nBits = 0, acc = 0, written = 0;
        bool skippedFirstZero = false;

        for (int bitIndex = 0; bitIndex < inv.Length * 8; bitIndex++) {
            int bit = ((inv[bitIndex / 8] & (0x80 >> (bitIndex % 8))) != 0) ? 1 : 0;

            if (!skippedFirstZero) {
                if (bit == 1) continue; // already counted by CountLeadingOnes
                skippedFirstZero = true; // skip this first 0
                continue;
            }

            acc = (acc << 1) | bit;
            if (++nBits == 8) {
                payload[written++] = (byte)acc;
                nBits = 0;
                if (written == payload.Length) break;
            }
        }

        if (nBits != 0)
            payload[written++] = (byte)(acc << (8 - nBits));

        byte[] addr = new byte[16];
        addr[0] = 0x02;            // 200::/8 host space
        addr[1] = (byte)ones;
        payload[..Math.Min(14, written)].CopyTo(addr.AsSpan(2));

        return new(addr);
    }

    public override string ToString() {
        byte[] bytes = this.key.GetPrivateKey();
        char[] chars = new char[128];
        for (int i = 0; i < 64; i++) {
            string hex = bytes[i].ToString("x2");
            chars[i * 2] = hex[0];
            chars[i * 2 + 1] = hex[1];
        }

        return new string(chars);
    }

    static int CountLeadingOnes(ReadOnlySpan<byte> data) {
        int ones = 0;
        foreach (byte b in data) {
            if (b == 0xFF) { ones += 8; continue; }
            for (int i = 7; i >= 0; i--) {
                if (((b >> i) & 1) == 1) ones++;
                else return ones;
            }
        }
        return ones;
    }

    #region Equality
    static bool SameKey(Ed25519 left, Ed25519 right) {
        byte[] leftBytes = left.GetPrivateKey();
        byte[] rightBytes = right.GetPrivateKey();
        for (int i = 0; i < 64; i++)
            if (leftBytes[i] != rightBytes[i])
                return false;
        return true;
    }

    public bool Equals(YggdrasilKey? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return SameKey(this.key, other.key);
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is YggdrasilKey other && this.Equals(other);
    }

    public override int GetHashCode() => BitConverter.ToInt32(this.key.GetPrivateKey(), 0);
    #endregion
}