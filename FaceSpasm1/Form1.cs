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
    public partial class Form1 : Form
    {
        FaceRecognizer recognizer = new FisherFaceRecognizer(0, 3500);//4000
        private Capture _capture = new Emgu.CV.Capture();
        private CascadeClassifier _cascadeClassifier = new CascadeClassifier(Application.StartupPath + "/haarcascade_frontalface_default.xml");
        private CascadeClassifier _mouthClassifier = new CascadeClassifier(Application.StartupPath + "/haarcascade_mcs_mouth.xml");
        Random rnd = new Random();
        int lastImage = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var imageFrame = _capture.QueryFrame().ToImage<Bgr, Byte>();


            if (imageFrame != null)
            {

                var grayframe = imageFrame.Convert<Gray, byte>();
                var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty); //the actual face detection happens here
                foreach (var face in faces)
                {
                    imageFrame.Draw(face, new Bgr(Color.BurlyWood), 3); //the detected face(s) is highlighted here using a box that is drawn around it/them

                }

                if (faces.Count() > 0)
                {

                    int foo = rnd.Next(0, faces.Count());

                    if(faces.Count() > 1)
                    {
                        while(foo == lastImage)
                        {
                            foo = rnd.Next(0, faces.Count());
                        }
                    }


                    imageFrame = imageFrame.GetSubRect(faces[foo]);

                    var grayframe_m = imageFrame.Convert<Gray, byte>();
                    var mouths = _mouthClassifier.DetectMultiScale(grayframe_m, 1.1, 10, Size.Empty);

                    foreach (var mouth in mouths)
                    {
                        //imageFrame.Draw(mouth, new Bgr(Color.BurlyWood), 3); //the detected face(s) is highlighted here using a box that is drawn around it/them
                        var mouthImg = imageFrame.Copy();
                        mouthImg.ROI = mouth;
                        mouthImg = mouthImg.Flip(Emgu.CV.CvEnum.FlipType.Vertical);
                        CvInvoke.cvCopy(mouthImg, imageFrame, IntPtr.Zero);
                    }

                    lastImage = foo;
                }

                Console.WriteLine(faces.Count());
            }

            imageBox1.Image = imageFrame;
        }
    }
}
