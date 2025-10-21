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
    
    class GraphForm : Form
    {
        // Параметри функції
        private const double X_START = 0;
        private const double X_END = 2;
        private const double DELTA_X = 0.4;
        
        public GraphForm()
        {
            this.Text = "Графік функції y = (x³ + 2x) / (3cos(√x) + 1)";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Встановлюємо подвійну буферизацію для плавного відображення
            this.DoubleBuffered = true;
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
            Pen axisPen = new Pen(Color.Black, 2);
            Pen gridPen = new Pen(Color.LightGray, 1);
            
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
            Font font = new Font("Arial", 9);
            Brush brush = Brushes.Black;
            
            for (double x = X_START; x <= X_END; x += DELTA_X)
            {
                float screenX = ScaleX(x);
                g.DrawLine(gridPen, screenX, margin, screenX, height - margin);
                
                string label = x.ToString("F1");
                SizeF labelSize = g.MeasureString(label, font);
                g.DrawString(label, font, brush, screenX - labelSize.Width / 2, height - margin + 5);
            }
            
            // Сітка та підписи осі Y
            int ySteps = 8;
            for (int i = 0; i <= ySteps; i++)
            {
                double y = minY + (maxY - minY) * i / ySteps;
                float screenY = ScaleY(y);
                g.DrawLine(gridPen, margin, screenY, width - margin, screenY);
                
                string label = y.ToString("F2");
                SizeF labelSize = g.MeasureString(label, font);
                g.DrawString(label, font, brush, margin - labelSize.Width - 5, screenY - labelSize.Height / 2);
            }
            
            // Підписи осей
            Font labelFont = new Font("Arial", 11, FontStyle.Bold);
            g.DrawString("x", labelFont, brush, width - margin + 10, xAxisY - 10);
            g.DrawString("y", labelFont, brush, yAxisX - 10, margin - 20);
            
            // Малюємо графік
            Pen graphPen = new Pen(Color.Blue, 2);
            
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
            
            // Малюємо точки
            Brush pointBrush = Brushes.Red;
            foreach (var point in points)
            {
                if (!double.IsInfinity(point.Y) && !double.IsNaN(point.Y))
                {
                    float x = ScaleX(point.X);
                    float y = ScaleY(point.Y);
                    g.FillEllipse(pointBrush, x - 3, y - 3, 6, 6);
                }
            }
            
            // Виводимо формулу
            Font formulaFont = new Font("Arial", 10);
            string formula = "y = (x³ + 2x) / (3cos(√x) + 1)";
            g.DrawString(formula, formulaFont, brush, 10, 10);
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
