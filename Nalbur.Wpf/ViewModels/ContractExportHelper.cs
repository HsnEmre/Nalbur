using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Win32;
using MigraDoc;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Nalbur.Domain.Entities;
using System.Windows;
using PdfSharp.Fonts;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Nalbur.Wpf.ViewModels;

public static class ContractExportHelper
{
    public static void ExportContractToExcel(WorkContract contract)
    {
        try
        {
            var filePath = GetSaveFilePath(
                $"Sozlesme-{contract.Id}.xlsx",
                "Excel Dosyası (*.xlsx)|*.xlsx");

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Sözleşme");

            ws.Cell("A1").Value = "İŞ / SÖZLEŞME FORMU";
            ws.Range("A1:D1").Merge();
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 18;
            ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell("A3").Value = "Sözleşme No:";
            ws.Cell("B3").Value = contract.Id;

            ws.Cell("A4").Value = "Tarih:";
            ws.Cell("B4").Value = contract.ContractDate.ToString("dd.MM.yyyy");

            ws.Cell("A5").Value = "Başlık:";
            ws.Cell("B5").Value = contract.Title;

            ws.Cell("A6").Value = "Müşteri / Firma:";
            ws.Cell("B6").Value = contract.CustomerName ?? "";

            ws.Cell("A7").Value = "Telefon:";
            ws.Cell("B7").Value = contract.CustomerPhone ?? "";

            ws.Range("A3:A7").Style.Font.Bold = true;

            AddExcelSection(ws, "A9", "D9", "A10", "D15", "YAPILACAK İŞLER", contract.WorkDescription);
            AddExcelSection(ws, "A17", "D17", "A18", "D23", "KULLANILACAK MALZEMELER", contract.Materials);
            AddExcelSection(ws, "A25", "D25", "A26", "D30", "NOTLAR", contract.Notes ?? "");

            ws.Cell("A33").Value = "MÜŞTERİ İMZA";
            ws.Cell("D33").Value = "FİRMA YETKİLİ İMZA";
            ws.Cell("A33").Style.Font.Bold = true;
            ws.Cell("D33").Style.Font.Bold = true;

            ws.Range("A34:B37").Merge();
            ws.Range("D34:E37").Merge();

            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 22;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 22;
            ws.Column(4).Width = 22;

            workbook.SaveAs(filePath);

            ShowSuccess("Excel sözleşme");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    public static void ExportContractToWord(WorkContract contract)
    {
        try
        {
            var filePath = GetSaveFilePath(
                $"Sozlesme-{contract.Id}.docx",
                "Word Dosyası (*.docx)|*.docx");

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            using var wordDocument = WordprocessingDocument.Create(
                filePath,
                WordprocessingDocumentType.Document);

            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new W.Document();

            var body = new W.Body();

            body.Append(CreateParagraph("İŞ / SÖZLEŞME FORMU", true, "36"));
            body.Append(CreateParagraph(""));
            body.Append(CreateParagraph($"Sözleşme No: {contract.Id}"));
            body.Append(CreateParagraph($"Tarih: {contract.ContractDate:dd.MM.yyyy}"));
            body.Append(CreateParagraph($"Başlık: {contract.Title}"));
            body.Append(CreateParagraph($"Müşteri / Firma: {contract.CustomerName ?? ""}"));
            body.Append(CreateParagraph($"Telefon: {contract.CustomerPhone ?? ""}"));
            body.Append(CreateParagraph(""));

            body.Append(CreateParagraph("YAPILACAK İŞLER", true, "28"));
            body.Append(CreateParagraph(contract.WorkDescription));
            body.Append(CreateParagraph(""));

            body.Append(CreateParagraph("KULLANILACAK MALZEMELER", true, "28"));
            body.Append(CreateParagraph(contract.Materials));
            body.Append(CreateParagraph(""));

            body.Append(CreateParagraph("NOTLAR", true, "28"));
            body.Append(CreateParagraph(contract.Notes ?? ""));
            body.Append(CreateParagraph(""));
            body.Append(CreateParagraph(""));
            body.Append(CreateParagraph("MÜŞTERİ İMZA                                      FİRMA YETKİLİ İMZA", true, "24"));
            body.Append(CreateParagraph(""));
            body.Append(CreateParagraph(""));
            body.Append(CreateParagraph("__________________________                    __________________________"));

            mainPart.Document.Append(body);
            mainPart.Document.Save();

            ShowSuccess("Word sözleşme");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    public static void ExportContractToPdf(WorkContract contract)
    {
        try
        {
            var filePath = GetSaveFilePath(
                $"Sozlesme-{contract.Id}.pdf",
                "PDF Dosyası (*.pdf)|*.pdf");

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            ConfigurePdfFonts();

            var document = new Document();
            document.Info.Title = $"Sözleşme - {contract.Id}";

            document.Styles["Normal"].Font.Name = "Arial";
            document.Styles["Normal"].Font.Size = 10;

            var section = document.AddSection();
            section.PageSetup.Orientation = Orientation.Portrait;
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);

            var title = section.AddParagraph("İŞ / SÖZLEŞME FORMU");
            title.Format.Font.Name = "Arial";
            title.Format.Font.Size = 18;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.6);

            AddInfoLine(section, "Sözleşme No:", contract.Id.ToString());
            AddInfoLine(section, "Tarih:", contract.ContractDate.ToString("dd.MM.yyyy"));
            AddInfoLine(section, "Başlık:", contract.Title);
            AddInfoLine(section, "Müşteri / Firma:", contract.CustomerName ?? "");
            AddInfoLine(section, "Telefon:", contract.CustomerPhone ?? "");

            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.4);

            AddSectionTitle(section, "YAPILACAK İŞLER");
            AddBoxText(section, contract.WorkDescription);

            AddSectionTitle(section, "KULLANILACAK MALZEMELER");
            AddBoxText(section, contract.Materials);

            AddSectionTitle(section, "NOTLAR");
            AddBoxText(section, contract.Notes ?? "");

            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(1.0);

            AddSignatureArea(section);

            var renderer = new PdfDocumentRenderer
            {
                Document = document
            };

            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);

            ShowSuccess("PDF sözleşme");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private static void AddExcelSection(
        IXLWorksheet ws,
        string headerStart,
        string headerEnd,
        string contentStart,
        string contentEnd,
        string header,
        string content)
    {
        ws.Cell(headerStart).Value = header;
        ws.Range($"{headerStart}:{headerEnd}").Merge();
        ws.Cell(headerStart).Style.Font.Bold = true;
        ws.Cell(headerStart).Style.Fill.BackgroundColor = XLColor.LightGray;

        ws.Cell(contentStart).Value = content ?? "";
        ws.Range($"{contentStart}:{contentEnd}").Merge();
        ws.Cell(contentStart).Style.Alignment.WrapText = true;
        ws.Cell(contentStart).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
    }

    private static W.Paragraph CreateParagraph(
        string text,
        bool bold = false,
        string fontSize = "24")
    {
        var runProperties = new W.RunProperties(
            new W.FontSize { Val = fontSize });

        if (bold)
            runProperties.Append(new W.Bold());

        return new W.Paragraph(
            new W.Run(
                runProperties,
                new W.Text(text ?? string.Empty)
                {
                    Space = SpaceProcessingModeValues.Preserve
                }));
    }

    private static void ConfigurePdfFonts()
    {
        PredefinedFontsAndChars.ErrorFontName = WindowsFontResolver.FontFamilyName;

        if (GlobalFontSettings.FontResolver == null)
        {
            GlobalFontSettings.FontResolver = new WindowsFontResolver();
        }
    }

    private static void AddInfoLine(Section section, string label, string value)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Name = "Arial";
        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);
        paragraph.AddFormattedText(label + " ", TextFormat.Bold);
        paragraph.AddText(value ?? "");
    }

