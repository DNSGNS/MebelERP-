using Microsoft.Maui.Graphics;

namespace MyApp1;

public class StoragePartDrawable : IDrawable
{
    private readonly StorageItem _item;

    public StoragePartDrawable(StorageItem item)
    {
        _item = item;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        if (_item == null) return;

        // Отступы, чтобы деталь не прилипала к краям
        float padding = 10;
        RectF drawingArea = new RectF(padding, padding, dirtyRect.Width - padding * 2, dirtyRect.Height - padding * 2);

        // Считаем масштаб, чтобы вписать деталь целиком
        float scale = Math.Min(drawingArea.Width / (float)_item.Length, drawingArea.Height / (float)_item.Width);

        // Центрируем
        float drawW = (float)_item.Length * scale;
        float drawH = (float)_item.Width * scale;
        float x = drawingArea.X + (drawingArea.Width - drawW) / 2;
        float y = drawingArea.Y + (drawingArea.Height - drawH) / 2;

        // Рисуем деталь (используем тот же генератор цветов)
        canvas.FillColor = CuttingDiagramDrawable.GenerateColor(_item.DetailId);
        canvas.FillRectangle(x, y, drawW, drawH);

        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 1;
        canvas.DrawRectangle(x, y, drawW, drawH);

        // Рисуем ID по центру
        canvas.FontColor = Colors.White;
        canvas.FontSize = 12;
        canvas.DrawString($"#{_item.DetailId}", x, y, drawW, drawH, HorizontalAlignment.Center, VerticalAlignment.Center);

        // Рисуем размеры (опционально)
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 10;
        canvas.DrawString($"{_item.Length}x{_item.Width}", 0, dirtyRect.Height - 15, dirtyRect.Width, 15, HorizontalAlignment.Center, VerticalAlignment.Bottom);
    }
}