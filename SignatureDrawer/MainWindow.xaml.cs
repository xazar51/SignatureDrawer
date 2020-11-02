using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SignatureDrawer.Classes;
using Path = System.IO.Path;

// ReSharper disable EmptyGeneralCatchClause

namespace SignatureDrawer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //disable events while loading
            UserComboBox.SelectionChanged -= UserComboBox_SelectionChanged;
            SignatureListBox.SelectionChanged -= SignatureListBox_SelectionChanged;

            SignatureRepo.Load();
            
            //re-enable events
            UserComboBox.SelectionChanged += UserComboBox_SelectionChanged;
            SignatureListBox.SelectionChanged += SignatureListBox_SelectionChanged;

            UserComboBox.ItemsSource = SignatureRepo.Users;
            UserComboBox.SelectedIndex = 0;
        }




        private DistortableSignature _currentSignature;

        private void ReDraw()
        {
            
            if (SignatureListBox.SelectedItem == null) return;

            //Title = _currentSignature.FileName + "  " + _currentSignature.Header;

            try
            {
                var itemName = SignatureListBox.SelectedItem.ToString();
                SignatureNameTextBox.Text = "User ID: " + _currentSignature.UserID + "\n" + "Signature ID: " + _currentSignature.SignatureID;

                var drawer = new Drawer(_currentSignature, SignatureCanvas);
                drawer.Draw(CbDistort.IsChecked != null && CbDistort.IsChecked.Value);
            }
            catch{}

        }


        private void SignatureListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentSignature = SignatureRepo.Signatures.Find(s => s.UserID == UserComboBox.SelectedItem.ToString() && s.SignatureID == SignatureListBox.SelectedItem.ToString());
            _currentSignature.Reset();
            ReDraw();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReDraw();
        }

        private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            SignatureListBox.ItemsSource = SignatureRepo.UserSignatureIds(UserComboBox.SelectedItem.ToString());
            SignatureListBox.SelectedIndex = 0;
            SignatureListBox_SelectionChanged(null, null);

        }

        private void CbDistort_OnClick(object sender, RoutedEventArgs e)
        {
            ReDraw();
        }

        private void BtnCenters_OnClick(object sender, RoutedEventArgs e)
        {
            
            _currentSignature?.GenerateGravityCenters();
            ReDraw();
        }



        private void RbGravity_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RbGravity?.IsChecked == true) DistortableSignature.DistortMode = DistortMode.Gravity;
                if (RbRandom?.IsChecked == true) DistortableSignature.DistortMode = DistortMode.Randomize;
            }
            catch 
            {
            }
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                _currentSignature.Reset();
                _currentSignature.SaveDistorted(i);
            }

            MessageBox.Show("10 '" + DistortableSignature.DistortMode + "' distorted files have been generated for " + _currentSignature.FileName + "\n\n Restart the app to load them");

        }

        private void BtnSaveAll_OnClick(object sender, RoutedEventArgs e)
        {

            if (!"This will generate 10 distortion variants for every signature in the working directory. The procedure takes about a minute but the app may not be able to load all these new files when it is restarted. Use with caution. Wait until 'Done' messagebox"
                .Confirmed()) return;

            foreach (var signature in SignatureRepo.Signatures)
            {
                for (int i = 0; i < 10; i++)
                {
                    signature.Reset();
                    signature.SaveDistorted(i);
                }
            }
            MessageBox.Show("Done");
        }

        private void CbSingleCenter_OnClick(object sender, RoutedEventArgs e)
        {
            if (CbSingleCenter.IsChecked == true)
            {
                DistortableSignature.RandomizeMassDensity = false;
                DistortableSignature.MassDensity = 1;
            }
            else
            {
                DistortableSignature.RandomizeMassDensity = true;
                DistortableSignature.MassDensity = DistortableSignature.DefaultMassDensity;
            }
            _currentSignature?.Reset();
            ReDraw();
        }

        int _blackholeTimerTicks = 0;
        private bool blackholeRunning = false;

        private void BtnBlackHole_OnClick(object sender, RoutedEventArgs e)
        {

            if (blackholeRunning)
            {
                _blackholeTimerTicks = 9999999;
                return;
            }

            if (DistortableSignature.DistortMode != DistortMode.Gravity)
            {
                MessageBox.Show("Not in gravity mode!");
                return;
            }

            if (_currentSignature.GravityCenters.Count<1)
            {
                MessageBox.Show("No gravity centers");
                return;
            }

            BtnBlackHole.Content = "Stop";

            var dt = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 20) };
            _blackholeTimerTicks = 0;
            blackholeRunning = true;

            double max = 500;

            dt.Tick += (ss, ee) =>
            {
                _blackholeTimerTicks++;
                Title = (100 * _blackholeTimerTicks / max).ToString("F1") + "%...";
                if (_currentSignature.GravityCenters.Count > 1) _currentSignature.GravityCenters.Remove(_currentSignature.GravityCenters[1]);
                _currentSignature.GravityCenters[0].Mass += 1000;
                ReDraw();
                if (_blackholeTimerTicks >= max)
                {
                    ((DispatcherTimer)ss).Stop();
                    blackholeRunning = false;
                    BtnBlackHole.Content = "Swallow by black hole";
                    Title = "";
                    _currentSignature?.Reset();
                    ReDraw();
                }
            };
            dt.Start();
        }
    }
}