    private static void AddSectionTitle(Section section, string title)
    {
        var paragraph = section.AddParagraph(title);
        paragraph.Format.Font.Name = "Arial";
        paragraph.Format.Font.Bold = true;
        paragraph.Format.Font.Size = 12;
        paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.2);
    }

    private static void AddBoxText(Section section, string text)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.AddColumn(Unit.FromCentimeter(17));

        var row = table.AddRow();
        row.Height = Unit.FromCentimeter(2.4);
        row.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;

        var paragraph = row.Cells[0].AddParagraph(text ?? "");
        paragraph.Format.Font.Name = "Arial";
    }

    private static void AddSignatureArea(Section section)
    {
        var signatureTable = section.AddTable();
        signatureTable.Borders.Width = 0;

        signatureTable.AddColumn(Unit.FromCentimeter(8));
        signatureTable.AddColumn(Unit.FromCentimeter(2));
        signatureTable.AddColumn(Unit.FromCentimeter(8));

        var signatureHeader = signatureTable.AddRow();

        signatureHeader.Cells[0].AddParagraph("MÜŞTERİ İMZA");
        signatureHeader.Cells[2].AddParagraph("FİRMA YETKİLİ İMZA");

        signatureHeader.Cells[0].Format.Font.Name = "Arial";
        signatureHeader.Cells[2].Format.Font.Name = "Arial";
        signatureHeader.Cells[0].Format.Font.Bold = true;
        signatureHeader.Cells[2].Format.Font.Bold = true;

        var emptyRow = signatureTable.AddRow();
        emptyRow.Height = Unit.FromCentimeter(2);

        var lineRow = signatureTable.AddRow();
        lineRow.Cells[0].AddParagraph("__________________________");
        lineRow.Cells[2].AddParagraph("__________________________");

        lineRow.Cells[0].Format.Font.Name = "Arial";
        lineRow.Cells[2].Format.Font.Name = "Arial";
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