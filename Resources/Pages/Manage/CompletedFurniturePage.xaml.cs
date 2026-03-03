using System.Collections.ObjectModel;

namespace MyApp1;

public partial class CompletedFurniturePage : ContentPage
{
    public string ProjectName { get; set; }
    public ObservableCollection<NameValueItem> FurnitureItems { get; set; } = new();

    public ObservableCollection<ProfileGroup> ProfileGroups { get; set; } = new();
    public CompletedFurniturePage(CompletedProject project)
    {
        InitializeComponent();
        ProjectName = project.ProjectName;
        LoadData(project);
        BindingContext = this;
    }

    private void LoadData(CompletedProject project)
    {
        // Берем первую запись (сводную) или создаем пустую, если нет данных
        var f = project.FurnitureCompletedDetails.FirstOrDefault() ?? new FurnitureCompletedDetail();

        FurnitureItems.Add(new NameValueItem("ХДФ (м²)", f.Hdf));
        FurnitureItems.Add(new NameValueItem("Направляющие без доводчика", f.NaprBez));
        FurnitureItems.Add(new NameValueItem("Направляющие с доводчиком", f.NaprS));
        FurnitureItems.Add(new NameValueItem("Петли без доводчика (шт)", f.PetliBez));
        FurnitureItems.Add(new NameValueItem("Петли с доводчиком (шт)", f.PetliS));
        FurnitureItems.Add(new NameValueItem("Ручка-скоба (шт)", f.Skoba));
        FurnitureItems.Add(new NameValueItem("Ручка накладная (шт)", f.Nakladnaya));
        FurnitureItems.Add(new NameValueItem("Ручка-кнопка (шт)", f.Knopka));
        FurnitureItems.Add(new NameValueItem("Профиль Gola (м)", f.Gola));
        FurnitureItems.Add(new NameValueItem("Труба хром (мп)", f.Truba));
        FurnitureItems.Add(new NameValueItem("Газ-лифт (шт)", f.GazLift));
        FurnitureItems.Add(new NameValueItem("Крючки (шт)", f.Kruchki));
        FurnitureItems.Add(new NameValueItem("Подсветка (мп)", f.Podsvetka));

        foreach (var p in f.Profiles)
        {
            var group = new ProfileGroup();

            if (p.TopGuideCount > 0)
                group.Items.Add(new ProfileDisplayItem(p.TopGuideName, p.TopGuideSize, p.TopGuideCount));

            if (p.BottomGuideCount > 0)
                group.Items.Add(new ProfileDisplayItem(p.BottomGuideName, p.BottomGuideSize, p.BottomGuideCount));

            if (p.VerticalSlimCount > 0)
                group.Items.Add(new ProfileDisplayItem(p.VerticalSlimName, p.VerticalSlimSize, p.VerticalSlimCount));

            if (p.NarrowFrameCount > 0)
                group.Items.Add(new ProfileDisplayItem(p.NarrowFrameName, p.NarrowFrameSize, p.NarrowFrameCount));

            if (p.WideFrameCount > 0)
                group.Items.Add(new ProfileDisplayItem(p.WideFrameName, p.WideFrameSize, p.WideFrameCount));

            if (p.MiddleFrameCount > 0)
                group.Items.Add(new ProfileDisplayItem(p.MiddleFrameName, p.MiddleFrameSize, p.MiddleFrameCount));

            if (group.Items.Count > 0)
                ProfileGroups.Add(group);
        }
    }
}

// Вспомогательный класс
public class NameValueItem
{
    public string Name { get; set; }
    public double Value { get; set; }
    public NameValueItem(string name, double value) { Name = name; Value = value; }
}
public class ProfileGroup
{
    public ObservableCollection<ProfileDisplayItem> Items { get; set; } = new();
}

// Вспомогательный класс для отображения строки профиля
public class ProfileDisplayItem
{
    public string Name { get; set; }
    public double Size { get; set; }
    public int Count { get; set; }

    public ProfileDisplayItem(string name, double size, int count)
    {
        Name = name; Size = size; Count = count;
    }
}