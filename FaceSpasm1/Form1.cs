using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceSpasm1
{
    public enum Phases
    {
        NOFACE,
        NEXTFACE,
        FOLLOW_FACE
    }



    public partial class Form1 : Form
    {
        FaceRecognizer recognizer = new FisherFaceRecognizer(0, 3500);//4000
        private Capture _capture = new Emgu.CV.Capture();
        private CascadeClassifier _cascadeClassifier = new CascadeClassifier(Application.StartupPath + "/haarcascade_frontalface_default.xml");
        private CascadeClassifier _mouthClassifier = new CascadeClassifier(Application.StartupPath + "/haarcascade_mcs_mouth.xml");
        Random rnd = new Random();

        Rectangle next = new Rectangle(1, 1, 1, 1);
        Rectangle current = new Rectangle(1, 1, 1, 1);
        Rectangle lastRect = new Rectangle(1, 1, 1, 1);
        Rectangle nextRect = new Rectangle(1, 1, 1, 1);

        int myPercent = 0;

        int lastImage = 0;
        int currentFaceIndex = 0;

        DateTime startTime = DateTime.Now;
        const double transitionTime = 1000;

        Phases currentPhase = Phases.NOFACE;
        Phases lastPhase = Phases.NOFACE;
        private int interpolate(int a, int b, int percentage)
        {
            return (a * (100 - percentage) / 100)  + (b * (percentage) / 100);
        }

        private Rectangle interpolateRect(Rectangle a, Rectangle b, int percentage)
        {
            return new Rectangle(interpolate(a.X, b.X, percentage), interpolate(a.Y, b.Y, percentage), interpolate(a.Width, b.Width, percentage), interpolate(a.Height, b.Height, percentage));
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var imageFrame = _capture.QueryFrame().ToImage<Bgr, Byte>();


            if (imageFrame != null && myPercent == 0)
            {
                var grayframe = imageFrame.Convert<Gray, byte>();
                var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty); //the actual face detection happens here
                foreach (var face in faces)
                {
                    imageFrame.Draw(face, new Bgr(Color.BurlyWood), 3); //the detected face(s) is highlighted here using a box that is drawn around it/them

                }

                if (faces.Count() > 0)
                {
                    current = new Rectangle(next.Location, next.Size);

                    int foo = rnd.Next(0, faces.Count());

                    if(faces.Count() > 1)
                    {
                        while(foo == lastImage)
                        {
                            foo = rnd.Next(0, faces.Count());
                        }
                    }

                    next = faces[foo];

                    imageFrame = imageFrame.GetSubRect(current);

                    lastImage = foo;
                }

                Console.WriteLine(faces.Count());

                myPercent++;
            }
            else if(imageFrame != null)
            {
                
                imageFrame = imageFrame.GetSubRect(interpolateRect(current, next, myPercent));

                myPercent += 10;
            }

            

            if (myPercent > 100)
                myPercent = 0;

            imageBox1.Image = imageFrame;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                var imageFrame = _capture.QueryFrame().ToImage<Bgr, Byte>();


                if (imageFrame != null)
                {
                    var grayframe = imageFrame.Convert<Gray, byte>();
                    var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty); //the actual face detection happens here

                    if (faces.Count() > 0)
                    {
                        currentPhase = Phases.FOLLOW_FACE;
                        currentFaceIndex = 0;
                    }
                    else
                    {
                        currentPhase = Phases.NOFACE;
                    }

                    switch (currentPhase)
                    {
                        case Phases.FOLLOW_FACE:
                            nextRect = faces[currentFaceIndex];
                            break;
                        case Phases.NEXTFACE:
                            break;
                        case Phases.NOFACE:
                            nextRect = new Rectangle(new Point(0, 0), imageFrame.Size);
                            break;
                    }


                    if(DateTime.Now.Subtract(startTime).TotalMilliseconds > transitionTime)
                    {
                        lastRect = new Rectangle(nextRect.Location, nextRect.Size);
                        startTime = DateTime.Now;
                    }

                    imageBox1.Image = imageFrame.GetSubRect(interpolateRect(lastRect, nextRect, (int)(Math.Floor((DateTime.Now.Subtract(startTime).TotalMilliseconds / transitionTime) * 100))));


                }
            }
                  
        }
    }
}
