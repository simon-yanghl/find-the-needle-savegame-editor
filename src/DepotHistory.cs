namespace FtnEditor;

/// <summary>
/// Experimental editing of purchase history, not a verified game price counter.
/// Never adds/removes world buildings or changes balances or unrelated records.
/// </summary>
public static class DepotHistory
{
    static bool Matches(GdVar entry) => entry?.List is { Count: >= 5 } list
        && list[1].Type is GdVar.STRING or GdVar.STRING_NAME && list[1].Str == "build"
        && list[2].Type is GdVar.STRING or GdVar.STRING_NAME && list[2].Str == "depot";

    public static int Count(SaveGame save) => save.State.Get("purchase_log")?.List?.Count(Matches) ?? 0;

    /// <summary>Keep the earliest N matching records; remove the latest excess records.</summary>
    public static int Trim(SaveGame save, int keep)
    {
        int current = Count(save);
        if (keep < 0 || keep > current) throw new ArgumentOutOfRangeException(nameof(keep));
        int remove = current - keep;
        if (remove == 0) return 0;
        var log = save.State.Get("purchase_log").List;
        for (int i = log.Count - 1, remaining = remove; i >= 0 && remaining > 0; i--)
            if (Matches(log[i])) { log.RemoveAt(i); remaining--; }
        save.Dirty = true;
        return remove;
    }
}
