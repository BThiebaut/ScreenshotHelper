using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShotHelper
{
    public partial class SelectArea : Form
    {
        private const int
           HTLEFT = 10,
           HTRIGHT = 11,
           HTTOP = 12,
           HTTOPLEFT = 13,
           HTTOPRIGHT = 14,
           HTBOTTOM = 15,
           HTBOTTOMLEFT = 16,
           HTBOTTOMRIGHT = 17;

        const int borderWidth = 5; // you can rename this variable if you like

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;
        private const double INITIAL_OPACITY = .1D;
        private const double TRANSP_OPACITY = 0D;

        Rectangle Top { get { return new Rectangle(0, 0, this.ClientSize.Width, borderWidth); } }
        Rectangle Left { get { return new Rectangle(0, 0, borderWidth, this.ClientSize.Height); } }
        Rectangle Bottom { get { return new Rectangle(0, this.ClientSize.Height - borderWidth, this.ClientSize.Width, borderWidth); } }
        Rectangle Right { get { return new Rectangle(this.ClientSize.Width - borderWidth, 0, borderWidth, this.ClientSize.Height); } }
        Rectangle TopLeft { get { return new Rectangle(0, 0, borderWidth, borderWidth); } }
        Rectangle TopRight { get { return new Rectangle(this.ClientSize.Width - borderWidth, 0, borderWidth, borderWidth); } }
        Rectangle BottomLeft { get { return new Rectangle(0, this.ClientSize.Height - borderWidth, borderWidth, borderWidth); } }
        Rectangle BottomRight { get { return new Rectangle(this.ClientSize.Width - borderWidth, this.ClientSize.Height - borderWidth, borderWidth, borderWidth); } }

        Bitmap bmp;

        public SelectArea()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None; // no borders
            this.Opacity = INITIAL_OPACITY; // make trasparent
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true); // this is to avoid visual artifacts
            this.TopMost = true;
        }

        protected override void OnPaint(PaintEventArgs e) // you can safely omit this method if you want
        {
            /*
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectArea));
            var png = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            Rectangle rc = new Rectangle(new Point(0, 0), png.Size);
            e.Graphics.DrawImage(png, rc);
            */
            e.Graphics.FillRectangle(Brushes.Black, Top);
            e.Graphics.FillRectangle(Brushes.Black, Left);
            e.Graphics.FillRectangle(Brushes.Black, Right);
            e.Graphics.FillRectangle(Brushes.Black, Bottom);
        }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);


            if (message.Msg == 0x84) // WM_NCHITTEST
            {
                var cursor = this.PointToClient(Cursor.Position);

                if (TopLeft.Contains(cursor)) message.Result = (IntPtr)HTTOPLEFT;
                else if (TopRight.Contains(cursor)) message.Result = (IntPtr)HTTOPRIGHT;
                else if (BottomLeft.Contains(cursor)) message.Result = (IntPtr)HTBOTTOMLEFT;
                else if (BottomRight.Contains(cursor)) message.Result = (IntPtr)HTBOTTOMRIGHT;

                else if (Top.Contains(cursor)) message.Result = (IntPtr)HTTOP;
                else if (Left.Contains(cursor)) message.Result = (IntPtr)HTLEFT;
                else if (Right.Contains(cursor)) message.Result = (IntPtr)HTRIGHT;
                else if (Bottom.Contains(cursor)) message.Result = (IntPtr)HTBOTTOM;
                else message.Result = (IntPtr)(HT_CAPTION);
            }
        }

        private void SelectArea_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Exit();
        }


        private void CaptureScreen()
        {
            try
            {

                this.Opacity = TRANSP_OPACITY;

                Rectangle rect = new Rectangle(this.Location.X, this.Location.Y, this.Width, this.Height);
                bmp = new Bitmap(rect.Width - 1, rect.Height - 1, PixelFormat.Format32bppArgb);

                Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, this.Size, CopyPixelOperation.SourceCopy);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.Dispose();

                this.Opacity = INITIAL_OPACITY;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }


        private void SelectArea_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CaptureScreen();
                Clipboard.SetImage(bmp);
            }else if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

    }
}
