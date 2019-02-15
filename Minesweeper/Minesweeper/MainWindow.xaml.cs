using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace Minesweeper
{
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Threading;
    using static Properties.Resources;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

        public struct SaveSettingInfor
        {
            public int[] SettingWindowInformation;
        }

        private const string SaveSettingFilePath = "settingInfor.json";

        internal BitmapImage BmpToBmpImg(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void SaveSetting()
        {
            var str = JsonConvert.SerializeObject(saveSettingInfor);
            File.WriteAllText(SaveSettingFilePath, str);
        }

        private void LoadSetting()
        {
            if (File.Exists(SaveSettingFilePath))
            {
                try
                {
                    saveSettingInfor = JsonConvert.DeserializeObject<SaveSettingInfor>(File.ReadAllText(SaveSettingFilePath));
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {

                }

            }
        }

        private DrawingMinesweeperEnv dme;
        private MinesweeperRule mr = new MinesweeperRule();
        SaveSettingInfor saveSettingInfor = new SaveSettingInfor { SettingWindowInformation = new int[4] };
        private int height;
        private int width;
        private int booms;
        private int time;
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        MinesweeperAI ma = new MinesweeperAI();

        public MainWindow()
        {
            InitializeComponent();
            string b = Encoding.UTF8.GetString(new byte[] { 1, 0, 3 });

            LoadSetting();

            var numList = new System.Drawing.Bitmap[] { time0, time1, time2, time3, time4, time5, time6, time7, time8, time9 };
            var faceList = new System.Drawing.Bitmap[] { facesmile, faceooh, facedead, facewin };
            var squareList = new System.Drawing.Bitmap[] { blank, bombdeath, bombflagged, bombmisflagged, bombquestion, bombrevealed, open0, open1, open2, open3, open4, open5, open6, open7, open8, shadow0 };
            var borderList = new System.Drawing.Bitmap[] { bordertopleft, bordertopright, borderbotleft, borderbotright, bordertopbot, borderleftright };
            dme = new DrawingMinesweeperEnv(cnvMain, numList.Select(x => BmpToBmpImg(x)).ToArray(), faceList.Select(x => BmpToBmpImg(x)).ToArray(), squareList.Select(x => BmpToBmpImg(x)).ToArray(), BmpToBmpImg(setting), borderList.Select(x => BmpToBmpImg(x)).ToArray(), BmpToBmpImg(AI));
            dme.upMouseDelegate += MouseClickControl;
            dme.SetPostion(10, 10);

            CreateDrawingMinesweeperEnv(saveSettingInfor.SettingWindowInformation);
            height = saveSettingInfor.SettingWindowInformation[0];
            width = saveSettingInfor.SettingWindowInformation[1];
            booms = saveSettingInfor.SettingWindowInformation[2];

            mr.CreateNewGame(height, width, booms);

            UpdateSizeWindow(saveSettingInfor.SettingWindowInformation[0], saveSettingInfor.SettingWindowInformation[1]);
            timer.Elapsed += HandleTimerElapsed;
            timer.Start();
        }

        private void CreateDrawingMinesweeperEnv(int[] information)
        {
            time = 0;
            timer.Stop();
            MinesweeperState ms = new MinesweeperState();
            ms.time = 0;
            ms.face = 0;
            ms.height = information[0];
            ms.width = information[1];
            ms.numOfBoom = information[2];
            ms.mapState = new int[ms.height][];
            for (int i = 0; i < ms.height; i++)
            {
                ms.mapState[i] = new int[ms.width];
            }

            dme.CreateNewEnvironment(ms);

            UpdateSizeWindow(ms.height, ms.width);
        }

        private void HandleTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { time++; dme.UpdateNumTime(time); }));
        }

        private void MouseClickControl(ControlType ct, MouseButton mouse, int row = 0, int col = 0)
        {
            if (mouse == MouseButton.Left)
            {
                switch (ct)
                {
                    case ControlType.Square:
                        if (mr.playState == MinesweeperRule.PlayState.Start)
                        {
                            time = 0;
                            timer.Start();
                        }
                        mr.Action(row, col);
                        dme.UpdateSquares(mr.GetCurrentState());
                        if (mr.playState == MinesweeperRule.PlayState.Win)
                        {
                            dme.UpdateFace(MinesweeperRule.F_facewin);
                            timer.Stop();
                            MessageBox.Show("Win game!");
                        }
                        else if (mr.playState == MinesweeperRule.PlayState.Lose)
                        {
                            dme.UpdateFace(MinesweeperRule.F_facedead);
                            timer.Stop();
                        }
                        break;
                    case ControlType.Face:
                        mr.CreateNewGame(height, width, booms);
                        CreateDrawingMinesweeperEnv(saveSettingInfor.SettingWindowInformation);
                        break;
                    case ControlType.Setting:
                        SettingWindow settingWindow = new SettingWindow(saveSettingInfor.SettingWindowInformation);
                        if (settingWindow.ShowDialog() == true)
                        {
                            CreateDrawingMinesweeperEnv(settingWindow.Information);
                            saveSettingInfor.SettingWindowInformation = settingWindow.Information;
                            SaveSetting();
                            height = settingWindow.Information[0];
                            width = settingWindow.Information[1];
                            booms = settingWindow.Information[2];

                            mr.CreateNewGame(height, width, booms);
                        }
                        break;
                    case ControlType.AI:
                        MinesweeperAI.Action act = ma.GetActionFromRule(mr);
                        if (act.mouse >= 0)
                            MouseClickControl(ControlType.Square, (act.mouse == 0) ? MouseButton.Left : MouseButton.Right, act.x, act.y);
                        break;
                    default:
                        break;
                }
            }
            else if (mouse == MouseButton.Right)
            {
                switch (ct)
                {
                    case ControlType.Square:
                        if (mr.playState == MinesweeperRule.PlayState.Start)
                        {
                            time = 0;
                            timer.Start();
                        }
                        mr.Action(row, col, 1);
                        dme.UpdateSquares(mr.GetCurrentState());
                        dme.UpdateNumBoom(booms - mr.totalFlag);
                        break;
                    default:
                        break;
                }
            }
        }

        void UpdateSizeWindow(int height, int weight)
        {
            int newHeight = 100 + height * DrawingMinesweeperEnv.whSquare;
            int newWidth = 20 + weight * DrawingMinesweeperEnv.whSquare;
            if (newWidth < 460)
                newWidth = 460;

            cnvMain.Width = newWidth;
            cnvMain.Height = newHeight;

            if (newWidth > 800)
                newWidth = 800;
            if (newHeight > 600)
                newHeight = 600;
            this.Width = newWidth + 40;
            this.Height = newHeight + 40;
        }

        double xTL = 0;
        double yTL = 0;
        double xRB = 0;
        double yRB = 0;
        MinesweeperSolver ms = new MinesweeperSolver(new System.Drawing.Bitmap[] { blank, bombdeath, bombflagged, bombmisflagged, bombquestion, bombrevealed, open0, open1, open2, open3, open4, open5, open6, open7, open8, shadow0 });

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.A == e.Key)
            {
                MouseClickControl(ControlType.AI, MouseButton.Left, 0, 0);
            }
            else if (Key.T == e.Key)
            {
                xTL = GetMousePosition().X;
                yTL = GetMousePosition().Y;
            }
            else if (Key.X == e.Key)
            {
                xRB = GetMousePosition().X;
                yRB = GetMousePosition().Y;
                ms.UpdateShapePostion(xTL, yTL, xRB, yRB);
                ms.UpdateSizeAndBoom(height, width, booms);
            }
            else if (Key.S == e.Key)
            {
                ms.UpdateSizeAndBoom(height, width, booms);
                int bx = (int)GetMousePosition().X;
                int by = (int)GetMousePosition().Y;
                mr.SetCurrentState(ms.GetMRState());
                int wSquare = (int)(xRB - xTL) / width;
                int hSquare = (int)(yRB - yTL) / height;
                int count = 0;

                while (count < width * width)
                {
                    MinesweeperAI.Action act = ma.GetActionFromRule(mr);

                    int x = (int)(act.y * wSquare + wSquare / 2 + xTL);
                    int y = (int)(act.x * hSquare + hSquare / 2 + yTL);
                    SetCursorPos(x, y);

                    if (act.mouse == 0)
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    }
                    else if(act.mouse == 2)
                    {
                        mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    }
                    else
                    {
                        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    }

                    SetCursorPos(bx, by);
                    Thread.Sleep(60);

                    mr.SetCurrentState(ms.GetMRState());
                    var curState = mr.GetCurrentState();
                    dme.UpdateSquares(curState);

                    int blankCount = 0;
                    int abnormalCount = 0;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            if(curState[i][j] == MinesweeperRule.MS_blank)
                            {
                                blankCount++;
                            }
                            if(curState[i][j] == MinesweeperRule.MS_bombdeath || curState[i][j] == MinesweeperRule.MS_bombmisflagged ||
                                curState[i][j] == MinesweeperRule.MS_bombrevealed)
                            {
                                abnormalCount++;
                            }
                        }
                    }
                    if(blankCount ==0 || abnormalCount>0)
                    {
                        break;
                    }

                    count++;
                }
            }
        }
    }
}
