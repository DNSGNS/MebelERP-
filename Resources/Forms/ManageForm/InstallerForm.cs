using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyApp1;

public class WorkMan : INotifyPropertyChanged
{
    private Guid _id;
    public Guid Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private WorkPosition _position;
    public WorkPosition Position
    {
        get => _position;
        set { _position = value; OnPropertyChanged(); }
    }



    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class InstallerForm : INotifyPropertyChanged
{
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }


    // Список ВСЕХ доступных установщиков (для выбора/назначения)
    private ObservableCollection<WorkMan> _allAvailableInstallers = new();
    public ObservableCollection<WorkMan> AllAvailableInstallers
    {
        get => _allAvailableInstallers;
        set { _allAvailableInstallers = value; OnPropertyChanged(); }
    }

    // Список РАСПРЕДЕЛЕННЫХ работников (те, кто уже в ProjectWork с DidMeasurement > 0)
    private ObservableCollection<WorkMan> _distributedInstallers = new();
    public ObservableCollection<WorkMan> DistributedInstallers
    {
        get => _distributedInstallers;
        set { _distributedInstallers = value; OnPropertyChanged(); }
    }

    // Список проектов (шапки), которые приходят из того же метода
    private ObservableCollection<ProjectManageData> _projects = new();
    public ObservableCollection<ProjectManageData> Projects
    {
        get => _projects;
        set { _projects = value; OnPropertyChanged(); }
    }

    // Выбранный работник (например, для Picker в MAUI)
    private WorkMan _selectedInstaller;
    public WorkMan SelectedInstaller
    {
        get => _selectedInstaller;
        set { _selectedInstaller = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

