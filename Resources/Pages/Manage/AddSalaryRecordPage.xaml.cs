using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public partial class AddSalaryRecordPage : ContentPage
{
    private AddRecordViewModel _vm;
    private ApiService _apiService = new ApiService();

    public AddSalaryRecordPage(string workerName, Guid workerId, List<ProjectSimpleDto> projects)
    {
        InitializeComponent();
        _vm = new AddRecordViewModel(workerName, workerId, projects);
        BindingContext = _vm;
    }

    // НОВЫЙ Конструктор для РЕДАКТИРОВАНИЯ существующей записи
    public AddSalaryRecordPage(SalaryReportItem existingItem, List<ProjectSimpleDto> projects)
    {
        InitializeComponent();
        Title = "Редактирование записи"; // Меняем заголовок
        _vm = new AddRecordViewModel(existingItem, projects);
        BindingContext = _vm;
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        // Используем Dispatcher, чтобы выделение сработало после того, как поле получит фокус
        Dispatcher.Dispatch(() =>
        {
            if (sender is Entry entry)
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            }
            else if (sender is Editor editor)
            {
                editor.CursorPosition = 0;
                editor.SelectionLength = editor.Text?.Length ?? 0;
            }
        });
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_vm.SelectedProject == null)
        {
            await DisplayAlert("Ошибка", "Выберите проект", "OK");
            return;
        }

        try
        {
            _vm.IsBusy = true; // Показываем загрузку

            var itemToSave = new SalaryReportItem
            {
                Id = _vm.WorkId ?? Guid.Empty,
                DatePerformed = _vm.DatePerformed,
                WorkerId = _vm.WorkerId,
                WorkerName = _vm.WorkerName,
                ProjectId = _vm.SelectedProject.Id,
                ProjectName = _vm.SelectedProject.Name,
                Material = _vm.SelectedMaterial,
                Saw = _vm.Saw,
                Edging = _vm.Edging,
                Additive = _vm.Additive,
                DoorCanvas = _vm.DoorCanvas,
                DoorSectional = _vm.DoorSectional,
                Packaging = _vm.Packaging,
                GrindingSoap = _vm.GrindingSoap,
                GrindingFrez = _vm.GrindingFrez,
                Milling = _vm.Milling,
                Installation = _vm.InstallationAmount,
                Additionally = _vm.Additionally,
                Measurement = _vm.Measuring,
                Comment = _vm.Comment,
                TotalSalary = _vm.TotalSalary
            };

            bool success;
            if (_vm.WorkId == null)
                success = await _apiService.CreateSalaryRecordAsync(itemToSave);
            else
                success = await _apiService.UpdateSalaryRecordAsync(itemToSave);

            if (success)
            {
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось сохранить данные", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            _vm.IsBusy = false; // Скрываем загрузку в любом случае
        }
    }

    // Вспомогательная ViewModel для страницы
    public class AddRecordViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }
        public Guid? WorkId { get; set; }
        public string WorkerName { get; set; }
        public Guid WorkerId { get; set; }
        public List<ProjectSimpleDto> AllProjects { get; set; }
        public List<MaterialType> Materials { get; } = Enum.GetValues(typeof(MaterialType)).Cast<MaterialType>().ToList();

        public DateTime DatePerformed { get; set; } = DateTime.Now;
        private ProjectSimpleDto _selectedProject;
        public ProjectSimpleDto SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject != value)
                {
                    _selectedProject = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsProjectSelected));

                    if (_selectedProject == null) IsMeasurement = false;

                    // Пересчитываем всё, что зависит от проекта
                    RefreshCalculations();
                }
            }
        }

        // Это свойство будет управлять доступностью чекбокса
        public bool IsProjectSelected => SelectedProject != null;
        public MaterialType? SelectedMaterial { get; set; }

        public string Comment { get; set; }

        // Поля с пересчетом
        private decimal _saw; public decimal Saw { get => _saw; set { _saw = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }
        private decimal _edging; public decimal Edging { get => _edging; set { _edging = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }
        private decimal _additive; public decimal Additive { get => _additive; set { _additive = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }
        private int _doorCanvas; public int DoorCanvas { get => _doorCanvas; set { _doorCanvas = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }
        private int _doorSectional; public int DoorSectional { get => _doorSectional; set { _doorSectional = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }
        private decimal _packaging; public decimal Packaging { get => _packaging; set { _packaging = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }

        private decimal _grindingSoap; public decimal GrindingSoap { get => _grindingSoap; set { _grindingSoap = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }
        private decimal _grindingFrez; public decimal GrindingFrez { get => _grindingFrez; set { _grindingFrez = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }
        private decimal _milling; public decimal Milling { get => _milling; set { _milling = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }

        private decimal _additionally; public decimal Additionally { get => _additionally; set { _additionally = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); } }

        private bool _isInstallation;
        public bool IsInstallation
        {
            get => _isInstallation;
            set { _isInstallation = value; RefreshCalculations(); OnPropertyChanged(); }
        }

        private bool _isMeasurement;
        public bool IsMeasurement
        {
            get => _isMeasurement;
            set { _isMeasurement = value; RefreshCalculations(); OnPropertyChanged(); }
        }

        // Свойства для хранения рассчитанных сумм
        private decimal _installationAmount;
        public decimal InstallationAmount
        {
            get => _installationAmount;
            set { _installationAmount = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); }
        }

        private decimal _measuring;
        public decimal Measuring
        {
            get => _measuring;
            set { _measuring = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalary)); }
        }

        private void RefreshCalculations()
        {
            if (SelectedProject == null)
            {
                InstallationAmount = 0;
                Measuring = 0;
                return;
            }

            // Логика расчета установки
            if (IsInstallation)
            {
                InstallationAmount = SelectedProject.TotalProjectPrice < 80000
                    ? SelectedProject.TotalProjectPrice * 0.09m + 2000
                    : SelectedProject.TotalProjectPrice * 0.11m;
            }
            else { InstallationAmount = 0; }

            // Логика расчета замера
            Measuring = IsMeasurement
                ? SelectedProject.TotalProjectPrice * 0.065m + 1000
                : 0;

            // Уведомляем, что итоговая сумма изменилась
            OnPropertyChanged(nameof(TotalSalary));
        }

        public decimal TotalSalary
        {
            get
            {
                decimal materialCoefficient = SelectedMaterial switch
                {
                    MaterialType.LDSP => 40m,
                    MaterialType.MDF => 50m,
                    MaterialType.HDF => 10m,
                    MaterialType.AGT => 85m,
                    _ => 0m
                };

                return Measuring
                    + InstallationAmount
                    + (Saw * materialCoefficient)
                    + Edging * 20
                    + Additive * 150
                    + DoorCanvas * 600
                    + DoorSectional * 800
                    + Packaging * 100
                    + GrindingSoap * 200
                    + GrindingFrez * 300
                    + Milling * 50
                    + Additionally;
            }
        }

        public AddRecordViewModel(string name, Guid id, List<ProjectSimpleDto> projects)
        {
            WorkerName = name;
            WorkerId = id;
            AllProjects = projects;
        }

        public AddRecordViewModel(SalaryReportItem item, List<ProjectSimpleDto> projects)
        {
            WorkId = item.Id;

            AllProjects = projects;

            // Заполняем основные данные
            WorkerName = item.WorkerName;
            WorkerId = item.WorkerId;
            DatePerformed = item.DatePerformed;
            Comment = item.Comment;
            SelectedMaterial = item.Material;

            // Поиск и установка выбранного проекта в выпадающем списке (Picker)
            SelectedProject = AllProjects?.FirstOrDefault(p => p.Id == item.ProjectId);

            // Заполняем числовые поля (свойства вызовут OnPropertyChanged и TotalSalary пересчитается)
            Saw = item.Saw;
            Edging = item.Edging;
            Additive = item.Additive;
            DoorCanvas = item.DoorCanvas;
            DoorSectional = item.DoorSectional;
            Packaging = item.Packaging;
            GrindingSoap = item.GrindingSoap;
            GrindingFrez = item.GrindingFrez;
            Milling = item.Milling;
            Measuring = item.Measurement;
            Additionally = item.Additionally;
            InstallationAmount = item.Installation;

            if (item.Measurement > 0)
            {
                IsMeasurement = true;
            }

            if (item.Installation > 0)
            {
                IsInstallation = true;
            }
            RefreshCalculations();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}