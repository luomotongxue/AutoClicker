using System.Runtime.InteropServices;
using System.Timers;
using MaterialSkin.Controls;
using Timer = System.Timers.Timer;

namespace AutoClicker
{
    public partial class Form1 : MaterialSkin.Controls.MaterialForm
    {
        private const int INPUT_MOUSE = 0;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int WM_HOTKEY = 0x0312;
        private int hotkeyId = 1; // 热键的唯一标识符  
        private bool isClicking = false;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                // 这里处理热键按下事件  
                if ((int)m.WParam == hotkeyId)
                {
                    if (isClicking)
                    {
                        stopClick();
                    }
                    else
                    {
                        startClick();
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private Timer clickTimer;
        private bool isRMB = false;

        private void ClickTimer_Elapsed(Object source, ElapsedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                // 获取当前光标位置并模拟点击  
                SimulateClick(Cursor.Position.X, Cursor.Position.Y, isRMB);

            }));
        }

        private void InitializeTimer(int value, bool useDelay = false)
        {
            clickTimer = new Timer();
            clickTimer.AutoReset = true;
            clickTimer.Interval = useDelay ? value : 1000 / value;
        }


        private void SimulateClick(int x, int y, bool isRightClick = false)
        {
            INPUT[] inputs = new INPUT[2];
            int mouseFlagsDown = isRightClick ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_LEFTDOWN;
            int mouseFlagsUp = isRightClick ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_LEFTUP;

            // 如果使用绝对坐标，则转换坐标  
            // 注意：这里假设你想要使用绝对坐标  
            x = (x * 65535) / Screen.PrimaryScreen.Bounds.Width;
            y = (y * 65535) / Screen.PrimaryScreen.Bounds.Height;

            // 按下鼠标  
            inputs[0].Type = INPUT_MOUSE;
            inputs[0].Data.Mouse.X = (int)x;
            inputs[0].Data.Mouse.Y = (int)y;
            inputs[0].Data.Mouse.MouseData = 0;
            inputs[0].Data.Mouse.Flags = (uint)(mouseFlagsDown | MOUSEEVENTF_ABSOLUTE); // 使用绝对坐标  
            inputs[0].Data.Mouse.Time = 0;
            inputs[0].Data.Mouse.ExtraInfo = IntPtr.Zero;

            // 释放鼠标  
            inputs[1].Type = INPUT_MOUSE;
            inputs[1].Data.Mouse.X = (int)x;
            inputs[1].Data.Mouse.Y = (int)y;
            inputs[1].Data.Mouse.MouseData = 0;
            inputs[1].Data.Mouse.Flags = (uint)(mouseFlagsUp | MOUSEEVENTF_ABSOLUTE); // 使用绝对坐标  
            inputs[1].Data.Mouse.Time = 0;
            inputs[1].Data.Mouse.ExtraInfo = IntPtr.Zero;

            // 发送输入  
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public Form1()
        {
            InitializeComponent();

            // 注册全局快捷键 Ctrl + F12  
            if (!RegisterHotKey(this.Handle, hotkeyId, 0, (uint)Keys.F8))
            {
                MessageBox.Show("Error: Could not register hotkey");
            }
        }

        private void startClick()
        {
            isClicking = true;
            int value = 0;
            try
            {
                value = Int32.Parse(materialTextBox21.Text);
            }
            catch
            {
                MaterialSkin.Controls.MaterialSnackBar materialSnackBar = new MaterialSnackBar();
                materialSnackBar.Text = "Please input a vaild number!";
                materialSnackBar.Show(this);
                isClicking = false;
                return;
            }

            if (clickTimer != null && clickTimer.Enabled)
            {
                clickTimer.Stop();
            }


            InitializeTimer(value, materialComboBox2.SelectedIndex == 1 ? true : false);

            clickTimer.Elapsed += ClickTimer_Elapsed;

            isRMB = materialRadioButton2.Checked ? true : false;

            // 启动定时器（如果需要立即开始）  
            clickTimer.Start();
            this.Hide();
        }

        private void stopClick()
        {
            isClicking = false;
            clickTimer.Stop();
            this.Show();
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            startClick();
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            stopClick();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            materialComboBox1.SelectedIndex = 0;
        }

        private void materialRadioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void materialTextBox21_Click(object sender, EventArgs e)
        {

        }
    }
}
