using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using Accord.Video.FFMPEG;
using Accord.Video.VFW;
using System.Timers;
using System.IO;

namespace VideoRecorder
{   
    class Recorder
    {
        public VideoCaptureDevice captureDevice;
        VideoFileWriter fileWriter = new VideoFileWriter();
        VideoCaptureDeviceForm captureDeviceForm = new VideoCaptureDeviceForm();
        Dictionary<string, RecordAction> schedule = new Dictionary<string, RecordAction>();
        Action startRecordingAction, stopRecordingAction;
        Dictionary<RecordAction, Action> actionMap;

        public Recorder()
        {
            startRecordingAction = StartRecording;
            stopRecordingAction = StopRecording;
            actionMap = new Dictionary<RecordAction, Action>()
            {
                { RecordAction.StartRecording, startRecordingAction },
                { RecordAction.StopRecording, stopRecordingAction }
            };

            FilterInfoCollection devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);            
            captureDevice = new VideoCaptureDevice(devices[0].MonikerString);                                    
        }

        bool writerSet = false;
        object locker = new object();
        private void CaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = eventArgs.Frame;
            lock (locker)
            {
                if (writerSet)
                {
                    try
                    {
                        fileWriter.WriteVideoFrame(frame);
                    }
                    catch(Exception e)
                    {

                    }
                    return;
                }

                DateTime dateTime = DateTime.Now;
                int width = frame.Width;
                int height = frame.Height;
                string name = dateTime.ToString("dd/MM/yyyy_hh-mm-ss");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Captures");                
                fileWriter.Open(Directory.GetCurrentDirectory() + "\\Captures\\" + name + ".avi", width, height);

                writerSet = true;
            }
        }
        
        public void StartRecording()
        {
            captureDevice.NewFrame += CaptureDevice_NewFrame;
            captureDevice.Start();            
        }

        public void StopRecording()
        {
            captureDevice.NewFrame -= CaptureDevice_NewFrame;
            writerSet = false;
            captureDevice.SignalToStop();
            fileWriter.Flush();            
        }

        bool listening = false;
        public void ListenForSchedule(Dictionary<string, RecordAction> schedule)
        {
            listening = true;
            this.schedule = schedule;
            Timer t = new Timer(10000);
            t.Elapsed += T_Elapsed;
            t.Start();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            string key = dt.Hour + ":" + dt.Minute;
            if (schedule.ContainsKey(key))
                actionMap[schedule[key]]();
        }
    }

    public enum RecordAction
    {
        StartRecording,
        StopRecording
    }
}
