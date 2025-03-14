using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace pdfscreenshottest
{
    public class ImgCapture
    {
        public int refX = 0;
        public int refY = 0;
        public int imgH = 0;
        public int imgW = 0;
        public string path = "";

        public ImgCapture()
        {

        }

        public void setRect(int refX, int refY, int imgH, int imgW)
        {
            this.refX = refX;
            this.refY = refY;
            this.imgH = imgH;
            this.imgW = imgW;
        }

        public void setPath(string path)
        {
            this.path = path;
        }

        [DllImport("gdi32")]
        private static extern int GetDeviceCaps(IntPtr deviceContextHandle, int index);
        //윈도우 배율 확인
        public double GetDeviceCapsCheck()
        {
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);

            IntPtr deviceHandle = graphics.GetHdc();

            int PhysicalScreenHeight = GetDeviceCaps(deviceHandle, 117);
            int LogicalScreenHeight = GetDeviceCaps(deviceHandle, 10);

            double scalingFactor = (double)PhysicalScreenHeight / (double)LogicalScreenHeight;

            return scalingFactor; // 1.25 = 125%
        }

        public void setControlRect(Control control, Form form)
        {
            double magnification = GetDeviceCapsCheck();
            this.refX = (int)(Math.Ceiling(magnification * (control.Location.X + form.Left + 1))); ;
            this.refY = (int)(Math.Ceiling(magnification * (control.Location.Y + form.Top - 1 + form.Height - form.ClientRectangle.Height))); ;
            this.imgH = (int)(Math.Ceiling(magnification * (control.ClientRectangle.Height))); ;
            this.imgW = (int)(Math.Ceiling(magnification * (control.ClientRectangle.Width))); ;
        }

        public void DoCaptureImg()
        {
            if (imgW == 0 || imgH == 0)
            {
                return;
            }
            using(System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)imgW, (int)imgH, PixelFormat.Format32bppArgb))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(refX, refY, 0, 0, bitmap.Size);
                }

                bitmap.Save(path, ImageFormat.Png);
                //error발생 errormessage : System.Runtime.InteropServices.ExternalException: 'A generic error occurred in GDI+.'
                //권한 혹은 경로 혹은 저장 데이터의 문제 여기서 발생하는 원인은 경로문제
            }
        }

        public void test(Form form)
        {

        }
    }
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        ImgCapture capture = new ImgCapture();
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("gdi32")]
        private static extern int GetDeviceCaps(IntPtr deviceContextHandle, int index);
        //윈도우 배율 확인
        public double GetDeviceCapsCheck()
        {
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);

            IntPtr deviceHandle = graphics.GetHdc();

            int PhysicalScreenHeight = GetDeviceCaps(deviceHandle, 117);
            int LogicalScreenHeight = GetDeviceCaps(deviceHandle, 10);

            double scalingFactor = (double)PhysicalScreenHeight / (double)LogicalScreenHeight;

            return scalingFactor; // 1.25 = 125%
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //string path = "C:\\Users\\THESYSTEM\\Documents\\project\\ASAN SnTech\\FCUsimulator\\PC_app\\test.jpeg";
            string path = "D:\\project\\project\\ASAN SnTech\\FCUsimulator\\PC_app\\test.png";
            capture.setPath(path);

            double magnification = GetDeviceCapsCheck();

            int x = (int)(Math.Ceiling(magnification * (chartControl1.Location.X + this.Left)));
            int y = (int)(Math.Ceiling(magnification * (chartControl1.Location.Y + this.Top + this.Height - this.ClientRectangle.Height)));
            int h = (int)(Math.Ceiling(magnification * (chartControl1.ClientRectangle.Height)));
            int w = (int)(Math.Ceiling(magnification * (chartControl1.ClientRectangle.Width)));

            int x1 = chartControl1.Location.X + this.Left;
            int y1 = chartControl1.Location.Y + this.Top + this.Height - this.ClientRectangle.Height;
            int h1 = chartControl1.ClientRectangle.Height;
            int w1 = chartControl1.ClientRectangle.Width;

            capture.setRect(x, y, h, w);

            //스크린샷 검증완료
            capture.setControlRect(chartControl1, this);
            capture.DoCaptureImg();
            capture.test(this);

            //Stream image_stream = new MemoryStream();
            //chartControl1.SaveToStream(image_stream);
            //Image image = Image.FromStream();
            //image.Save(path);
        }
    }
}
