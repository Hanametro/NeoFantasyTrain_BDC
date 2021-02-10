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
        private DispatcherTimer ShowTimer; //时间计时器
        private DispatcherTimer SpeedTimer; //速度计时器
        private DispatcherTimer BluetoothTimer;//蓝牙发送计时器
        private int speed = 0;
        private double voltage = 24;
        private double currentA=0;
        private double force = 240;
        private double Rforce = 0;
        private double mass = 100;//重量 t
        private int[] targetArray = new int[17] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 20,40, 60, 80, 100, 120, 140, 160};
        private int[] intervalArray = new int[17] { 50, 80, 110, 140, 170, 200, 230, 260, 2000, 260, 230, 200, 170, 140, 110, 80, 50 };
        private int[] forceArray = new int[17] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 50, 100, 150, 200, 250, 300, 350, 420 };
        private byte[] commandSend = new byte[6] { 0x7C,0x00,0x00,0x00,0x00, 0x7D};


        private string run_mode = "00010000";
        private string headlight_status="00";
        private string rearlight_status = "00";
        private string cablight_status = "00";
        private string PWM_level = "0000";
        private string direction_status = "00";
        private string acceleration_status = "00";


        private int vtarget, ftarget,interval;

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
            
            loadList.Items.Add("空载");
            loadList.Items.Add("25T客车 18辆（建议使用HXD3D）");
            loadList.Items.Add("C70货车 50辆（建议使用HXN5）");
            loadList.SelectedIndex = 0;

            modelList.Items.Add("HXD3D电力机车");
            modelList.Items.Add("HXN5内燃机车");
            modelList.SelectedIndex = 0;

            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowTime);
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();

            BluetoothTimer = new System.Windows.Threading.DispatcherTimer();
            BluetoothTimer.Tick += new EventHandler(command_SendHandle);
            BluetoothTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            BluetoothTimer.Start();
        }





        public void ShowTime(object sender, EventArgs e)
        { timeview.Content = DateTime.Now.ToString(); }


        private void Button1_Click(object sender, RoutedEventArgs e)
        {

            if (serialPort1.IsOpen)
            { }
            else
            {
                bluetooth_notice.Visibility = Visibility.Collapsed;
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
                        //serialPort1.Write("T010C0");
                        MessageBox.Show("蓝牙连接成功:", "提示");
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

        private void command_SendHandle(object sender, EventArgs e)
        {
            commandSend[1]= Convert.ToByte(run_mode, 2);
            commandSend[2]= Convert.ToByte(direction_status+ acceleration_status+ PWM_level, 2);
            commandSend[3]= Convert.ToByte(headlight_status + rearlight_status+ cablight_status+"00", 2);

            if (serialPort1.IsOpen)
            {         
              serialPort1.Write(commandSend,0,6);
              if (ListBox1.Items.Count > 5) { ListBox1.Items.RemoveAt(0); }
              ListBox1.Items.Add($"{System.DateTime.Now.ToString("T")} TXD:{BitConverter.ToString(commandSend)}");
            }
        }


        private void serialport_DataReceived(Object sender, SerialDataReceivedEventArgs e) //蓝牙接收处理
        {

            byte[] readcmd = new byte[8];
            try
            {
                serialPort1.Read(readcmd, 0, 1);
                serialPort1.Read(readcmd, 1, 7);
            }
            catch { }


            Action action1 = () =>
                {
                if (ListBox1.Items.Count > 5) { ListBox1.Items.RemoveAt(0); }
                ListBox1.Items.Add($"{System.DateTime.Now.ToString("T")} RXD:{BitConverter.ToString(readcmd)}");
                
                try
                {

                        int lv = Convert.ToInt32(readcmd[1]);
                        testView.Content = lv.ToString();
                        speed = (lv < 40) ? 0 : lv-40;
                        voltage = Convert.ToDouble(readcmd[2])+ Convert.ToDouble(readcmd[3])/100;
                        VoltageBar.Value = voltage;
                        UValue.Content = voltage.ToString();
                                 
                        currentA = 0.1*(Convert.ToDouble(readcmd[4])+ Convert.ToDouble(readcmd[5])/100);
                        iaBar.Value = currentA;
                        IValue.Content = currentA.ToString();
                 
                        
                    }
                catch { }
                    Array.Clear(readcmd, 0, readcmd.Length);
                };

            if (readcmd[0] == 0x7E & readcmd[7] == 0xEF) { ListBox1.Dispatcher.BeginInvoke(action1); } //处理正确数据
            else { Array.Clear(readcmd, 0, readcmd.Length); } //弃用错误数据

        }

        private void RXDfeedback(string str){ ListBox1.Items.Add($"{System.DateTime.Now.ToString("T")} RXD:{str}"); }


        private void runHandle(object sender, EventArgs e)
        {
            switch (loadList.SelectedIndex) 
            {
                case 0: Rforce = (1.20+0.0065*speed+0.000279*speed*speed);break; //机车基本阻力 公式单位 N/kN //HXD3D
                case 1: Rforce = (0.95 + 0.0023 * speed + 0.000497 * speed * speed); break; //HXN5
            }
            mass = 100;

            switch (loadList.SelectedIndex)
            {   case 1: Rforce = Rforce + (1.61 + 0.004 * speed + 0.000187 * speed * speed) / 1000 * 640 * 16;mass =mass+16*64; break; //按16t轴重计算
                case 2: Rforce = Rforce + (1.07 + 0.0011 * speed + 0.000236 * speed * speed) / 1000 * 1000 * 50; mass = mass + 50*100; break;//按25t轴重计算
            }
            speedValue.Text = speed.ToString();
            ForceValue.Content = force.ToString("#0.0");
            ForceBar.Value = force;
            RforceValue.Content = Rforce.ToString("#0.000");
            PowerValue.Content = (force * speed / 3.6).ToString("#0");
        }


        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PWM_level =  Convert.ToString(Convert.ToInt32(Math.Abs(Slider1.Value)),2);
            while (PWM_level.Length < 4) { PWM_level = "0" + PWM_level; }

            acceleration_status = (Slider1.Value > 0) ? "01" : "10";
            if (Math.Abs(Slider1.Value) != 1) { SpeedTimer.Stop(); }
            SpeedTimer = new DispatcherTimer();
            interval =intervalArray[Convert.ToInt32(Slider1.Value)+8];
            vtarget = targetArray[Convert.ToInt32(Slider1.Value)+8]; 
            ftarget=forceArray[Convert.ToInt32(Slider1.Value) + 8];
            force = ftarget;
            ForceValue.Content = ftarget.ToString("#0.0");
            ForceBar.Value = ftarget;
            ModeTargetValue.Content = (ModeSlider.Value==0)?vtarget:(vtarget/1.6);

            if (Slider1.Value > 0) { TBar.Value = Slider1.Value; TValue.Content = $"T{Slider1.Value}"; }
            else if (Slider1.Value < 0) { BBar.Value = -Slider1.Value; BValue.Content = $"B{-Slider1.Value}"; }
            else { TBar.Value = 0; BBar.Value = 0; TValue.Content = "T0"; BValue.Content = "B0"; }

            SpeedTimer.Interval = TimeSpan.FromMilliseconds(interval);
            SpeedTimer.Tick += runHandle;
            SpeedTimer.Start();
        }

        private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            direction_status = (Slider2.Value >= 0) ? $"0{Convert.ToString(Slider2.Value)}" : "10";
        }

        private void ModeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ModeLabel.Content = (ModeSlider.Value == 0) ? $"速度模式" : $"转矩模式";
            Modeunit.Content = (ModeSlider.Value == 0) ? $"km/h" : $"%";
            ModeTargetName.Content = (ModeSlider.Value == 0) ? $"设定速度" : $"设定转矩";
        }

        private void Cablight_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {       
            cablight_status = $"0{Convert.ToString(Cablight_Slider.Value)}";

        }

        private void Rearlight_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rearlight_status = (Rearlight_Slider.Value >= 0) ? $"{Convert.ToString(Rearlight_Slider.Value)}0" : "01";
        }

        private void Headlight_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            headlight_status = (Headlight_Slider.Value >= 0) ? $"{Convert.ToString(Headlight_Slider.Value)}0" : "01";
            
        }

        private void VoltageBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void EB_Button_Click(object sender, RoutedEventArgs e)
        {
           // command_Send("EB", 00);
            Slider1.Value = -8;
            TBar.Value = 0;
            Slider2.Value = 0;
        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TestButton1_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void loadList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EB_Button_Copy_Click(object sender, RoutedEventArgs e)
        {
           
            byte[] tosend = new byte [6]{0x00,0x11,0x01,0x20,0xAA,0xFF };
            serialPort1.Write(tosend,0,6);
            MessageBox.Show("TESTING");
        }
    }
}
 