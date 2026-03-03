using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public class FurnitureForm : INotifyPropertyChanged
{
    public FurnitureForm()
    {
        if (Profiles == null) Profiles = new ObservableCollection<FurnitureProfileForm>();
        if (Profiles.Count == 0)
        {
            Profiles.Add(new FurnitureProfileForm());
        }
    }

    private double? _hdf;
    private double? _naprBez;
    private double? _naprS;
    private double? _petliBez;
    private double? _petliS;
    private double? _skoba;
    private double? _nakladnaya;
    private double? _knopka;
    private double? _gola;
    private double? _truba;
    private double? _gazLift;
    private double? _kruchki;
    private double? _podsvetka;
    private string? _comment;
    public ObservableCollection<FurnitureProfileForm> Profiles { get; set; } = new();

    // Свойства для каждого поля
    public double? Hdf { get => _hdf; set { _hdf = value; OnPropertyChanged(); } }
    public double? NaprBez { get => _naprBez; set { _naprBez = value; OnPropertyChanged(); } }
    public double? NaprS { get => _naprS; set { _naprS = value; OnPropertyChanged(); } }
    public double? PetliBez { get => _petliBez; set { _petliBez = value; OnPropertyChanged(); } }
    public double? PetliS { get => _petliS; set { _petliS = value; OnPropertyChanged(); } }
    public double? Skoba { get => _skoba; set { _skoba = value; OnPropertyChanged(); } }
    public double? Nakladnaya { get => _nakladnaya; set { _nakladnaya = value; OnPropertyChanged(); } }
    public double? Knopka { get => _knopka; set { _knopka = value; OnPropertyChanged(); } }
    public double? Gola { get => _gola; set { _gola = value; OnPropertyChanged(); } }
    public double? Truba { get => _truba; set { _truba = value; OnPropertyChanged(); } }
    public double? GazLift { get => _gazLift; set { _gazLift = value; OnPropertyChanged(); } }
    public double? Kruchki { get => _kruchki; set { _kruchki = value; OnPropertyChanged(); } }
    public double? Podsvetka { get => _podsvetka; set { _podsvetka = value; OnPropertyChanged(); } }
    public string? Comment { get => _comment; set { _comment = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}