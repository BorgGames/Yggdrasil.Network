namespace Yggdrasil;

using Hjson;

class UriCollection(JsonArray json): Collection<Uri>(json) {
    protected override Uri Parse(JsonValue arg) => ParseUri(arg);
    protected override JsonValue Serialize(Uri arg) => arg.ToString();

    public static Uri ParseUri(JsonValue arg) => new(arg.Qs(), UriKind.Absolute);
}