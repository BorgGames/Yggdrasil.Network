namespace Yggdrasil;

using System.Collections;

using Hjson;

abstract class Collection<T>(JsonArray json): ICollection<T> {
    readonly JsonArray json = json ?? throw new ArgumentNullException(nameof(json));

    protected abstract T Parse(JsonValue arg);
    protected abstract JsonValue Serialize(T arg);

    IEnumerable<T> Enumerable => this.json.Select(this.Parse);
    public IEnumerator<T> GetEnumerator() => this.Enumerable.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public void Add(T item) {
        if (item is null) throw new ArgumentNullException(nameof(item));
        this.json.Add(this.Serialize(item));
    }

    public void Clear() => this.json.Clear();

    public bool Contains(T item) {
        if (item is null) throw new ArgumentNullException(nameof(item));
        return this.Enumerable.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        if (array is null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (arrayIndex + this.Count > array.Length) throw new ArgumentException();
        foreach (var item in this.Enumerable) array[arrayIndex++] = item;
    }

    public bool Remove(T item) {
        if (item is null) throw new ArgumentNullException(nameof(item));
        int index = -1;
        int current = 0;
        foreach (var element in this.Enumerable) {
            if (item.Equals(element)) {
                index = current;
                break;
            }

            current++;
        }

        if (index == -1) return false;
        this.json.RemoveAt(index);
        return true;
    }

    public int Count => this.json.Count;
    public bool IsReadOnly => false;
}