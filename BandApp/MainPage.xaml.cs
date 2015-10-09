using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

//MS Band
using Microsoft.Band;
using Microsoft.Band.Sensors;

//Threading & Tasks
using System.Threading.Tasks;

namespace BandApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        /// <summary>
        /// Connect_Click responds to the user touch event from the button on the main page. It starts the
        /// finding of paired bands and adds event handlers as needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    this.messageBlock.Text = "This app requires Microsoft Health Band paired to your phone";
                    return;
                }

                using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    IEnumerable<TimeSpan> supportedIntervals = bandClient.SensorManager.Accelerometer.SupportedReportingIntervals;

                    bandClient.SensorManager.Accelerometer.ReportingInterval = supportedIntervals.Last();
                    bandClient.SensorManager.Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                    await bandClient.SensorManager.Accelerometer.StartReadingsAsync();

                    await Task.Delay(TimeSpan.FromMinutes(1));

                    await bandClient.SensorManager.Accelerometer.StopReadingsAsync();

                }

            }
            catch (Exception ex)
            {
                this.messageBlock.Text = ex.ToString();
            }
        }

        private async void Accelerometer_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> e)
        {
            IBandAccelerometerReading accel = e.SensorReading;

            string text = string.Format("X = {0}\nY = {1}\nZ = {2}", accel.AccelerationX, accel.AccelerationY, accel.AccelerationZ);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.accelerometerBlock.Text = text;
            }).AsTask();

        }

    }
}
