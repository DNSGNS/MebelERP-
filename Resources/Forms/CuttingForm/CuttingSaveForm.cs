using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public class CuttingSaveForm : INotifyPropertyChanged
{
    // Параметры листа
    private double _sheetLength;
    public double SheetLength { get => _sheetLength; set { _sheetLength = value; OnPropertyChanged(); } }

    private double _sheetWidth;
    public double SheetWidth { get => _sheetWidth; set { _sheetWidth = value; OnPropertyChanged(); } }

    private double _sheetArea;
    public double SheetArea { get => _sheetArea; set { _sheetArea = value; OnPropertyChanged(); } }


    // Итоги раскроя
    private int _totalSheets;
    public int TotalSheets { get => _totalSheets; set { _totalSheets = value; OnPropertyChanged(); } }

    private double _totalSheetArea;
    public double TotalSheetArea { get => _totalSheetArea; set { _totalSheetArea = value; OnPropertyChanged(); } }

    private int _totalPartsCount;
    public int TotalPartsCount { get => _totalPartsCount; set { _totalPartsCount = value; OnPropertyChanged(); } }

    private double _totalPartsArea;
    public double TotalPartsArea { get => _totalPartsArea; set { _totalPartsArea = value; OnPropertyChanged(); } }

    // Кромка 1
    private string _edge1Name;
    public string Edge1Name { get => _edge1Name; set { _edge1Name = value; OnPropertyChanged(); } }

    private double _edge1Thickness;
    public double Edge1Thickness { get => _edge1Thickness; set { _edge1Thickness = value; OnPropertyChanged(); } }

    private double _totalEdge1;
    public double TotalEdge1 { get => _totalEdge1; set { _totalEdge1 = value; OnPropertyChanged(); } }

    // Кромка 2
    private string _edge2Name;
    public string Edge2Name { get => _edge2Name; set { _edge2Name = value; OnPropertyChanged(); } }

    private double _edge2Thickness;
    public double Edge2Thickness { get => _edge2Thickness; set { _edge2Thickness = value; OnPropertyChanged(); } }

    private double _totalEdge2;
    public double TotalEdge2 { get => _totalEdge2; set { _totalEdge2 = value; OnPropertyChanged(); } }

    //Детали раскроя
    private ObservableCollection<CuttingDetails> _details = new();
    public ObservableCollection<CuttingDetails> Details
    {
        get => _details;
        set { _details = value; OnPropertyChanged(); }
    }

    private string? _materialColor;
    public string? MaterialColor
    {
        get => _materialColor;
        set { _materialColor = value; OnPropertyChanged(); }
    }



    // Листы раскроя (чертежи)
    private ObservableCollection<SheetLayout> _sheets = new();
    public ObservableCollection<SheetLayout> Sheets
    {
        get => _sheets;
        set { _sheets = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}