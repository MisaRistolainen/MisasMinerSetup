using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MisasMinerSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private WebClient webClient = null;
        private int gpuChoice = 2;
        public string visi { get; set; }    //Visibility on "Miner not found"
        public string pool { get; set; }    //Custom pool
        public string wallet { get; set; }  //Wallet address
        private int _i;
        public int i { get { return _i; } set { _i = value; inCalc.Text = value.ToString(); } } //Intensity
        public int n { get; set; }          //nFactor
        public bool fileCheck { get; set; } //Boolean for checking if sgminer.exe/ccminer.exe exists
        public string appPath;              // Current application path
        private string _selectedPool;
        public string selectedPool { get { return _selectedPool; } set   { _selectedPool = value; checkCustomPool();}  } //Selected pool from the list
        public string strArg; //Storing arguments

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
            DataContext = this;
            i = 15; //Default intensity
            n = 11; //Default nFactor
            txtDonatos.IsReadOnly = true;                //Donation box
            pool = Properties.Settings.Default.Pool;     //Loading saved custom pool address
            wallet = Properties.Settings.Default.Wallet; //Loading saved wallet address
            i = Properties.Settings.Default.Inten;       //Loading saved intensity
            n = Properties.Settings.Default.nFac;        //Loading saved nFactor
                                         //Calling checkingfiles to check if I am in the right folder with sgminer
        }
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) //Moving the window
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void btnNvidia_Click(object sender, RoutedEventArgs e) //
        {
            gpuChoice = 0;
            btnAMD.Visibility = System.Windows.Visibility.Hidden;
            btnNvidia.Visibility = System.Windows.Visibility.Hidden;
            txtChoose.Visibility = System.Windows.Visibility.Hidden;
            checkingFiles();
            sldrIntensity.Visibility = System.Windows.Visibility.Hidden;
            txtn.Visibility = System.Windows.Visibility.Hidden;
            txtIntensity.Visibility = System.Windows.Visibility.Hidden;
            txtnFactor.Visibility = System.Windows.Visibility.Hidden;
            txtTouch.Visibility = System.Windows.Visibility.Hidden;
            inCalc.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnAMD_Click(object sender, RoutedEventArgs e) //
        {
            gpuChoice = 1;
            btnAMD.Visibility = System.Windows.Visibility.Hidden;
            btnNvidia.Visibility = System.Windows.Visibility.Hidden;
            txtChoose.Visibility = System.Windows.Visibility.Hidden;
            checkingFiles();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) //X-button
        {
            Properties.Settings.Default.Pool = pool;
            Properties.Settings.Default.Wallet = wallet;
            Properties.Settings.Default.Inten = i;
            Properties.Settings.Default.nFac =n;
            Properties.Settings.Default.Save();  
            Close();
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e) //Show wallet addresses for donations
        {
            txtDonatos.Visibility = System.Windows.Visibility.Visible;
            btnCloseDonate.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnCloseDonate_Click(object sender, RoutedEventArgs e) //Close donation window
        {
            txtDonatos.Visibility = System.Windows.Visibility.Hidden;
            btnCloseDonate.Visibility = System.Windows.Visibility.Hidden;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) //"RIP GPU" button which starts cmd
        {
            string strPool;
            if (selectedPool == "Custom")
            {
                strPool = pool;                                          
            }
            else                                                         //Checking what pool to use and storing it
            {
                strPool = "stratum+tcp://" + selectedPool;
            }
            
            string strWallet = wallet;                      //Storing wallet
            string strInt = i.ToString();                   //Storing intensity
            string strFac = n.ToString();                   //Storing nFactor
            if (gpuChoice == 1)
            {
                strArg = "sgminer --algorithm scrypt-n --nfactor " + n + " -o " + strPool + " -u " + strWallet + " -p x -I " + strInt; //Constructing final string to run
            }
            else if (gpuChoice == 0)
            {
                strArg = "ccminer-x64 --algo=scrypt:10 -l auto -o " + strPool + " -u " + strWallet + " --lookup-gap=2 --max-temp=85 "; //Constructing final string to run
            }
            Process cmd = new Process();
            //Opening cmd with given arguments
            cmd.StartInfo.FileName = "cmd.exe";
            MessageBox.Show(strArg);
            cmd.StartInfo.Arguments = "/K cd " + appPath + " && color 02 && setx GPU_MAX_HEAP_SIZE 100 && setx GPU_MAX_SINGLE_ALLOC_PERCENT 100 && setx GPU_MAX_ALLOC_PERCENT 100 && setx GPU_USE_SYNC_OBJECTS 1 && " + strArg + "&& pause";
            cmd.Start();
        }

        private void btnDownload_Click(object sender, EventArgs e) //Install button
        {
            txtwait.Visibility = System.Windows.Visibility.Visible; //Show "Downloading..." text
            btnInstall.IsEnabled = false; //Disable install button
            // Is file downloading yet?
            if (webClient != null)
                return;

            webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            if(gpuChoice == 1)
            { 
                webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/sgminer-5.5.5.zip"), appPath + "\\sg.zip"); //Downloading sgminer
            }
            else if (gpuChoice == 0)
            {
                webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/ccminer.zip"), appPath + "\\cc.zip"); //Downloading ccminer
            }
        }
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Completed(object sender, AsyncCompletedEventArgs e) //After download completion
        {
            webClient = null;
            unZip();                                               //Unzipping sgminer and moving MisasMinerSetup.exe to the new folder
            btnInstall.IsEnabled = true;                           //Enable button if something goes wrong
            txtwait.Visibility = System.Windows.Visibility.Hidden; //Hide "Downloading..." text
            
        }

        private void checkCustomPool() //Checking if Custom is selected on pool and opening/closing textbox for custom pool
        {
            if (selectedPool == "Custom")
            { txbxPool.IsEnabled = true; }
            else { txbxPool.IsEnabled = false; }
        }

        private void checkingFiles() //Checking if I am in the right folder with sgminer
        {
            if (gpuChoice != 2)
            {
                
                appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString(); //Storing current path
                if (gpuChoice == 1)
                {
                    fileCheck = File.Exists(appPath + "\\sgminer.exe");                                                               //Checking if sgminer exists
                }
                else if (gpuChoice == 0)
                {
                    fileCheck = File.Exists(appPath + "\\ccminer-x64.exe");                                                               //Checking if sgminer exists
                }
                if (fileCheck == true)
                {
                    txtFileCheck.Text = "Miner found✔";
                    visi = "Hidden";
                    txtninstall.Visibility = System.Windows.Visibility.Hidden;
                    txtninstall2.Visibility = System.Windows.Visibility.Hidden;
                    btnInstall.Visibility = System.Windows.Visibility.Hidden;
                    btnStart.IsEnabled = true;
                }
                else
                {
                    txtFileCheck.Text = "";
                    visi = "Visible";
                    btnStart.IsEnabled = false;
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) //Allowing only numbers for nFactor
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void unZip() //Unzipping sgminer or ccminer and moving MisasMinerSetup.exe to the new folder
        {
            if (gpuChoice == 1)
            {
                string zipPath = appPath + "\\sg.zip";
                string extractPath = appPath + "\\MisasMinerSetup";
                ZipFile.ExtractToDirectory(zipPath, extractPath);                                                   //Extracting sg.zip into new folder called MisasMinerSetup
                File.Delete(appPath + "\\sg.zip");                                                                  //Delete old zipfile
            }
            else if (gpuChoice == 0)
            {
                string zipPath = appPath + "\\cc.zip";
                string extractPath = appPath + "\\MisasMinerSetup";
                ZipFile.ExtractToDirectory(zipPath, extractPath);                                                   //Extracting cc.zip into new folder called MisasMinerSetup
                File.Delete(appPath + "\\cc.zip");                                                                  //Delete old zipfile
            }
            File.Move(appPath + "\\MisasMinerSetup.exe", appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");   //Move MisasMinerSetup.exe to the new folder
            MessageBox.Show("Download completed! You will now find me inside the folder \"MisasMinerSetup\"."); //Hooray!
            Close();                                                                                            //Close application for restart from the new location
            Process.Start(appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");                                  //Restart
        }



    }
   
}
