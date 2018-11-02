using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;

namespace Minesweeper
{
    public enum ImageType
    {
        Normal = 0,
        MultiChoices,
    }

    class MultiChoicesImageControlObject : ImageControlObject
    {
        internal MultiChoicesImageControlObject(BitmapImage[] bmpList, Canvas cnv)
        {
            canvas = cnv;
            imgType = ImageType.MultiChoices;
            mainImage = new Image();
            BitmapList = bmpList;
            if (BitmapList.Length > 0)
                mainImage.Source = BitmapList[0];
            cnv.Children.Add(mainImage);
        }

        public override void ChoiceImage(int index)
        {
            if (BitmapList.Length > index && index >= 0)
            {
                mainImage.Source = BitmapList[index];
                Index = index;
            }
        }
    }

    class NormalImageControlObject : ImageControlObject
    {
        internal NormalImageControlObject(BitmapImage bmp, Canvas cnv)
        {
            canvas = cnv;
            imgType = ImageType.Normal;
            mainImage = new Image();
            mainImage.Source = bmp;
            cnv.Children.Add(mainImage);
        }

        public override void ChoiceImage(int index)
        {
            return;
        }
    }

    abstract class ImageControlObject
    {
        public static ImageControlObject CreateImage(BitmapImage bmp, Canvas cnv)
        {
            return new NormalImageControlObject(bmp, cnv);
        }

        public static ImageControlObject CreateImage(BitmapImage[] bmpList, Canvas cnv)
        {
            return new MultiChoicesImageControlObject(bmpList, cnv);
        }

        internal Image mainImage;
        internal BitmapImage[] BitmapList;
        internal ImageType imgType;
        internal Canvas canvas;
        public int Index = 0;

        public ImageControlObject()
        {
            mainImage = new Image();
            imgType = ImageType.Normal;
        }

        abstract public void ChoiceImage(int index);

        public void SetPosition(int x, int y)
        {
            Canvas.SetLeft(mainImage, x);
            Canvas.SetTop(mainImage, y);
        }

        public void RemoveFromCanvas()
        {
            if (canvas != null && canvas.Children.Contains(mainImage))
            {
                canvas.Children.Remove(mainImage);
            }
        }
        public void AddToCanvas()
        {
            if (canvas != null && !canvas.Children.Contains(mainImage))
            {
                canvas.Children.Add(mainImage);
            }
        }

    }
}
