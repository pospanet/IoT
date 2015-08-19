using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.Spi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TemperatureSensor
{
    public sealed partial class MainPage : Page
    {
        private const int SensorPin = 5;
		private const string SPI_CONTROLLER_NAME = "SPI0";                              
        private const Int32 SPI_CHIP_SELECT_LINE = 0;
		private const byte ARDUINO_I2C_ADDRESS = 0x04;

		private I2cDevice I2cArduino;
		private SpiDevice SpiArduino;

		private DispatcherTimer timer;

		public MainPage()
        {
            this.InitializeComponent();

			InitGPIO();

			InitSPI();
			InitI2C();

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += Timer_Tick;
        }

		private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                //tbInfo1.Text = "There is no GPIO controller on this device.";
                return;
            }

            GpioPin pin = gpio.OpenPin(SensorPin);

            // Show an error if the pin wasn't initialized properly
            if (pin == null)
            {
                //tbInfo1.Text = "There were problems initializing the GPIO pin.";
                return;
            }

            pin.Write(GpioPinValue.High);
            pin.SetDriveMode(GpioPinDriveMode.Input);

            //tbInfo1.Text = "GPIO pin initialized correctly.";
            pin.Dispose();

        }

		private async Task InitSPI()
		{
			try
			{
				SpiConnectionSettings settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
				settings.ClockFrequency = 500000;// 10000000;  
				//settings.ClockFrequency = 10000000;// 500000;  
				settings.Mode = SpiMode.Mode0; //Mode3;  
				//settings.Mode = SpiMode.Mode3; //Mode0;  

				string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
				DeviceInformationCollection deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
				SpiArduino = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
				tbInfo3.Text = "SPI initialized correctly.";
			}

			/* If initialization fails, display the exception and stop running */
			catch (Exception ex)
			{
				tbInfo3.Text = "There were problems initializing the SPI.";
			}
		}

		private async Task InitI2C()
		{
			string aqs = I2cDevice.GetDeviceSelector();										
			DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);	
			if (dis.Count == 0)
			{
				//tbInfo2.Text = "No I2C controllers were found on the system";
				return;
			}

			I2cConnectionSettings settings = new I2cConnectionSettings(ARDUINO_I2C_ADDRESS);
			settings.BusSpeed = I2cBusSpeed.FastMode;
			I2cArduino = await I2cDevice.FromIdAsync(dis[0].Id, settings);   
			if (I2cArduino == null)
			{
				//tbInfo2.Text = string.Format(
				//	"Slave address {0} on I2C Controller {1} is currently in use by " +
				//	"another application. Please ensure that no other applications are using I2C.",
				//	settings.SlaveAddress,
				//	dis[0].Id);
				return;
			}
			//tbInfo2.Text = "I2C controllers inicialized correctly";

		}

		private double GetTemperature(out double temperature1, out double temperature2, out double temperature3)
		{
			byte[] buffer = new byte[1024];
			SpiArduino.Read(buffer);
			string s = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
			double[] values = s.Substring(0, s.IndexOf(';')).Split('\t').Select(str => Convert.ToDouble(str)).ToArray();
			temperature1 = values[1];
			temperature2 = values[2];
			temperature3 = values[3];
			return (values[1] + values[2] + values[3]) / 3;
		}

		private void Timer_Tick(object sender, object e)
		{
			try
			{
				double ta, t1, t2, t3;
				ta = GetTemperature(out t1, out t2, out t3);
				ShowTemperature(ta, t1, t2, t3);
				Task task =  EventHub.SendEventHubEventAsync(new TemperatureData("SaSaZu", ta));
				task.Wait();
            }
			catch (Exception ex)
			{
				//Nothing to handle, bad value from sensors
			}
        }

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			if (timer.IsEnabled)
			{
				timer.Stop();
				btnRefresh.Content = "Start";
				tbTemperature.Text = "N/A";
				//tbTemperature1.Text = "N/A";
				//tbTemperature2.Text = "N/A";
				//tbTemperature3.Text = "N/A";
			}
			else
			{
				timer.Start();
				btnRefresh.Content = "Stop";
			} 
        }

		private void ShowTemperature(double avgTemp, double temp1, double temp2, double temp3)
		{
			tbTemperature.Text = string.Concat(avgTemp.ToString("F2"), "°C");
			//tbTemperature1.Text = temp1.ToString("F2");
			//tbTemperature2.Text = temp2.ToString("F2");
			//tbTemperature3.Text = temp3.ToString("F2");
		}
	}
}
