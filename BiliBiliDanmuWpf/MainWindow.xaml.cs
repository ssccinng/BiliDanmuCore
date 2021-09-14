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
            TextBlock textBlock = new TextBlock();
            textBlock.Text = biliBiliDanmu.ToString();
            textBlock.Name = $"textdanmu{idx++}";
            textBlock.FontSize = 15;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Foreground = Brushes.White;
            textBlock.MaxWidth = 300;
            textBlock.Margin = new Thickness(0, 10, 0, 0);
            textBlock.TextWrapping = TextWrapping.WrapWithOverflow;

            this.RegisterName(textBlock.Name, textBlock);
            stack.Children.Add(textBlock);

            var myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 0.0;
            myDoubleAnimation.To = 1;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));


            var myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, textBlock.Name);

            //Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(TextBlock.HeightProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(TextBlock.OpacityProperty));
            myStoryboard.Begin(this);
            int v = 1 + 1;


            if (stack.Children.Count > 10)
            {

                var g = (TextBlock)stack.Children[sidx++];
                
                var myDoubleAnimation1 = new ThicknessAnimation();
                
                myDoubleAnimation1.From = new Thickness(0, g.Margin.Bottom, 0, 0);
                myDoubleAnimation1.To = new Thickness(0, -20, 0, 0);
                myDoubleAnimation1.Duration = new Duration(TimeSpan.FromSeconds(1));
                var myStoryboard1 = new Storyboard();
                myStoryboard1.Children.Add(myDoubleAnimation1);
                Storyboard.SetTargetName(myDoubleAnimation1, g.Name);

                Storyboard.SetTargetProperty(myDoubleAnimation1, new PropertyPath(TextBlock.MarginProperty));
                myStoryboard1.Begin(this);

                await Task.Delay(1000);
                stack.Children.Remove(g);
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
            myDoubleAnimation.To = 20;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(2));
            var myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, textBlock.Name);

            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(TextBlock.HeightProperty));
            myStoryboard.Begin(this);
        }
    }
}
