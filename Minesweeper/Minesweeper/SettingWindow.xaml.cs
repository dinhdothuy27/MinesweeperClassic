using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private int rdbCheck = 0;
        public int[] Information; // index 0: Height, index 1: Width, index 2: Number of Booms, index 3: rdbCheck

        public SettingWindow()
        {
            InitializeComponent();
            Information = new int[4];
            UpdateControlInfor();
        }

        public SettingWindow(int[] _Information)
        {
            InitializeComponent();
            Information = _Information;
            UpdateControlInfor();
        }

        private void UpdateControlInfor()
        {
            tbxBooms.Text = Information[2].ToString();
            tbxHeight.Text = Information[0].ToString();
            tbxWidth.Text = Information[1].ToString();
            
            rdbCheck = Information[3];

            if (rdbCheck == 1)
            {
                rdbMedium.IsChecked = true;
            }
            else if (rdbCheck == 2)
            {
                rdbHard.IsChecked = true;
            }
            else if (rdbCheck == 3)
            {
                rdbCustom.IsChecked = true;
            }
            else
            {
                rdbEazy.IsChecked = true;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            int w = 1;
            int h = 1;
            int boom = 1;
            int.TryParse(tbxWidth.Text, out w);
            int.TryParse(tbxHeight.Text, out h);
            int.TryParse(tbxBooms.Text, out boom);
            if (w < 5) w = 5;
            if (w > DrawingMinesweeperEnv.maxWidth) w = DrawingMinesweeperEnv.maxWidth;
            if (h < 5) h = 5;
            if (h > DrawingMinesweeperEnv.maxHeight) h = DrawingMinesweeperEnv.maxHeight;
            if (boom < 1) boom = 1;
            if (boom > w * h / 3) boom = w * h / 3;
            Information = new int[] { h, w, boom, rdbCheck };

            DialogResult = true;
            Hide();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Hide();
        }

        private void rdbEazy_Checked(object sender, RoutedEventArgs e)
        {
            rdbCheck = 0;
            DisableTextBlock();
            tbxBooms.Text = "10";
            tbxHeight.Text = "8";
            tbxWidth.Text = "8";
        }

        private void rdbMedium_Checked(object sender, RoutedEventArgs e)
        {
            rdbCheck = 1;
            DisableTextBlock();
            tbxBooms.Text = "40";
            tbxHeight.Text = "16";
            tbxWidth.Text = "16";
        }

        private void rdbHard_Checked(object sender, RoutedEventArgs e)
        {
            rdbCheck = 2;
            DisableTextBlock();
            tbxBooms.Text = "99";
            tbxHeight.Text = "16";
            tbxWidth.Text = "31";
        }

        private void rdbCustom_Checked(object sender, RoutedEventArgs e)
        {
            rdbCheck = 3;
            EnableTextBlock();
        }

        private void DisableTextBlock()
        {
            tbxBooms.IsReadOnly = true;
            tbxHeight.IsReadOnly = true;
            tbxWidth.IsReadOnly = true;
        }

        private void EnableTextBlock()
        {
            tbxBooms.IsReadOnly = false;
            tbxHeight.IsReadOnly = false;
            tbxWidth.IsReadOnly = false;
        }
    }
}
