using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MyApp1;

// Результат расположения одной детали
public class PlacedPart
{
    public int DetailId { get; set; }
    public string? Color { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public bool IsRotated { get; set; }
}

// Представление одного целого листа с деталями
public class SheetLayout : INotifyPropertyChanged
{
    private int _sheetIndex;
    public int SheetIndex
    {
        get => _sheetIndex;
        set { _sheetIndex = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
    }

    private string _colorName = string.Empty;
    public string ColorName
    {
        get => _colorName;
        set { _colorName = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
    }

    // Это свойство будет использоваться в UI для отображения названия
    public string DisplayName => $"Лист {SheetIndex} ({ColorName})";

    public string DisplayNameLayout => $"Стол {SheetIndex} ({ColorName})";
    public double SheetW { get; set; }
    public double SheetH { get; set; }
    public List<PlacedPart> Parts { get; set; } = new();
    public List<RectF> WasteRects { get; set; } = new();

    // Реализация интерфейса для обновления UI
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Сам алгоритм
public class GuillotinePacker
{
    private double _sheetWidth;
    private double _sheetHeight;
    private double _sawWidth;     // Ширина пропила
    private double _edgeOffset;   // Обпил края
    private bool _cutByLength;

    public GuillotinePacker(double sheetL, double sheetW, double sawWidth, double edgeOffset, string method)
    {
        _sheetWidth = sheetL;
        _sheetHeight = sheetW;
        _sawWidth = sawWidth;
        _edgeOffset = edgeOffset;
        _cutByLength = method == "По длине";
    }

    public List<SheetLayout> Pack(List<CuttingDetails> details)
    {
        var finalSheets = new List<SheetLayout>();
        int globalSheetIndex = 1;

        // 1. Разворачиваем детали в плоский список задач (учитывая Count)
        var allTasks = new List<CuttingDetails>();
        foreach (var d in details)
        {
            for (int i = 0; i < d.Count; i++) allTasks.Add(d);
        }

        // 2. Группируем задачи по цвету.
        // Детали без цвета (null/empty) попадают в группу с ключом string.Empty
        var tasksByColor = allTasks.GroupBy(t => t.Color ?? "Без цвета").ToList();
       
        // 3. Обрабатываем каждую группу (цвет) отдельно
        foreach (var group in tasksByColor)
        {
            string currentColor = group.Key;
            var groupTasks = group.ToList();

            // Сортировка внутри группы (Эвристика: сначала крупные детали)
            if (_cutByLength)
            {
                groupTasks = groupTasks.OrderByDescending(t => Math.Max(t.Width, t.Length))
                                       .ThenByDescending(t => Math.Min(t.Width, t.Length))
                                       .ToList();
            }
            else
            {
                groupTasks = groupTasks.OrderByDescending(t => Math.Max(t.Length, t.Width))
                                       .ThenByDescending(t => Math.Min(t.Length, t.Width))
                                       .ToList();
            }

            // Упаковка деталей текущего цвета
            while (groupTasks.Count > 0)
            {
                var currentSheet = new SheetLayout
                {
                    SheetIndex = globalSheetIndex++,
                    ColorName = currentColor, // Присваиваем цвет листу
                    SheetW = _sheetWidth,
                    SheetH = _sheetHeight
                };

                // Уменьшаем рабочую область на величину обпила
                double usefulWidth = _sheetWidth - (_edgeOffset * 2);
                double usefulHeight = _sheetHeight - (_edgeOffset * 2);

                if (usefulWidth < 0) usefulWidth = 0;
                if (usefulHeight < 0) usefulHeight = 0;

                Node rootNode = new Node { X = 0, Y = 0, W = usefulWidth, H = usefulHeight };

                var unpackedInThisPass = new List<CuttingDetails>();

                foreach (var task in groupTasks)
                {
                    Node bestNode = FindBestNode(rootNode, task.Length, task.Width, task.CanRotate);

                    if (bestNode != null)
                    {
                        currentSheet.Parts.Add(new PlacedPart
                        {
                            DetailId = task.Id,
                            Color = task.Color,
                            X = bestNode.X,
                            Y = bestNode.Y,
                            Length = bestNode.UsedL,
                            Width = bestNode.UsedW,
                            IsRotated = bestNode.IsRotated
                        });

                        // Разрезаем пространство
                        SplitNode(bestNode, bestNode.UsedL, bestNode.UsedW);
                    }
                    else
                    {
                        // Деталь не влезла на текущий лист, пойдет на следующий (того же цвета)
                        unpackedInThisPass.Add(task);
                    }
                }

                // Собираем остатки для текущего листа
                CollectWaste(rootNode, currentSheet.WasteRects);

                finalSheets.Add(currentSheet);

                // Оставшиеся задачи переходят на следующую итерацию (следующий лист этого же цвета)
                groupTasks = unpackedInThisPass;
            }
        }

        return finalSheets;
    }

    private Node FindBestNode(Node root, double w, double h, bool canRotate)
    {
        if (root.Used)
        {
            var right = FindBestNode(root.Right, w, h, canRotate);
            var down = FindBestNode(root.Down, w, h, canRotate);

            if (right == null) return down;
            if (down == null) return right;

            return (right.W * right.H <= down.W * down.H) ? right : down;
        }

        bool fitsNormal = w <= root.W && h <= root.H;
        bool fitsRotated = canRotate && h <= root.W && w <= root.H;

        if (!fitsNormal && !fitsRotated) return null;

        if (fitsNormal && fitsRotated)
        {
            if (_cutByLength)
            {
                root.UsedL = w; root.UsedW = h; root.IsRotated = false;
                return root;
            }
        }

        if (fitsNormal)
        {
            root.UsedL = w; root.UsedW = h; root.IsRotated = false;
            return root;
        }

        if (fitsRotated)
        {
            root.UsedL = h; root.UsedW = w; root.IsRotated = true;
            return root;
        }

        return null;
    }

    private void SplitNode(Node node, double w, double h)
    {
        node.Used = true;

        if (_cutByLength)
        {
            node.Right = new Node
            {
                X = node.X + w + _sawWidth,
                Y = node.Y,
                W = node.W - w - _sawWidth,
                H = h
            };

            node.Down = new Node
            {
                X = node.X,
                Y = node.Y + h + _sawWidth,
                W = node.W,
                H = node.H - h - _sawWidth
            };
        }
        else
        {
            node.Right = new Node
            {
                X = node.X + w + _sawWidth,
                Y = node.Y,
                W = node.W - w - _sawWidth,
                H = node.H
            };

            node.Down = new Node
            {
                X = node.X,
                Y = node.Y + h + _sawWidth,
                W = w,
                H = node.H - h - _sawWidth
            };
        }
    }

    public static void RebuildWaste(SheetLayout sheet, double sawWidth, double edgeOffset, bool preferLengthCuts)
    {
        sheet.WasteRects.Clear();

        double usefulW = sheet.SheetW - (edgeOffset * 2);
        double usefulH = sheet.SheetH - (edgeOffset * 2);

        if (usefulW <= 0 || usefulH <= 0) return;

        Node root = new Node { X = 0, Y = 0, W = usefulW, H = usefulH };

        ReconstructRecursive(root, sheet.Parts, sawWidth, preferLengthCuts);
        CollectWasteManual(root, sheet.WasteRects);
    }

    private static void ReconstructRecursive(Node node, List<PlacedPart> allParts, double saw, bool preferLengthCuts)
    {
        double eps = 0.5;
        var partsInside = allParts.Where(p =>
            p.X >= node.X - eps &&
            p.Y >= node.Y - eps &&
            (p.X + p.Length) <= (node.X + node.W + eps) &&
            (p.Y + p.Width) <= (node.Y + node.H + eps)
        ).ToList();

        if (partsInside.Count == 0)
        {
            node.Used = false;
            return;
        }

        node.Used = true;
        bool splitFound = false;
        bool isHorizontalSplit = false;
        double splitCoordinate = 0;

        if (preferLengthCuts)
        {
            if (TryFindSplit(partsInside, node, true, saw, out splitCoordinate))
            {
                splitFound = true;
                isHorizontalSplit = true;
            }
            else if (TryFindSplit(partsInside, node, false, saw, out splitCoordinate))
            {
                splitFound = true;
                isHorizontalSplit = false;
            }
        }
        else
        {
            if (TryFindSplit(partsInside, node, false, saw, out splitCoordinate))
            {
                splitFound = true;
                isHorizontalSplit = false;
            }
            else if (TryFindSplit(partsInside, node, true, saw, out splitCoordinate))
            {
                splitFound = true;
                isHorizontalSplit = true;
            }
        }

        if (splitFound)
        {
            if (isHorizontalSplit)
            {
                double splitRelY = splitCoordinate - node.Y;
                node.Right = new Node { X = node.X, Y = node.Y, W = node.W, H = splitRelY };
                node.Down = new Node { X = node.X, Y = splitCoordinate + saw, W = node.W, H = node.H - splitRelY - saw };
            }
            else
            {
                double splitRelX = splitCoordinate - node.X;
                node.Right = new Node { X = node.X, Y = node.Y, W = splitRelX, H = node.H };
                node.Down = new Node { X = splitCoordinate + saw, Y = node.Y, W = node.W - splitRelX - saw, H = node.H };
            }

            ReconstructRecursive(node.Right, partsInside, saw, preferLengthCuts);
            ReconstructRecursive(node.Down, partsInside, saw, preferLengthCuts);
        }
        else
        {
            if (partsInside.Count == 1)
            {
                SplitNodeManual(node, partsInside[0], allParts, saw);
            }
        }
    }


    private static bool TryFindSplit(List<PlacedPart> parts, Node node, bool horizontal, double saw, out double splitCoord)
    {
        splitCoord = 0;
        var candidates = new HashSet<double>();
        foreach (var p in parts)
        {
            double val = horizontal ? (p.Y + p.Width) : (p.X + p.Length);
            if (horizontal)
            {
                if (val + saw < node.Y + node.H) candidates.Add(val);
            }
            else
            {
                if (val + saw < node.X + node.W) candidates.Add(val);
            }
        }

        var sortedCandidates = candidates.OrderBy(c => c).ToList();

        foreach (var cand in sortedCandidates)
        {
            double cutStart = cand;
            double cutEnd = cand + saw;

            bool intersects = false;
            foreach (var p in parts)
            {
                if (horizontal)
                {
                    if (p.Y < cutEnd - 0.1 && (p.Y + p.Width) > cutStart + 0.1)
                    {
                        intersects = true;
                        break;
                    }
                }
                else
                {
                    if (p.X < cutEnd - 0.1 && (p.X + p.Length) > cutStart + 0.1)
                    {
                        intersects = true;
                        break;
                    }
                }
            }

            if (!intersects)
            {
                splitCoord = cand;
                return true;
            }
        }

        return false;
    }

    private static void SplitNodeManual(Node node, PlacedPart currentPart, List<PlacedPart> allParts, double saw)
    {
        node.Used = true;

        double w = currentPart.Length;
        double h = currentPart.Width;
        bool useRowMode = true;

        foreach (var p in allParts)
        {
            if (p == currentPart) continue;
            if (p.X >= (node.X + w + saw - 0.1))
            {
                if ((p.Y + p.Width) > (node.Y + h + 0.1))
                {
                    useRowMode = false;
                    break;
                }
            }
        }

        if (useRowMode)
        {
            node.Right = new Node { X = node.X + w + saw, Y = node.Y, W = node.W - w - saw, H = h };
            node.Down = new Node { X = node.X, Y = node.Y + h + saw, W = node.W, H = node.H - h - saw };
        }
        else
        {
            node.Right = new Node { X = node.X + w + saw, Y = node.Y, W = node.W - w - saw, H = node.H };
            node.Down = new Node { X = node.X, Y = node.Y + h + saw, W = w, H = node.H - h - saw };
        }
    }

    private static void CollectWasteManual(Node node, List<RectF> waste)
    {
        if (node == null) return;

        if (!node.Used && node.W > 1 && node.H > 1)
        {
            waste.Add(new RectF((float)node.X, (float)node.Y, (float)node.W, (float)node.H));
            return;
        }

        if (node.Right != null) CollectWasteManual(node.Right, waste);
        if (node.Down != null) CollectWasteManual(node.Down, waste);
    }

    private void CollectWaste(Node node, List<RectF> waste)
    {
        if (node == null) return;
        if (node.W <= 1 || node.H <= 1) return;

        if (!node.Used)
        {
            waste.Add(new RectF((float)node.X, (float)node.Y, (float)node.W, (float)node.H));
        }
        else
        {
            CollectWaste(node.Right, waste);
            CollectWaste(node.Down, waste);
        }
    }

    private class Node
    {
        public double X, Y, W, H;
        public bool Used = false;
        public Node Right, Down;
        public double UsedL, UsedW;
        public bool IsRotated;
    }
}