using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace LabWork
{
    // Даний проект є шаблоном для виконання лабораторних робіт
    // з курсу "Об'єктно-орієнтоване програмування та патерни проектування"
    // Необхідно змінювати і дописувати код лише в цьому проекті
    // Відео-інструкції щодо роботи з github можна переглянути 
    // за посиланням https://www.youtube.com/@ViktorZhukovskyy/videos 
    
    // Перелічення режимів відображення графіка
    enum DisplayMode
    {
        Line,    // Лінійний режим (з'єднані сегменти)
        Point    // Точковий режим (тільки маркери)
    }
    
    class GraphForm : Form
    {
        // Параметри функції
        private const double X_START = 0;
        private const double X_END = 2;
        private const double DELTA_X = 0.4;
        
        // Поточний режим відображення
        private DisplayMode currentDisplayMode = DisplayMode.Line;
        
        // Кешовані ресурси для малювання
        private Pen axisPen;
        private Pen gridPen;
        private Pen graphPen;
        private Brush pointBrush;
        private Brush textBrush;
        private Font standardFont;
        private Font labelFont;
        private Font formulaFont;
        
        // Елементи керування
        private Panel controlPanel;
        private RadioButton radioLine;
        private RadioButton radioPoint;
        private Label modeLabel;
        
        public GraphForm()
        {
            this.Text = "Графік функції y = (x³ + 2x) / (3cos(√x) + 1)";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Встановлюємо подвійну буферизацію для плавного відображення
            this.DoubleBuffered = true;
            
            // Ініціалізуємо кешовані ресурси
            InitializeResources();
            
            // Створюємо панель керування
            CreateControlPanel();
        }
        
        // Ініціалізація кешованих ресурсів для малювання
        private void InitializeResources()
        {
            axisPen = new Pen(Color.Black, 2);
            gridPen = new Pen(Color.LightGray, 1);
            graphPen = new Pen(Color.Blue, 2);
            pointBrush = new SolidBrush(Color.Red);
            textBrush = new SolidBrush(Color.Black);
            standardFont = new Font("Arial", 9);
            labelFont = new Font("Arial", 11, FontStyle.Bold);
            formulaFont = new Font("Arial", 10);
        }
        
        // Створення панелі керування з перемикачами режимів
        private void CreateControlPanel()
        {
            controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            
            modeLabel = new Label
            {
                Text = "Режим відображення:",
                Location = new Point(10, 15),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            
            radioLine = new RadioButton
            {
                Text = "Лінійний (з'єднані сегменти)",
                Location = new Point(180, 13),
                AutoSize = true,
                Checked = true,
                Font = new Font("Arial", 9)
            };
            radioLine.CheckedChanged += OnDisplayModeChanged;
            
            radioPoint = new RadioButton
            {
                Text = "Точковий (тільки маркери)",
                Location = new Point(380, 13),
                AutoSize = true,
                Font = new Font("Arial", 9)
            };
            radioPoint.CheckedChanged += OnDisplayModeChanged;
            
            controlPanel.Controls.Add(modeLabel);
            controlPanel.Controls.Add(radioLine);
            controlPanel.Controls.Add(radioPoint);
            
            this.Controls.Add(controlPanel);
        }
        
        // Обробник зміни режиму відображення
        private void OnDisplayModeChanged(object sender, EventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio != null && radio.Checked)
            {
                currentDisplayMode = radio == radioLine ? DisplayMode.Line : DisplayMode.Point;
                this.Invalidate(); // Перемальовуємо форму
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Отримуємо розміри клієнтської області
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;
            
            // Відступи від країв
            int margin = 60;
            int graphWidth = width - 2 * margin;
            int graphHeight = height - 2 * margin;
            
            // Обчислюємо значення функції
            List<PointF> points = CalculateFunctionPoints();
            
            if (points.Count == 0)
                return;
            
            // Знаходимо мінімальне та максимальне значення Y
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            
            foreach (var point in points)
            {
                if (!double.IsInfinity(point.Y) && !double.IsNaN(point.Y))
                {
                    if (point.Y < minY) minY = point.Y;
                    if (point.Y > maxY) maxY = point.Y;
                }
            }
            
            // Додаємо невеликий запас для кращого відображення
            double rangeY = maxY - minY;
            minY -= rangeY * 0.1;
            maxY += rangeY * 0.1;
            
            // Функції для перетворення координат
            float ScaleX(double x)
            {
                return margin + (float)((x - X_START) / (X_END - X_START) * graphWidth);
            }
            
            float ScaleY(double y)
            {
                return margin + graphHeight - (float)((y - minY) / (maxY - minY) * graphHeight);
            }
            
            // Малюємо осі координат
            // Вертикальна вісь Y
            float yAxisX = ScaleX(0);
            if (yAxisX >= margin && yAxisX <= width - margin)
            {
                g.DrawLine(axisPen, yAxisX, margin, yAxisX, height - margin);
            }
            
            // Горизонтальна вісь X
            float xAxisY = ScaleY(0);
            if (xAxisY >= margin && xAxisY <= height - margin)
            {
                g.DrawLine(axisPen, margin, xAxisY, width - margin, xAxisY);
            }
            
            // Сітка та підписи осі X
            for (double x = X_START; x <= X_END; x += DELTA_X)
            {
                float screenX = ScaleX(x);
                g.DrawLine(gridPen, screenX, margin, screenX, height - margin);
                
                string label = x.ToString("F1");
                SizeF labelSize = g.MeasureString(label, standardFont);
                g.DrawString(label, standardFont, textBrush, screenX - labelSize.Width / 2, height - margin + 5);
            }
            
            // Сітка та підписи осі Y
            int ySteps = 8;
            for (int i = 0; i <= ySteps; i++)
            {
                double y = minY + (maxY - minY) * i / ySteps;
                float screenY = ScaleY(y);
                g.DrawLine(gridPen, margin, screenY, width - margin, screenY);
                
                string label = y.ToString("F2");
                SizeF labelSize = g.MeasureString(label, standardFont);
                g.DrawString(label, standardFont, textBrush, margin - labelSize.Width - 5, screenY - labelSize.Height / 2);
            }
            
            // Підписи осей
            g.DrawString("x", labelFont, textBrush, width - margin + 10, xAxisY - 10);
            g.DrawString("y", labelFont, textBrush, yAxisX - 10, margin - 20);
            
            // Малюємо графік в залежності від режиму відображення
            if (currentDisplayMode == DisplayMode.Line)
            {
                // Лінійний режим: малюємо з'єднані сегменти
                for (int i = 0; i < points.Count - 1; i++)
                {
                    if (!double.IsInfinity(points[i].Y) && !double.IsNaN(points[i].Y) &&
                        !double.IsInfinity(points[i + 1].Y) && !double.IsNaN(points[i + 1].Y))
                    {
                        float x1 = ScaleX(points[i].X);
                        float y1 = ScaleY(points[i].Y);
                        float x2 = ScaleX(points[i + 1].X);
                        float y2 = ScaleY(points[i + 1].Y);
                        
                        g.DrawLine(graphPen, x1, y1, x2, y2);
                    }
                }
                
                // Додатково малюємо точки поверх ліній
                foreach (var point in points)
                {
                    if (!double.IsInfinity(point.Y) && !double.IsNaN(point.Y))
                    {
                        float x = ScaleX(point.X);
                        float y = ScaleY(point.Y);
                        g.FillEllipse(pointBrush, x - 3, y - 3, 6, 6);
                    }
                }
            }
            else
            {
                // Точковий режим: малюємо тільки маркери (більші)
                foreach (var point in points)
                {
                    if (!double.IsInfinity(point.Y) && !double.IsNaN(point.Y))
                    {
                        float x = ScaleX(point.X);
                        float y = ScaleY(point.Y);
                        // Малюємо більші точки в точковому режимі
                        g.FillEllipse(pointBrush, x - 5, y - 5, 10, 10);
                        // Додаємо обводку для кращої видимості
                        g.DrawEllipse(graphPen, x - 5, y - 5, 10, 10);
                    }
                }
            }
            
            // Виводимо формулу
            string formula = "y = (x³ + 2x) / (3cos(√x) + 1)";
            g.DrawString(formula, formulaFont, textBrush, 10, 10);
        }
        
        // Функція для обчислення значень
        private double CalculateFunction(double x)
        {
            if (x < 0)
                return double.NaN;
            
            double numerator = Math.Pow(x, 3) + 2 * x;
            double denominator = 3 * Math.Cos(Math.Sqrt(x)) + 1;
            
            if (Math.Abs(denominator) < 1e-10)
                return double.NaN;
            
            return numerator / denominator;
        }
        
        // Обчислюємо точки функції
        private List<PointF> CalculateFunctionPoints()
        {
            List<PointF> points = new List<PointF>();
            
            for (double x = X_START; x <= X_END; x += DELTA_X)
            {
                double y = CalculateFunction(x);
                points.Add(new PointF((float)x, (float)y));
            }
            
            return points;
        }
        
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate(); // Перемальовуємо форму при зміні розміру
        }
        
        // Правильне звільнення ресурсів
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Звільняємо кешовані ресурси
                axisPen?.Dispose();
                gridPen?.Dispose();
                graphPen?.Dispose();
                pointBrush?.Dispose();
                textBrush?.Dispose();
                standardFont?.Dispose();
                labelFont?.Dispose();
                formulaFont?.Dispose();
                
                // Звільняємо елементи керування
                controlPanel?.Dispose();
                radioLine?.Dispose();
                radioPoint?.Dispose();
                modeLabel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GraphForm());
        }
    }
}
