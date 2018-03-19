using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
using VideoProcessor;

namespace VideoRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, RecordAction> schedule = new Dictionary<string, RecordAction>()
        {/*
            //micro-guys' breakfast
            { "9:10", RecordAction.StartRecording },
            { "9:25", RecordAction.StopRecording },
            //big-guys' breakfast
            { "10:05", RecordAction.StartRecording },
            { "10:20", RecordAction.StopRecording  },
            //any-guys' lunch
            { "11:50", RecordAction.StartRecording },
            { "12:10", RecordAction.StopRecording },*/

            { "8:39", RecordAction.StartRecording },
            { "8:41", RecordAction.StopRecording },            
            { "8:45", RecordAction.StartRecording },
            { "8:55", RecordAction.StopRecording },
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
            rec.StartRecording();

            rec.captureDevice.NewFrame += CaptureDevice_NewFrame;
        }


        bool processingFinished = true;
        List<Rectangle> objects = new List<Rectangle>();
        List<int> forgetTicks = new List<int>();
        private void CaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {                        
            Bitmap frame = eventArgs.Frame;  
            ui(() => {
                ImageRealTime.Source = BitmapToImageSource(frame);
            });

            //we'll process next one only when we've finished previos 
            if (!processingFinished)
                return;
            processingFinished = false;            
            new Thread((obj) =>
            {
                Bitmap fr = (Bitmap)obj;

                string info = "";
                Bitmap processedFrame = ImageProcessor.getEdges(fr, objects, forgetTicks, out info);
                ui(() =>
                {                    
                    ImageRealTimeProcessed.Source = BitmapToImageSource(processedFrame);
                    Title = info;
                });
                processingFinished = true;
            }).Start(frame.Clone());                        
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
