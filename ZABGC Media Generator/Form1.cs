using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using ZABGC_Media_Generator.Properties;

namespace ZABGC_Media_Generator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [STAThread] // Важно для работы с диалоговыми окнами
        private void button1_Click(object sender, EventArgs e)
        {
            // 1. Создаем изображение с закругленным прямоугольником
            var imageInfo = new SKImageInfo(1920, 1080);
            using (var surface = SKSurface.Create(imageInfo))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColor.Parse("#BC7205"));

                var paint = new SKPaint
                {
                    Color = SKColors.White,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                };

                var rect = new SKRect(210, 10, 1910, 870);
                var rect1 = new SKRect(210, 880, 1910, 1070);

                canvas.DrawRoundRect(rect, 32, 32, paint);
                canvas.DrawRoundRect(rect1, 32, 32, paint);

                var winFormsBitmap = Properties.Resources.Professinalitet_logo;

                using (var stream = new MemoryStream())
                {
                    winFormsBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Position = 0;
                    using (var skiaBitmap = SKBitmap.Decode(stream))
                    {
                        // Теперь можно использовать skiaBitmap на canvas
                        canvas.DrawBitmap(skiaBitmap, 210, 880); // Примерные координаты
                    }
                }

                var fontStream = new MemoryStream(Properties.Resources.DelaGothicOne_Regular); // "Arial" — имя файла в Resources
                var typeface = new SKFont(SKTypeface.FromStream(fontStream));
                typeface.Size = 48;
                var textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true
                };

                // Создание текстового блока
                var text = "Пример текста через SKTexaskdjhalkdjakljdhakldhlkajdhsjlakdjksadhkjldljkhdkjlshakljdhalkhlkjhdkljahdtBlob";
                var textBlob = SKTextBlob.Create(text, typeface);

                // Рисование на canvas
                canvas.DrawText(textBlob, 50, 100, textPaint);





                // 2. Экспортируем в PNG (во временный файл)
                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    string tempFilePath = Path.GetTempFileName() + ".png";
                    File.WriteAllBytes(tempFilePath, data.ToArray());

                    // 3. Открываем в стандартном просмотрщике изображений
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFilePath,
                        UseShellExecute = true // Открыть через ассоциированную программу
                    });

                    // Можно удалить файл после закрытия просмотрщика, 
                    // но это сложно отследить. В реальном приложении 
                    // лучше предложить пользователю сохранить файл явно.
                }
            }
        }

        // Метод для создания текста с переносами
    }
}
