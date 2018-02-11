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
using Memory;

namespace DS3_Walls {
    public partial class FormOverlay : Form {

        private const string WINDOW_NAME = "DARK SOULS III";
        private const string PROCESS_NAME = "DarkSoulsIII";
        const int PROCESS_WM_READ = 0x0010;
        int timerInterval = (int)(1000 / 1); // 1000 / N where N is number of times per second

        IntPtr handle = FindWindow(null, WINDOW_NAME);
        RECT rect;

        public Mem MemLib = new Mem();
        static Graphics graphics;
        static SolidBrush brush = new SolidBrush(Color.Red);
        static Pen pen = new Pen(brush, 2.0f);
        static Font font = new Font("Consolas", 12);

        long gameAddress_BaseB = 0x0;

        private Color backgroundColour = Color.Magenta;

        private struct Player_Coordinates {
            public float a, x, y, z; // the "a" stands for angle

            public Player_Coordinates(float a, float x, float y, float z) {
                this.a = a; // 0x74
                this.x = x; // 0x80
                this.y = y; // 0x88
                this.z = z; // 0x84
            }
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
        public static extern bool ReadProcessMemory(int hProcess, UInt64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public FormOverlay() {
            InitializeComponent();
        }

        private async Task<long> OpenProcessAsync() {
            long address = 0x0;
            if (MemLib.OpenProcess(PROCESS_NAME)) {
                address = await MemLib.AoBScan(0x0000000100000000, 0x00000002FFFFFFFF, "48 8B 1D ?? ?? ?? 04 48 8B F9 48 85 DB ?? ?? 8B 11 85 D2 ?? ?? 8D");
                if (address != 0x0) {
                    System.Console.WriteLine("yay found address <3 -> 0x" + address.ToString("x"));
                    //string p = MemLib.readLong((address + 3).ToString("x")).ToString();
                    //System.Console.WriteLine(p);
                    address = address + MemLib.readInt((address + 3).ToString("x")) + 7;
                    gameAddress_BaseB = address;
                    System.Console.WriteLine("BaseB: 0x" + gameAddress_BaseB.ToString("x"));
                }
                else {
                    System.Console.WriteLine("failed to find address :c");
                }
            }
            return address;
        }

        private void FormOverlay_Load(object sender, EventArgs e) {
            this.BackColor = backgroundColour;
            this.TransparencyKey = backgroundColour;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;

            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

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

            // LUA: s = '[[[BaseB]+40]+28]+' .. Stat_offsets[o]
            // Player 0
            long address = MemLib.readLong((gameAddress_BaseB).ToString("x"));
            //System.Console.WriteLine("+00: 0x" + address.ToString("x"));
            address = MemLib.readLong((address + 0x40).ToString("x"));
            //System.Console.WriteLine("+40: 0x" + address.ToString("x"));
            address = MemLib.readLong((address + 0x28).ToString("x"));
            //System.Console.WriteLine("+28: 0x" + address.ToString("x"));
            Player_Coordinates player_0 = new Player_Coordinates(
                MemLib.readFloat((address + 0x74).ToString("x")),
                MemLib.readFloat((address + 0x80).ToString("x")),
                MemLib.readFloat((address + 0x88).ToString("x")),
                MemLib.readFloat((address + 0x84).ToString("x"))
                );
            // Paint
            graphics.DrawString("Player Coordinates:\nA = " + player_0.a + "\nX = " + player_0.x + "\nY = " + player_0.y + "\nZ = " + player_0.z, font, brush, x, y);
            //graphics.DrawString("Player Coordinates:\nA = " + "a" + "\nX = " + "x" + "\nY = " + "y" + "\nZ = " + "z", font, brush, x, y);
        }

        private void FormOverlay_VisibleChangedAsync(object sender, EventArgs e) {
            if (!this.Visible) { // hidden
                MemLib.closeProcess();
            }
            else {
                bool result = GetWindowRect(handle, out rect);
                if (result) { // Window exists
                              // size
                    this.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                    this.Top = rect.top;
                    this.Left = rect.left;
                    OpenProcessAsync();
                }
                else {
                    this.Size = new Size(800, 600);
                    this.Top = 0;
                    this.Left = 0;
                }
            }
        }

        private void FormOverlay_FormClosing(object sender, FormClosingEventArgs e) {
            MemLib.closeProcess();
        }
    }
}
