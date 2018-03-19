using System;
using System.Collections.Generic;
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

namespace PhotoCropper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string dropPath = "";
        string FOLDER_NAME = "Cropped";
        public MainWindow()
        {
            InitializeComponent();

            myWindow.MouseUp += MyWindow_MouseUp;
            ListBoxPhotos.Drop += ListBoxPhotos_Drop;
            ImagePhoto.MouseLeftButtonDown += ImagePhoto_MouseLeftButtonDown;
            ImagePhoto.MouseLeftButtonUp += ImagePhoto_MouseLeftButtonUp;            
            ImagePhoto.MouseMove += ImagePhoto_MouseMove;
            myWindow.KeyUp += MyWindow_KeyUp;            
        }        

        private void MyWindow_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C:
                    //get
                    BitmapImage bitmapImage = (BitmapImage)ImagePhoto.Source;
                    string name = Path.GetFileName((string)ListBoxPhotos.SelectedValue);

                    //crop            
                    double scaleX = bitmapImage.Width / ImagePhoto.RenderSize.Width;
                    double scaleY = bitmapImage.Height / ImagePhoto.RenderSize.Height;
                    CroppedBitmap croppedBitmap = new CroppedBitmap(
                        bitmapImage, 
                        new Int32Rect(
                            (int)Math.Min(cropStartPoint.X * scaleX, cropEndPoint.X * scaleX),
                            (int)Math.Min(cropStartPoint.Y * scaleY, cropEndPoint.Y * scaleY),
                            (int)Math.Abs(cropEndPoint.X * scaleX - cropStartPoint.X * scaleX),
                            (int)Math.Abs(cropEndPoint.Y * scaleY - cropStartPoint.Y * scaleY)
                            ));

                    //encode
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

                    //save
                    using (var fileStream = new FileStream(dropPath + "\\" + FOLDER_NAME + "\\" + name, FileMode.Create))                    
                        encoder.Save(fileStream);

                    Title = "Capture saved to " + dropPath + "\\" + FOLDER_NAME + "\\" + name;

                    ListBoxPhotos.SelectedIndex += 1;
                    setViewedPhoto(ListBoxPhotos.SelectedIndex);
                    break;
            }
        }

        bool mousedown = false;
        Point cropStartPoint = new Point(0, 0);
        Point cropEndPoint = new Point(0, 0);

        private void ImagePhoto_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mousedown)
                return;

            cropEndPoint = e.GetPosition(ImagePhoto);            
            drawCropRect();

            Title = "Capturing started..";
        }

        private void ImagePhoto_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            cropStartPoint = e.GetPosition(ImagePhoto);            
            mousedown = true;
        }    

        private void ImagePhoto_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            endCapturing();
        }

        private void MyWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            endCapturing();
        }

        private void endCapturing()
        {
            mousedown = false;
            Title = "Capturing ended";
        }

        private void drawCropRect()
        {
            Point startPoint = cropStartPoint;
            Point endPoint = cropEndPoint;
            startPoint = ImagePhoto.PointToScreen(startPoint);
            endPoint = ImagePhoto.PointToScreen(endPoint);
            startPoint = CanvasCropRect.PointFromScreen(startPoint);
            endPoint = CanvasCropRect.PointFromScreen(endPoint);

            Canvas.SetLeft(RectangleCropArea, Math.Min(startPoint.X, endPoint.X));
            Canvas.SetTop(RectangleCropArea, Math.Min(startPoint.Y, endPoint.Y));
            RectangleCropArea.Width = Math.Abs(endPoint.X - startPoint.X);
            RectangleCropArea.Height = Math.Abs(endPoint.Y - startPoint.Y);
        }

        private void ListBoxPhotos_Drop(object sender, DragEventArgs e)
        {            
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            dropPath = Path.GetDirectoryName(files[0]);
            for (int i = 0; i < files.Length; i++)
                ListBoxPhotos.Items.Add(files[i]);
            ListBoxPhotos.SelectedIndex = 0;

            Directory.CreateDirectory(dropPath + "\\" + FOLDER_NAME);

            setViewedPhoto(0);
        }

        private void setViewedPhoto(int index)
        {
            if (index >= ListBoxPhotos.Items.Count)
                return;
            string path = (string)ListBoxPhotos.Items[index];
            Uri uri = new Uri(path);
            ImagePhoto.Source = new BitmapImage(uri);
        }
    }
}
