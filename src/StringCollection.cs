namespace Yggdrasil;

using Hjson;

class StringCollection(JsonArray json): Collection<string>(json) {
    protected override string Parse(JsonValue arg) => arg.Qs();

    protected override JsonValue Serialize(string arg) => arg;
}