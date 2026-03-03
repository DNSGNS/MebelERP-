namespace MyApp1;

public partial class DoorSlimLineReadPage : ContentPage
{
    public ObjectData? ParentObject { get; }
    public DoorSlimLineReadPage(ObjectData objectData)
    {
        InitializeComponent();
        ParentObject = objectData;

        Title = $"Slim Line: {ParentObject.ObjectName}";
        ParentObject.DoorSlimLineData.Recalculate();
        BindingContext = ParentObject.DoorSlimLineData;
        
    }

    public DoorSlimLineReadPage(object doorData)
    {
        InitializeComponent();

        if (doorData is DoorDetail detail)
        {
            // Создаем "умную" форму из "голых" данных
            var viewModel = new DoorSlimLineForm(detail);
            BindingContext = viewModel;
        }
        else if (doorData is DoorSlimLineForm form)
        {
            BindingContext = form;
        }

        Title = "Детали Slim Line (Просмотр)";
    }

}