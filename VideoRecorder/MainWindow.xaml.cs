using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

namespace VideoRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, RecordAction> schedule = new Dictionary<string, RecordAction>()
        {
            { "18:10", RecordAction.StartRecording }
        };

        Recorder rec = new Recorder();
        public MainWindow()
        {
            InitializeComponent();
            ButtonStartRecording.Click += (a, b) =>
            {
                rec.StartRecording();
            };
            ButtonStopRecording.Click += (a, b) =>
            {
                rec.StopRecording();
            };

            rec.ListenForSchedule(schedule);        

            rec.captureDevice.NewFrame += CaptureDevice_NewFrame;
        }
        
        private void CaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap frame = eventArgs.Frame;
            ui(() =>
            {
                ImageRealTime.Source = BitmapToImageSource(frame);
            });
        }

        public void ui(Action act)
        {
            try
            {
                Dispatcher.Invoke(act);
            }
            catch(Exception e)
            {

            }
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
