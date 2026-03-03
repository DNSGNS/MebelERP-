using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public class CuttingSettingForm : INotifyPropertyChanged
{
    // Список доступных размеров листов
    public List<SheetSizeOption> AvailableSheetSizes { get; } = new()
    {
        new SheetSizeOption("2440 x 1830", 2440, 1830),
        new SheetSizeOption("2750 x 1830", 2750, 1830),
        new SheetSizeOption("2750 x 1750", 2750, 1750),
        new SheetSizeOption("2800 x 2070", 2800, 2070),
        new SheetSizeOption("2800 x 1220", 2800, 1220)
    };

    private SheetSizeOption _selectedSheetSize;
    public SheetSizeOption SelectedSheetSize
    {
        get => _selectedSheetSize;
        set
        {
            if (_selectedSheetSize != value)
            {
                _selectedSheetSize = value;
                OnPropertyChanged();

                if (value != null)
                {
                    // Данные записываются в свойства, даже если их нет на экране
                    SheetLength = value.Length;
                    SheetWidth = value.Width;
                }
            }
        }
    }

    private double? _sheetLength;
    public double? SheetLength
    {
        get => _sheetLength;
        set { _sheetLength = value; OnPropertyChanged(); }
    }

    private double? _sheetWidth;
    public double? SheetWidth
    {
        get => _sheetWidth;
        set { _sheetWidth = value; OnPropertyChanged(); }
    }

    private double _cutWidth = 4.0; // Значение по умолчанию (ширина пилы)
    public double CutWidth
    {
        get => _cutWidth;
        set { _cutWidth = value; OnPropertyChanged(); }
    }

    private double _edgeOffset = 10.0; // Значение по умолчанию (опил края)
    public double EdgeOffset
    {
        get => _edgeOffset;
        set { _edgeOffset = value; OnPropertyChanged(); }
    }

    private string _cuttingMethod = "По длине"; // "По длине" или "По ширине"
    public string CuttingMethod
    {
        get => _cuttingMethod;
        set { _cuttingMethod = value; OnPropertyChanged(); }
    }

    public List<string> CuttingMethods { get; } = new() { "По длине", "По ширине" };

    // --- НАСТРОЙКИ КРОМКИ ---
    private string _edge1Name = "2mm";
    public string Edge1Name { get => _edge1Name; set { _edge1Name = value; OnPropertyChanged(); } }

    private double _edge1Thickness = 2.0;
    public double Edge1Thickness { get => _edge1Thickness; set { _edge1Thickness = value; OnPropertyChanged(); } }

    private string _edge2Name = "1mm";
    public string Edge2Name { get => _edge2Name; set { _edge2Name = value; OnPropertyChanged(); } }

    private double _edge2Thickness = 1.0;
    public double Edge2Thickness { get => _edge2Thickness; set { _edge2Thickness = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// Вспомогательный класс для списка размеров
public class SheetSizeOption
{
    public string Name { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }

    public SheetSizeOption(string name, double l, double w)
    {
        Name = name;
        Length = l;
        Width = w;
    }
    public override string ToString() => Name;
}