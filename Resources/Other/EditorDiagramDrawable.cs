using Microsoft.Maui.Graphics;

namespace MyApp1;

public class EditorDiagramDrawable : IDrawable
{
    private readonly CuttingEditorForm _editor;
    private readonly CuttingSettingForm _settings; // [FIX] Ссылка на настройки

    // [FIX] Добавляем передачу настроек в конструктор
    public EditorDiagramDrawable(CuttingEditorForm editor, CuttingSettingForm settings)
    {
        _editor = editor;
        _settings = settings;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_editor?.SelectedSheet == null) return;
        var sheet = _editor.SelectedSheet;

        canvas.SaveState();

        // 1. Очистка фона
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        // 2. Расчет базового масштаба
        float padding = CuttingEditorForm.CanvasPadding;
        float baseScale = Math.Min((dirtyRect.Width - padding * 2) / (float)sheet.SheetW,
                                   (dirtyRect.Height - padding * 2) / (float)sheet.SheetH);

        // 3. ПРИМЕНЕНИЕ ЗУМА (Scale оставляем, если захотите программно менять, но Pan убираем)
        float totalScale = baseScale * (float)_editor.Scale;

        // [ИЗМЕНЕНО] Перестаем использовать PanX и PanY. 
        // Вместо них возвращаем фиксированный отступ, чтобы лист не двигался.
        // canvas.Translate((float)_editor.PanX, (float)_editor.PanY); // <-- Больше не используем
        canvas.Translate(padding, padding);

        canvas.Scale(totalScale, totalScale);

        // --- РИСОВАНИЕ ЛИСТА ---
        float sW = (float)sheet.SheetW;
        float sH = (float)sheet.SheetH;

        // Подложка листа
        canvas.FillColor = Color.FromArgb("#FAFAFA");
        canvas.FillRectangle(0, 0, sW, sH);

        // Рамка листа
        canvas.StrokeColor = Colors.Black;
        // Толщина линии корректируется обратно пропорционально масштабу, чтобы оставаться тонкой
        float lineThickness = 1 / totalScale;
        canvas.StrokeSize = lineThickness;

        float halfLine = lineThickness / 2;
        canvas.DrawRectangle(halfLine, halfLine, sW - lineThickness, sH - lineThickness);

        // --- СДВИГ ВНУТРЕННЕЙ ЗОНЫ (ОБПИЛ) ---
        // Получаем отступ из настроек
        float edgeOffset = (float)(_settings?.EdgeOffset ?? 10.0);

        // Сдвигаем начало координат для деталей.
        // Визуально показываем, что (0,0) деталей начинается внутри листа после обпила.
        canvas.Translate(edgeOffset, edgeOffset);

        // --- РИСОВАНИЕ ДЕТАЛЕЙ ---
        if (sheet.Parts != null)
        {
            foreach (var part in sheet.Parts)
            {
                float x = (float)part.X;
                float y = (float)part.Y;
                float w = (float)part.Length;
                float h = (float)part.Width;

                bool isSelected = _editor.SelectedPart == part;
                // Генерируем цвет на основе ID (метод из CuttingDiagramDrawable)
                Color partColor = CuttingDiagramDrawable.GenerateColor(part.DetailId);

                if (isSelected)
                {
                    // Подсветка выбранной детали
                    canvas.FillColor = Colors.Orange.WithAlpha(0.8f);
                    canvas.StrokeColor = Colors.Blue;
                    canvas.StrokeSize = 2 / totalScale;
                }
                else
                {
                    canvas.FillColor = partColor;
                    canvas.StrokeColor = Colors.Black;
                    canvas.StrokeSize = 1 / totalScale;
                }

                canvas.FillRectangle(x, y, w, h);
                canvas.DrawRectangle(x, y, w, h);

                // --- ОТРИСОВКА ТЕКСТА (ID) ---
                string idText = $"#{part.DetailId}";

                float baseIdSize = 12 / totalScale;
                float adaptiveIdSize = baseIdSize;

                // Если деталь маленькая, уменьшаем шрифт
                if (w < 60 || h < 60)
                {
                    adaptiveIdSize = (float)(baseIdSize * Math.Min(w, h) / 60.0);
                }

                canvas.FontSize = Math.Max(adaptiveIdSize, 5 / totalScale);
                canvas.FontColor = Colors.White;

                float idOffset = 5 / totalScale;

                canvas.DrawString(
                    idText,
                    x,
                    y,
                    w - idOffset,
                    h - idOffset,
                    HorizontalAlignment.Right,
                    VerticalAlignment.Bottom);

                // --- ОТРИСОВКА РАЗМЕРОВ ---
                DrawSideSize(canvas, x, y, w, h, false, totalScale);
            }
        }

        // --- РИСОВАНИЕ ОСТАТКОВ ---
        if (sheet.WasteRects != null)
        {
            foreach (var w in sheet.WasteRects)
            {
                // Проверяем, выбрана ли эта зона
                bool isSelected = _editor.SelectedWasteRects.Contains(w);

                if (isSelected)
                {
                    canvas.StrokeColor = Colors.Red;     // Красная рамка
                    canvas.StrokeSize = 3 / totalScale;  // Жирная линия
                    canvas.FillColor = Colors.Red.WithAlpha(0.2f); // Легкая заливка
                    canvas.FillRectangle((float)w.X, (float)w.Y, (float)w.Width, (float)w.Height);
                }
                else
                {
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 1 / totalScale;
                    // Без заливки (или стандартная)
                }

                canvas.DrawRectangle((float)w.X, (float)w.Y, (float)w.Width, (float)w.Height);

                // Пишем размеры (существующий код)
                if (w.Width > 40 && w.Height > 40)
                {
                    DrawSideSize(canvas, w.X, w.Y, w.Width, w.Height, true, totalScale);
                }
            }
        }

        // Восстанавливаем состояние (убираем Pan, Scale и Translate)
        canvas.RestoreState();

      
    }

    private void DrawSideSize(ICanvas canvas, double x, double y, double w, double h, bool isWaste, float scale)
    {
        // ... (Без изменений, метод вспомогательный)
        float baseFontSize = 10 / scale;
        canvas.FontColor = isWaste ? Colors.DimGray : Colors.Black;

        float paddingW = (float)Math.Min(w * 0.05, 15 / scale);
        float paddingH = (float)Math.Min(h * 0.05, 15 / scale);

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
                canvas.DrawString(heightText, (float)(-availableH / 2), 0, (float)availableH, textBlockHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
            }
            canvas.RestoreState();
        }
    }
}