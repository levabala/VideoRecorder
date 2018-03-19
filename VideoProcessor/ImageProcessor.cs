using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VideoProcessor
{
    public class ImageProcessor
    {
        static double maxDistance = 50;
        static double forgetTicksMax = 10;
        static DateTime lastDateTime = DateTime.Now;
        public static Bitmap getEdges(Bitmap bitmap, List<Rectangle> objects, List<int> forgetTicks, out string output)
        {
            double timeDelta = (DateTime.Now - lastDateTime).TotalMilliseconds;
            lastDateTime = DateTime.Now;
            output = "Delay: " + Math.Round(timeDelta).ToString() + "ms ForgetTime: " + Math.Round(forgetTicksMax * timeDelta).ToString() + "ms";

            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
            CascadeClassifier faceClassifier = new CascadeClassifier(@"face_haar.xml");            
            Image<Gray, byte> gray = new Image<Gray, byte>(bitmap);
            Image<Gray, float> sobel = gray.Sobel(0, 1, 3).Add(gray.Sobel(1, 0, 3)).AbsDiff(new Gray(0.0));

            Rectangle[] faces = faceClassifier.DetectMultiScale(gray);
            List<int> took = new List<int>();
            foreach (Rectangle face in faces)
            {
                bool found = false;
                string name = "Unknown";
                for (int i = 0; i < objects.Count; i++)
                {
                    if (took.IndexOf(i) != -1)
                        continue;
                    Rectangle rect = objects[i];
                    double diffX = Math.Abs(rect.X + rect.Width / 2 - (face.X + face.Width / 2));
                    double diffY = Math.Abs(rect.Y + rect.Height / 2 - (face.Y + face.Height / 2));
                    double diff = Math.Sqrt(diffX * diffX + diffY * diffY);                    
                    
                    if (diff <= maxDistance)
                    {
                        objects[i] = face;
                        forgetTicks[i] = 0;
                        name = i.ToString();// + " " + Math.Round(diff).ToString();                        
                        found = true;
                        took.Add(i);
                        break;
                    }
                    forgetTicks[i]++;
                    if (forgetTicks[i] > forgetTicksMax)
                    {
                        forgetTicks.RemoveAt(i);
                        objects.RemoveAt(i);
                    }
                }
                if (!found)
                {
                    objects.Add(face);
                    forgetTicks.Add(0);
                }
                gray.Draw(face, new Gray(1), 3);
                gray.Draw(name, new Point(face.X, face.Y + face.Height), FontFace.HersheyPlain, 4, new Gray(0), 5, LineType.EightConnected);
            }

            CascadeClassifier myClassifier = new CascadeClassifier();
            

            return gray.ToBitmap();
        }
    }
}
