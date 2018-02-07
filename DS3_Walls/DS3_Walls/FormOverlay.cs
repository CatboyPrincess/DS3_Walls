using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS3_Walls {
    public partial class FormOverlay : Form {

        private const string WINDOW_NAME = "DARK SOULS III";
        const int PROCESS_WM_READ = 0x0010;
        int timerInterval = (int)(1000 / 1); // 1000 / N where N is number of times per second

        IntPtr handle = FindWindow(null, WINDOW_NAME);
        RECT rect;

        static Graphics graphics;
        static SolidBrush brush = new SolidBrush(Color.Red);
        static Pen pen = new Pen(brush, 2.0f);
        static Font font = new Font("Consolas", 12);

        private Color backgroundColour = Color.Magenta;

        private struct Player_Coordinates {
            public float a, x, y, z; // the "a" stands for angle
        }

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

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public FormOverlay() {
            InitializeComponent();
        }

        private void FormOverlay_Load(object sender, EventArgs e) {
            this.BackColor = backgroundColour;
            this.TransparencyKey = backgroundColour;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;

            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            bool result = GetWindowRect(handle, out rect);
            if (result) {
                // size
                this.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                this.Top = rect.top;
                this.Left = rect.left;
            }
            else {
                this.Size = new Size(800, 600);
                this.Top = 0;
                this.Left = 0;
            }

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer(); // please explicitly use System.Windows.Forms.Timer
            timer.Interval = (timerInterval);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e) {
            this.Invalidate();
        }

        private void FormOverlay_Paint(object sender, PaintEventArgs e) {
            graphics = e.Graphics;

            int x = this.Right - 192;
            int y = this.Top + 10;

            // process
            Process process = Process.GetProcessesByName("DarkSoulsIII")[0];
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);
            int bytesRead = 0;
            byte[] buffer = new byte[4]; // memory needed
            Int64 address = 0x7FF5AEE12780; // placeholder. Still need a way to get the right address every time

            ReadProcessMemory((int)processHandle, address, buffer, buffer.Length, ref bytesRead);

            float coord_x = BitConverter.ToSingle(buffer, 0);
            //System.Console.WriteLine(x);

            // paint
            //graphics.Clear(backgroundColour);
            graphics.DrawString("Player Coordinates:\nA = na" + "\nX = " + coord_x + "\nY = na" + "\nZ = na", font, brush, x, y);
        }
    }
}
