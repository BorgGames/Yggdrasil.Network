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