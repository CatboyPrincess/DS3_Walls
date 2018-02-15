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
        private const int DEFAULT_WIDTH = 1920;
        private const int DEFAULT_HEIGHT = 1080;
        private const int FONT_SIZE = 12;
        private const int PROCESS_WM_READ = 0x0010;
        private const float min_resolution = 15;

        int timerInterval = (int)(1000 / 1); // 1000 / N where N is number of times per second

        IntPtr handle = FindWindow(null, WINDOW_NAME);
        RECT rect;
        static Mem MemLib = new Mem();
        static Graphics graphics;
        static SolidBrush brush;
        static Pen pen;
        static Font font;

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

            public bool IsEmpty() {
                foreach (float v in new float[] { this.a, this.x, this.y, this.z }) {
                    if (v != 0f) return false;
                }
                return true;
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

        private bool ProcessExists(string processName) {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0) {
                return true;
            }
            return false;
        }

        private async Task<long> OpenProcessAsync() {
            long address = 0x0;
            if (MemLib.OpenProcess(PROCESS_NAME)) {
                //0x00000001404c1a3a
                address = await MemLib.AoBScan(0x0000000140000000, 0x000000015FFFFFFF, "48 8B 1D ?? ?? ?? 04 48 8B F9 48 85 DB ?? ?? 8B 11 85 D2 ?? ?? 8D");
                if (address != 0x0) {
                    System.Console.WriteLine("yay found address <3 -> 0x" + address.ToString("x"));
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
        
        private long PointerChainPeeler(long basePointer, long[] pointerOffsets) {
            // LUA: s = '[[[BaseB]+40]+28]+' .. Stat_offsets[o]
            long address = MemLib.readLong(basePointer.ToString("x"));
            foreach (long offset in pointerOffsets) {
                address = MemLib.readLong((address + offset).ToString("x"));
            }
            return address;
        }

        private void FormOverlay_Load(object sender, EventArgs e) {
            // Properties of form
            this.BackColor = backgroundColour;
            this.TransparencyKey = backgroundColour;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            // Timer set up
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer(); // Explicitly use System.Windows.Forms.Timer
            timer.Interval = (timerInterval);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e) {
            this.Refresh(); // invalidate and redraw
        }

        private Player_Coordinates GetPlayerCoordinates(int playerNumber) {
            // LUA: s = '[[[BaseB]+40]+28]+' .. Stat_offsets[o]
            long address;
            Player_Coordinates empty = new Player_Coordinates(0, 0, 0, 0);
            if (!ProcessExists(PROCESS_NAME)) return empty;
            if (playerNumber == 0) {
                // Player 0 -> self
                address = PointerChainPeeler(gameAddress_BaseB, new long[] { 0x40, 0x28 });
                return new Player_Coordinates(
                    MemLib.readFloat((address + 0x74).ToString("x")),
                    MemLib.readFloat((address + 0x80).ToString("x")),
                    MemLib.readFloat((address + 0x88).ToString("x")),
                    MemLib.readFloat((address + 0x84).ToString("x"))
                    );
            } else if (playerNumber >= 1 && playerNumber <= 5 ) {
                // LUA: s = '[[[[[BaseB]+40]+' .. PlayerN_offsets[n] .. ']+18]+28]+' .. Stat_offsets[o]
                address = PointerChainPeeler(gameAddress_BaseB, new long[] { 0x40, 0x38 * playerNumber, 0x18, 0x28 });
                return new Player_Coordinates(
                    MemLib.readFloat((address + 0x74).ToString("x")),
                    MemLib.readFloat((address + 0x80).ToString("x")),
                    MemLib.readFloat((address + 0x88).ToString("x")),
                    MemLib.readFloat((address + 0x84).ToString("x"))
                    );
            }
            else {
                System.Console.WriteLine("invalid player number");
                return empty;
            }
        }

        private double GetDistance(double x1, double y1, double x2, double y2) {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        private void FormOverlay_Paint(object sender, PaintEventArgs e) {
            graphics = e.Graphics;

            brush = new SolidBrush(Color.Orange);
            pen = new Pen(brush, 2.0f);
            font = new Font("Consolas", FONT_SIZE);

            float x, y, width, height;

            Player_Coordinates[] players = new Player_Coordinates[] {
                    GetPlayerCoordinates(0),
                    GetPlayerCoordinates(1),
                    GetPlayerCoordinates(2),
                    GetPlayerCoordinates(3),
                    GetPlayerCoordinates(4),
                    GetPlayerCoordinates(5)
                };

            // Paint
#if DEBUG
            for (int i = 0; i <= 5; i++) {
                if (players[i].IsEmpty()) continue;
                x = this.Right - FONT_SIZE * 14;
                y = this.Top + FONT_SIZE * (i * 10 + 1);
                graphics.DrawString(String.Format("Player {0} Coords:\nA = {1,8:F2}\nX = {2,8:F2}\nY = {3,8:F2}\nZ = {4,8:F2}", i, players[i].a, players[i].x, players[i].y, players[i].z), font, brush, x, y);
            }
#endif

            // Minimap
            width = this.Bottom / 4;
            height = width;
#if DEBUG
            x = this.Right - width - FONT_SIZE * (14 + 2);
#else
            x = this.Right - width - FONT_SIZE * 2;
#endif
            y = this.Top + FONT_SIZE * 2;
            float xCentre = x + width / 2;
            float yCentre = y + height / 2;
            graphics.DrawRectangle(pen, x, y, width, height);

            // Player 0
            pen = new Pen(brush, 1.0f);
            font = new Font("Consolas", FONT_SIZE - 2);
            graphics.DrawString("^", font, brush, xCentre, yCentre);

            // Other players

            // Find x and y maximum deltas
            brush = new SolidBrush(Color.LightGreen);
            float max_distance = 0;
            for (int i = 1; i <= 5; i++) {
                if (players[i].IsEmpty()) continue;
                float dist = (float)GetDistance(players[i].x, players[i].y, players[0].x, players[0].y);
                if (dist > max_distance) max_distance = dist;
            }
            // Minimum resolution
            max_distance = Math.Max(max_distance, min_resolution);

            // Draw other players
            float a = players[0].a;
            float width2 = width * 0.9f;
            float height2 = height * 0.9f;
            for (int i = 1; i <= 5; i++) {
                float x2, y2, xRel, yRel, xRel2, yRel2;
                if (players[i].IsEmpty()) continue;

                xRel = (players[i].x - players[0].x) / max_distance * width2 / 2;
                yRel = (players[i].y - players[0].y) / max_distance * height2 / 2;
                
                // Rotate
                xRel2 = -(xRel * (float)Math.Cos(a) - yRel * (float)Math.Sin(a));
                yRel2 = yRel * (float)Math.Cos(a) + xRel * (float)Math.Sin(a);

                x2 = xCentre + xRel2;
                y2 = yCentre + yRel2;
                graphics.DrawString(String.Format("{0}:{1,3:F1}", i, players[i].z - players[0].z), font, brush, x2, y2);
            }
        }

        private void FormOverlay_VisibleChangedAsync(object sender, EventArgs e) {
            if (!this.Visible) { // hidden
                MemLib.closeProcess();
            }
            else { // shown
                bool result = GetWindowRect(handle, out rect);
                if (result) { // Window exists
                    // size
                    this.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                    this.Top = rect.top;
                    this.Left = rect.left;
                    OpenProcessAsync(); // open process for MemLib
                }
                else { // Window doesn't exist
                    this.Size = new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);
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
