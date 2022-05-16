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
using System.Threading;
using ArduinoUploader;
using ArduinoUploader.Hardware;

namespace Arduino
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        SerialPort sp = new SerialPort();
        string[] ports = SerialPort.GetPortNames();
        SerialPort[] spPorts = new SerialPort[SerialPort.GetPortNames().Length];
        bool result = false;

        public MainWindow()
        {
            InitializeComponent();
            COM.ItemsSource = ports;
            sp.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            PortInitialization();
            _ = PortInitializationTimeOut();
            ArdModel.ItemsSource = Enum.GetValues(typeof(ArduinoModel));
        }

        void PortInitialization()
        {
            for(int i = 0; i < ports.Length; i++)
            {
                try
                {
                    spPorts[i] = new SerialPort();
                    spPorts[i].BaudRate = 9600;
                    spPorts[i].PortName = ports[i];
                    spPorts[i].DataReceived += new SerialDataReceivedEventHandler(InitializationDataReceived);
                    spPorts[i].Open();
                }
                catch { }
            }

            Thread.Sleep(2000);

            for(int i = 0; i < spPorts.Length; i++)
            {
                try
                {
                    spPorts[i].Write("Hello");
                } 
                catch { }
            }
        }

        void InitializationDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort mySerial = (SerialPort)sender;
            string str = mySerial.ReadExisting();
            if(str == "Hello")
            {
                sp.PortName = mySerial.PortName;
                result = true;
            }
        }

        async Task PortInitializationTimeOut()
        {
            await Task.Delay(1000);
            for (int i = 0; i < ports.Length; i++)
                spPorts[i].Close();
            if (result)
                COM.SelectedItem = sp.PortName;
            else MessageBox.Show("Не удалось автоматически обнаружить устройство\nВыберите порт вручную");
        }

        private void COM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sp.IsOpen)
                    sp.Close();
                sp.PortName = COM.SelectedItem as string;
                sp.BaudRate = 9600;
                sp.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Dispatcher.Invoke(() => TextIn.Text = sp.ReadExisting());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sp.Write("on");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            sp.Write("off");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            sp.Write(TextOut.Text);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try 
            {
                if (sp.IsOpen)
                    sp.Close();
                var uploader = new ArduinoSketchUploader(
                    new ArduinoSketchUploaderOptions()
                    {
                        FileName = AppDomain.CurrentDomain.BaseDirectory + "sketch_may16a.ino.hex",
                        PortName = sp.PortName,
                        ArduinoModel = (ArduinoModel)ArdModel.SelectedItem
                    });

                uploader.UploadSketch();
                MessageBox.Show("Прошивка успешно завершена");
                sp.Open();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); sp.Open(); }
        }
    }
}
