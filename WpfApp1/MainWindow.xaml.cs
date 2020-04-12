using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.label.Content = "计算中...";
            this.button.IsEnabled = false;
            Task.Run(() =>
            {
                Thread.Sleep(100);
                this.Dispatcher.Invoke(() =>
                {
                    var num = this.textBox.Text.ToString();
                    if (!BigInteger.TryParse(num, out BigInteger resNum))
                    {
                        this.label.Content = $"计算结果:非有效数字";
                        this.button.IsEnabled = true;
                        return;
                    }
                    Stopwatch sp = new Stopwatch();
                    sp.Start();
                    if (IsZS(resNum))
                        this.label.Content = $"计算结果:质数,耗时：{sp.ElapsedMilliseconds}ms";
                    else
                        this.label.Content = $"计算结果:非质数,耗时：{sp.ElapsedMilliseconds}ms";
                    this.button.IsEnabled = true;
                });
            });
        }
        protected bool IsPrime(BigInteger n)
        {
            if (n > ulong.MaxValue)
            {
                return IsZS(n);
            }
            else
            {
                return IsZS((ulong)n);
            }
        }
        protected bool IsZS(BigInteger n)
        {
            bool isZs = true;
            if (n <= 5 && n != 4)
                return n > 1;
            if (n % 6 != 1 && n % 6 != 5)
                return false;
            if (n.IsEven)
                return false;
            //Miller-Rabin素数检测算法，即素数一定通过测试，合数可能通过测试，准确率1-0.25^n
            int loop = 100;
            BigInteger a = n - 1;
            while ((a & 1) == 0)
                a >>= 1;
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount;
            Parallel.For(0, loop, options, (r, state) =>
             {
                 var v = new Random().Next();
                 if (BigInteger.ModPow(v, n - 1, n) != 1) //v^(n-1) != 1 mod n 则一定为合数
                 {
                     isZs = false;
                     state.Stop();
                 }
                 if (BigInteger.ModPow(v, a, n) == 1)//v^(a) == 1 mod n 则可能为素质
                 {
                     state.Stop();
                 }
             });
            return isZs;
        }
        protected bool IsZS(ulong n)
        {
            bool isZs = true;
            if (n <= 5 && n != 4)
                return n > 1;
            if (n % 6 != 1 && n % 6 != 5)
            {
                return false;
            }
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount;
            var sqrt = Math.Sqrt(n);
            var segnum = ((long)(sqrt / Environment.ProcessorCount / 6) + 1) * 6;
            Parallel.For(0, Environment.ProcessorCount, options, (k, state) =>
            {
                for (var i = (segnum * k + 5); i < ((segnum * (k + 1)) + 5) && i < sqrt; i += 6)
                {
                    if (n % (ulong)i == 0 || n % (ulong)(i + 2) == 0)
                    {
                        isZs = false;
                        state.Stop();
                    }
                }
            });
            return isZs;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.label3.Content = this.textBox.Text.ToString().Length;
        }
    }
}
