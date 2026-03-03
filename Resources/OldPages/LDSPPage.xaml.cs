using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MyApp1;

public partial class LDSPPage : ContentPage, INotifyPropertyChanged
{
    public ObjectData CurrentOrder { get; set; }
    private readonly ProjectData _parentProject;

    // Используем коллекцию напрямую из CurrentOrder
    public ObservableCollection<LDSPForm> Forms => CurrentOrder.LdspForms;

    private double _totalArea;
    public double TotalArea
    {
        get => _totalArea;
        set
        {
            _totalArea = value;
            OnPropertyChanged(nameof(TotalArea));
        }
    }

    // 2. Конструктор теперь ПРИНИМАЕТ данные
    public LDSPPage(ObjectData order, ProjectData project)
    {
        InitializeComponent();

        _parentProject = project;

        // Сохраняем переданный объект
        CurrentOrder = order;

        // Важно: Привязываем контекст к самой странице (или к CurrentOrder)
        BindingContext = this;

        // Если список пустой (новый заказ), добавляем первую строку
        if (Forms.Count == 0)
        {
            var firstForm = new LDSPForm();
            Forms.Add(firstForm);
        }

        // Подписываем существующие и новые формы на расчет
        foreach (var form in Forms)
            form.PropertyChanged += OnFormPropertyChanged;

        Forms.CollectionChanged += OnFormsCollectionChanged;

        CalculateTotalArea();
    }
    /// <summary>
    /// Метод пересчета общей площади
    /// </summary>
    private void CalculateTotalArea()
    {
        // Суммируем площади всех деталей. 
        // Если в модели Area возвращает double?, используем Sum(f => f.Area ?? 0)
        TotalArea = Forms.Sum(f => f.Area);
    }

    /// <summary>
    /// Срабатывает, когда внутри любой карточки изменилось число
    /// </summary>
    private void OnFormPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Пересчитываем итог, если изменилось свойство, влияющее на результат
        if (e.PropertyName == nameof(LDSPForm.Area) ||
            e.PropertyName == nameof(LDSPForm.Length) ||
            e.PropertyName == nameof(LDSPForm.Width) ||
            e.PropertyName == nameof(LDSPForm.Count))
        {
            CalculateTotalArea();
        }
    }

    /// <summary>
    /// Срабатывает при добавлении или удалении карточек из списка
    /// </summary>
    private void OnFormsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CalculateTotalArea();
    }

    private async void OnAddFormClicked(object sender, EventArgs e)
    {
        var lastForm = Forms.LastOrDefault();

        // Проверка заполненности предыдущей детали
        if (lastForm != null && (lastForm.Length == null || lastForm.Width == null || lastForm.Length <= 0 || lastForm.Width <= 0))
        {
            await DisplayAlert("Внимание", "Пожалуйста, заполните размеры текущей детали перед добавлением новой.", "OK");
            return;
        }

        var newForm = new LDSPForm();

        // ВАЖНО: Подписываем новую форму на слежение за изменениями свойств
        newForm.PropertyChanged += OnFormPropertyChanged;

        Forms.Add(newForm);
    }

    private void OnRemoveFormClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var form = button?.CommandParameter as LDSPForm;

        // Разрешаем удалять, только если в списке больше одной детали
        if (form != null && Forms.Count > 1)
        {
            // Отписываемся от события, чтобы избежать утечек памяти
            form.PropertyChanged -= OnFormPropertyChanged;
            Forms.Remove(form);
        }
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            // Выделяем текст при нажатии (с задержкой для стабильности на Android/iOS)
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            });
        }
    }

    private async void OnNavigateToFasadClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FasadPage(CurrentOrder, _parentProject));
    }
}