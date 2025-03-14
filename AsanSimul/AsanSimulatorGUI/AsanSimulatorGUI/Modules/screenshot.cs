using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
            this.refX = (int)(Math.Ceiling(magnification * (control.Location.X + form.Left + 8))); ;
            this.refY = (int)(Math.Ceiling(magnification * (control.Location.Y + form.Top - 8 + form.Height - form.ClientRectangle.Height))); ;
            this.imgH = (int)(Math.Ceiling(magnification * (control.ClientRectangle.Height))); ;
            this.imgW = (int)(Math.Ceiling(magnification * (control.ClientRectangle.Width))); ;
        }

        public void DoCaptureImg(string path)
        {
            if (imgW == 0 || imgH == 0)
            {
                return;
            }
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)imgW, (int)imgH))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(refX, refY, 0, 0, bitmap.Size);
                }

                bitmap.Save(path, ImageFormat.Png);
            }
        }
    }
}
