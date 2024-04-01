using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MandelbrotFractal
{
    public partial class MainWindow : Window
    {
        private static int MAX_ITERATIONS = 100;
        private const double SCALE_FACTOR = 1.5;
        private double centerX = -0.5; // Center point X coordinate
        private double centerY = 0;    // Center point Y coordinate
        private double scale = 0.005;
        private double scaleFactor = 1.0;
        private WriteableBitmap bitmap;


        private enum ColorTheme
        {
            Theme1,
            Theme2,
            Theme3
        }
        private enum FractalType
        {
            Mandelbrot,
            SierpinskiTriangle,
            SierpinskiCarpet
        }

        private FractalType selectedFractal = FractalType.Mandelbrot;
        private ColorTheme selectedColorTheme = ColorTheme.Theme1;
        private int depth = 2; 

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded; // Subscribe to Loaded event
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int defaultWidth = 800;
            int defaultHeight = 600;
            bitmap = new WriteableBitmap(defaultWidth, defaultHeight, 96, 96, PixelFormats.Bgr32, null);
            image.Source = bitmap;
            await DrawFractalAsync();
        }

        private async Task DrawFractalAsync()
        {
            switch (selectedFractal)
            {
                case FractalType.Mandelbrot:
                    await DrawMandelbrotAsync();
                    break;
                case FractalType.SierpinskiTriangle:
                    DrawSierpinskiTriangle(depth, (canvas.ActualWidth - 600 * scaleFactor) / 2, ( canvas.ActualHeight - 600 * scaleFactor) / 2, 600 );
                    break;
                case FractalType.SierpinskiCarpet:
                    DrawSierpinskiCarpet(depth, (canvas.ActualWidth - 600 * scaleFactor) / 2, (canvas.ActualHeight - 600 * scaleFactor) / 2, 600 * scaleFactor);
                    break;
            }
        }
            
        private void DrawSierpinskiTriangle(int depth, double x, double y, double size)
        {
            if (depth == 0)
            {
                Point p1 = new Point(x, y + size * scaleFactor);
                Point p2 = new Point(x + size / 2 * scaleFactor, y);
                Point p3 = new Point(x + size * scaleFactor, y + size * scaleFactor);

                Polygon triangle = new Polygon();
                triangle.Points.Add(p1);
                triangle.Points.Add(p2);
                triangle.Points.Add(p3);
                triangle.Stroke = Brushes.Black;

                triangle.Fill = Brushes.Black;

                canvas.Children.Add(triangle);
            }
            else
            {
                double newSize = size / 2;
                double xOffset = newSize / 2;

                DrawSierpinskiTriangle(depth - 1, x, y + newSize * scaleFactor, newSize); // Lower left triangle
                DrawSierpinskiTriangle(depth - 1, x + newSize * scaleFactor, y + newSize * scaleFactor, newSize); // Lower right triangle
                DrawSierpinskiTriangle(depth - 1, x + xOffset * scaleFactor, y, newSize); // Upper triangle
            }
        }

        private void DrawSierpinskiCarpet(int depth, double x, double y, double size)
        {
            if (depth <= 0)
                return;

            var newSize = size / 3 ;

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (i == 1 && j == 1)
                        continue;

                    DrawSierpinskiCarpet(depth - 1, x + i * newSize, y + j * newSize, newSize);
                }
            }

            var rect = new Rectangle
            {
                Width = newSize,
                Height = newSize,
                Fill = Brushes.Black
            };

            Canvas.SetLeft(rect, x + newSize );
            Canvas.SetTop(rect, y + newSize );

            canvas.Children.Add(rect);
        }







        private async Task DrawMandelbrotAsync()
{
    int width = 0;
    int height = 0;
    byte[] pixels = null;

    await Task.Run(() =>
    {
        if (bitmap == null)
            return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            width = bitmap.PixelWidth;
            height = bitmap.PixelHeight;
            pixels = new byte[width * height * 4];
        });

        double halfWidth = width / 2.0;
        double halfHeight = height / 2.0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double a = centerX + (x - halfWidth) * scale;
                double b = centerY + (y - halfHeight) * scale;
                int iteration = ComputeMandelbrot(a, b);

                Color color = iteration < MAX_ITERATIONS ? GetColor(iteration) : Colors.Black;

                int index = (y * width + x) * 4;
                pixels[index] = color.B;
                pixels[index + 1] = color.G;
                pixels[index + 2] = color.R;
                pixels[index + 3] = 255;
            }
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
        });
    });
}




        private int ComputeMandelbrot(double a, double b)
        {
            double x = 0, y = 0;
            int iteration = 0;

            while (x * x + y * y <= 4 && iteration < MAX_ITERATIONS)
            {

                double xtemp = x * x - y * y + a;
                y = 2 * x * y + b;
                //double xtemp = Math.Cos(x) * Math.Cosh(y) + a; ;
                //y = -Math.Sin(x) * Math.Sinh(y)  +b;
                x = xtemp;
                x = xtemp;
              
                
                
                
                
                
                
                
                iteration++;
            }

            return iteration;
        }

        private Dictionary<ColorTheme, Func<double, Color>> colorThemes = new Dictionary<ColorTheme, Func<double, Color>>()
        {
            { ColorTheme.Theme1, (iteration) =>
                {
                    double t = (double)iteration / MAX_ITERATIONS;
                    byte r = (byte)(255 * Math.Sqrt(t));
                    byte g = (byte)(255 * Math.Sqrt(t));
                    byte b = (byte)(255 * t * t);
                    return Color.FromRgb(r, g, b);
                }
            },
            { ColorTheme.Theme2, (iteration) =>
                {
                    double t = (double)iteration / MAX_ITERATIONS;
    byte val = (byte)(255 * t);
    return Color.FromRgb(val, val, val);
                }
            },
            { ColorTheme.Theme3, (iteration) =>
                {
                    double t = (double)iteration / MAX_ITERATIONS;
    byte r = (byte)(255 * 2 * Math.PI * t);
    byte g = (byte)(255 * 2 * Math.PI * (t + 1.0 / 3));
    byte b = (byte)(255 * 2 * Math.PI * (t + 2.0 / 3));
    return Color.FromRgb(r, g, b);
                }
            }
        };

        // Modify GetColor method to use selected color theme
        private Color GetColor(int iteration)
        {
            return colorThemes[selectedColorTheme](iteration);
        }

        // Add a method to change the color theme
        private void ChangeColorTheme(ColorTheme theme)
        {
            selectedColorTheme = theme;
            DrawFractalAsync();
        }




        private async void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleChange = e.Delta > 0 ? SCALE_FACTOR : 1 / SCALE_FACTOR;
            await ChangeScaleAsync(scaleChange);
        }

        private async Task ChangeScaleAsync(double scaleChange)
        {
            switch (selectedFractal)
            {
                case FractalType.Mandelbrot:
                    scale *= scaleChange;
                    break;
                case FractalType.SierpinskiTriangle:
                case FractalType.SierpinskiCarpet:
                    canvas.Children.Clear();
                    scaleFactor *= scaleChange;
                    break;
            }

            await DrawFractalAsync();
        }

        private async void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            double moveDistance = 70 * scale; // Adjusted the move distance by scale factor

            switch (e.Key)
            {
                case Key.W:
                    centerY -= moveDistance;
                    break;
                case Key.A:
                    centerX -= moveDistance;
                    break;
                case Key.S:
                    centerY += moveDistance;
                    break;
                case Key.D:
                    centerX += moveDistance;
                    break;
                case Key.Up:
                    await ChangeScaleAsync(SCALE_FACTOR);
                    break;
                case Key.Down:
                    await ChangeScaleAsync(1 / SCALE_FACTOR);
                    break;
            }

            await DrawFractalAsync();
        }




        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                if (radioButton.Tag.ToString() == "Mandelbrot")
                {
                    canvas.Children.Clear();
                    selectedFractal = FractalType.Mandelbrot;
                    image.Visibility = Visibility.Visible;
                    canvas.Visibility = Visibility.Hidden;
                }
                else if (radioButton.Tag.ToString() == "SierpinskiTriangle")
                {
                    canvas.Children.Clear();
                    selectedFractal = FractalType.SierpinskiTriangle;
                    image.Visibility = Visibility.Hidden;
                    canvas.Visibility = Visibility.Visible;
                }
                else if (radioButton.Tag.ToString() == "SierpinskiCarpet")
                {
                    canvas.Children.Clear();
                    selectedFractal = FractalType.SierpinskiCarpet;
                    image.Visibility = Visibility.Hidden;
                    canvas.Visibility = Visibility.Visible;
                }
                else if (radioButton.Tag.ToString() == "Theme1")
                {
                    ChangeColorTheme(ColorTheme.Theme1);
                }
                else if (radioButton.Tag.ToString() == "Theme2")
                {
                    ChangeColorTheme(ColorTheme.Theme2);
                }
                else if (radioButton.Tag.ToString() == "Theme3")
                {
                    ChangeColorTheme(ColorTheme.Theme3);
                }
            }
            DrawFractalAsync();
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MaxIterationsTextBox.Text, out int maxIterations))
            {
                switch (selectedFractal)
                {
                    case FractalType.Mandelbrot:
                        MAX_ITERATIONS = maxIterations;
                        break;
                    case FractalType.SierpinskiTriangle:
                        canvas.Children.Clear();
                        depth = maxIterations;
                        break;
                    case FractalType.SierpinskiCarpet:
                        canvas.Children.Clear();
                        depth = maxIterations;
                        break;
                }
                DrawFractalAsync();
            }
        }
        private void SaveFractalImage()
        {
            try
            {
                string fileName = $"Fractal_{DateTime.Now:yyyyMMddHHmmss}.png";

                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                if (selectedFractal == FractalType.Mandelbrot)
                {
                    renderTargetBitmap.Render(image);
                }
                else
                {
                    renderTargetBitmap.Render(canvas);
                }

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (FileStream file = File.Create(fileName))
                {
                    encoder.Save(file);
                }

                MessageBox.Show($"Fractal image saved as {fileName}", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving fractal image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFractalImage();
        }
    }
}
