
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
 
namespace Radar_Launcher
{
    public partial class Form1 : Form
    {
        private Form MagForm;
        private Thread BackgroundJob;
        private System.Windows.Forms.Timer MagTimer;

        private IntPtr HwndMag;
        private float MagnificationVal;
        private RECT MagWindowRect = new RECT();

        private bool Initialized;
        private bool IsNormal = true; // window is normal
        private bool IsHidden = true; // window is hidden

        private enum GWL
        {
            ExStyle = -20
        }

        private enum WS_EX
        {
            Transparent = 0x20,
            Layered = 0x80000
        }

        private enum LWA
        {
            ColorKey = 0x1,
            Alpha = 0x1
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte alpha, LWA dwFlags);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        /**
         * Form constructor
        */
        public Form1()
        {
            // init component
            InitializeComponent();

            // set MagForm
            MagForm = this;

            // set initial magnification value
            MagnificationVal = 2.0f;

            // set window properties
            SetupWindow();

            // set events
            MagForm.Resize += new EventHandler(Form_Resize);
            MagForm.FormClosing += new FormClosingEventHandler(Form_FormClosing);

            // set timer
            MagTimer = new System.Windows.Forms.Timer();
            MagTimer.Tick += new EventHandler(Timer_Tick);

            // init NativeMethods
            Initialized = NativeMethods.MagInitialize();

            // check if initialized
            if (Initialized)
            {
                SetupMagnifier();
                MagTimer.Interval = NativeMethods.USER_TIMER_MINIMUM;
                MagTimer.Enabled = true;
            }

            // check for key press
            CheckForKeysJob();

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, Width, Height);
            Region = new Region(path);
            this.FormBorderStyle = FormBorderStyle.None;
        }

