using Microsoft.Maui.Graphics;

namespace MyApp1;

public class CuttingDiagramDrawable : IDrawable
{
    private readonly SheetLayout _layout;
    private readonly Dictionary<int, Color> _colors;
    private readonly double _edgeOffset; // Переменная для хранения отступа

    // Конструктор принимает layout и величину отступа
    public CuttingDiagramDrawable(SheetLayout layout, double edgeOffset)
    {
        _layout = layout;
        _edgeOffset = edgeOffset;
        _colors = new Dictionary<int, Color>();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_layout == null) return;

        canvas.SaveState();

        // 1. Очистка фона (принудительно белый)
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        // 2. Расчет масштаба (вписываем лист в экран с небольшим отступом padding)
        float padding = 40;
        // Используем реальные размеры листа
        float sheetW = (float)_layout.SheetW;
        float sheetH = (float)_layout.SheetH;

        float scale = Math.Min((dirtyRect.Width - padding * 2) / sheetW,
                               (dirtyRect.Height - padding * 2) / sheetH);

        // Сдвигаем холст в центр отступов
        canvas.Translate(padding, padding);
        canvas.Scale(scale, scale);

        // 3. Рисуем сам ЛИСТ (подложку)
        // Он рисуется от (0,0) до (SheetW, SheetH)
        canvas.FillColor = Color.FromArgb("#FAFAFA"); // Светло-серый
        canvas.FillRectangle(0, 0, sheetW, sheetH);

        // Рисуем рамку листа
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 1 / scale;
        canvas.DrawRectangle(0, 0, sheetW, sheetH);

        // --- ВАЖНОЕ ИЗМЕНЕНИЕ ---
        // Смещаем координаты рисования деталей на величину отступа (обпила).
        // Так как алгоритм возвращает координаты (0,0) для первой детали,
        // нам нужно визуально сдвинуть их внутрь листа.
        float offset = (float)_edgeOffset;

        // Рисуем пунктиром зону обпила (опционально, для красоты)
        canvas.StrokeColor = Colors.Red.WithAlpha(0.3f);
        canvas.StrokeDashPattern = new float[] { 5 / scale, 5 / scale };
        canvas.DrawRectangle(offset, offset, sheetW - offset * 2, sheetH - offset * 2);
        canvas.StrokeDashPattern = null; // Сброс пунктира

        // Применяем сдвиг для всех последующих элементов (деталей и остатков)
        canvas.Translate(offset, offset);

        // 4. Рисуем ДЕТАЛИ
        foreach (var part in _layout.Parts)
        {
            float x = (float)part.X;
            float y = (float)part.Y;
            float w = (float)part.Length;
            float h = (float)part.Width;

            // Генерируем цвет
            if (!_colors.ContainsKey(part.DetailId))
            {
                _colors[part.DetailId] = GenerateColor(part.DetailId);
            }
            Color partColor = _colors[part.DetailId];

            canvas.FillColor = partColor;
            canvas.FillRectangle(x, y, w, h);

            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 1 / scale;
            canvas.DrawRectangle(x, y, w, h);

            // Текст (ID и размеры)
            DrawPartInfo(canvas, part, x, y, w, h, scale);
        }

        // 5. Рисуем ОСТАТКИ (Свободное место)
        if (_layout.WasteRects != null)
        {
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 1 / scale;

            foreach (var waste in _layout.WasteRects)
            {
                float wx = (float)waste.X;
                float wy = (float)waste.Y;
                float ww = (float)waste.Width;
                float wh = (float)waste.Height;

                // Крест на остатках (или просто рамка)
                canvas.DrawRectangle(wx, wy, ww, wh);

                // Если остаток достаточно большой, пишем размер
                if (ww > 40 && wh > 40)
                {
                    DrawSideSize(canvas, wx, wy, ww, wh, true, scale);
                }
            }
        }

        canvas.RestoreState();
    }

    // Метод отрисовки информации на детали
    private void DrawPartInfo(ICanvas canvas, PlacedPart part, float x, float y, float w, float h, float scale)
    {
        // Рисуем ID детали
        string idText = $"#{part.DetailId}";

        float baseFontSize = 14 / scale;
        // Адаптация шрифта под размер детали
        float fontSize = Math.Min(baseFontSize, Math.Min(w, h) / 3);
        if (fontSize < 5 / scale) fontSize = 5 / scale; // Минимальный размер

        canvas.FontSize = fontSize;
        canvas.FontColor = Colors.White; // Белый текст на цветном фоне

        // Тень текста для читаемости (опционально)
        canvas.FontColor = Colors.Black.WithAlpha(0.5f);
        canvas.DrawString(idText, x + 1 / scale, y + 1 / scale, w, h, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontColor = Colors.White;
        canvas.DrawString(idText, x, y, w, h, HorizontalAlignment.Center, VerticalAlignment.Center);

        // Рисуем размеры по краям
        DrawSideSize(canvas, x, y, w, h, false, scale);
    }

    private void DrawSideSize(ICanvas canvas, double x, double y, double w, double h, bool isWaste, float scale)
    {
        float baseFontSize = 10 / scale;
        canvas.FontColor = isWaste ? Colors.DimGray : Colors.Black;

        float paddingW = (float)Math.Min(w * 0.05, 15 / scale);
        float paddingH = (float)Math.Min(h * 0.05, 15 / scale);

        // --- Верхний размер (Ширина) ---
        string widthText = Math.Round(w).ToString();
        float adaptiveWFontSize = baseFontSize;
        double estimatedWWidth = widthText.Length * (7 / scale);
        double availableW = w - (paddingW * 2);

        if (estimatedWWidth > availableW && availableW > 0)
            adaptiveWFontSize = (float)(baseFontSize * (availableW / estimatedWWidth));

        if (adaptiveWFontSize > 4 / scale)
        {
            canvas.FontSize = adaptiveWFontSize;
            canvas.DrawString(widthText, (float)(x + paddingW), (float)(y + 2 / scale), (float)availableW, 25 / scale, HorizontalAlignment.Center, VerticalAlignment.Top);
        }

        // --- Боковой размер (Высота) ---
        if (h > 10 / scale)
        {
            canvas.SaveState();
            float centerX = (float)x + paddingW + (1 / scale);
            float centerY = (float)(y + h / 2);

            canvas.Translate(centerX, centerY);
            canvas.Rotate(-90);

            string heightText = Math.Round(h).ToString();
            float adaptiveHFontSize = baseFontSize;
            double estimatedHWidth = heightText.Length * (7 / scale);
            double availableH = h - (paddingH * 2);

            if (estimatedHWidth > availableH && availableH > 0)
                adaptiveHFontSize = (float)(baseFontSize * (availableH / estimatedHWidth));

            if (adaptiveHFontSize > 4 / scale)
            {
                canvas.FontSize = adaptiveHFontSize;
                float textBlockHeight = (float)Math.Min(30 / scale, w * 0.8);
                canvas.DrawString(
                    heightText,
                    (float)(-availableH / 2),
                    0,
                    (float)availableH,
                    textBlockHeight,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Top);
            }
            canvas.RestoreState();
        }
    }

    public static Color GenerateColor(int id)
    {
        // Золотое сечение для разброса оттенков (Golden Ratio Conjugate)
        double goldenRatioConjugate = 0.618033988749895;
        double hue = (id * goldenRatioConjugate) % 1;

        // Перевод HSV в RGB (упрощенно или через Hsl)
        return Color.FromHsla(hue, 0.65, 0.6, 1.0);
    }
}