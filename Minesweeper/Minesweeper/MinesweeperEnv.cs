using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace Minesweeper
{
    public struct MinesweeperState
    {
        public int time;
        public int face;
        public int numOfBoom;
        public int height;
        public int width;
        public int[][] mapState;
    }

    public enum ControlType
    {
        None = 0,
        Square,
        Face,
        Setting,
        AI,
    }

    public class DrawingMinesweeperEnv
    {

        public delegate void DownMouseDelegate(ControlType ct, MouseButton mouse = 0, int row = 0, int col = 0);
        public DownMouseDelegate upMouseDelegate;

        private int x = 20;
        private int y = 10;

        private const int yNumTop = 2;
        private const int yFaceTop = 0;
        private const int ySettingTop = 0;
        private const int ySquareTop = 50;
        private const int yAITop = 0;

        private const int xNumBoomLeft = 0;
        private const int xFaceLeft = 100;
        private const int xTimeLeft = 200;
        private const int xSettingLeft = 300;
        private const int xAILeft = 350;

        private const int xSquareLeft = 0;

        private const int wNum = 13;
        private const int whFace = 26;
        public const int whSquare = 24;
        private const int whSetting = 26;
        private const int whBorder = 10;
        private const int whAI = 26;

        public const int maxHeight = 40;
        public const int maxWidth = 60;

        private Canvas canvas;
        private ImageControlObject firstNumBoom;
        private ImageControlObject secondNumBoom;
        private ImageControlObject thirdNumBoom;

        private ImageControlObject firstNumTime;
        private ImageControlObject secondNumTime;
        private ImageControlObject thirdNumTime;

        private ImageControlObject face;

        private ImageControlObject setting;

        private ImageControlObject ai;

        private ImageControlObject borderTopLef;
        private ImageControlObject borderTopRight;
        private ImageControlObject borderBotLef;
        private ImageControlObject borderBotRight;

        private ImageControlObject[] borderLeft;
        private ImageControlObject[] borderRight;
        private ImageControlObject[] borderTop;
        private ImageControlObject[] borderBot;

        private ImageControlObject[][] squares;
        private int height = 0;
        private int width = 0;

        private BitmapImage[] squareBmpList;
        private BitmapImage[] borderBmpList;

        public DrawingMinesweeperEnv(Canvas cnv, BitmapImage[] numberList, BitmapImage[] faceList, BitmapImage[] squareList, BitmapImage _setting, BitmapImage[] bolderList, BitmapImage _ai)
        {
            canvas = cnv;
            squareBmpList = squareList;
            firstNumBoom = ImageControlObject.CreateImage(numberList, canvas);
            secondNumBoom = ImageControlObject.CreateImage(numberList, canvas);
            thirdNumBoom = ImageControlObject.CreateImage(numberList, canvas);

            firstNumTime = ImageControlObject.CreateImage(numberList, canvas);
            secondNumTime = ImageControlObject.CreateImage(numberList, canvas);
            thirdNumTime = ImageControlObject.CreateImage(numberList, canvas);

            face = ImageControlObject.CreateImage(faceList, canvas);

            setting = ImageControlObject.CreateImage(_setting, canvas);

            ai = ImageControlObject.CreateImage(_ai, canvas);

            borderTopLef = ImageControlObject.CreateImage(bolderList[0], canvas);
            borderTopRight = ImageControlObject.CreateImage(bolderList[1], canvas);
            borderBotLef = ImageControlObject.CreateImage(bolderList[2], canvas);
            borderBotRight = ImageControlObject.CreateImage(bolderList[3], canvas);

            borderBmpList = bolderList;

            canvas.MouseUp += canvas_MouseUp;
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(canvas);
            point.X -= x;
            point.Y -= y;
            if (point.X >= xFaceLeft && point.X < xFaceLeft + whFace && point.Y >= yFaceTop && point.Y < yFaceTop + whFace)
            {
                upMouseDelegate?.Invoke(ControlType.Face, e.ChangedButton);
            }
            else if (point.X >= xSquareLeft && point.Y >= ySquareTop)
            {
                int col = ((int)point.X - xSquareLeft) / whSquare;
                int row = ((int)point.Y - ySquareTop) / whSquare;
                if (row < height && col < width)
                {
                    upMouseDelegate?.Invoke(ControlType.Square, e.ChangedButton, row, col);
                }
            }
            else if (point.X >= xSettingLeft && point.X < xSettingLeft + whSetting && point.Y >= ySettingTop && point.Y < ySettingTop + whSetting)
            {
                upMouseDelegate?.Invoke(ControlType.Setting, e.ChangedButton);
            }
            else if (point.X >= xAILeft && point.X < xAILeft + whAI && point.Y >= yAITop && point.Y < yAITop + whAI)
            {
                upMouseDelegate?.Invoke(ControlType.AI, e.ChangedButton);
            }
        }

        private int getNum(int num, int index)
        {
            if (num > 999)
                num = 999;
            for (int i = 0; i < 2 - index; i++)
            {
                num /= 10;
            }
            return num % 10;
        }

        public void UpdateNumBoom(int numOfBoom)
        {
            firstNumBoom.ChoiceImage(getNum(numOfBoom, 0));
            secondNumBoom.ChoiceImage(getNum(numOfBoom, 1));
            thirdNumBoom.ChoiceImage(getNum(numOfBoom, 2));
        }

        public void UpdateNumTime(int time)
        {
            firstNumTime.ChoiceImage(getNum(time, 0));
            secondNumTime.ChoiceImage(getNum(time, 1));
            thirdNumTime.ChoiceImage(getNum(time, 2));
        }

        public void UpdateFace(int faceIndex)
        {
            face.ChoiceImage(faceIndex);
        }

        public void UpdateSquares(int[][] mapState)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    squares[i][j].ChoiceImage(mapState[i][j]);
                }
            }
        }

        public void RemoveOldFromCanvas()
        {
            if (squares != null)
            {
                for (int i = 0; i < height; i++)
                {
                    if (squares[i] != null)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            squares[i][j].RemoveFromCanvas();
                        }
                    }
                }
            }

            if (borderLeft != null)
            {
                foreach (var b in borderLeft)
                {
                    b.RemoveFromCanvas();
                }
            }
            if (borderRight != null)
            {
                foreach (var b in borderRight)
                {
                    b.RemoveFromCanvas();
                }
            }
            if (borderTop != null)
            {
                foreach (var b in borderTop)
                {
                    b.RemoveFromCanvas();
                }
            }
            if (borderBot != null)
            {
                foreach (var b in borderBot)
                {
                    b.RemoveFromCanvas();
                }
            }
        }

        public void CreateNewEnvironment(MinesweeperState ms)
        {
            UpdateNumBoom(ms.numOfBoom);
            UpdateNumTime(ms.time);
            UpdateFace(ms.face);

            if (ms.height > maxHeight)
                ms.height = maxHeight;
            if (ms.width > maxWidth)
                ms.width = maxWidth;

            if (height != ms.height || width != ms.width)
            {
                RemoveOldFromCanvas();
                squares = new ImageControlObject[ms.height][];
                height = ms.height;
                width = ms.width;
                for (int i = 0; i < ms.height; i++)
                {
                    squares[i] = new ImageControlObject[ms.width];
                    for (int j = 0; j < ms.width; j++)
                    {
                        squares[i][j] = ImageControlObject.CreateImage(squareBmpList, canvas);
                    }
                }

                borderLeft = new ImageControlObject[height];
                borderRight = new ImageControlObject[height];
                borderTop = new ImageControlObject[width];
                borderBot = new ImageControlObject[width];

                for (int i = 0; i < height; i++)
                {
                    borderLeft[i] = ImageControlObject.CreateImage(borderBmpList[5], canvas);
                    borderRight[i] = ImageControlObject.CreateImage(borderBmpList[5], canvas);
                }
                for (int i = 0; i < width; i++)
                {
                    borderTop[i] = ImageControlObject.CreateImage(borderBmpList[4], canvas);
                    borderBot[i] = ImageControlObject.CreateImage(borderBmpList[4], canvas);
                }
            }
            UpdateSquares(ms.mapState);

            SetPostion(x, y);
        }

        public void CreateNewEnvironment(string json)
        {
            MinesweeperState ms = JsonConvert.DeserializeObject<MinesweeperState>(json);
            CreateNewEnvironment(ms);
        }

        public void UpdateEnvironment(MinesweeperState ms)
        {
            UpdateNumBoom(ms.numOfBoom);
            UpdateNumTime(ms.time);
            UpdateFace(ms.face);
            UpdateSquares(ms.mapState);
        }

        public void UpdateEnvironment(string json)
        {
            MinesweeperState ms = JsonConvert.DeserializeObject<MinesweeperState>(json);
            UpdateEnvironment(ms);
        }

        public void SetPostion(int _x, int _y)
        {
            x = _x;
            y = _y;
            firstNumBoom.SetPosition(x + xNumBoomLeft, y + yNumTop);
            secondNumBoom.SetPosition(x + xNumBoomLeft + wNum, y + yNumTop);
            thirdNumBoom.SetPosition(x + xNumBoomLeft + wNum * 2, y + yNumTop);

            firstNumTime.SetPosition(x + xTimeLeft, y + yNumTop);
            secondNumTime.SetPosition(x + xTimeLeft + wNum, y + yNumTop);
            thirdNumTime.SetPosition(x + xTimeLeft + wNum * 2, y + yNumTop);

            face.SetPosition(x + xFaceLeft, y + yFaceTop);

            setting.SetPosition(x + xSettingLeft, y + ySettingTop);

            ai.SetPosition(x + xAILeft, y + yAITop);

            if (squares != null)
            {
                for (int i = 0; i < squares.Length; i++)
                {
                    if (squares[i] != null)
                    {
                        for (int j = 0; j < squares[i].Length; j++)
                        {
                            squares[i][j].SetPosition(x + xSquareLeft + whSquare * j, y + ySquareTop + whSquare * i);
                        }
                    }
                }
            }

            borderTopLef.SetPosition(x - whBorder + xSquareLeft, y - whBorder + ySquareTop);
            borderTopRight.SetPosition(x + width * whSquare + xSquareLeft, y - whBorder + ySquareTop);
            borderBotLef.SetPosition(x - whBorder + xSquareLeft, y + height * whSquare + ySquareTop);
            borderBotRight.SetPosition(x + width * whSquare + xSquareLeft, y + height * whSquare + ySquareTop);

            for (int i = 0; i < height; i++)
            {
                borderLeft[i].SetPosition(x - whBorder + xSquareLeft, y + i * whSquare + ySquareTop);
                borderRight[i].SetPosition(x + width * whSquare + xSquareLeft, y + i * whSquare + ySquareTop);
            }
            for (int i = 0; i < width; i++)
            {
                borderTop[i].SetPosition(x + i * whSquare + xSquareLeft, y - whBorder + ySquareTop);
                borderBot[i].SetPosition(x + i * whSquare + xSquareLeft, y + height * whSquare + ySquareTop);
            }
        }
    }

    public class MinesweeperRule
    {
        // Map State defined value
        public const int MS_blank = 0;
        public const int MS_bombdeath = 1;
        public const int MS_bombflagged = 2;
        public const int MS_bombmisflagged = 3;
        public const int MS_bombquestion = 4;
        public const int MS_bombrevealed = 5;
        public const int MS_open0 = 6;
        public const int MS_open1 = 7;
        public const int MS_open2 = 8;
        public const int MS_open3 = 9;
        public const int MS_open4 = 10;
        public const int MS_open5 = 11;
        public const int MS_open6 = 12;
        public const int MS_open7 = 13;
        public const int MS_open8 = 14;
        public const int MS_shadow0 = 15;

        // Face defined value
        public const int F_facesmile = 0;
        public const int F_faceooh = 1;
        public const int F_facedead = 2;
        public const int F_facewin = 3;

        public enum PlayState
        {
            Start = 0,
            Playing,
            Win,
            Lose,
        }
        public int height;
        public int width;
        public int booms;
        int[][] rightState;
        int[][] curState;
        public PlayState playState;
        int totalBlank;
        public int totalFlag;
        int totalNonBoom;
        public List<int> blankIndexList;

        public void CreateNewGame(int _height, int _width, int _booms)
        {
            height = _height;
            width = _width;
            booms = _booms;
            totalBlank = height * width;
            totalFlag = 0;
            totalNonBoom = 0;
            playState = PlayState.Start;
            blankIndexList = (new int[totalBlank]).Select((x, i) => i).ToList();
            rightState = new int[height][];
            curState = new int[height][];
            for (int i = 0; i < height; i++)
            {
                rightState[i] = new int[width];
                curState[i] = new int[width];
            }
        }

        public int[][] GetCurrentState()
        {
            return curState;
        }

        public void SetCurrentState(int[][] _curState)
        {
            curState = _curState;
        }

        public void Action(int x, int y, int mouse = 0)
        {
            if (x < 0 || x >= height || y < 0 || y >= width)
            {
                return;
            }
            if (mouse == 0)
            {
                if (playState == PlayState.Start)
                {
                    CreateRightState(x, y);

                    playState = PlayState.Playing;
                    Action(x, y);
                }
                else if (playState == PlayState.Playing)
                {
                    if (curState[x][y] == MS_blank || curState[x][y] == MS_bombquestion)
                    {
                        if (rightState[x][y] == MS_bombrevealed)
                        {
                            curState[x][y] = MS_bombdeath;
                            for (int i = 0; i < height; i++)
                            {
                                for (int j = 0; j < width; j++)
                                {
                                    if (curState[i][j] == MS_blank && rightState[i][j] == MS_bombrevealed)
                                        curState[i][j] = MS_bombrevealed;
                                    else if (curState[i][j] == MS_bombflagged)
                                    {
                                        if (rightState[i][j] != MS_bombrevealed)
                                        {
                                            curState[i][j] = MS_bombmisflagged;
                                        }
                                    }
                                }
                            }
                            playState = PlayState.Lose;
                        }
                        else
                        {
                            curState[x][y] = rightState[x][y];
                            totalBlank--;
                            totalNonBoom++;
                            blankIndexList.Remove(x * width + height);

                            if (totalNonBoom == height * width - booms)
                            {
                                playState = PlayState.Win;
                                for (int i = 0; i < height; i++)
                                {
                                    for (int j = 0; j < width; j++)
                                    {
                                        if (curState[i][j] == MS_blank|| curState[i][j] == MS_bombquestion)
                                        {
                                            curState[i][j] = MS_bombflagged;
                                        }
                                    }
                                }
                                return;
                            }

                            if (rightState[x][y] == MS_open0)
                            {
                                ClickAround(x, y);
                            }
                        }
                    }
                    else if (curState[x][y] == MS_open0)
                    {
                        ClickAround(x, y);
                    }
                    else if (curState[x][y] >= MS_open1 && curState[x][y] <= MS_open8 && curState[x][y] - MS_open0 == CountBoomAround(x, y, curState, MS_bombflagged))
                    {
                        ClickAround(x, y);
                    }
                }
            }
            else
            {
                if (curState[x][y] == MS_blank)
                {
                    curState[x][y] = MS_bombflagged;
                    totalFlag++;
                    totalBlank--;
                }
                else if (curState[x][y] == MS_bombflagged)
                {
                    curState[x][y] = MS_bombquestion;
                    totalFlag--;
                    totalBlank++;
                }
                else if (curState[x][y] == MS_bombquestion)
                {
                    curState[x][y] = MS_blank;
                }
            }
        }

        private void ClickAround(int x, int y)
        {
            if (x > 0 && y > 0 && (curState[x - 1][y - 1] == MS_blank || curState[x - 1][y - 1] == MS_bombquestion))
                Action(x - 1, y - 1);
            if (x > 0 && (curState[x - 1][y] == MS_blank || curState[x - 1][y] == MS_bombquestion))
                Action(x - 1, y);
            if (x > 0 && y < width - 1 && (curState[x - 1][y + 1] == MS_blank || curState[x - 1][y + 1] == MS_bombquestion))
                Action(x - 1, y + 1);
            if (y > 0 && (curState[x][y - 1] == MS_blank || curState[x][y - 1] == MS_bombquestion))
                Action(x, y - 1);
            if (y < width - 1 && (curState[x][y + 1] == MS_blank || curState[x][y + 1] == MS_bombquestion))
                Action(x, y + 1);
            if (x < height - 1 && y > 0 && (curState[x + 1][y - 1] == MS_blank || curState[x + 1][y - 1] == MS_bombquestion))
                Action(x + 1, y - 1);
            if (x < height - 1 && (curState[x + 1][y] == MS_blank || curState[x + 1][y] == MS_bombquestion))
                Action(x + 1, y);
            if (x < height - 1 && y < width - 1 && (curState[x + 1][y + 1] == MS_blank || curState[x + 1][y + 1] == MS_bombquestion))
                Action(x + 1, y + 1);
        }

        private void CreateRightState(int x, int y)
        {
            // choice boom position
            List<int> hashIndexs = new List<int>();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (i < x - 1 || i > x + 1 || j < y - 1 || j > y + 1)
                    {
                        hashIndexs.Add(i * width + j);
                    }
                }
            }

            int pPlacedBoom = 0;
            Random rd = new Random();
            while (pPlacedBoom < booms)
            {
                int hashIndex = rd.Next(hashIndexs.Count);
                rightState[hashIndexs[hashIndex] / width][hashIndexs[hashIndex] % width] = MS_bombrevealed;
                pPlacedBoom++;
                hashIndexs.RemoveAt(hashIndex);
            }

            // Update number of booms around
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (rightState[i][j] != MS_bombrevealed)
                    {
                        rightState[i][j] = MS_open0 + CountBoomAround(i, j, rightState, MS_bombrevealed);
                    }
                }
            }
        }

        public int CountBoomAround(int i, int j, int[][] state, int checkWhat)
        {
            int count = 0;
            if (i == 0 && j == 0)
            {
                if (state[i + 1][j] == checkWhat) count++;
                if (state[i][j + 1] == checkWhat) count++;
                if (state[i + 1][j + 1] == checkWhat) count++;
            }
            else if (i == height - 1 && j == 0)
            {
                if (state[i - 1][j] == checkWhat) count++;
                if (state[i][j + 1] == checkWhat) count++;
                if (state[i - 1][j + 1] == checkWhat) count++;
            }
            else if (i == height - 1 && j == width - 1)
            {
                if (state[i - 1][j] == checkWhat) count++;
                if (state[i][j - 1] == checkWhat) count++;
                if (state[i - 1][j - 1] == checkWhat) count++;
            }
            else if (i == 0 && j == width - 1)
            {
                if (state[i + 1][j] == checkWhat) count++;
                if (state[i][j - 1] == checkWhat) count++;
                if (state[i + 1][j - 1] == checkWhat) count++;
            }
            else if (i == 0)
            {
                if (state[i][j - 1] == checkWhat) count++;
                if (state[i][j + 1] == checkWhat) count++;
                if (state[i + 1][j - 1] == checkWhat) count++;
                if (state[i + 1][j] == checkWhat) count++;
                if (state[i + 1][j + 1] == checkWhat) count++;
            }
            else if (j == 0)
            {
                if (state[i - 1][j] == checkWhat) count++;
                if (state[i + 1][j] == checkWhat) count++;
                if (state[i - 1][j + 1] == checkWhat) count++;
                if (state[i][j + 1] == checkWhat) count++;
                if (state[i + 1][j + 1] == checkWhat) count++;
            }
            else if (i == height - 1)
            {
                if (state[i][j - 1] == checkWhat) count++;
                if (state[i][j + 1] == checkWhat) count++;
                if (state[i - 1][j - 1] == checkWhat) count++;
                if (state[i - 1][j] == checkWhat) count++;
                if (state[i - 1][j + 1] == checkWhat) count++;
            }
            else if (j == width - 1)
            {
                if (state[i - 1][j] == checkWhat) count++;
                if (state[i + 1][j] == checkWhat) count++;
                if (state[i - 1][j - 1] == checkWhat) count++;
                if (state[i][j - 1] == checkWhat) count++;
                if (state[i + 1][j - 1] == checkWhat) count++;
            }
            else
            {
                if (state[i][j - 1] == checkWhat) count++;
                if (state[i][j + 1] == checkWhat) count++;
                if (state[i - 1][j - 1] == checkWhat) count++;
                if (state[i - 1][j] == checkWhat) count++;
                if (state[i - 1][j + 1] == checkWhat) count++;
                if (state[i + 1][j - 1] == checkWhat) count++;
                if (state[i + 1][j] == checkWhat) count++;
                if (state[i + 1][j + 1] == checkWhat) count++;
            }

            return count;
        }
    }
}
