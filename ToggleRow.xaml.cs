using System.Windows.Input;

namespace MyApp1;

public partial class ToggleRow : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(ToggleRow), string.Empty);

    public static readonly BindableProperty IsOnProperty =
        BindableProperty.Create(nameof(IsOn), typeof(bool), typeof(ToggleRow), false);

    public static readonly BindableProperty ToggleCommandProperty =
        BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(ToggleRow), null);

    public static readonly BindableProperty CommandParamProperty =
        BindableProperty.Create(nameof(CommandParam), typeof(string), typeof(ToggleRow), null);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }
    public ICommand ToggleCommand
    {
        get => (ICommand)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }
    public string CommandParam
    {
        get => (string)GetValue(CommandParamProperty);
        set => SetValue(CommandParamProperty, value);
    }

    public ToggleRow()
    {
        InitializeComponent();
    }
}