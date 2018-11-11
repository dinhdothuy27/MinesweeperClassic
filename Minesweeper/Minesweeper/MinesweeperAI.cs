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

            if (blankList.Count == 0)
            {
                return new Action { x = 0, y = 0, mouse = -1 };
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

            Dictionary<Position, double> estimations = blankBoundary.ToDictionary(x => x, x => 0.0);
            List<List<Position>> groupPosList = new List<List<Position>>();
            List<Position> posList = new List<Position>();
            Queue<Position> groupPosQue = new Queue<Position>();
            if (blankBoundary.Count > 0)
                groupPosQue.Enqueue(blankBoundary.First());
            while (blankBoundary.Count > 0 && groupPosQue.Count > 0)
            {
                var pos = groupPosQue.Dequeue();
                posList.Add(pos);
                blankBoundary.Remove(pos);
                foreach (var num in numberNearBlank[pos])
                {
                    foreach (var blank in blankNearNumber[num])
                    {
                        if (blankBoundary.Contains(blank) && !groupPosQue.Contains(blank))
                        {
                            groupPosQue.Enqueue(blank);
                        }
                    }
                }

                if (groupPosQue.Count == 0)
                {
                    groupPosList.Add(posList);
                    posList = new List<Position>();
                    if (blankBoundary.Count > 0)
                    {
                        groupPosQue.Enqueue(blankBoundary.First());
                    }
                }
            }

            double placedFlag = 0;
            double alpha = 0.1;
            double epsilon = 0.05;
            double addition = 0;
            double max = -1000;
            double est = 0;
            double gama = 0.9;

            foreach (var groupPos in groupPosList)
            {
                List<Position> numList = new List<Position>();
                foreach (var pos in groupPos)
                {
                    foreach (var num in numberNearBlank[pos])
                    {
                        if (!numList.Contains(num))
                        {
                            numList.Add(num);
                        }
                    }
                }

                List<double> placedFlagList = new List<double>();

                int enchos = groupPos.Count * 20;
                for (int count = 0; count < enchos; count++)
                {
                    if (count < enchos * 3 / 4)
                    {
                        epsilon = 0.05;
                    }
                    else
                    {
                        epsilon = 0;
                    }

                    List<Position> tempGroupPos = groupPos.ToList();
                    Dictionary<Position, int> actions = groupPos.ToDictionary(x => x, x => 0); // 1: place flag, -1: don't place flag, 0: not yet action
                    List<Position> positiveAction = new List<Position>();
                    Dictionary<Position, int> numOfBlankAround = blankNearNumber.ToDictionary(x => x.Key, x => x.Value.Count);
                    int[][] tempAiView = aiView.Select(x => x.ToArray()).ToArray();
                    bool isFirst = true;

                    while (true)
                    {
                        bool checkWrong = false;
                        for (int i = 0; i < numList.Count; i++)
                        {
                            if (tempAiView[numList[i].x][numList[i].y] < 0 || tempAiView[numList[i].x][numList[i].y] > numOfBlankAround[numList[i]])
                            {
                                checkWrong = true;
                                break;
                            }
                        }
                        if (checkWrong)
                        {
                            gama = 1;
                            // update estimatios when wrong
                            for (int i = 0; i < positiveAction.Count; i++)
                            {
                                var pos = positiveAction[i];
                                estimations[pos] += alpha * gama * ((-1) * actions[pos] - estimations[pos]);
                                gama *= 0.8;
                            }

                            break;
                        }
                        if (!actions.Values.Contains(0))
                        {
                            // update estimatios when success
                            int hs = 1;
                            if (estimations.Values.Where(x => x == 1).Count() > remainBoom)
                            {
                                hs = -1;
                            }

                            gama = 1;
                            for (int i = 0; i < positiveAction.Count; i++)
                            {
                                var pos = positiveAction[i];
                                estimations[pos] += alpha * gama * (hs * actions[pos] - estimations[pos]);
                                gama *= 0.8;
                            }

                            placedFlagList.Add(actions.Values.Where(x => x == 1).Count());

                            break;
                        }

                        List<Position> needNoFlagList = new List<Position>();
                        foreach (var num in numList)
                        {
                            if (tempAiView[num.x][num.y] == 0)
                            {
                                foreach (var blank in blankNearNumber[num])
                                {
                                    if (actions[blank] == 0 && !needNoFlagList.Contains(blank))
                                    {
                                        needNoFlagList.Add(blank);
                                    }
                                }
                            }
                        }

                        if (needNoFlagList.Count > 0)
                        {
                            foreach (var blank in needNoFlagList)
                            {
                                actions[blank] = -1;
                                tempGroupPos.Remove(blank);
                                foreach (var num in numberNearBlank[blank])
                                {
                                    numOfBlankAround[num]--;
                                }
                            }
                            continue;
                        }

                        List<Position> needFlagList = new List<Position>();
                        foreach (var num in numList)
                        {
                            if (tempAiView[num.x][num.y] == numOfBlankAround[num])
                            {
                                foreach (var blank in blankNearNumber[num])
                                {
                                    if (actions[blank] == 0 && !needFlagList.Contains(blank))
                                    {
                                        needFlagList.Add(blank);
                                    }
                                }
                            }
                        }

                        if (needFlagList.Count > 0)
                        {
                            foreach (var blank in needFlagList)
                            {
                                actions[blank] = 1;
                                tempGroupPos.Remove(blank);
                                foreach (var num in numberNearBlank[blank])
                                {
                                    numOfBlankAround[num]--;
                                    tempAiView[num.x][num.y]--;
                                }
                            }
                            continue;
                        }

                        if (isFirst)
                        {
                            Position pos = groupPos[rd.Next(groupPos.Count)];
                            int act = rd.Next(2) * 2 - 1;
                            actions[pos] = act;
                            tempGroupPos.Remove(pos);
                            positiveAction.Add(pos);
                            foreach (var num in numberNearBlank[pos])
                            {
                                numOfBlankAround[num]--;
                                if (act == 1)
                                    tempAiView[num.x][num.y]--;
                            }
                            isFirst = false;
                        }
                        else
                        {
                            if (rd.NextDouble() < epsilon)
                            {
                                Position pos = tempGroupPos[rd.Next(tempGroupPos.Count)];
                                int act = rd.Next(2) * 2 - 1;
                                actions[pos] = act;
                                tempGroupPos.Remove(pos);
                                positiveAction.Add(pos);
                                foreach (var num in numberNearBlank[pos])
                                {
                                    numOfBlankAround[num]--;
                                    if (act == 1)
                                        tempAiView[num.x][num.y]--;
                                }
                            }
                            else
                            {
                                List<Position> bestChoiceList = new List<Position>();
                                max = -1000;
                                foreach (var pos in tempGroupPos)
                                {
                                    est = estimations[pos] < 0 ? -estimations[pos] : estimations[pos];
                                    if (est > max)
                                    {
                                        max = est;
                                        bestChoiceList = new List<Position>();
                                        bestChoiceList.Add(pos);
                                    }
                                    else if (est == max)
                                    {
                                        bestChoiceList.Add(pos);
                                    }
                                }

                                Position bestChoice = bestChoiceList[rd.Next(bestChoiceList.Count)];
                                int act = estimations[bestChoice] >= 0 ? 1 : -1;
                                actions[bestChoice] = act;
                                tempGroupPos.Remove(bestChoice);
                                positiveAction.Add(bestChoice);
                                foreach (var num in numberNearBlank[bestChoice])
                                {
                                    numOfBlankAround[num]--;
                                    if (act == 1)
                                        tempAiView[num.x][num.y]--;
                                }
                            }
                        }
                    }
                }

                if (placedFlagList.Count > 0)
                {
                    placedFlag += placedFlagList.Sum() * 1.0 / placedFlagList.Count;
                }
                else
                {
                    addition += groupPos.Count;
                }
            }

            List<Position> bestPosList = new List<Position>();
            max = -1000;
            foreach (var pos in estimations.Keys)
            {
                est = estimations[pos] < 0 ? -estimations[pos] : estimations[pos];
                if (est > max)
                {
                    max = est;
                    bestPosList = new List<Position>();
                    bestPosList.Add(pos);
                }
                else if (est == max)
                {
                    bestPosList.Add(pos);
                }
            }

            Position bestPos = new Position(0, 0);
            if (bestPosList.Count > 0)
            {
                bestPos = bestPosList[rd.Next(bestPosList.Count)];
            }

            est = 0;
            if (unknownBlank.Count > 0)
            {
                est = (remainBoom - placedFlag) * 1.0 / (unknownBlank.Count + addition) * 2 - 1;
                foreach (var blk in unknownBlank)
                {
                    estimations[blk] = est;
                }
            }

            if (max < 0.9 && (est > max || -est > max))
            {
                bestPos = unknownBlank[rd.Next(unknownBlank.Count)];
            }

            return new Action { x = bestPos.x, y = bestPos.y, mouse = estimations[bestPos] >= 0 ? 1 : 0 };
        }
    }
}
