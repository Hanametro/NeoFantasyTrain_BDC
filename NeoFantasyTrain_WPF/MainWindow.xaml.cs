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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Threading;
using System.Threading;


namespace NeoFantasyTrain_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serialPort1 = new SerialPort();
        private DispatcherTimer ShowTimer;
        private DispatcherTimer SpeedTimer;

        private string RXDdata;
        private int speed = 0;
        private int voltage = 24;
        private int[] targetArray = new int[17] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 40, 60, 80, 100, 120, 140, 160, 180 };
        private int[] intervalArray = new int[17] { 50, 80, 110, 140, 170, 200, 230, 260, 2000, 260, 230, 200, 170, 140, 110, 80, 50 };
        private int target, interval;


        public MainWindow()
        {
            InitializeComponent();

            string[] portList = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i < portList.Length; ++i)
            {
                string name = portList[i];
                ComboBox1.Items.Add(name);
                ComboBox2.Items.Add($"00{(i + 1).ToString()}");
            }

            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowTime);
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();
            VoltageBar.Value = voltage;
            UValue.Content = voltage.ToString();
        }

        public void ShowTime(object sender, EventArgs e)
        { timeview.Content = DateTime.Now.ToString(); }


        private void Button1_Click(object sender, RoutedEventArgs e)
        {

            if (serialPort1.IsOpen)
            { }
            else
            {
                serialPort1.BaudRate = 9600;//波特率
                serialPort1.PortName = ComboBox1.Text.ToString();
                serialPort1.Parity = Parity.None;//校验法：无
                serialPort1.DataBits = 8;//数据位：8
                serialPort1.StopBits = StopBits.One;//停止位：1   



                try
                {
                    serialPort1.Open();//打开串口
                    if (serialPort1.IsOpen)
                    {   
                        serialPort1.Encoding = System.Text.Encoding.GetEncoding(65001);
                        serialPort1.Write("T01000");
                        MessageBox.Show("蓝牙连接成功", "提示");
                        LocoNumber.Content = ComboBox2.Text.Trim();
                        bluetooth_status.Visibility = Visibility.Visible;
                        ListBox1.Items.Add($"{System.DateTime.Now.ToString("T")} 蓝牙已连接");
                        Button1.Content = "已连接";
                        Button1.IsEnabled = false;
                        serialPort1.DataReceived += new SerialDataReceivedEventHandler(this.serialport_DataReceived);
                    }

                }
                catch { }
            }
        }
        private void command_Send(string command, int value)
        {
            string level = (value >= 0) ? $"0{value}" : $"1{-value}";

            string cmd = command + level + $"C{new Random().Next(0, 9)}";

            if (serialPort1.IsOpen)
            {
                serialPort1.Encoding = System.Text.Encoding.GetEncoding(65001);
                serialPort1.Write(cmd);
                if (ListBox1.Items.Count > 5) { ListBox1.Items.RemoveAt(0); }
                ListBox1.Items.Add($"{System.DateTime.Now.ToString("T")} TXD:{cmd}");

            }

        }



        private void serialport_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        { string str = "";
            do
            {
                int count = serialPort1.BytesToRead;
                if (count <= 0)
                    break;
                byte[] readBuffer = new byte[count];
                serialPort1.Read(readBuffer, 0, count);
                str += System.Text.Encoding.Default.GetString(readBuffer);
            } while (serialPort1.BytesToRead > 0);

            Action action1 = () =>
            {
                if (ListBox1.Items.Count > 5) { ListBox1.Items.RemoveAt(0); }
                ListBox1.Items.Add($"{System.DateTime.Now.ToString("T")} RXD:{str}");
            };
            ListBox1.Dispatcher.BeginInvoke(action1);
        }

        private void RXDfeedback(string str){ ListBox1.Items.Add($"{System.DateTime.Now.ToString("T")} RXD:{str}"); }


        private void speedrunHandle(object sender, EventArgs e)
        {
            speedrun(target);
        }
        private void speedrun(int tar)
        {
            speedValue.Text = speed.ToString();
            if (speed == target) { SpeedTimer.Stop(); }
            else if(speed > target){ speed--; }
            else { speed++; }
        }


        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            command_Send("PW", Convert.ToInt32(Slider1.Value));

            if (Math.Abs(Slider1.Value) != 1) { SpeedTimer.Stop(); }
                SpeedTimer = new DispatcherTimer();

            interval =intervalArray[Convert.ToInt32(Slider1.Value)+8];
            target = targetArray[Convert.ToInt32(Slider1.Value)+8]; ;
            
            ModeTargetValue.Content = target;

            SpeedTimer.Interval = TimeSpan.FromMilliseconds(interval);

            if (Slider1.Value > 0) { TBar.Value = Slider1.Value; TValue.Content = $"T{Slider1.Value}"; }
            else if (Slider1.Value < 0) { BBar.Value = -Slider1.Value; BValue.Content = $"B{-Slider1.Value}"; }
            else { TBar.Value = 0; BBar.Value = 0; TValue.Content = "T0"; BValue.Content = "B0"; }

            SpeedTimer.Tick += speedrunHandle;
            SpeedTimer.Start();                               

        }

        private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            command_Send("DI", Convert.ToInt32(Slider2.Value));
        }

        private void ModeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ModeLabel.Content = (ModeSlider.Value == 0) ? $"速度模式" : $"转矩模式";
            Modeunit.Content = (ModeSlider.Value == 0) ? $"km/h" : $"%";
            ModeTargetName.Content = (ModeSlider.Value == 0) ? $"设定速度" : $"设定转矩";
        }

        private void Cablight_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            command_Send("CL", Convert.ToInt32(Cablight_Slider.Value));
        }

        private void Rearlight_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            command_Send("RL", Convert.ToInt32(Rearlight_Slider.Value));
        }

        private void Headlight_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            command_Send("HL", Convert.ToInt32(Headlight_Slider.Value));
        }

        private void VoltageBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void EB_Button_Click(object sender, RoutedEventArgs e)
        {
            command_Send("EB", 00);
            Slider1.Value = -8;
            TBar.Value = 0;

        }

        private void EB_Button_Copy_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
