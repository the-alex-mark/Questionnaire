﻿using ProgLib.IO;
using ProgLib.Network.Tcp;
using ProgLib.Windows.Forms.VSCode;
using Questionnaire;
using Questionnaire.Controls;
using Questionnaire.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Student
{
    public partial class FormMain : Form
    {
        #region Import

        [System.Runtime.InteropServices.DllImport("USER32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private extern static Boolean PostMessage(IntPtr hWnd, uint Msg, uint WParam, uint LParam);

        [System.Runtime.InteropServices.DllImport("USER32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private extern static Boolean ReleaseCapture();

        #endregion

        #region Shadow

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        private bool m_aeroEnabled;
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            Int32 nLeftRect, Int32 nTopRect, Int32 nRightRect, Int32 nBottomRect, Int32 nWidthEllipse, Int32 nHeightEllipse);

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message e)
        {
            switch (e.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;

                //case 70:
                //    Rectangle WorkingArea = SystemInformation.WorkingArea;
                //    Rectangle Rectangle = (Rectangle)System.Runtime.InteropServices.Marshal.PtrToStructure((IntPtr)((long)(IntPtr.Size * 2) + e.LParam.ToInt64()), typeof(Rectangle));
                //    if (Rectangle.X <= WorkingArea.Left + 10)
                //        System.Runtime.InteropServices.Marshal.WriteInt32(e.LParam, IntPtr.Size * 2, WorkingArea.Left);
                //    if (Rectangle.X + Rectangle.Width >= WorkingArea.Width - 10)
                //        System.Runtime.InteropServices.Marshal.WriteInt32(e.LParam, IntPtr.Size * 2, WorkingArea.Right - Rectangle.Width);
                //    if (Rectangle.Y <= WorkingArea.Top + 10)
                //        System.Runtime.InteropServices.Marshal.WriteInt32(e.LParam, IntPtr.Size * 2 + 4, WorkingArea.Top);
                //    if (Rectangle.Y + Rectangle.Height >= WorkingArea.Height - 10)
                //        System.Runtime.InteropServices.Marshal.WriteInt32(e.LParam, IntPtr.Size * 2 + 4, WorkingArea.Bottom - Rectangle.Height);
                //    break;

                default: break;
            }
            base.WndProc(ref e);
        }


        #endregion

        public FormMain()
        {
            InitializeComponent();

            // Установка максимального размера завёртывания формы
            MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;

            // Растяжение формы
            MouseMove += delegate (Object _object, MouseEventArgs _mouseEventArgs)
            {
                if (WindowState != FormWindowState.Maximized)
                {
                    if (_mouseEventArgs.X >= Width - 4 && _mouseEventArgs.Y >= Height - 4) { Cursor = Cursors.SizeNWSE; }
                    else if (_mouseEventArgs.X >= Width - 4 && _mouseEventArgs.Y > 25) { Cursor = Cursors.SizeWE; }
                    else if (_mouseEventArgs.Y >= Height - 4) { Cursor = Cursors.SizeNS; }
                    else { Cursor = Cursors.Default; }
                }
            };
            MouseDown += delegate (Object _object, MouseEventArgs _mouseEventArgs)
            {
                if (WindowState != FormWindowState.Maximized)
                {
                    uint Param = 0;

                    if (Cursor == Cursors.Default) { Param = 0; }
                    else
                    if (Cursor == Cursors.SizeNWSE) { Param = 0xF008; }
                    else
                    if (Cursor == Cursors.SizeWE) { Param = 0xF002; }
                    else
                    if (Cursor == Cursors.SizeNS) { Param = 0xF006; }

                    ReleaseCapture();
                    PostMessage(Handle, 0x0112, Param, 0);
                }
            };

            // Оформление MainMenu
            MainMenu.MouseDown += delegate (Object _object, MouseEventArgs _mouseEventArgs)
            {
                ReleaseCapture();
                PostMessage(Handle, 0x0112, 0xF012, 0);
            };
            MainMenu.Items["mmTitle"].MouseDown += delegate (Object _object, MouseEventArgs _mouseEventArgs)
            {
                ReleaseCapture();
                PostMessage(Handle, 0x0112, 0xF012, 0);
            };
            MainMenu.Items["mmMinimum"].Click += delegate (Object _object, EventArgs _eventArgs)
            {
                WindowState = FormWindowState.Minimized;
            };
            MainMenu.Items["mmMaximum"].Click += delegate (Object _object, EventArgs _eventArgs)
            {
                WindowState = (WindowState == FormWindowState.Maximized)
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            };
            MainMenu.Items["mmClose"].Click += delegate (Object _object, EventArgs _eventArgs)
            {
                Close();
                //Application.Exit();
            };
        }

        #region Variables
        
        private Thread _flow;
        private Boolean _translation = false;

        #endregion

        #region Methods

        /// <summary>
        /// Обновляет цветовую тему.
        /// </summary>
        /// <param name="Theme"></param>
        /// <param name="IconTheme"></param>
        private void UTheme(VSCodeTheme Theme, VSCodeIconTheme IconTheme)
        {
            VSCodeToolStripRenderer _renderer = new VSCodeToolStripRenderer(Theme, new VSCodeToolStripSettings(this, MainMenu, IconTheme));
            MainMenu.Renderer = _renderer;

            BackColor = _renderer.WindowBackColor;
            pStartPage.BackColor = _renderer.WindowBackColor;
            pQuestion.BackColor = _renderer.WindowBackColor;
        }

        /// <summary>
        /// Проверяет доступность сервера.
        /// </summary>
        private void OnCheckConnect()
        {
            while (true)
            {
                //if (_translation == false)
                //{
                    Byte[] Request = Encoding.UTF8.GetBytes(TcpRequest.Connect);
                    Boolean Result = Program.Server.Send(Program.Config.Server, Program.Config.Port, Request);

                    BeginInvoke(new MethodInvoker(delegate 
                    {
                        MainMenu.Items["mmTitle"].Text = (Result) ? "Опросник" : "Опросник (нет подключения)";
                    }));
                //}
            }
        }

        /// <summary>
        /// Получает данные сервера.
        /// </summary>
        /// <param name="_object"></param>
        /// <param name="_tcpEventArgs"></param>
        private void OnReceiver(Object _object, TcpReceiverEventArgs _tcpEventArgs)
        {
            String Client = TcpServer.GetHostName(_tcpEventArgs.Socket);
            String Message = Encoding.UTF8.GetString(_tcpEventArgs.Buffer, 0, _tcpEventArgs.Length);

            BeginInvoke(new MethodInvoker(delegate
            {
                if (Message.IsConnect())
                {
                    MainMenu.Items["mmTitle"].Text = "Опросник";
                }

                else if (Message.IsStart())
                {
                    MainMenu.Items["mmTitle"].Text = "Опросник";
                    mmMinimum.Enabled = false;
                    mmClose.Enabled = false;
                }

                else if (Message.IsStop())
                {
                    materialTabControl1.SelectTab(pStartPage);
                    mmMinimum.Enabled = true;
                    mmClose.Enabled = true;
                }

                else if (Message.IsDisconnect())
                {
                    MainMenu.Items["mmTitle"].Text = "Опросник (нет подключения)";
                    materialTabControl1.SelectTab(pStartPage);
                    mmMinimum.Enabled = true;
                    mmClose.Enabled = true;
                }

                else
                {
                    Question _question = new Question(XElement.Parse(Message));
                    label3.Text = _question.Name;

                    switch (_question.Type)
                    {
                        case "Выбор одного правильного ответа":
                            panel3.Visible = true;
                            panel4.Visible = false;
                            panel2.Height = 200;
                            panel4.Dock = DockStyle.Left;

                            listBox1.Items.Clear();
                            foreach (String Answer in _question.Answers) listBox1.Items.Add(Answer);
                            break;

                        case "Свободный ответ":
                            panel3.Visible = false;
                            panel4.Visible = true;
                            panel4.Dock = DockStyle.Fill;
                            panel2.Height = 100;
                            //panel2.MaximumSize = new Size(panel2.Width, 100);
                            break;
                    }

                    if (_question.Image != null)
                    {
                        pictureBox1.Image = _question.Image;
                        pictureBox1.Visible = true;
                        pictureBox2.Visible = true;
                    }
                    else
                    {
                        pictureBox1.Visible = false;
                        pictureBox2.Visible = false;
                    }

                    materialTabControl1.SelectTab(pQuestion);
                }
            }));
        }

        #endregion
        
        private void FormMain_Load(Object sender, EventArgs e)
        {
            // Обновление темы оформления
            UTheme(Program.Config.Theme, Program.Config.IconTheme);

            // Запуск сервера
            Program.Server.Receiver += OnReceiver;
            Program.Server.Start();

            _flow = new Thread(new ThreadStart(OnCheckConnect));
            _flow.Start();
        }
        private void FormMain_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (_flow != null)
            {
                _flow.Interrupt();
                _flow.Abort();
            }

            Program.Server.Receiver -= OnReceiver;
            Program.Server.Dispose();
        }
        private void FormMain_KeyDown(Object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    FormAbout About = new FormAbout();
                    About.ShowDialog();
                    break;

                case Keys.F2:
                    materialTabControl1.SelectTab(pStartPage);
                    break;

                case Keys.F3:
                    materialTabControl1.SelectTab(pQuestion);
                    break;


                default: break;
            }
        }
    }
}