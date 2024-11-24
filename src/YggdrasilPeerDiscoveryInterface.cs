namespace Yggdrasil;

using Hjson;

public class YggdrasilPeerDiscoveryInterface {
    internal readonly JsonObject Json;

    public string Regex {
        get => this.Json["Regex"].Qs();
        set => this.Json["Regex"] = value;
    }
    public bool Beacon {
        get => this.Json["Beacon"].Qb();
        set => this.Json["Beacon"] = value;
    }
    public bool Listen {
        get => this.Json["Listen"].Qb();
        set => this.Json["Listen"] = value;
    }
    public int Port {
        get => this.Json["Port"].Qi();
        set => this.Json["Port"] = value;
    }
    public int Priority {
        get => this.Json["Priority"].Qi();
        set => this.Json["Priority"] = value;
    }
    public string Password {
        get => this.Json["Password"].Qs();
        set => this.Json["Password"] = value;
    }

    internal YggdrasilPeerDiscoveryInterface(JsonObject json) {
        this.Json = json ?? throw new ArgumentNullException(nameof(json));
    }

    public YggdrasilPeerDiscoveryInterface() {
        this.Json = new();
    }
}