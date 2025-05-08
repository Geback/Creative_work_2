using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ColorDetectorApp
{
    public partial class MainForm : Form
    {
        // Элементы управления формы
        private PictureBox picOriginal;
        private PictureBox picProcessed;
        private Label lblColorName;
        private Label lblRGB;
        private Label lblHEX;
        private Label lblFileName;
        private Label lblFileSize;
        private Panel panelColor;
        private Button btnLoadImage;
        private Button btnSaveResults;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Инициализация всех элементов управления
            this.picOriginal = new PictureBox();
            this.picProcessed = new PictureBox();
            this.lblColorName = new Label();
            this.lblRGB = new Label();
            this.lblHEX = new Label();
            this.lblFileName = new Label();
            this.lblFileSize = new Label();
            this.panelColor = new Panel();
            this.btnLoadImage = new Button();
            this.btnSaveResults = new Button();

            // Настройка свойств элементов
            // PictureBox для оригинального изображения
            picOriginal.Location = new System.Drawing.Point(12, 58);
            picOriginal.Size = new System.Drawing.Size(380, 380);
            picOriginal.BorderStyle = BorderStyle.FixedSingle;
            picOriginal.SizeMode = PictureBoxSizeMode.Zoom;

            // PictureBox для обработанного изображения
            picProcessed.Location = new System.Drawing.Point(408, 58);
            picProcessed.Size = new System.Drawing.Size(380, 380);
            picProcessed.BorderStyle = BorderStyle.FixedSingle;
            picProcessed.SizeMode = PictureBoxSizeMode.Zoom;

            // Надписи
            lblColorName.Location = new System.Drawing.Point(12, 451);
            lblColorName.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            lblColorName.AutoSize = true;

            lblRGB.Location = new System.Drawing.Point(12, 481);
            lblRGB.Font = new Font("Microsoft Sans Serif", 10F);
            lblRGB.AutoSize = true;

            lblHEX.Location = new System.Drawing.Point(12, 508);
            lblHEX.Font = new Font("Microsoft Sans Serif", 10F);
            lblHEX.AutoSize = true;

            lblFileName.Location = new System.Drawing.Point(408, 12);
            lblFileName.AutoSize = true;

            lblFileSize.Location = new System.Drawing.Point(408, 29);
            lblFileSize.AutoSize = true;

            // Панель для отображения цвета
            panelColor.Location = new System.Drawing.Point(663, 451);
            panelColor.Size = new System.Drawing.Size(125, 74);
            panelColor.BorderStyle = BorderStyle.FixedSingle;

            // Кнопки
            btnLoadImage.Location = new System.Drawing.Point(663, 12);
            btnLoadImage.Size = new System.Drawing.Size(125, 40);
            btnLoadImage.Text = "Загрузить изображение";
            btnLoadImage.Click += btnLoadImage_Click;

            btnSaveResults.Location = new System.Drawing.Point(663, 58);
            btnSaveResults.Size = new System.Drawing.Size(125, 40);
            btnSaveResults.Text = "Сохранить результат";
            btnSaveResults.Click += btnSaveResults_Click;

            // Добавление элементов на форму
            this.Controls.Add(picOriginal);
            this.Controls.Add(picProcessed);
            this.Controls.Add(lblColorName);
            this.Controls.Add(lblRGB);
            this.Controls.Add(lblHEX);
            this.Controls.Add(lblFileName);
            this.Controls.Add(lblFileSize);
            this.Controls.Add(panelColor);
            this.Controls.Add(btnLoadImage);
            this.Controls.Add(btnSaveResults);

            // Настройка формы
            this.Text = "Анализатор доминирующего цвета";
            this.ClientSize = new System.Drawing.Size(800, 537);
            this.MinimumSize = new Size(800, 600);
        }

        private void DisplayResults(Bitmap originalImage, Bitmap processedImage, Color dominantColor, string imagePath)
        {
            if (originalImage != null && picOriginal != null)
            {
                picOriginal.Image = new Bitmap(originalImage);
            }

            if (processedImage != null && picProcessed != null)
            {
                picProcessed.Image = new Bitmap(processedImage);
            }

            if (lblColorName != null)
            {
                lblColorName.Text = $"Цвет: {GetColorName(dominantColor)}";
            }

            if (lblRGB != null)
            {
                lblRGB.Text = $"RGB: ({dominantColor.R}, {dominantColor.G}, {dominantColor.B})";
            }

            if (lblHEX != null)
            {
                lblHEX.Text = $"HEX: #{dominantColor.R:X2}{dominantColor.G:X2}{dominantColor.B:X2}";
            }

            if (panelColor != null)
            {
                panelColor.BackColor = dominantColor;
            }

            if (lblFileName != null && !string.IsNullOrEmpty(imagePath))
            {
                lblFileName.Text = Path.GetFileName(imagePath);
            }

            if (lblFileSize != null && !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                lblFileSize.Text = $"Размер: {new FileInfo(imagePath).Length / 1024} KB";
            }
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp|Все файлы|*.*";
                openFileDialog.Title = "Выберите изображение для анализа";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    AnalyzeImage(openFileDialog.FileName);
                }
            }
        }

        private void AnalyzeImage(string imagePath)
        {
            try
            {
                // Загрузка изображения
                Bitmap originalImage = new Bitmap(imagePath);

                // Обработка изображения
                Bitmap processedImage = ProcessImage(originalImage);

                // Получение доминирующего цвета
                Color dominantColor = GetDominantColor(processedImage);

                // Отображение результатов
                DisplayResults(originalImage, processedImage, dominantColor, imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при анализе изображения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap ProcessImage(Bitmap inputImage)
        {
            Bitmap processed = new Bitmap(inputImage.Width, inputImage.Height);

            // 1. Упрощенное сглаживание (замена bilateral filter)
            for (int i = 1; i < inputImage.Width - 1; i++)
            {
                for (int j = 1; j < inputImage.Height - 1; j++)
                {
                    // Простое усреднение 3x3
                    Color c1 = inputImage.GetPixel(i - 1, j - 1);
                    Color c2 = inputImage.GetPixel(i, j - 1);
                    Color c3 = inputImage.GetPixel(i + 1, j - 1);
                    Color c4 = inputImage.GetPixel(i - 1, j);
                    Color c5 = inputImage.GetPixel(i, j);
                    Color c6 = inputImage.GetPixel(i + 1, j);
                    Color c7 = inputImage.GetPixel(i - 1, j + 1);
                    Color c8 = inputImage.GetPixel(i, j + 1);
                    Color c9 = inputImage.GetPixel(i + 1, j + 1);

                    int r = (c1.R + c2.R + c3.R + c4.R + c5.R + c6.R + c7.R + c8.R + c9.R) / 9;
                    int g = (c1.G + c2.G + c3.G + c4.G + c5.G + c6.G + c7.G + c8.G + c9.G) / 9;
                    int b = (c1.B + c2.B + c3.B + c4.B + c5.B + c6.B + c7.B + c8.B + c9.B) / 9;

                    processed.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }

            // 2. Упрощенная коррекция освещения (замена CLAHE)
            // Просто увеличиваем контрастность
            processed = AdjustContrast(processed, 1.2f);

            return processed;
        }

        private Bitmap AdjustContrast(Bitmap image, float contrast)
        {
            Bitmap adjustedImage = new Bitmap(image.Width, image.Height);

            // Матрица контрастности
            float[][] colorMatrixElements = {
                new float[] {contrast, 0, 0, 0, 0},
                new float[] {0, contrast, 0, 0, 0},
                new float[] {0, 0, contrast, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            };

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            using (ImageAttributes attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(colorMatrix);

                using (Graphics g = Graphics.FromImage(adjustedImage))
                {
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                        0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return adjustedImage;
        }

        private Color GetDominantColor(Bitmap image)
        {
            // Уменьшение размера для ускорения обработки
            Bitmap resized = new Bitmap(image, new Size(100, 100));

            // Словарь для подсчета цветов
            Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

            // Проход по всем пикселям
            for (int x = 0; x < resized.Width; x++)
            {
                for (int y = 0; y < resized.Height; y++)
                {
                    Color pixelColor = resized.GetPixel(x, y);

                    // Игнорируем слишком светлые и темные пиксели
                    if (pixelColor.GetBrightness() < 0.2f || pixelColor.GetBrightness() > 0.9f)
                        continue;

                    // Квантование цвета для уменьшения вариативности
                    Color quantizedColor = Color.FromArgb(
                        pixelColor.R / 10 * 10,
                        pixelColor.G / 10 * 10,
                        pixelColor.B / 10 * 10);

                    if (colorCounts.ContainsKey(quantizedColor))
                        colorCounts[quantizedColor]++;
                    else
                        colorCounts[quantizedColor] = 1;
                }
            }

            // Находим наиболее часто встречающийся цвет
            if (colorCounts.Count == 0)
                return Color.Gray; // Возвращаем серый по умолчанию

            var dominantColor = colorCounts.OrderByDescending(pair => pair.Value).First().Key;
            return dominantColor;
        }

        private string GetColorName(Color color)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;

            if (r > 200 && g > 200 && b > 200) return "Белый";
            if (r < 50 && g < 50 && b < 50) return "Чёрный";

            if (r > g && r > b)
            {
                if (Math.Abs(g - b) < 30)
                {
                    if (r > 200 && g > 100) return "Оранжевый";
                    return "Красный";
                }
            }
            if (g > r && g > b)
            {
                if (Math.Abs(r - b) < 30)
                {
                    if (g > 200 && r > 150) return "Лаймовый";
                    return "Зелёный";
                }
            }
            if (b > r && b > g)
            {
                if (Math.Abs(r - g) < 30)
                {
                    if (b > 200 && r > 150) return "Голубой";
                    return "Синий";
                }
            }
            if (Math.Abs(r - g) < 30 && Math.Abs(g - b) < 30) return "Серый";
            if (r > 180 && g > 180 && b < 100) return "Жёлтый";
            if (r > 180 && b > 180 && g < 100) return "Пурпурный";
            if (g > 180 && b > 180 && r < 100) return "Бирюзовый";

            return "Неопределённый";
        }

        

        private void btnSaveResults_Click(object sender, EventArgs e)
        {
            if (picProcessed.Image == null)
            {
                MessageBox.Show("Нет обработанного изображения для сохранения", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png";
                saveDialog.Title = "Сохранить обработанное изображение";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        picProcessed.Image.Save(saveDialog.FileName);
                        MessageBox.Show("Изображение успешно сохранено", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}