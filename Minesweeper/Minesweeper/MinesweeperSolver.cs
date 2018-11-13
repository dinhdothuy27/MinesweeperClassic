using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Minesweeper
{
    class MinesweeperSolver
    {
        const int NumOfStep = 9;

        double xTL = 0;
        double yTL = 0;
        double xRB = 0;
        double yRB = 0;

        int height;
        int width;
        int booms;

        int[][] featureForm;

        Bitmap[][] curBitmap;
        int[][] curState;

        internal MinesweeperSolver(Bitmap[] bmpList)
        {
            featureForm = bmpList.Select(x => GetFeature(x)).ToArray();
        }

        public int[][] GetMRState()
        {
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;
            Bitmap bmp = new Bitmap((int)(xRB - xTL), (int)(yRB - yTL));
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen((int)xTL, (int)yTL, 0, 0, bmp.Size);

            int wSquare = bmp.Width / width;
            int hSquare = bmp.Height / height;

            if (curBitmap == null || curBitmap.Length != height || curBitmap[0].Length != width)
            {
                curBitmap = new Bitmap[height][];
                for (int i = 0; i < height; i++)
                {
                    curBitmap[i] = new Bitmap[width];
                }
            }

            Bitmap[][] nowBitmap = new Bitmap[height][];
            for (int i = 0; i < height; i++)
            {
                nowBitmap[i] = new Bitmap[width];
                for (int j = 0; j < width; j++)
                {
                    nowBitmap[i][j] = bmp.Clone(new Rectangle(j * wSquare, i * hSquare, wSquare, hSquare), System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                }
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (!CheckSameBmp(curBitmap[i][j], nowBitmap[i][j]))
                    {
                        curBitmap[i][j] = nowBitmap[i][j];
                        int[] fet = GetFeature(curBitmap[i][j]);
                        double min = CompareFeature(fet, featureForm[0]);
                        int state = 0;
                        for (int k = 1; k < featureForm.Length; k++)
                        {
                            double m = CompareFeature(fet, featureForm[k]);
                            if (m < min)
                            {
                                min = m;
                                state = k;
                            }
                        }
                        curState[i][j] = state;
                    }
                }
            }

            return curState;
        }

        int[] GetFeature(Bitmap bmp)
        {
            List<int> result = new List<int>();

            double xStep = bmp.Width * 1.0 / (NumOfStep + 1);
            double yStep = bmp.Height * 1.0 / (NumOfStep + 1);
            for (int i = 0; i < NumOfStep; i++)
            {
                for (int j = 0; j < NumOfStep; j++)
                {
                    int x = (int)(xStep * (i + 1));
                    int y = (int)(yStep * (j + 1));
                    var pi = bmp.GetPixel(x, y);
                    result.Add(pi.R);
                    result.Add(pi.G);
                    result.Add(pi.B);
                }
            }

            return result.ToArray();
        }

        double CompareFeature(int[] left, int[] right)
        {
            double result = 0;
            for (int i = 0; i < left.Length; i++)
            {
                result += (left[i] - right[i]) * (left[i] - right[i]);
            }
            return Math.Sqrt(result);
        }

        bool CheckSameBmp(Bitmap left, Bitmap right)
        {
            if (left == null || right == null || left.Width != right.Width || left.Height != right.Height)
            {
                return false;
            }

            int w = left.Width;
            int h = left.Height;
            Random rd = new Random();

            for (int i = 0; i < 10; i++)
            {
                int x = rd.Next(w);
                int y = rd.Next(h);
                if (left.GetPixel(x, y) != right.GetPixel(x, y))
                {
                    return false;
                }
            }

            return true;
        }

        internal void UpdateShapePostion(double _xTL, double _yTL, double _xRB, double _yRB)
        {
            xTL = _xTL;
            yTL = _yTL;
            xRB = _xRB;
            yRB = _yRB;
        }

        internal void UpdateSizeAndBoom(int _height, int _width, int _booms)
        {
            height = _height;
            width = _width;
            booms = _booms;
            curState = new int[height][];
            for (int i = 0; i < height; i++)
            {
                curState[i] = new int[width];
            }
        }
    }
}
