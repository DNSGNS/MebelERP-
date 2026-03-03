using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public partial class LDSPWarehouseContentView : ContentView, INotifyPropertyChanged
{
    public ObjectData CurrentOrder { get; set; }

    // Используем коллекцию напрямую из CurrentOrder
    public ObservableCollection<LDSPForm> Forms => CurrentOrder?.LdspForms;

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

    public LDSPWarehouseContentView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Метод вызывается при установке BindingContext (передаче данных в View)
    /// </summary>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ObjectData order)
        {
            CurrentOrder = order;

            // Если список пустой (новый заказ), добавляем первую строку
            if (Forms != null && Forms.Count == 0)
            {
                var firstForm = new LDSPForm();
                Forms.Add(firstForm);
            }

            // Подписываем существующие и новые формы на расчет
            if (Forms != null)
            {
                RenumberForms();
                // Сначала отписываемся, чтобы не дублировать подписки при обновлении контекста
                Forms.CollectionChanged -= OnFormsCollectionChanged;
                foreach (var form in Forms)
                    form.PropertyChanged -= OnFormPropertyChanged;

                // Подписываемся заново
                foreach (var form in Forms)
                    form.PropertyChanged += OnFormPropertyChanged;

                Forms.CollectionChanged += OnFormsCollectionChanged;

                // Первичный расчет
                CalculateTotalArea();

                // Уведомляем интерфейс, что свойство Forms изменилось (для обновления CollectionView)
                OnPropertyChanged(nameof(Forms));
            }
        }
    }

    private void CalculateTotalArea()
    {
        if (Forms == null) return;
        TotalArea = Forms.Sum(f => f.Area);
    }

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

    private void OnFormsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (LDSPForm item in e.NewItems)
                item.PropertyChanged += OnFormPropertyChanged;
        }

        if (e.OldItems != null)
        {
            foreach (LDSPForm item in e.OldItems)
                item.PropertyChanged -= OnFormPropertyChanged;
        }

        CalculateTotalArea();
    }
    private void RenumberForms()
    {
        if (Forms == null) return;

        for (int i = 0; i < Forms.Count; i++)
        {
            Forms[i].Id = i + 1;
        }
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            });
        }
    }

    // Вспомогательный метод для поиска родительской страницы (для DisplayAlert)
    private ContentPage? FindParentPage()
    {
        var parent = this.Parent;
        while (parent != null && parent is not ContentPage)
        {
            parent = parent.Parent;
        }
        return parent as ContentPage;
    }

    // Реализация INotifyPropertyChanged для самого View
    public new event PropertyChangedEventHandler PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}