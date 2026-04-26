using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Win32;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.Windows;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Nalbur.Wpf.ViewModels;

public class ExportColumn<T>
{
    public string Header { get; }
    public Func<T, object?> ValueSelector { get; }

    public ExportColumn(string header, Func<T, object?> valueSelector)
    {
        Header = header;
        ValueSelector = valueSelector;
    }
}

public static class ExportHelper
{
    public static void ExportToExcel<T>(
        string title,
        IEnumerable<T> data,
        List<ExportColumn<T>> columns)
    {
        try
        {
            var filePath = GetSaveFilePath($"{title}.xlsx", "Excel Dosyası (*.xlsx)|*.xlsx");
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(SanitizeSheetName(title));

            worksheet.Cell(1, 1).Value = title;
            worksheet.Range(1, 1, 1, columns.Count).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;

            for (int i = 0; i < columns.Count; i++)
            {
                worksheet.Cell(3, i + 1).Value = columns[i].Header;
                worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 4;

            foreach (var item in data)
            {
                for (int col = 0; col < columns.Count; col++)
                {
                    worksheet.Cell(row, col + 1).Value = FormatValue(columns[col].ValueSelector(item));
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);

            ShowSuccess("Excel");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    public static void ExportToWord<T>(
        string title,
        IEnumerable<T> data,
        List<ExportColumn<T>> columns)
    {
        try
        {
            var filePath = GetSaveFilePath($"{title}.docx", "Word Dosyası (*.docx)|*.docx");
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            using var wordDocument = WordprocessingDocument.Create(
                filePath,
                WordprocessingDocumentType.Document);

            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new W.Document();

            var body = new W.Body();

            body.Append(new W.Paragraph(
                new W.Run(
                    new W.RunProperties(
                        new W.Bold(),
                        new W.FontSize { Val = "32" }),
                    new W.Text(title))));

            body.Append(new W.Paragraph(
                new W.Run(
                    new W.Text($"Oluşturma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}"))));

            var table = new W.Table();

            table.AppendChild(new W.TableProperties(
                new W.TableBorders(
                    new W.TopBorder { Val = W.BorderValues.Single, Size = 6 },
                    new W.BottomBorder { Val = W.BorderValues.Single, Size = 6 },
                    new W.LeftBorder { Val = W.BorderValues.Single, Size = 6 },
                    new W.RightBorder { Val = W.BorderValues.Single, Size = 6 },
                    new W.InsideHorizontalBorder { Val = W.BorderValues.Single, Size = 6 },
                    new W.InsideVerticalBorder { Val = W.BorderValues.Single, Size = 6 }
                )));

            var headerRow = new W.TableRow();

            foreach (var column in columns)
            {
                headerRow.Append(CreateWordCell(column.Header, true));
            }

            table.Append(headerRow);

            foreach (var item in data)
            {
                var row = new W.TableRow();

                foreach (var column in columns)
                {
                    row.Append(CreateWordCell(FormatValue(column.ValueSelector(item)), false));
                }

                table.Append(row);
            }

            body.Append(table);
            mainPart.Document.Append(body);
            mainPart.Document.Save();

            ShowSuccess("Word");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    public static void ExportToPdf<T>(
        string title,
        IEnumerable<T> data,
        List<ExportColumn<T>> columns)
    {
        try
        {
            var filePath = GetSaveFilePath($"{title}.pdf", "PDF Dosyası (*.pdf)|*.pdf");
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var document = new Document();
            document.Info.Title = title;

            var section = document.AddSection();

            section.PageSetup.Orientation = columns.Count > 5
                ? Orientation.Landscape
                : Orientation.Portrait;

            var titleParagraph = section.AddParagraph(title);
            titleParagraph.Format.Font.Size = 16;
            titleParagraph.Format.Font.Bold = true;
            titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            var dateParagraph = section.AddParagraph($"Oluşturma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}");
            dateParagraph.Format.Font.Size = 9;
            dateParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            double pageWidth = columns.Count > 5 ? 25.0 : 17.0;
            double columnWidth = Math.Max(2.2, pageWidth / columns.Count);

            foreach (var _ in columns)
            {
                var column = table.AddColumn(Unit.FromCentimeter(columnWidth));
                column.Format.Alignment = ParagraphAlignment.Left;
            }

            var headerRow = table.AddRow();
            headerRow.Shading.Color = Colors.LightGray;
            headerRow.Format.Font.Bold = true;

            for (int i = 0; i < columns.Count; i++)
            {
                headerRow.Cells[i].AddParagraph(columns[i].Header);
            }

            foreach (var item in data)
            {
                var row = table.AddRow();

                for (int i = 0; i < columns.Count; i++)
                {
                    row.Cells[i].AddParagraph(FormatValue(columns[i].ValueSelector(item)));
                }
            }

            var renderer = new PdfDocumentRenderer
            {
                Document = document
            };

            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);

            ShowSuccess("PDF");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private static W.TableCell CreateWordCell(string text, bool isHeader)
    {
        var runProperties = isHeader
            ? new W.RunProperties(new W.Bold())
            : new W.RunProperties();

        return new W.TableCell(
            new W.Paragraph(
                new W.Run(
                    runProperties,
                    new W.Text(text ?? string.Empty))));
    }

    private static string? GetSaveFilePath(string fileName, string filter)
    {
        var dialog = new SaveFileDialog
        {
            FileName = fileName,
            Filter = filter,
            AddExtension = true,
            OverwritePrompt = true
        };

        return dialog.ShowDialog() == true
            ? dialog.FileName
            : null;
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime date => date.ToString("dd.MM.yyyy HH:mm"),
            decimal number => number.ToString("N2"),
            double number => number.ToString("N2"),
            float number => number.ToString("N2"),
            bool boolValue => boolValue ? "Evet" : "Hayır",
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string SanitizeSheetName(string name)
    {
        var invalidChars = new[] { '\\', '/', '*', '[', ']', ':', '?' };

        foreach (var invalidChar in invalidChars)
            name = name.Replace(invalidChar, '-');

        return name.Length > 31
            ? name[..31]
            : name;
    }

    private static void ShowSuccess(string fileType)
    {
        MessageBox.Show(
            $"{fileType} çıktısı başarıyla oluşturuldu.",
            "Başarılı",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private static void ShowError(Exception ex)
    {
        MessageBox.Show(
            $"Çıktı alınırken hata oluştu:\n{ex.Message}",
            "Hata",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}