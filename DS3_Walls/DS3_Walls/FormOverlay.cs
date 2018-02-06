using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS3_Walls {
    public partial class FormOverlay : Form {

        private const string WINDOW_NAME = "DARK SOULS III";

        IntPtr handle = FindWindow(null, WINDOW_NAME);
        RECT rect;

        Graphics graphics;

        private struct RECT {
            public int left, top, right, bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClass, string lpWindow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        public FormOverlay() {
            InitializeComponent();
        }

        private void FormOverlay_Load(object sender, EventArgs e) {
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;

            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            bool result = GetWindowRect(handle, out rect);
            if (result) {
                this.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                this.Top = rect.top;
                this.Left = rect.left;
            }
            else {
                this.Size = new Size(800, 600);
                this.Top = 0;
                this.Left = 0;
            }
        }

        private void FormOverlay_Paint(object sender, PaintEventArgs e) {
            graphics = e.Graphics;
            SolidBrush brush = new SolidBrush(Color.White);
            Pen pen = new Pen(brush, 2.0f);
            Font font = new Font("Arial", 14);

            int x = this.Left + 200;
            int y = this.Top + 200;

            graphics.DrawString("testing this thingy", font, brush, x, y);
            graphics.DrawRectangle(pen, x, y, x + 100, y + 150);
        }
    }
}
