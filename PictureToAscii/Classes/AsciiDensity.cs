using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class AsciiDensity
{
    public static char[] BuildRamp(string characters, string fontFamily, double fontSize)
    {
        Dictionary<char, double> map = new Dictionary<char, double>();

        foreach (char c in characters)
        {
            double darkness = MeasureChar(c, fontFamily, fontSize);
            map[c] = darkness;
        }

        return map.OrderByDescending(kv => kv.Value)
                  .Select(kv => kv.Key)
                  .ToArray();
    }

    private static double MeasureChar(char c, string fontFamily, double fontSize)
    {
        const int size = 64;

        DrawingVisual visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, size, size));

            FormattedText text = new FormattedText(
                c.ToString(),
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily),
                fontSize,
                Brushes.Black,
                1.0);

            Point center = new Point(
                (size - text.Width) / 2,
                (size - text.Height) / 2);

            dc.DrawText(text, center);
        }

        RenderTargetBitmap bmp = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(visual);

        byte[] pixels = new byte[size * size * 4];
        bmp.CopyPixels(pixels, size * 4, 0);

        int dark = 0;
        int total = size * size;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte r = pixels[i + 2];
            byte g = pixels[i + 1];
            byte b = pixels[i + 0];
            int luminance = (r + g + b) / 3;

            if (luminance < 128) dark++;
        }

        return (double)dark / total;
    }
}
