using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace Minesweeper
{
    public class MinesweeperAI
    {
        // Map State by AI view
        public const int AI_open0 = 0;
        public const int AI_open1 = 1;
        public const int AI_open2 = 2;
        public const int AI_open3 = 3;
        public const int AI_open4 = 4;
        public const int AI_open5 = 5;
        public const int AI_open6 = 6;
        public const int AI_open7 = 7;
        public const int AI_open8 = 8;
        public const int AI_blank = 9;
        public const int AI_wall = 10;

        // direction
        public const int LEFT = 0;
        public const int RIGHT = 1;
        public const int UP = 2;
        public const int DOWN = 3;

        public struct Action
        {
            public int x;
            public int y;
            public int mouse;
        }

        public struct Position
        {
            public Position(int _x, int _y)
            {
                x = _x;
                y = _y;
            }

            public int x;
            public int y;
        }

        int height = 0;
        int width = 0;
        int booms = 0;

        public MinesweeperAI()
        {

        }

        public void Trainning(int times)
        {
            MinesweeperRule mr = new MinesweeperRule();
            int winCount = 0;
            for (int count = 0; count < times; count++)
            {
                height = 8;
                width = 8;
                booms = 10;
                mr.CreateNewGame(height, width, booms);
                int c = 0;
                List<Action> acts = new List<Action>();

                while (c < height * width)
                {
                    c++;
                    int remainBoom = booms - mr.totalFlag;
                    Action act = GetActionFromRule(mr);
                    mr.Action(act.x, act.y, act.mouse);
                    acts.Add(act);

                    if (mr.playState == MinesweeperRule.PlayState.Win || mr.playState == MinesweeperRule.PlayState.Lose)
                    {
                        break;
                    }
                }

                var rightState = mr.GetRightState();
                for (int i = 0; i < acts.Count; i++)
                {
                }
                if (mr.playState == MinesweeperRule.PlayState.Win)
                {
                    winCount++;
                }

                if (count % 500 == 499)
                    Console.WriteLine(string.Format("Win {0}/{1} games", winCount, count + 1));
            }

            Console.WriteLine(string.Format("Win {0}/{1} games", winCount, times));
        }

        public Action GetActionFromRule(MinesweeperRule mr)
        {
            height = mr.height;
            width = mr.width;
            booms = mr.booms;
            int[][] curState = mr.GetCurrentState();
            int[][] aiView = new int[height][];
            List<Position> blankList = new List<Position>();
            List<Position> numberList = new List<Position>();
            List<Position> blankBoundary = new List<Position>();
            List<Position> unknownBlank = new List<Position>();
            Dictionary<Position, List<Position>> numberNearBlank = new Dictionary<Position, List<Position>>();
            Dictionary<Position, List<Position>> blankNearNumber = new Dictionary<Position, List<Position>>();
            int remainBoom = mr.booms - mr.totalFlag;
            Random rd = new Random();

            for (int i = 0; i < height; i++)
            {
                aiView[i] = new int[width];
                for (int j = 0; j < width; j++)
                {
                    if (curState[i][j] == MinesweeperRule.MS_blank || curState[i][j] == MinesweeperRule.MS_bombquestion)
                    {
                        aiView[i][j] = AI_blank;
                        blankList.Add(new Position(i, j));
                    }
                    else if (curState[i][j] >= MinesweeperRule.MS_open0 && curState[i][j] <= MinesweeperRule.MS_open8)
                    {
                        numberList.Add(new Position(i, j));
                        int boomAround = curState[i][j] - MinesweeperRule.MS_open0 - mr.CountBoomAround(i, j, curState, MinesweeperRule.MS_bombflagged);
                        if (boomAround > 0)
                            aiView[i][j] = boomAround;
                        else
                            aiView[i][j] = AI_open0;
                    }
                    else
                    {
                        aiView[i][j] = AI_wall;
                    }
                }
            }

            if (blankList.Count == height * width)
            {
                return new Action { x = rd.Next(height), y = rd.Next(width), mouse = 0 };
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (aiView[i][j] == AI_blank)
                    {
                        Position curPos = new Position(i, j);
                        List<Position> nearNumbers = new List<Position>();

                        Position pos;
                        if (numberList.Contains(pos = new Position(i - 1, j - 1)))
                        {
                            nearNumbers.Add(pos);
                        }
                        if (numberList.Contains(pos = new Position(i - 1, j)))
                        {
                            nearNumbers.Add(pos);
                        }
                        if (numberList.Contains(pos = new Position(i - 1, j + 1)))
                        {
                            nearNumbers.Add(pos);
                        }
                        if (numberList.Contains(pos = new Position(i, j - 1)))
                        {
                            nearNumbers.Add(pos);
                        }
                        if (numberList.Contains(pos = new Position(i, j + 1)))
                        {
                            nearNumbers.Add(pos);
                        }
                        if (numberList.Contains(pos = new Position(i + 1, j - 1)))
                        {
                            nearNumbers.Add(pos);
                        }
                        if (numberList.Contains(pos = new Position(i + 1, j)))
                        {
                            nearNumbers.Add(pos);
                        }
                        if (numberList.Contains(pos = new Position(i + 1, j + 1)))
                        {
                            nearNumbers.Add(pos);
                        }

                        if (nearNumbers.Count > 0)
                        {
                            blankBoundary.Add(curPos);
                            numberNearBlank[curPos] = nearNumbers;
                            foreach (Position num in nearNumbers)
                            {
                                if (!blankNearNumber.ContainsKey(num))
                                {
                                    List<Position> nearBlanks = new List<Position>();
                                    if (blankList.Contains(pos = new Position(num.x - 1, num.y - 1)))
                                    {
                                        nearBlanks.Add(pos);
                                    }
                                    if (blankList.Contains(pos = new Position(num.x - 1, num.y)))
                                    {
                                        nearBlanks.Add(pos);
                                    }
                                    if (blankList.Contains(pos = new Position(num.x - 1, num.y + 1)))
                                    {
                                        nearBlanks.Add(pos);
                                    }
                                    if (blankList.Contains(pos = new Position(num.x, num.y - 1)))
                                    {
                                        nearBlanks.Add(pos);
                                    }
                                    if (blankList.Contains(pos = new Position(num.x, num.y + 1)))
                                    {
                                        nearBlanks.Add(pos);
                                    }
                                    if (blankList.Contains(pos = new Position(num.x + 1, num.y - 1)))
                                    {
                                        nearBlanks.Add(pos);
                                    }
                                    if (blankList.Contains(pos = new Position(num.x + 1, num.y)))
                                    {
                                        nearBlanks.Add(pos);
                                    }
                                    if (blankList.Contains(pos = new Position(num.x + 1, num.y + 1)))
                                    {
                                        nearBlanks.Add(pos);
                                    }

                                    blankNearNumber[num] = nearBlanks;
                                }
                            }
                        }
                        else
                        {
                            unknownBlank.Add(new Position(i, j));
                        }
                    }
                }
            }

            foreach (var pos in blankBoundary)
            {
                foreach (var num in numberNearBlank[pos])
                {
                    if (blankNearNumber[num].Count == aiView[num.x][num.y])
                    {
                        return new Action { x = pos.x, y = pos.y, mouse = 1 }; // right click to pos
                    }
                    else if (AI_open0 == aiView[num.x][num.y])
                    {
                        return new Action { x = pos.x, y = pos.y, mouse = 0 }; // left click to pos
                    }
                }
            }



            return new Action { x = 0, y = 0, mouse = -1 };
        }

        private string GetState(int x, int y, int[][] aiView, int remainBoom)
        {
            int[][] subView = new int[5][]; // subView is 5x5 int array array
            for (int i = 0; i < 5; i++)
            {
                subView[i] = new int[5];
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    int _x = x - 2 + i;
                    int _y = y + 2 - j;
                    if (_x < 0 || _x >= height || _y < 0 || _y >= width)
                    {
                        subView[i][j] = AI_wall;
                    }
                    else
                    {
                        subView[i][j] = aiView[_x][_y];
                    }
                }
            }

            int blankCount = 0;
            List<byte> hashByte = new List<byte>();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (subView[i][j] == AI_blank)
                    {
                        blankCount++;
                    }
                    hashByte.Add((byte)subView[i][j]);
                }
            }

            if (remainBoom > blankCount) remainBoom = blankCount;
            hashByte.Add((byte)(remainBoom));
            return Encoding.Default.GetString(hashByte.ToArray());
        }
    }
}
