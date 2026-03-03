using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace MyApp1;

public class LDSPForm : INotifyPropertyChanged
{
    private int _id;
    private double? _length;
    private double? _width;
    private int? _count = 1;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }
    public double? Length
    {
        get => _length;
        set { _length = value; OnPropertyChanged(); OnPropertyChanged(nameof(Area)); }
    }

    public double? Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(); OnPropertyChanged(nameof(Area)); }
    }

    public int? Count
    {
        get => _count;
        set { _count = value; OnPropertyChanged(); OnPropertyChanged(nameof(Area)); }
    }

    // Автоматический расчет площади
    public double Area => ((Length ?? 0) * (Width ?? 0) * (Count ?? 0)) / 1000000.0;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
