using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

// Определяем перечисления для мягких элементов
public enum SoftShieldType { Peretyazhka, Karetka }
public enum SoftCategory { None, Standard, Peretyazhka, Karetka }

public class SoftlyForm : INotifyPropertyChanged
{
    private double? _length;
    private double? _width;
    private int? _count = 1;

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
    public double Area => ((Length ?? 0) * (Width ?? 0) * (Count ?? 0)) / 1000000.0;

    // --- Мягкий щит ---
    private SoftShieldType _selectedShieldType;
    public SoftShieldType SelectedShieldType
    {
        get => _selectedShieldType;
        set { _selectedShieldType = value; OnPropertyChanged(); }
    }

    // Список для привязки к UI (Picker/ComboBox)
    public List<SoftShieldType> ShieldTypes { get; } =
        Enum.GetValues(typeof(SoftShieldType)).Cast<SoftShieldType>().ToList();

    // --- Категории ---
    private SoftCategory _selectedCategory;
    public SoftCategory SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }

    public List<SoftCategory> Categories { get; } =
        Enum.GetValues(typeof(SoftCategory)).Cast<SoftCategory>().ToList();

    // --- Чекбоксы ---
    private bool _hasBase;
    public bool HasBase
    {
        get => _hasBase;
        set { _hasBase = value; OnPropertyChanged(); }
    }

    private bool _hasDryer;
    public bool HasDryer
    {
        get => _hasDryer;
        set { _hasDryer = value; OnPropertyChanged(); }
    }

    // --- Поля для целых чисел ---
    private int? _tabletop;
    public int? Tabletop
    {
        get => _tabletop;
        set { _tabletop = value; OnPropertyChanged(); }
    }

    private int? _apron;
    public int? Apron
    {
        get => _apron;
        set { _apron = value; OnPropertyChanged(); }
    }

    private int? _additional;
    public int? Additional
    {
        get => _additional;
        set { _additional = value; OnPropertyChanged(); }
    }

    public SoftlyForm()
    {
        SelectedShieldType = SoftShieldType.Peretyazhka;
        SelectedCategory = SoftCategory.None;
        HasBase = false;
        HasDryer = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}