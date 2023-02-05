using Aniwari.DAL.Jikan;

namespace Aniwari.DAL.Interfaces;

public interface ITitle
{
    public Dictionary<TitleType, List<string>> Titles { get; set; }

    public string GetDefaultTitle()
    {
        return Titles[TitleType.Default][0];
    }

    public string? GetTitle(TitleType type)
    {
        if (Titles[type].Count == 0) return null;

        return Titles[type][0];
    }

    public void UpdateTitles(Dictionary<TitleType, List<string>> titles)
    {
        var oldTitles = new Dictionary<TitleType, List<string>>(titles);

        // remove items that are in OLD but are not in NEW
        foreach (var (k, v) in oldTitles)
        {
            foreach (var item in v)
            {
                if (!titles[k].Contains(item))
                {
                    Titles[k].Remove(item);
                }
            }
        }

        // add items that are in NEW but are not in OLD
        foreach (var (k, v) in titles)
        {
            if (!Titles.ContainsKey(k))
            {
                Titles[k] = new List<string>();
            }

            foreach (var item in v)
            {
                if (!Titles[k].Contains(item))
                {
                    Titles[k].Add(item);
                }
            }
        }
    }
}