        /**
         * Form setup
        */
        private void SetupWindow()
        {
            // double buffer
            DoubleBuffered = true;

            // window state
            WindowState = FormWindowState.Normal;

            // window position
            StartPosition = FormStartPosition.CenterScreen;

            // hide from taskbar
            ShowInTaskbar = false;

            // show instructions
            MessageBox.Show(this, "Middle mouse = Toggle" + Environment.NewLine + "Add (+) = Zoom in" + Environment.NewLine + "Substract (-) = Zoom out" + Environment.NewLine + "Multiply (*) = Toggle fullscreen" + Environment.NewLine + "F10 = Exit application", "MyScopeIsFantastic", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /**
         * Prepare check for keys job to be run in another
         * thread.
        */
        private void CheckForKeysJob()
        {
            BackgroundJob = new Thread(() => CheckForKeys())
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };

            BackgroundJob.Start();
        }

        /**
         * Check for key press
         */
        private void CheckForKeys()
        {
            // hide
            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
            }));

            while (true)
            {
                //sleeping for while, this will reduce load on cpu
                Thread.Sleep(10);

                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    // if key is pressed
                    if (((GetAsyncKeyState(key) & (1 << 15)) != 0))
                    {
                        // enable or disable
                        if (key == Keys.MButton)
                        {
                            if (IsHidden)
                            {
                                BeginInvoke(new MethodInvoker(delegate
                                {
                                    Show();
                                }));
                                Thread.Sleep(500);

                                // set hidden
                                IsHidden = false;
                            }
                            else
                            {
                                BeginInvoke(new MethodInvoker(delegate
                                {
                                    Hide();
                                }));
                                Thread.Sleep(500);

                                // set hidden
                                IsHidden = true;
                            }
                        }

                        // zoom in
                        if (key == Keys.Add)
                        {
                            BeginInvoke(new MethodInvoker(delegate
                            {
                                Magnification++;
                            }));
                            Thread.Sleep(500);
                        }

                        // zoom out
                        if (key == Keys.Subtract)
                        {
                            BeginInvoke(new MethodInvoker(delegate
                            {
                                if (Magnification > 1f)
                                {
                                    Magnification--;
                                }
                            }));
                            Thread.Sleep(500);
                        }

                        // toggle fullscreen
                        if (key == Keys.Multiply)
                        {
                            if (IsNormal)
                            {
                                IsNormal = false;
                                BeginInvoke(new MethodInvoker(delegate
                                {
                                    WindowState = FormWindowState.Maximized;
                                }));
                                Thread.Sleep(500);
                            }
                            else
                            {
                                IsNormal = true;
                                BeginInvoke(new MethodInvoker(delegate
                                {
                                    WindowState = FormWindowState.Normal;
                                }));
                                Thread.Sleep(500);
                            }
                        }
                        // exit application
                        if (key == Keys.F10)
                        {
                            Application.Exit();
                        }
                    }
                }
            }
        }

        /**
         * Form closing events
        */
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            MagTimer.Enabled = false;
        }

        /**
         * Form resize events
        */
        private void Form_Resize(object sender, EventArgs e)
        {
            ResizeMagnifier();
        }

        /**
         * Timer tick events
        */
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateMaginifier();
        }

        /**
         * On shown event, used for click through
        */
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            int wl = GetWindowLong(Handle, GWL.ExStyle);
            wl = wl | 0x80000 | 0x20;
            SetWindowLong(Handle, GWL.ExStyle, wl);
            SetLayeredWindowAttributes(Handle, 0, 128, LWA.Alpha);
        }

        /**
         * Resize the magnifier
        */
        protected virtual void ResizeMagnifier()
        {
            if (Initialized && (HwndMag != IntPtr.Zero))
            {
                NativeMethods.GetClientRect(MagForm.Handle, ref MagWindowRect);

                // Resize the control to fill the window.
                NativeMethods.SetWindowPos(HwndMag, IntPtr.Zero, MagWindowRect.left, MagWindowRect.top, MagWindowRect.right, MagWindowRect.bottom, 0);
            }
        }

        /**
         * Update the magnifier
        */
        public virtual void UpdateMaginifier()
        {
            if ((!Initialized) || (HwndMag == IntPtr.Zero))
            {
                return;
            }

            RECT sourceRect = new RECT();

            Point MousePosition = new Point(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2);

            int width = (int)((MagWindowRect.right - MagWindowRect.left) / MagnificationVal);
            int height = (int)((MagWindowRect.bottom - MagWindowRect.top) / MagnificationVal);

            sourceRect.left = MousePosition.X - width / 2;
            sourceRect.top = MousePosition.Y - height / 2;

            // Don't scroll outside desktop area.
            if (sourceRect.left < 0)
            {
                sourceRect.left = 0;
            }

            if (sourceRect.left > NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN) - width)
            {
                sourceRect.left = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN) - width;
            }

            sourceRect.right = sourceRect.left + width;

            if (sourceRect.top < 0)
            {
                sourceRect.top = 0;
            }

            if (sourceRect.top > NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN) - height)
            {
                sourceRect.top = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN) - height;
            }

            sourceRect.bottom = sourceRect.top + height;

            if (MagForm == null)
            {
                MagTimer.Enabled = false;
                return;
            }

            if (MagForm.IsDisposed)
            {
                MagTimer.Enabled = false;
                return;
            }

            // Set the source rectangle for the magnifier control.
            NativeMethods.MagSetWindowSource(HwndMag, sourceRect);

            // Reclaim topmost status, to prevent unmagnified menus from remaining in view. 
            NativeMethods.SetWindowPos(MagForm.Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE);

            // Force redraw.
            NativeMethods.InvalidateRect(HwndMag, IntPtr.Zero, true);
        }

        /**
         * Magnification
        */
        private float Magnification
        {
            get { return MagnificationVal; }
            set
            {
                if (MagnificationVal != value)
                {
                    MagnificationVal = value;

                    // Set the magnification factor.
                    Transformation matrix = new Transformation(MagnificationVal);
                    NativeMethods.MagSetWindowTransform(HwndMag, ref matrix);
                }
            }
        }

        /**
         * Setup magnifier
        */
        protected void SetupMagnifier()
        {
            if (!Initialized)
            {
                return;
            }

            IntPtr hInst;

            hInst = NativeMethods.GetModuleHandle(null);

            // Make the window opaque.
            MagForm.AllowTransparency = true;
            MagForm.TransparencyKey = Color.Empty;
            MagForm.Opacity = 255;

            // Create a magnifier control that fills the client area.
            // To show curosr add: (int)MagnifierStyle.MS_SHOWMAGNIFIEDCURSOR
            NativeMethods.GetClientRect(MagForm.Handle, ref MagWindowRect);
            HwndMag = NativeMethods.CreateWindow((int)ExtendedWindowStyles.WS_EX_TRANSPARENT, NativeMethods.WC_MAGNIFIER,
                "MagnifierWindow", (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_VISIBLE,
                MagWindowRect.left, MagWindowRect.top, MagWindowRect.right, MagWindowRect.bottom, MagForm.Handle, IntPtr.Zero, hInst, IntPtr.Zero);

            if (HwndMag == IntPtr.Zero)
            {
                return;
            }

            // Set the magnification factor.
            Transformation matrix = new Transformation(MagnificationVal);
            NativeMethods.MagSetWindowTransform(HwndMag, ref matrix);
        }

        /**
         * Remove magnifier
        */
        protected void RemoveMagnifier()
        {
            if (Initialized)
            {
                NativeMethods.MagUninitialize();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}