using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public partial class LDSPContentView : ContentView, INotifyPropertyChanged
{
    // Флаг, указывающий, нужно ли фокусироваться на новой строке при её появлении
    private bool _shouldFocusNewRow = false;
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

    public LDSPContentView()
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

    private void OnAddFormClicked(object sender, EventArgs e)
    {
        if (Forms == null) return;

        var lastForm = Forms.LastOrDefault();

        // Проверка заполненности предыдущей детали
        if (lastForm != null && (lastForm.Length == null || lastForm.Width == null || lastForm.Length <= 0 || lastForm.Width <= 0))
        {
            // Находим родительскую страницу для показа алерта
            var page = FindParentPage();
            page?.DisplayAlert("Внимание", "Пожалуйста, заполните размеры текущей детали перед добавлением новой.", "OK");
            return;
        }

        // Создаем новую форму и сразу присваиваем ей следующий порядковый номер
        var newForm = new LDSPForm();
        //var newForm = new LDSPForm
        //{
        //    Id = Forms.Count + 1
        //};

        Forms.Add(newForm);
        RenumberForms();

        _shouldFocusNewRow = true;
            }
    private void RenumberForms()
    {
        if (Forms == null) return;

        for (int i = 0; i < Forms.Count; i++)
        {
            Forms[i].Id = i + 1;
        }
    }

    private void OnRemoveFormClicked(object sender, EventArgs e)
    {
        if (Forms == null) return;

        var button = sender as Button;
        var form = button?.CommandParameter as LDSPForm;

        if (form != null && Forms.Count > 1)
        {
            Forms.Remove(form);
            RenumberForms();
        }
    }

    private void OnLengthCompleted(object sender, EventArgs e)
    {
        var entry = sender as Entry;
        // Ищем соседний Entry (Ширина) в той же строке (Grid)
        // Ширина находится в Column="3"
        var parentGrid = entry.Parent as Grid;
        var widthEntry = parentGrid?.Children.FirstOrDefault(c => Grid.GetColumn((View)c) == 3) as Entry;
        widthEntry?.Focus();
    }

    private void OnWidthCompleted(object sender, EventArgs e)
    {
        var entry = sender as Entry;
        // Кол-во находится в Column="4"
        var parentGrid = entry.Parent as Grid;
        var countEntry = parentGrid?.Children.FirstOrDefault(c => Grid.GetColumn((View)c) == 4) as Entry;
        countEntry?.Focus();
    }

    private void OnCountCompleted(object sender, EventArgs e)
    {
        var entry = sender as Entry;
        var currentForm = entry.BindingContext as LDSPForm;

        // Если это последняя строка в списке, добавляем новую
        if (Forms != null && currentForm == Forms.LastOrDefault())
        {
            // Вызываем логику добавления (она сама проверит валидацию и поставит флаг _shouldFocusNewRow)
            OnAddFormClicked(sender, e);
        }
    }

    private async void OnLengthEntryLoaded(object sender, EventArgs e)
    {
        if (!_shouldFocusNewRow || sender is not Entry entry)
            return;

        var form = entry.BindingContext as LDSPForm;
        if (form != Forms.LastOrDefault())
            return;

        _shouldFocusNewRow = false;

        // 1️ Сначала прокрутка
        FormsCollectionView.ScrollTo(
            form,
            position: ScrollToPosition.End,
            animate: false);

        // 2️ Даем CollectionView завершить layout
        await Task.Delay(100);

        // 3️ Только теперь фокус
        entry.Focus();
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

            var form = entry.BindingContext as LDSPForm;
            if (form != null)
            {
                    FormsCollectionView.ScrollTo(form, position: ScrollToPosition.Center, animate: true);
            }
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