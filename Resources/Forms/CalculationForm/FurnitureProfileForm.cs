using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public class FurnitureProfileForm : INotifyPropertyChanged
{
    private string _topGuideName = string.Empty;
    public string TopGuideName { get => _topGuideName; set { _topGuideName = value; OnPropertyChanged(); } }

    private double _topGuideSize;
    public double TopGuideSize { get => _topGuideSize; set { _topGuideSize = value; OnPropertyChanged(); } }

    private int _topGuideCount;
    public int TopGuideCount { get => _topGuideCount; set { _topGuideCount = value; OnPropertyChanged(); } }

    private string _bottomGuideName = string.Empty;
    public string BottomGuideName { get => _bottomGuideName; set { _bottomGuideName = value; OnPropertyChanged(); } }

    private double _bottomGuideSize;
    public double BottomGuideSize { get => _bottomGuideSize; set { _bottomGuideSize = value; OnPropertyChanged(); } }

    private int _bottomGuideCount;
    public int BottomGuideCount { get => _bottomGuideCount; set { _bottomGuideCount = value; OnPropertyChanged(); } }

    private string _verticalSlimName = string.Empty;
    public string VerticalSlimName { get => _verticalSlimName; set { _verticalSlimName = value; OnPropertyChanged(); } }

    private double _verticalSlimSize;
    public double VerticalSlimSize { get => _verticalSlimSize; set { _verticalSlimSize = value; OnPropertyChanged(); } }

    private int _verticalSlimCount;
    public int VerticalSlimCount { get => _verticalSlimCount; set { _verticalSlimCount = value; OnPropertyChanged(); } }

    private string _narrowFrameName = string.Empty;
    public string NarrowFrameName { get => _narrowFrameName; set { _narrowFrameName = value; OnPropertyChanged(); } }

    private double _narrowFrameSize;
    public double NarrowFrameSize { get => _narrowFrameSize; set { _narrowFrameSize = value; OnPropertyChanged(); } }

    private int _narrowFrameCount;
    public int NarrowFrameCount { get => _narrowFrameCount; set { _narrowFrameCount = value; OnPropertyChanged(); } }

    private string _wideFrameName = string.Empty;
    public string WideFrameName { get => _wideFrameName; set { _wideFrameName = value; OnPropertyChanged(); } }

    private double _wideFrameSize;
    public double WideFrameSize { get => _wideFrameSize; set { _wideFrameSize = value; OnPropertyChanged(); } }

    private int _wideFrameCount;
    public int WideFrameCount { get => _wideFrameCount; set { _wideFrameCount = value; OnPropertyChanged(); } }

    private string _middleFrameName = string.Empty;
    public string MiddleFrameName { get => _middleFrameName; set { _middleFrameName = value; OnPropertyChanged(); } }

    private double _middleFrameSize;
    public double MiddleFrameSize { get => _middleFrameSize; set { _middleFrameSize = value; OnPropertyChanged(); } }

    private int _middleFrameCount;
    public int MiddleFrameCount { get => _middleFrameCount; set { _middleFrameCount = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}