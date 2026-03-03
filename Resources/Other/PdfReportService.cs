using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp; // Обязательно для рисования!

namespace MyApp1;

public class PdfReportService
{
    public void GenerateReport(string filePath, CuttingData data)
    {
        // Настройка лицензии
        // global::QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(QuestPDF.Helpers.Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Verdana));

                // 1. ЗАГОЛОВОК
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ИТОГОВЫЙ ОТЧЁТ").SemiBold().FontSize(24).FontColor("#6750A4");
                        col.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                    });
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(15);

                    // 2. БЛОК: РЕЗУЛЬТАТЫ
                    column.Item().Decoration(decoration =>
                    {
                        decoration.Before().PaddingBottom(5).Text("Результаты раскроя").SemiBold().FontSize(16).FontColor("#6750A4");

                        decoration.Content().Border(1).BorderColor("#E6E0E9").Padding(10).Column(c =>
                        {
                            double sheetL = data.Settings.SheetLength ?? 0;
                            double sheetW = data.Settings.SheetWidth ?? 0;
                            double oneSheetArea = (sheetL * sheetW) / 1_000_000.0;

                            c.Item().Row(r => { r.RelativeItem().Text("Размер листа (мм)"); r.RelativeItem().AlignRight().Text($"{sheetL} x {sheetW}"); });
                            c.Item().Row(r => { r.RelativeItem().Text("Площадь 1 листа"); r.RelativeItem().AlignRight().Text($"{oneSheetArea:F2} м²"); });
                            c.Item().Row(r => { r.RelativeItem().Text("Использовано листов"); r.RelativeItem().AlignRight().Text($"{data.LastResult?.TotalSheets ?? 0} шт.").Bold(); });
                            c.Item().Row(r => { r.RelativeItem().Text("Общая площадь материала"); r.RelativeItem().AlignRight().Text($"{(data.LastResult?.TotalSheetArea ?? 0) / 1_000_000.0:F2} м²"); });
                            c.Item().Row(r => { r.RelativeItem().Text("Количество деталей"); r.RelativeItem().AlignRight().Text($"{data.LastResult?.TotalPartsCount ?? 0} шт."); });
                            c.Item().Row(r => { r.RelativeItem().Text("Площадь деталей"); r.RelativeItem().AlignRight().Text($"{(data.LastResult?.TotalPartsArea ?? 0) / 1_000_000.0:F2} м²"); });
                            c.Item().Row(r => { r.RelativeItem().Text("Цвет материала"); r.RelativeItem().AlignRight().Text($"{(data.MaterialColor)}"); });
                        });
                    });


                    // 3. БЛОК: КРОМКА
                    column.Item().Decoration(decoration =>
                    {
                        decoration.Before().PaddingBottom(5).Text("Кромка (длина)").SemiBold().FontSize(16).FontColor("#6750A4");

                        decoration.Content().Border(1).BorderColor("#E6E0E9").Padding(10).Column(c =>
                        {
                            c.Item().Row(r => {
                                r.RelativeItem().Text($"2 мм");
                                r.RelativeItem().AlignRight().Text($"{data.TotalEdge1:F1} м").Bold();
                            });
                            c.Item().Row(r => {
                                r.RelativeItem().Text($"1 мм");
                                r.RelativeItem().AlignRight().Text($"{data.TotalEdge2:F1} м").Bold();
                            });
                        });
                    });

                    //  Блок: Список деталей
                    //  Блок: Список деталей
                    if (data.SavedReport.Details != null && data.SavedReport.Details.Count > 0)
                    {
                        // Определяем, нужно ли вообще показывать колонку с цветом
                        bool hasColors = data.SavedReport.Details.Any(d => !string.IsNullOrEmpty(d.Color));

                        column.Item().PaddingTop(10).Text("Список деталей").Bold().FontSize(14).FontColor("#6750A4");

                        column.Item().Table(table =>
                        {
                            // 1. Определение колонок (ColumnsDefinition)
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);   // №
                                columns.RelativeColumn(2);    // Длина
                                columns.RelativeColumn(2);    // Ширина
                                if (hasColors)
                                    columns.RelativeColumn(2); // Цвет (только если есть)
                                columns.ConstantColumn(45);   // Кол-во
                            });

                            // 2. Хедер таблицы (Header)
                            table.Header(header =>
                            {
                                header.Cell().Element(c => HeaderStyle(c)).AlignCenter().Text("№");
                                header.Cell().Element(c => HeaderStyle(c)).AlignCenter().Text("Длина");
                                header.Cell().Element(c => HeaderStyle(c)).AlignCenter().Text("Ширина");

                                if (hasColors)
                                    header.Cell().Element(c => HeaderStyle(c)).AlignCenter().Text("Цвет");

                                header.Cell().Element(c => HeaderStyle(c)).AlignCenter().Text("Кол.");
                            });

                            // 3. Данные (Rows)
                            foreach (var detail in data.SavedReport.Details)
                            {
                                // №
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(detail.Id.ToString());

                                // Длина (с отрисовкой кромки внутри ячейки)
                                table.Cell().Element(c => CellStyle(c)).Element(c =>
                                    DrawDimensionCell(c, detail.Length, detail.E1L1, detail.E1L2, detail.E2L1, detail.E2L2));

                                // Ширина (с отрисовкой кромки внутри ячейки)
                                table.Cell().Element(c => CellStyle(c)).Element(c =>
                                    DrawDimensionCell(c, detail.Width, detail.E1W1, detail.E1W2, detail.E2W1, detail.E2W2));

                                // Цвет (добавляем ячейку только если колонка активна)
                                if (hasColors)
                                {
                                    table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(detail.Color ?? "-");
                                }

                                // Кол-во
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(detail.Count.ToString());
                            }
                        });
                    }

                    // 4. БЛОК: ЧЕРТЕЖИ
                    if (data.LastResult?.Sheets != null)
                    {
                        // ⬅️ правильный разрыв страницы
                        column.Item().PageBreak();

                        column.Item()
                            .Text("Карты раскроя")
                            .SemiBold()
                            .FontSize(18)
                            .FontColor("#6750A4");

                        foreach (var sheet in data.LastResult.Sheets)
                        {
                            column.Item().PaddingVertical(10).Decoration(dec =>
                            {
                                dec.Before()
                                    .PaddingBottom(5)
                                    .Text($"Лист {sheet.SheetIndex} {sheet.ColorName} ({sheet.SheetW} x {sheet.SheetH} мм)")
                                    .SemiBold();


                                dec.Content()
                                    .Border(1)
                                    .BorderColor(QuestPDF.Helpers.Colors.Black)
                                    .Height(300)
                                    .Canvas((canvas, size) =>
                                    {

                                        DrawSheetOnSkiaCanvas(
                                            canvas,
                                            size,
                                            sheet,
                                            data.Settings.EdgeOffset);
                                    });
                            });
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Страница ");
                    x.CurrentPageNumber();
                });
            });
        })
        .GeneratePdf(filePath);
    }

    public void GenerateReportLayout(string filePath, CuttingData data)
    {
        // Настройка лицензии (раскомментируйте, если нужно)
        // global::QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(QuestPDF.Helpers.Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana)); // Немного уменьшил шрифт для вместимости

                // 1. ЗАГОЛОВОК
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ИТОГОВЫЙ ОТЧЁТ (МДФ)").SemiBold().FontSize(24).FontColor("#6750A4");
                        col.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                    });
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(15);

                    // 2. БЛОК: РЕЗУЛЬТАТЫ (оставляем без изменений)
                    column.Item().Decoration(decoration =>
                    {
                        decoration.Before().PaddingBottom(5).Text("Результаты").SemiBold().FontSize(16).FontColor("#6750A4");
                        decoration.Content().Border(1).BorderColor("#E6E0E9").Padding(10).Column(c =>
                        {
                            double sheetL = data.Settings.SheetLength ?? 0;
                            double sheetW = data.Settings.SheetWidth ?? 0;
                            double oneSheetArea = (sheetL * sheetW) / 1_000_000.0;

                            c.Item().Row(r => { r.RelativeItem().Text("Размер Стола"); r.RelativeItem().AlignRight().Text($"{sheetL} x {sheetW}"); });
                            c.Item().Row(r => { r.RelativeItem().Text("Использовано Столов"); r.RelativeItem().AlignRight().Text($"{data.LastResult?.TotalSheets ?? 0} шт.").Bold(); });
                            c.Item().Row(r => { r.RelativeItem().Text("Количество деталей"); r.RelativeItem().AlignRight().Text($"{data.LastResult?.TotalPartsCount ?? 0} шт."); });
                        });
                    });

                    // 3. БЛОК: СПИСОК ДЕТАЛЕЙ
                    if (data.SavedReport.Details != null && data.SavedReport.Details.Count > 0)
                    {
                        bool hasColors = data.SavedReport.Details.Any(d => !string.IsNullOrEmpty(d.Color));

                        column.Item().PaddingTop(10).Text("Детализация заказа").Bold().FontSize(14).FontColor("#6750A4");

                        column.Item().Table(table =>
                        {
                            // 1. Определение колонок (Добавили 2 новые колонки)
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);    // №
                                columns.RelativeColumn(1.5f);  // Длина
                                columns.RelativeColumn(1.5f);  // Ширина
                                if (hasColors)
                                    columns.RelativeColumn(2); // Цвет

                                columns.RelativeColumn(2);     // ТИП КРАЯ (Новое)
                                columns.RelativeColumn(2);     // ФРЕЗА (Новое)

                                columns.ConstantColumn(35);    // Кол-во
                            });

                            // 2. Хедер таблицы
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("№");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("Длина");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("Ширина");

                                if (hasColors)
                                    header.Cell().Element(HeaderStyle).AlignCenter().Text("Цвет");

                                header.Cell().Element(HeaderStyle).AlignCenter().Text("Край");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("Фреза");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("Кол.");
                            });

                            // 3. Данные (Убрали DrawDimensionCell, чтобы не было линий)
                            foreach (var detail in data.SavedReport.Details)
                            {
                                table.Cell().Element(CellStyle).AlignCenter().Text(detail.Id.ToString());

                                // Выводим просто текст без "подчеркиваний" (кромок)
                                table.Cell().Element(CellStyle).AlignCenter().Text(detail.Length.ToString());
                                table.Cell().Element(CellStyle).AlignCenter().Text(detail.Width.ToString());

                                if (hasColors)
                                    table.Cell().Element(CellStyle).AlignCenter().Text(detail.Color ?? "-");

                                // Новые колонки
                                table.Cell().Element(CellStyle).AlignCenter().Text(detail.SelectedEdgeType?.ToString() ?? "-");
                                table.Cell().Element(CellStyle).AlignCenter().Text(detail.MillingText ?? "-");

                                table.Cell().Element(CellStyle).AlignCenter().Text(detail.Count.ToString());
                            }
                        });
                    }

                    // 4. БЛОК: ЧЕРТЕЖИ (оставляем без изменений)
                    if (data.LastResult?.Sheets != null)
                    {
                        column.Item().PageBreak();
                        column.Item().Text("Раскладка").SemiBold().FontSize(18).FontColor("#6750A4");

                        foreach (var sheet in data.LastResult.Sheets)
                        {
                            column.Item().PaddingVertical(10).Decoration(dec =>
                            {
                                dec.Before().PaddingBottom(5).Text($"Стол {sheet.SheetIndex} ({sheet.SheetW} x {sheet.SheetH} мм)").SemiBold();
                                dec.Content().Border(1).BorderColor(QuestPDF.Helpers.Colors.Black).Height(300).Canvas((canvas, size) =>
                                {
                                    DrawSheetOnSkiaCanvas(canvas, size, sheet, data.Settings.EdgeOffset);
                                });
                            });
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Страница ");
                    x.CurrentPageNumber();
                });
            });
        })
        .GeneratePdf(filePath);
    }


    private static QuestPDF.Infrastructure.IContainer HeaderStyle(QuestPDF.Infrastructure.IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten1)
            .Background(QuestPDF.Helpers.Colors.Grey.Lighten4)
            .Padding(5);
    }

    private static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3)
            .PaddingVertical(5);
    }

    private void DrawDimensionCell(QuestPDF.Infrastructure.IContainer container, double value, bool e1_1, bool e1_2, bool e2_1, bool e2_2)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text(value.ToString()).SemiBold();
            if (e1_1) col.Item().Element(c => DrawSolidLine(c));
            if (e1_2) col.Item().Element(c => DrawSolidLine(c));
            if (e2_1) col.Item().Element(c => DrawDashedLine(c));
            if (e2_2) col.Item().Element(c => DrawDashedLine(c));
        });
    }

    private void DrawSolidLine(QuestPDF.Infrastructure.IContainer container)
    {
        container.PaddingHorizontal(5).PaddingTop(1).Height(1.5f).Background(QuestPDF.Helpers.Colors.Black);
    }

    private void DrawDashedLine(QuestPDF.Infrastructure.IContainer container)
    {
        container.PaddingHorizontal(5).PaddingTop(1).Height(1.5f).Canvas((canvas, size) =>
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                PathEffect = SKPathEffect.CreateDash(new float[] { 3, 2 }, 0)
            };
            canvas.DrawLine(0, size.Height / 2, size.Width, size.Height / 2, paint);
        });
    }

    // МЕТОД РИСОВАНИЯ НА SKIASHARP (Специально для PDF)
    // ГЛАВНЫЙ МЕТОД РИСОВАНИЯ (Исправленный)
    private void DrawSheetOnSkiaCanvas(SKCanvas canvas, QuestPDF.Infrastructure.Size size, SheetLayout sheet, double edgeOffset)
    {
        // 1. Настройка области рисования и масштаба
        float pdfPadding = 10;
        float availableW = size.Width - pdfPadding * 2;
        float availableH = size.Height - pdfPadding * 2;

        float scaleX = availableW / (float)sheet.SheetW;
        float scaleY = availableH / (float)sheet.SheetH;
        float scale = Math.Min(scaleX, scaleY);

        // Центрирование листа на холсте PDF
        float offsetX = (size.Width - (float)sheet.SheetW * scale) / 2;
        float offsetY = (size.Height - (float)sheet.SheetH * scale) / 2;

        // 2. Определение инструментов (Paints)
        using var paintFillSheet = new SKPaint { Color = SKColors.WhiteSmoke, IsAntialias = true };
        using var paintBorder = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
        using var paintEdge = new SKPaint { Color = SKColors.Red.WithAlpha(60), Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f, IsAntialias = true, PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0) };
        using var paintFillPart = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };
        using var paintWaste = new SKPaint { Color = SKColors.Gray.WithAlpha(100), Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f, IsAntialias = true };

        // Настройка текста
        using var paintTextId = new SKPaint { Color = SKColors.Black.WithAlpha(220), IsAntialias = true, TextAlign = SKTextAlign.Center, Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) };
        using var paintTextSize = new SKPaint { Color = SKColors.DarkSlateGray, IsAntialias = true, TextAlign = SKTextAlign.Center };

        // 3. Рисуем сам лист (основа)
        var sheetRect = new SKRect(offsetX, offsetY, offsetX + (float)sheet.SheetW * scale, offsetY + (float)sheet.SheetH * scale);
        canvas.DrawRect(sheetRect, paintFillSheet);
        canvas.DrawRect(sheetRect, paintBorder);

        // 4. Рисуем пунктирную линию обпила (edge offset)
        float scaledEdge = (float)edgeOffset * scale;
        if (edgeOffset > 0)
        {
            var edgeRect = new SKRect(
                sheetRect.Left + scaledEdge,
                sheetRect.Top + scaledEdge,
                sheetRect.Right - scaledEdge,
                sheetRect.Bottom - scaledEdge);
            canvas.DrawRect(edgeRect, paintEdge);
        }

        // 5. Отрисовка деталей
        foreach (var part in sheet.Parts)
        {
            // Координаты: Смещение листа + Смещение обпила + Координата детали
            float px = offsetX + scaledEdge + (float)part.X * scale;
            float py = offsetY + scaledEdge + (float)part.Y * scale;
            float pw = (float)part.Length * scale;
            float ph = (float)part.Width * scale;

            var partRect = new SKRect(px, py, px + pw, py + ph);

            // Цвет детали
            paintFillPart.Color = GenerateSkiaColor(part.DetailId);
            canvas.DrawRect(partRect, paintFillPart);
            canvas.DrawRect(partRect, paintBorder);

            // --- ЛОГИКА ТЕКСТА ---
            float cx = px + pw / 2;
            float cy = py + ph / 2;

            // Расчет адаптивного шрифта (базируется на меньшей стороне детали)
            float minDim = Math.Min(pw, ph);
            float fontSize = minDim / 4f;

            // Ограничения: не более 12pt, не менее 5pt
            if (fontSize > 12) fontSize = 12;
            if (fontSize < 5) fontSize = 5;

            paintTextId.TextSize = fontSize;
            paintTextSize.TextSize = fontSize * 0.9f;

            string idText = $"#{part.DetailId}";
            string L = Math.Round(part.Length).ToString();
            string W = Math.Round(part.Width).ToString();
            string fullDimText = $"{L} x {W}";

            // Проверяем, влезает ли текст "L x W" в ширину детали
            float measuredWidth = paintTextSize.MeasureText(fullDimText);
            bool isTooNarrow = measuredWidth > (pw * 0.9f);

            if (isTooNarrow)
            {
                // РЕЖИМ В СТОЛБЕЦ (3 строки: ID, L, W)
                float spacing = fontSize * 1.1f;
                float startY = cy - spacing; // Центрируем блок из 3-х строк

                canvas.DrawText(idText, cx, startY, paintTextId);
                canvas.DrawText(L, cx, startY + spacing, paintTextSize);
                canvas.DrawText("x", cx, startY + spacing, paintTextSize);
                canvas.DrawText(W, cx, startY + (spacing * 2), paintTextSize);
            }
            else
            {
                // РЕЖИМ В СТРОКУ (2 строки: ID и размеры)
                float spacing = fontSize * 1.2f;
                canvas.DrawText(idText, cx, cy - (spacing * 0.1f), paintTextId);
                canvas.DrawText(fullDimText, cx, cy + (spacing * 0.9f), paintTextSize);
            }
        }

        // 6. Отрисовка остатков (Waste)
        if (sheet.WasteRects != null)
        {
            foreach (var waste in sheet.WasteRects)
            {
                float wx = offsetX + scaledEdge + (float)waste.X * scale;
                float wy = offsetY + scaledEdge + (float)waste.Y * scale;
                float ww = (float)waste.Width * scale;
                float wh = (float)waste.Height * scale;

                if (ww <= 0 || wh <= 0) continue;

                var wasteRect = new SKRect(wx, wy, wx + ww, wy + wh);
                canvas.DrawRect(wasteRect, paintWaste);

                // Диагональные линии для обозначения пустоты (как в чертежах)
                if (ww > 15 && wh > 15)
                {
                    canvas.DrawLine(wx, wy, wx + ww, wy + wh, paintWaste);
                    canvas.DrawLine(wx, wy + wh, wx + ww, wy, paintWaste);
                }
            }
        }
    }

    // Хелпер для генерации цвета (Аналог MAUI Color.FromHsla)
    private SKColor GenerateSkiaColor(int id)
    {
        double goldenRatioConjugate = 0.618033988749895;
        double hue = (id * goldenRatioConjugate) % 1;

        // SKColor.FromHsl принимает Hue [0..360], Saturation [0..100], Lightness [0..100]
        return SKColor.FromHsl((float)(hue * 360), 65, 80); // Чуть светлее (L=80), чтобы текст был виден
    }
}