namespace MyApp1;

public partial class DoorDetailsPage : ContentPage
{
    public DoorWorkshopItem ParentObject { get; }
    public DoorDetailsPage(DoorWorkshopItem objectData)
    {
        InitializeComponent();
        ParentObject = objectData;

        Title = $"Slim Line: {ParentObject.ObjectName}";
        ParentObject.Data.Recalculate();
        BindingContext = ParentObject.Data;

    }
}