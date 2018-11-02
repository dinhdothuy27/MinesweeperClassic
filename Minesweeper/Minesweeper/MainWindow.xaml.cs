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
    using System.Windows.Threading;
    using static Properties.Resources;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            string b = Encoding.UTF8.GetString(new byte[] { 1, 0, 3 });

            LoadSetting();

            var numList = new System.Drawing.Bitmap[] { time0, time1, time2, time3, time4, time5, time6, time7, time8, time9 };
            var faceList = new System.Drawing.Bitmap[] { facesmile, faceooh, facedead, facewin };
            var squareList = new System.Drawing.Bitmap[] { blank, bombdeath, bombflagged, bombmisflagged, bombquestion, bombrevealed, open0, open1, open2, open3, open4, open5, open6, open7, open8, shadow0 };
            var borderList = new System.Drawing.Bitmap[] { bordertopleft, bordertopright, borderbotleft, borderbotright, bordertopbot, borderleftright };
            dme = new DrawingMinesweeperEnv(cnvMain, numList.Select(x => BmpToBmpImg(x)).ToArray(), faceList.Select(x => BmpToBmpImg(x)).ToArray(), squareList.Select(x => BmpToBmpImg(x)).ToArray(), BmpToBmpImg(setting), borderList.Select(x=>BmpToBmpImg(x)).ToArray());
            dme.upMouseDelegate += MouseClickControl;
            dme.SetPostion(10, 10);

            CreateDrawingMinesweeperEnv(saveSettingInfor.SettingWindowInformation);
            height = saveSettingInfor.SettingWindowInformation[0];
            width = saveSettingInfor.SettingWindowInformation[1];
            booms = saveSettingInfor.SettingWindowInformation[2];

            mr.CreateNewGame(height, width, booms);

            UpdateSizeWindow(saveSettingInfor.SettingWindowInformation[0], saveSettingInfor.SettingWindowInformation[1]);

            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Tick += (s, e) => { time++; dme.UpdateNumTime(time); };
        }

        private void CreateDrawingMinesweeperEnv(int[] information)
        {
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

        private void MouseClickControl(ControlType ct, MouseButton mouse, int row, int col)
        {
            if (mouse == MouseButton.Left)
            {
                switch (ct)
                {
                    case ControlType.Square:
                        if (mr.playState == MinesweeperRule.PlayState.Start)
                        {
                            time = 0;
                            dispatcherTimer.Start();
                        }
                        mr.Action(row, col);
                        dme.UpdateSquares(mr.GetCurrentState());
                        if (mr.playState == MinesweeperRule.PlayState.Win)
                        {
                            dme.UpdateFace(MinesweeperRule.F_facewin);
                            dispatcherTimer.Stop();
                            MessageBox.Show("Win game!");
                        }
                        else if (mr.playState == MinesweeperRule.PlayState.Lose)
                        {
                            dme.UpdateFace(MinesweeperRule.F_facedead);
                            dispatcherTimer.Stop();
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
                            dispatcherTimer.Start();
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
            this.Width = newWidth + 80;
            this.Height = newHeight + 40;
        }
    }
}
