using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public class WorkLogItem : INotifyPropertyChanged
{
    // --- Данные "только для чтения" (подтягиваются из Заказа) ---
    public DateTime Date { get; set; }
    public string OrderName { get; set; } // Заказ (Объект)
    public string WorkerName { get; set; }

    // --- Редактируемые поля ---

    private bool _isMeasurement;
    public bool IsMeasurement
    {
        get => _isMeasurement;
        set { _isMeasurement = value; OnPropertyChanged(); }
    }

    private bool _isInstallation;
    public bool IsInstallation
    {
        get => _isInstallation;
        set { _isInstallation = value; OnPropertyChanged(); }
    }

    private string _material = "ЛДСП";
    public string Material
    {
        get => _material;
        set { _material = value; OnPropertyChanged(); }
    }

    // --- Числовые показатели (участвуют в формуле) ---

    private double _sawing; // Пила м2
    public double Sawing { get => _sawing; set { _sawing = value; OnPropertyChanged(); RecalculateTotal(); } }

    private double _edging; // Кромление м
    public double Edging { get => _edging; set { _edging = value; OnPropertyChanged(); RecalculateTotal(); } }

    private double _drilling; // Присадка м2
    public double Drilling { get => _drilling; set { _drilling = value; OnPropertyChanged(); RecalculateTotal(); } }

    private int _doorCount; // Двери купе шт
    public int DoorCount { get => _doorCount; set { _doorCount = value; OnPropertyChanged(); RecalculateTotal(); } }

    private double _packaging; // Упаковка м2
    public double Packaging { get => _packaging; set { _packaging = value; OnPropertyChanged(); RecalculateTotal(); } }

    private int _installationCount; // Установка (кол-во чел)
    public int InstallationCount { get => _installationCount; set { _installationCount = value; OnPropertyChanged(); RecalculateTotal(); } }

    private double _sandingSoap; // Шлифовка МЫЛО м2
    public double SandingSoap { get => _sandingSoap; set { _sandingSoap = value; OnPropertyChanged(); RecalculateTotal(); } }

    private double _sandingFreza; // Шлифовка ФРЕЗА м2
    public double SandingFreza { get => _sandingFreza; set { _sandingFreza = value; OnPropertyChanged(); RecalculateTotal(); } }

    private double _milling; // Фрезеровка м
    public double Milling { get => _milling; set { _milling = value; OnPropertyChanged(); RecalculateTotal(); } }

    private decimal _extraCost; // Доп руб
    public decimal ExtraCost { get => _extraCost; set { _extraCost = value; OnPropertyChanged(); RecalculateTotal(); } }

    private string _comment;
    public string Comment
    {
        get => _comment;
        set { _comment = value; OnPropertyChanged(); }
    }

    // --- ИТОГ ---
    private double _total;
    public double Total
    {
        get => _total;
        private set { _total = value; OnPropertyChanged(); }
    }

    private void RecalculateTotal()
    {
        // Простая сумма всех показателей по вашему требованию
        // (int приводится к double, decimal к double для суммы)
        Total = Sawing + Edging + Drilling + DoorCount + Packaging +
                InstallationCount + SandingSoap + SandingFreza + Milling + (double)ExtraCost;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}