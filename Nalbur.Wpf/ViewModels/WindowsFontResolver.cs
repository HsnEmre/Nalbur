using PdfSharp.Fonts;
using System.IO;

namespace Nalbur.Wpf.ViewModels;

public sealed class WindowsFontResolver : IFontResolver
{
    public const string FontFamilyName = "Arial";

    private const string ArialRegularFace = "Arial#Regular";
    private const string ArialBoldFace = "Arial#Bold";
    private const string ArialItalicFace = "Arial#Italic";
    private const string ArialBoldItalicFace = "Arial#BoldItalic";

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (isBold && isItalic)
            return new FontResolverInfo(ArialBoldItalicFace);

        if (isBold)
            return new FontResolverInfo(ArialBoldFace);

        if (isItalic)
            return new FontResolverInfo(ArialItalicFace);

        return new FontResolverInfo(ArialRegularFace);
    }

    public byte[] GetFont(string faceName)
    {
        return faceName switch
        {
            ArialBoldFace => LoadFontFile("arialbd.ttf", "segoeuib.ttf"),
            ArialItalicFace => LoadFontFile("ariali.ttf", "segoeuii.ttf"),
            ArialBoldItalicFace => LoadFontFile("arialbi.ttf", "segoeuiz.ttf"),
            _ => LoadFontFile("arial.ttf", "segoeui.ttf")
        };
    }

    private static byte[] LoadFontFile(params string[] fontFileNames)
    {
        var fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        foreach (var fontFileName in fontFileNames)
        {
            var path = Path.Combine(fontsFolder, fontFileName);

            if (File.Exists(path))
                return File.ReadAllBytes(path);
        }

        throw new FileNotFoundException(
            "PDF oluşturmak için gerekli Windows font dosyası bulunamadı.",
            string.Join(", ", fontFileNames));
    }
}