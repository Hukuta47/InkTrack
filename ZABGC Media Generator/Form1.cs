using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

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
    }
}
