using System.Text.Json.Serialization;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.UserAggregation;

public class Repertoire
{
    [JsonPropertyName("Sheets")] public List<SheetSearchResult> Sheets { get; }

    [JsonConstructor]
    public Repertoire()
    {
        Sheets = [];
    }


    public Repertoire(SheetSearchResult[] sheets) : this()
    {
        Sheets = sheets.ToList();
    }

    public bool TryAdd(SheetSearchResult sheet)
    {
        if (Exists(sheet))
        {
            return false;
        }
            
        sheet.FillId();

        Sheets.Add(sheet);
        return true;
    }

    public void Remove(SheetSearchResult sheet)
    {
        if (Exists(sheet))
            Sheets.Remove(sheet);
    }

    public void Up(SheetSearchResult sheet)
    {
        MoveUp(sheet, Sheets.IndexOf(sheet) - 1);
    }

    public void Down(SheetSearchResult sheet)
    {
        MoveDown(sheet, Sheets.IndexOf(sheet) + 1);
    }

    public void First(SheetSearchResult sheet)
    {
        MoveUp(sheet, 0);
    }

    public void Last(SheetSearchResult sheet)
    {
        MoveDown(sheet, Sheets.Count - 1);
    }

    public void Clear()
    {
        Sheets.Clear();
    }

    private bool Exists(SheetSearchResult sheet)
    {
        return Sheets.Any(s => s.Equals(sheet));
    }

    private void MoveDown(SheetSearchResult sheet, int targetIndex)
    {
        Move(sheet, targetIndex, index => index < Sheets.Count - 1);
    }

    private void MoveUp(SheetSearchResult sheet, int targetIndex)
    {
        Move(sheet, targetIndex, index => index > 0);
    }

    private void Move(SheetSearchResult sheet, int targetIndex, Func<int, bool> func)
    {
        if (!Exists(sheet)) return;

        var index = Sheets.IndexOf(sheet);
        if (!func(index)) return;

        Sheets.RemoveAt(index);
        Sheets.Insert(targetIndex, sheet);
    }
}