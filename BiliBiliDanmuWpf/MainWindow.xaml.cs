using BiliBiliDanmuWpf.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net.Http;

namespace BiliBiliDanmuWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BiliBiliDanmuCore.BiliBiliLiveDanmuClient bc;
        public event Action soso;
        public MainWindow()
        {

            if (!Directory.Exists("Images"))
            {
                Directory.CreateDirectory("Images");
            }
            InitializeComponent();
            ((MainViewModel)DataContext)._client.soso += Addsome;
            //bc = new BiliBiliDanmuCore.BiliBiliLiveDanmuClient(213);
            //bc.Start();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int a = 1 + 1;
        }
        static int idx = 0;
        static int sidx = 0;
        public async void Addsome(BiliBiliDanmuCore.BiliBiliDanmu biliBiliDanmu)
        {
            //if (biliBiliDanmu.DanmuType == BiliBiliDanmuCore.DanmuType.Join) return;


            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Name = $"textdanmu{idx++}";
            //stackPanel.VerticalAlignment = VerticalAlignment.Top;
            //stackPanel.HorizontalAlignment = HorizontalAlignment.Center;

            Border border = new Border();

            //border.CornerRadius = new CornerRadius(20);
            border.Background = Brushes.White;
            border.Width = 30;
            border.Height = 30;
            border.Margin = new Thickness(0, 5, 5, 0);
            var rect = new RectangleGeometry();
            rect.RadiusX = 15;
            rect.RadiusY = 15;
            rect.Rect = new Rect(0, 0, 30, 30);
            border.Clip = rect;

            Image avatar = new Image();
            avatar.Width = 30;
            avatar.Height = 30;
            border.VerticalAlignment = VerticalAlignment.Top;
            //avatar.VerticalAlignment = VerticalAlignment.Center;
            //avatar.HorizontalAlignment = HorizontalAlignment.Center;
            if (File.Exists($"Images/{biliBiliDanmu.UID}.jpg"))
            {
                //avatar.Source = new BitmapImage(new Uri($"/Images/{biliBiliDanmu.UID}.png"));
                avatar.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}/Images/{biliBiliDanmu.UID}.jpg"));
            }
            else
            {
                //var a = new BitmapImage(new Uri());
                HttpClient httpClient = new HttpClient();
                var aurl = await BiliBiliDanmuCore.BiliBiliTools.GetAvatarURL(biliBiliDanmu.UID);
                if (aurl != null)
                {
                    //var v11 = await httpClient.GetByteArrayAsync(aurl);
                    HttpResponseMessage v112 = await httpClient.GetAsync(aurl);
                    int retryCnt = 0;
                    while (v112.StatusCode != System.Net.HttpStatusCode.OK && retryCnt < 5)
                    {
                        v112 = await httpClient.GetAsync(aurl);
                        retryCnt++;
                    }
                    if (retryCnt < 5)
                    {
                        byte[] v11 = await v112.Content.ReadAsByteArrayAsync();
                        //BitmapEncoder encoder = new JpegBitmapEncoder();
                        //encoder.Frames.Add(BitmapFrame.Create(a));
                        try
                        {
                            using (FileStream fileStream = new($"Images/{biliBiliDanmu.UID}.jpg", System.IO.FileMode.Create))
                            {
                                fileStream.Write(v11);
                                fileStream.Close();
                                //encoder.Save(fileStream);
                            }
                        }
                        catch (Exception)
                        {

                        }

                        avatar.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}/Images/{biliBiliDanmu.UID}.jpg"));
                    }

                }

            }

            //var
            //avatar.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}/Images/xuema.png"));
            border.Child = avatar;

            TextBlock textBlock = new TextBlock();

            textBlock.Text = biliBiliDanmu.ToString();
            //textBlock.Name = $"textdanmu{idx++}";
            textBlock.FontSize = 15;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Foreground = Brushes.White;
            textBlock.MaxWidth = 220;
            textBlock.Margin = new Thickness(0, 10, 0, 0);
            textBlock.TextWrapping = TextWrapping.WrapWithOverflow;

            if (biliBiliDanmu.DanmuType == BiliBiliDanmuCore.DanmuType.Gift)
            {
                textBlock.Foreground = Brushes.SkyBlue;
            }

            this.RegisterName(stackPanel.Name, stackPanel);
            stackPanel.Children.Add(border);
            stackPanel.Children.Add(textBlock);
            stack.Children.Add(stackPanel);

            var myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 0.0;
            myDoubleAnimation.To = 1;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));

            var myDoubleAnimation2 = new ThicknessAnimation();
            myDoubleAnimation2.From = new Thickness(20, 0, 0, 0);
            myDoubleAnimation2.To = new Thickness(0, 0, 0, 0);
            myDoubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(0.1));


            var myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            myStoryboard.Children.Add(myDoubleAnimation2);
            Storyboard.SetTargetName(myDoubleAnimation, stackPanel.Name);
            Storyboard.SetTargetName(myDoubleAnimation2, stackPanel.Name);

            //Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(TextBlock.HeightProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(StackPanel.OpacityProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath(StackPanel.MarginProperty));
            myStoryboard.Begin(this);
            int v = 1 + 1;


            if (stack.Children.Count > 10)
            {

                var g = (StackPanel)stack.Children[sidx++];
                if (sidx < 5)
                {
                    var myDoubleAnimation1 = new ThicknessAnimation
                    {
                        From = new Thickness(0, g.Margin.Bottom, 0, 0),
                        To = new Thickness(0, -g.ActualHeight - 5, 0, 0),
                        Duration = new Duration(TimeSpan.FromSeconds(0.1 * sidx))
                    };
                    Storyboard myStoryboard1 = new();
                    myStoryboard1.Children.Add(myDoubleAnimation1);
                    Storyboard.SetTargetName(myDoubleAnimation1, g.Name);

                    Storyboard.SetTargetProperty(myDoubleAnimation1, new PropertyPath(StackPanel.MarginProperty));
                    myStoryboard1.Begin(this);

                    await Task.Delay(100 * sidx);
                }

                stack.Children.Remove(g);
                this.UnregisterName(g.Name);
                sidx--;
            }


        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = "1111";
            textBlock.Name = $"textdanmu{idx++}";
            this.RegisterName(textBlock.Name, textBlock);
            stack.Children.Add(textBlock);

            var myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 0;
            myDoubleAnimation.To = 40;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(2));
            var myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, textBlock.Name);

            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(TextBlock.HeightProperty));
            myStoryboard.Begin(this);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
