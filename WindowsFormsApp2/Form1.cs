using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class MemoGameForm : Form
    {
        private bool isProcessing = false;
        private List<Image> images;
        private PictureBox firstPictureBox;
        private int pairsFound;
        private DateTime startTime;
        private Timer gameTimer;
        private int score;
        private int mistakes;

        public MemoGameForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Ініціалізація зображень
            List<Image> originalImages = new List<Image>
            {
                Properties.Resources.images__1_,
                Properties.Resources.images__2_,
                Properties.Resources.images__3_,
                Properties.Resources.images__4_,
                Properties.Resources.images__5_,
                Properties.Resources.images__6_,
                Properties.Resources.images__7_,
                Properties.Resources.images__8_
            };

            // Подвоюємо кожне зображення для створення пар
            images = new List<Image>(originalImages);
            images.AddRange(originalImages);

            // Перемішуємо список зображень
            Shuffle(images);

            // Розміщення картинок на формі
            int row = 0;
            int col = 0;
            const int cardSize = 120; // Розмір картинок
            const int spacing = 10; // Відступи між картинками

            foreach (Image image in images)
            {
                PictureBox pictureBox = new PictureBox();
                pictureBox.Size = new Size(cardSize, cardSize);
                pictureBox.Location = new Point(col * (cardSize + spacing), row * (cardSize + spacing));
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Image = Properties.Resources.question_mark; // Зображення за замовчуванням - закрите
                pictureBox.Tag = image; // Пов'язуємо зображення з PictureBox для порівняння
                pictureBox.Click += PictureBox_Click;
                Controls.Add(pictureBox);

                col++;
                if (col == 4) // 4 картинки в ряду
                {
                    col = 0;
                    row++;
                }
            }

            // Ініціалізуємо таймер гри
            gameTimer = new Timer();
            gameTimer.Interval = 1000; // Оновлення таймера кожну секунду
            gameTimer.Tick += GameTimer_Tick;

            // Оновлення відображення часу та очків на формі
            UpdateLabels();
        }

        private async void GameTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            // Оновлення лейбла з часом гри
            lblTime.Text = $"Час: {elapsedTime:mm\\:ss}";

            // Розрахунок та оновлення лейбла з очками
            if (pairsFound > 0 && elapsedTime.TotalSeconds > 0)
            {
                double scorePerSecond = score / elapsedTime.TotalSeconds;
                lblScore.Text = $"Очки: {score} ({scorePerSecond:F2} очків/с)";
            }
        }

        private async void PictureBox_Click(object sender, EventArgs e)
        {
            if (isProcessing)
                return; // Якщо зараз проводиться порівняння, ігноруємо клік

            PictureBox pictureBox = (PictureBox)sender;

            // Перевірка, чи картинка є закритою (має початкове зображення)
            if (pictureBox.Image != Properties.Resources.question_mark)
            {
                // Відкриття картинки
                pictureBox.Image = (Image)pictureBox.Tag;

                if (firstPictureBox == null)
                {
                    // Запам'ятовуємо першу відкриту картинку
                    firstPictureBox = pictureBox;
                }
                else
                {
                    // Порівняння двох відкритих картинок
                    isProcessing = true; // Позначаємо, що почалася обробка порівняння

                    if ((Image)pictureBox.Tag != (Image)firstPictureBox.Tag)
                    {
                        // Не співпадають, перевертаємо назад обидві картинки через коротку затримку
                        await Task.Delay(1000); // Затримка для відображення картинки

                        pictureBox.Image = Properties.Resources.question_mark;
                        firstPictureBox.Image = Properties.Resources.question_mark;

                        // Збільшуємо кількість помилок
                        mistakes++;
                        // Зменшуємо очки за помилку
                        score -= 100;
                    }
                    else
                    {
                        // Співпадають, залишаємо відкритими обидві картинки
                        pairsFound++;
                        score += 1000;
                    }

                    // Очищення першої вибраної картинки
                    firstPictureBox = null;

                    // Перевірка на закінчення гри
                    if (pairsFound == images.Count / 2)
                    {
                        gameTimer.Stop();
                        MessageBox.Show($"Гра завершена! Очки: {score}");
                    }

                    isProcessing = false; // Завершили обробку порівняння
                }

                // Оновлення відображення
                UpdateLabels();
            }
        }

        private void UpdateLabels()
        {
            // Оновлення лейблів з часом та очками
            TimeSpan elapsedTime = DateTime.Now - startTime;
            lblTime.Text = $"Час: {elapsedTime:mm\\:ss}";
            lblScore.Text = $"Очки: {score}";
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            // Початок нової гри
            pairsFound = 0;
            score = 0;
            mistakes = 0;
            Shuffle(images); // Перемішати список зображень

            // Оголошення змінної index для ітерації через список images
            int index = 0;

            // Відображення всіх картинок на короткий час
            foreach (Control control in Controls)
            {
                if (control is PictureBox pictureBox)
                {
                    pictureBox.Tag = images[index]; // Прив'язуємо нове зображення до PictureBox
                    pictureBox.Image = (Image)pictureBox.Tag; // Відкрите зображення
                    index++; // Збільшуємо індекс для наступного зображення
                }
            }

            // Затримка перед початком гри
            await Task.Delay(1000);

            // Закриття всіх картинок перед початком гри
            foreach (Control control in Controls)
            {
                if (control is PictureBox pictureBox)
                {
                    pictureBox.Image = Properties.Resources.question_mark; // Закрите зображення
                }
            }

            gameTimer.Start();
            startTime = DateTime.Now;

            UpdateLabels(); // Оновлення відображення часу та очків
        }

        private void Shuffle<T>(List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
