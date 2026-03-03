using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1
{
    // Типы для "Других вставок"
    public enum OtherInsertType
    {
        MDF,
        Lacobel,
        GraphiteMirror,
        MatteGlass,
        GraphiteGlass
    }

    public class SingleInsert : INotifyPropertyChanged
    {
        private double? _length;
        private double? _width;
        private int? _count = 0;

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

        public virtual double Area => ((Length ?? 0) * (Width ?? 0) * (Count ?? 0)) / 1000000.0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RouterInsert : SingleInsert
    {
        public override double Area =>
            (((Length ?? 0) + (Width ?? 0)) * 2 * (Count ?? 0)) / 1000.0;
    }
    
    public class OtherInsert : SingleInsert
    {
        private OtherInsertType _selectedType;
        public OtherInsertType SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
        }

        // Список для UI
        public List<OtherInsertType> Types { get; } =
            Enum.GetValues(typeof(OtherInsertType)).Cast<OtherInsertType>().ToList();

        public OtherInsert()
        {
            SelectedType = OtherInsertType.MDF;
        }
    }

    public class DoorForm : INotifyPropertyChanged
    {
        private int _doorCount;
        public int DoorCount
        {
            get => _doorCount;
            set { _doorCount = value; OnPropertyChanged(); }
        }

        public RouterInsert Router { get; set; } = new();
        public SingleInsert Mirror { get; set; } = new();
        public SingleInsert Ldsp { get; set; } = new();
        public OtherInsert Other { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}