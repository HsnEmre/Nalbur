using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Win32;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Nalbur.Domain.Entities;
using System.Windows;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Nalbur.Wpf.ViewModels;

public static class InvoiceExportHelper
{
    public static void ExportSaleInvoiceToExcel(Sale sale)
    {
        try
        {
            var filePath = GetSaveFilePath($"Fatura-Satis-{sale.Id}.xlsx", "Excel Dosyası (*.xlsx)|*.xlsx");
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Fatura");

            ws.Cell("A1").Value = "NALBUR SATIŞ FATURASI";
            ws.Range("A1:E1").Merge();
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 18;
            ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell("A3").Value = "Fatura No:";
            ws.Cell("B3").Value = sale.Id;

            ws.Cell("A4").Value = "Tarih:";
            ws.Cell("B4").Value = sale.SaleDate.ToString("dd.MM.yyyy HH:mm");

            ws.Cell("A5").Value = "Müşteri:";
            ws.Cell("B5").Value = GetCustomerName(sale);

            ws.Cell("A6").Value = "Telefon:";
            ws.Cell("B6").Value = sale.Customer?.Phone ?? "";

            ws.Cell("A7").Value = "Satış Türü:";
            ws.Cell("B7").Value = sale.SaleType.ToString();

            ws.Cell("A8").Value = "Durum:";
            ws.Cell("B8").Value = sale.IsReturned ? "İade Edildi" : "Satış";

            int headerRow = 10;

            ws.Cell(headerRow, 1).Value = "Ürün";
            ws.Cell(headerRow, 2).Value = "Miktar";
            ws.Cell(headerRow, 3).Value = "Birim Fiyat";
            ws.Cell(headerRow, 4).Value = "Toplam";

            ws.Range(headerRow, 1, headerRow, 4).Style.Font.Bold = true;
            ws.Range(headerRow, 1, headerRow, 4).Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = headerRow + 1;

            foreach (var item in sale.SaleItems)
            {
                ws.Cell(row, 1).Value = item.Product?.Name ?? "";
                ws.Cell(row, 2).Value = item.Quantity;
                ws.Cell(row, 3).Value = item.UnitPrice;
                ws.Cell(row, 4).Value = item.TotalPrice;

                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 ₺";
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";

                row++;
            }

            ws.Cell(row + 1, 3).Value = "GENEL TOPLAM:";
            ws.Cell(row + 1, 3).Style.Font.Bold = true;

            ws.Cell(row + 1, 4).Value = sale.TotalAmount;
            ws.Cell(row + 1, 4).Style.Font.Bold = true;
            ws.Cell(row + 1, 4).Style.NumberFormat.Format = "#,##0.00 ₺";

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);

            ShowSuccess("Excel fatura");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    public static void ExportSaleInvoiceToWord(Sale sale)
    {
        try
        {
            var filePath = GetSaveFilePath($"Fatura-Satis-{sale.Id}.docx", "Word Dosyası (*.docx)|*.docx");
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            using var wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new W.Document();

            var body = new W.Body();

            body.Append(CreateParagraph("NALBUR SATIŞ FATURASI", true, "32"));
            body.Append(CreateParagraph($"Fatura No: {sale.Id}", false));
            body.Append(CreateParagraph($"Tarih: {sale.SaleDate:dd.MM.yyyy HH:mm}", false));
            body.Append(CreateParagraph($"Müşteri: {GetCustomerName(sale)}", false));
            body.Append(CreateParagraph($"Telefon: {sale.Customer?.Phone ?? ""}", false));
            body.Append(CreateParagraph($"Satış Türü: {sale.SaleType}", false));
            body.Append(CreateParagraph($"Durum: {(sale.IsReturned ? "İade Edildi" : "Satış")}", false));
            body.Append(new W.Paragraph(new W.Run(new W.Text(""))));

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
            headerRow.Append(CreateWordCell("Ürün", true));
            headerRow.Append(CreateWordCell("Miktar", true));
            headerRow.Append(CreateWordCell("Birim Fiyat", true));
            headerRow.Append(CreateWordCell("Toplam", true));
            table.Append(headerRow);

            foreach (var item in sale.SaleItems)
            {
                var row = new W.TableRow();
                row.Append(CreateWordCell(item.Product?.Name ?? "", false));
                row.Append(CreateWordCell(item.Quantity.ToString("N2"), false));
                row.Append(CreateWordCell(item.UnitPrice.ToString("C2"), false));
                row.Append(CreateWordCell(item.TotalPrice.ToString("C2"), false));
                table.Append(row);
            }

            body.Append(table);
            body.Append(new W.Paragraph(new W.Run(new W.Text(""))));
            body.Append(CreateParagraph($"GENEL TOPLAM: {sale.TotalAmount:C2}", true, "28"));

            mainPart.Document.Append(body);
            mainPart.Document.Save();

            ShowSuccess("Word fatura");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    public static void ExportSaleInvoiceToPdf(Sale sale)
    {
        try
        {
            var filePath = GetSaveFilePath($"Fatura-Satis-{sale.Id}.pdf", "PDF Dosyası (*.pdf)|*.pdf");
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var document = new Document();
            document.Info.Title = $"Fatura - Satış {sale.Id}";

            var section = document.AddSection();
            section.PageSetup.Orientation = Orientation.Portrait;

            var title = section.AddParagraph("NALBUR SATIŞ FATURASI");
            title.Format.Font.Size = 18;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            AddInfoLine(section, "Fatura No:", sale.Id.ToString());
            AddInfoLine(section, "Tarih:", sale.SaleDate.ToString("dd.MM.yyyy HH:mm"));
            AddInfoLine(section, "Müşteri:", GetCustomerName(sale));
            AddInfoLine(section, "Telefon:", sale.Customer?.Phone ?? "");
            AddInfoLine(section, "Satış Türü:", sale.SaleType.ToString());
            AddInfoLine(section, "Durum:", sale.IsReturned ? "İade Edildi" : "Satış");

            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.3);

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn(Unit.FromCentimeter(7));
            table.AddColumn(Unit.FromCentimeter(3));
            table.AddColumn(Unit.FromCentimeter(3.5));
            table.AddColumn(Unit.FromCentimeter(3.5));

            var header = table.AddRow();
            header.Shading.Color = Colors.LightGray;
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Ürün");
            header.Cells[1].AddParagraph("Miktar");
            header.Cells[2].AddParagraph("Birim Fiyat");
            header.Cells[3].AddParagraph("Toplam");

            foreach (var item in sale.SaleItems)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(item.Product?.Name ?? "");
                row.Cells[1].AddParagraph(item.Quantity.ToString("N2"));
                row.Cells[2].AddParagraph(item.UnitPrice.ToString("C2"));
                row.Cells[3].AddParagraph(item.TotalPrice.ToString("C2"));
            }

            var totalParagraph = section.AddParagraph();
            totalParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            totalParagraph.Format.Alignment = ParagraphAlignment.Right;
            totalParagraph.Format.Font.Size = 14;
            totalParagraph.Format.Font.Bold = true;
            totalParagraph.AddText($"GENEL TOPLAM: {sale.TotalAmount:C2}");

            var renderer = new PdfDocumentRenderer
            {
                Document = document
            };

            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);

            ShowSuccess("PDF fatura");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private static string GetCustomerName(Sale sale)
    {
        var name = $"{sale.Customer?.Name} {sale.Customer?.SurnameCompany}".Trim();
        return string.IsNullOrWhiteSpace(name) ? "Perakende Müşteri" : name;
    }

    private static void AddInfoLine(Section section, string label, string value)
    {
        var p = section.AddParagraph();
        p.AddFormattedText(label + " ", TextFormat.Bold);
        p.AddText(value);
    }

    private static W.Paragraph CreateParagraph(string text, bool bold, string fontSize = "24")
    {
        return new W.Paragraph(
            new W.Run(
                new W.RunProperties(
                    bold ? new W.Bold() : null,
                    new W.FontSize { Val = fontSize }),
                new W.Text(text ?? "")));
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
                    new W.Text(text ?? ""))));
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

    private static void ShowSuccess(string text)
    {
        MessageBox.Show(
            $"{text} çıktısı başarıyla oluşturuldu.",
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