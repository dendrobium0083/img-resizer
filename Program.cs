using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace img_resizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("画像リサイザー");
            Console.WriteLine("==============");
            
            if (args.Length < 2)
            {
                Console.WriteLine("使用方法: img-resizer <入力ファイル> <出力ファイル> [幅] [高さ]");
                Console.WriteLine("例: img-resizer input.jpg output.jpg 800 600");
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];
            int? width = args.Length > 2 ? int.Parse(args[2]) : null;
            int? height = args.Length > 3 ? int.Parse(args[3]) : null;

            try
            {
                ResizeImage(inputPath, outputPath, width, height);
                Console.WriteLine($"画像を変換しました: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラーが発生しました: {ex.Message}");
            }
        }

        static void ResizeImage(string inputPath, string outputPath, int? width, int? height)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"ファイルが見つかりません: {inputPath}");
            }

            using (var originalImage = new Bitmap(inputPath))
            {
                int newWidth = width ?? originalImage.Width;
                int newHeight = height ?? originalImage.Height;

                // アスペクト比を維持する場合
                if (width.HasValue && !height.HasValue)
                {
                    double ratio = (double)width.Value / originalImage.Width;
                    newHeight = (int)(originalImage.Height * ratio);
                }
                else if (!width.HasValue && height.HasValue)
                {
                    double ratio = (double)height.Value / originalImage.Height;
                    newWidth = (int)(originalImage.Width * ratio);
                }

                using (var resizedImage = new Bitmap(newWidth, newHeight))
                {
                    using (var graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                    }

                    // 出力形式を決定（拡張子から）
                    ImageFormat format = GetImageFormat(outputPath);
                    resizedImage.Save(outputPath, format);
                }
            }
        }

        static ImageFormat GetImageFormat(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".gif" => ImageFormat.Gif,
                ".bmp" => ImageFormat.Bmp,
                _ => ImageFormat.Png
            };
        }
    }
}

