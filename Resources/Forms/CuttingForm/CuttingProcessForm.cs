using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MyApp1;

public class CuttingProcessForm : INotifyPropertyChanged
{
    private ObservableCollection<SheetLayout> _sheets = new ObservableCollection<SheetLayout>();

    public ObservableCollection<SheetLayout> Sheets
    {
        get => _sheets;
        set
        {
            if (_sheets != value)
            {
                _sheets = value;
                NotifyAllProperties();
            }
        }
    }

    // Вычисляемые свойства
    public int TotalSheets => Sheets?.Count ?? 0;
    public int TotalPartsCount => Sheets?.Sum(s => s.Parts.Count) ?? 0;
    public double TotalSheetArea => Sheets?.Sum(s => s.SheetW * s.SheetH) ?? 0;
    public double TotalPartsArea => Sheets?.Sum(s => s.Parts.Sum(p => p.Length * p.Width)) ?? 0;

    // Очистка
    public void Clear()
    {
        Sheets.Clear();
        NotifyAllProperties();
    }

    // Уведомление обо всех изменениях сразу
    public void NotifyAllProperties()
    {
        OnPropertyChanged(nameof(Sheets));
        OnPropertyChanged(nameof(TotalSheets));
        OnPropertyChanged(nameof(TotalPartsCount));
        OnPropertyChanged(nameof(TotalSheetArea));
        OnPropertyChanged(nameof(TotalPartsArea));
    }

    // Реализация INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}