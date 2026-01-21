using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PictureToAscii
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string DefAscii = ",.-;:_*'^¨~`´|?+=})]([/{&%€¤$#£@!µɸΦΔΩ";
        string CharSet = DefAscii;
        string SelFont = "Consolas";
        string PicPath = "";
        bool IsIntialised = false;
        int AsciiGridSize = 120;

        public MainWindow()
        {
            InitializeComponent();
            IsIntialised = true;
        }

        static string ConvertImageToAscii(BitmapSource image, char[] ramp, int outputWidth)
        {
            int originalWidth = image.PixelWidth;
            int originalHeight = image.PixelHeight;

            double aspectRatio = (double)originalHeight / originalWidth;
            int outputHeight = (int)(outputWidth * aspectRatio * 0.5);

            TransformedBitmap resized = new TransformedBitmap(
                image,
                new ScaleTransform(
                    (double)outputWidth / originalWidth,
                    (double)outputHeight / originalHeight));

            byte[] pixels = new byte[resized.PixelWidth * resized.PixelHeight * 4];
            resized.CopyPixels(pixels, resized.PixelWidth * 4, 0);

            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < resized.PixelHeight; y++)
            {
                for (int x = 0; x < resized.PixelWidth; x++)
                {
                    int index = (y * resized.PixelWidth + x) * 4;
                    byte b = pixels[index + 0];
                    byte g = pixels[index + 1];
                    byte r = pixels[index + 2];

                    int luminance = (r + g + b) / 3;

                    float normalized = luminance / 255f;
                    int rampIndex = (int)(normalized * (ramp.Length - 1));
                    sb.Append(ramp[rampIndex]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }


        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                BitmapImage image = new BitmapImage(new Uri(PicPath));
                char[] ramp = AsciiDensity.BuildRamp(CharSet, SelFont, 16);
                string ascii = ConvertImageToAscii(image, ramp, AsciiGridSize);
                resultWindow result = new resultWindow();
                result.Show();
                result.OutputBox.FontFamily = new FontFamily(SelFont);
                result.OutputBox.Text = ascii;
           
            }
            catch
            {
                
            }
        }

        private void PicBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog picDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.png)|*.jpg;*.png"
            };

            if (picDialog.ShowDialog() == true)
            {
                PicPath = picDialog.FileName;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(PicPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                DisplayPic.Source = bitmap;
            }
        }

        private void AsciiPicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (AsciiPicker.Text.Trim() != "")
                {
                    CharSet = AsciiPicker.Text.ToString();
                } else
                {
                    CharSet = DefAscii;
                }
                
            }
            catch { }
        
        }

        private void FontPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsIntialised != true)
            {
                return;
            }

            if (FontPicker.SelectedItem is FontFamily selectedFont)
            {
                SelFont = selectedFont.Source;
            }

        }

        private void SizePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Convert.ToInt32(SizePicker.Text.ToString()) >= 30 && 100 >= Convert.ToInt32(SizePicker.Text.ToString()))
                {
                    AsciiGridSize = Convert.ToInt32(SizePicker.Text.ToString());
                }
                else if (Convert.ToInt32(SizePicker.Text.ToString()) < 30)
                {
                    SizePicker.Text = "30";
                    AsciiGridSizeSlider.Value = 30;
                }
                else if (Convert.ToInt32(SizePicker.Text.ToString()) > 1000) 
                {
                    SizePicker.Text = "1000";
                    AsciiGridSizeSlider.Value = 1000;
                }
                else
                {
                    try
                    {
                        AsciiGridSizeSlider.Value = Convert.ToInt32(SizePicker.Text.ToString());
                    }
                    catch { }
                }
            }
            catch { SizePicker.Text = "120"; }
        }

        private void SizePicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            string result = null;
            foreach (char c in SizePicker.Text)
            {
                if (char.IsLetter(c))
                {
                    result = SizePicker.Text.Replace(c.ToString(), "");
                    SizePicker.Text = result;
                }
            }
            
        }

        private void AsciiGridSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsIntialised)
            {
                return;
            }

            AsciiGridSizeSlider.Value = Math.Round(AsciiGridSizeSlider.Value);

            SizePicker.Text = AsciiGridSizeSlider.Value.ToString();

            AsciiGridSize = Convert.ToInt32(AsciiGridSizeSlider.Value);
        }
    }
}
