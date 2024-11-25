namespace Yggdrasil;

public class KeyTests {
    [Fact]
    public void Roundtrip() {
        var key = YggdrasilKey.Generate();
        var key2 = YggdrasilKey.Parse(key.ToString());
        Assert.Equal(key, key2);
    }
}