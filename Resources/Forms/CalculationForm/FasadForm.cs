using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public enum FasadType { Standard, AGT, Mirror, Aluminium, LDSP, NonStandard }
public enum FasadEdgeType { Freza, Mylo }

public class FasadForm : INotifyPropertyChanged
{
    private double? _length;
    private double? _width;
    private int? _count = 1;
    private FasadType _selectedType;
    private FasadEdgeType _selectedEdgeType;
    private string _color;
    private string _millingText;

    // Новое поле для выделения
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
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

    public FasadType SelectedType
    {
        get => _selectedType;
        set { _selectedType = value; OnPropertyChanged(); }
    }

    public FasadEdgeType SelectedEdgeType
    {
        get => _selectedEdgeType;
        set { _selectedEdgeType = value; OnPropertyChanged(); }
    }

    public string Color
    {
        get => _color;
        set { _color = value; OnPropertyChanged(); }
    }

    public string MillingText
    {
        get => _millingText;
        set { _millingText = value; OnPropertyChanged(); }
    }

    public List<FasadEdgeType> EdgeTypes { get; } = new() { FasadEdgeType.Freza, FasadEdgeType.Mylo };

    public List<FasadType> SpecialTypes { get; } = new()
        { FasadType.AGT, FasadType.Mirror, FasadType.Aluminium, FasadType.LDSP };

    public List<FasadType> NonStandardTypes { get; } = new()
        { FasadType.NonStandard };

    public double Area => ((Length ?? 0) * (Width ?? 0) * (Count ?? 0)) / 1000000.0;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}