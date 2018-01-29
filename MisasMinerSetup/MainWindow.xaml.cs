﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

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
        public string l { get; set; }          //-l for nvidia
        public int setx1 { get; set; }
        public int setx2 { get; set; }
        public int setx3 { get; set; }
        public int setx4 { get; set; }
        public int setx5 { get; set; }
        public int temp { get; set; }
        private double _balance;
        private string worth;
        public double balance { get { return _balance; } set { _balance = value; txtBal2.Text = balance.ToString(); } }
        public bool fileCheck { get; set; } //Boolean for checking if sgminer.exe/ccminer.exe exists
        public string appPath;              // Current application path
        public int intcheckLook;
        public string strlookup;
        private string _selectedPool;
        public string selectedPool { get { return _selectedPool; } set   { _selectedPool = value; checkCustomPool();}  } //Selected pool from the list
        public string selectedgap { get; set; }
        public string strArg; //Storing arguments
        public string hoverText { get; set; }
        public System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        public Process cmd2 = new Process();
        public Process cmd3 = new Process();
        public Process cmd5 = new Process();


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
            DataContext = this;
            i = 15; //Default intensity
            n = 11; //Default nFactor
            temp = 85; //Default maxtemp
            setx1 = 0;
            setx2 = 100;
            setx3 = 100;
            setx4 = 100;
            setx5 = 1;
            balance = 0;
            worth = "0,0";
            l = "Auto";
            txtDonatos.IsReadOnly = true;                //Donation box
            pool = Properties.Settings.Default.Pool;     //Loading saved custom pool address
            wallet = Properties.Settings.Default.Wallet; //Loading saved wallet address
            i = Properties.Settings.Default.Inten;       //Loading saved intensity
            n = Properties.Settings.Default.nFac;        //Loading saved nFactor
            setx2 = Properties.Settings.Default.setx2;
            setx3 = Properties.Settings.Default.setx3;
            setx4 = Properties.Settings.Default.setx4;
            setx5 = Properties.Settings.Default.setx5;
            temp = Properties.Settings.Default.temp;
            selectedgap = Properties.Settings.Default.selectedgap;
            selectedPool = Properties.Settings.Default.selectedPool;
            if (l != "Auto")
            {
                l = Properties.Settings.Default.l;
            }            
            ni.Icon = MisasMinerSetup.Properties.Resources.Myicon;
            ni.Visible = true;
            System.Windows.Forms.ContextMenu mn = new System.Windows.Forms.ContextMenu();
            ni.ContextMenu = mn;
            mn.MenuItems.Add("Copy active wallet", WalletTray);
            mn.MenuItems.Add("Exit",  ExitApplication);
            updateHover();
        }
        private void updateHover()
        {

            Double doubleWorth = Double.Parse(worth, CultureInfo.InvariantCulture) * balance;
            string strWorth = doubleWorth.ToString();
            hoverText = "Balance: " + balance + "GRLC \nWorth: " + strWorth + "$";
            ni.Text = hoverText;
            ni.DoubleClick +=
                delegate(object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            
        }
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ExitApplication(object sender, EventArgs e)
        {
            shutClose();
        }
        private void WalletTray(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(wallet);
        }


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
            btnNvidiaSolo.Visibility = System.Windows.Visibility.Hidden;
            txtChoose.Visibility = System.Windows.Visibility.Hidden;
            checkingFiles();
            sldrIntensity.Visibility = System.Windows.Visibility.Hidden;
            txtn.Visibility = System.Windows.Visibility.Hidden;
            txtIntensity.Visibility = System.Windows.Visibility.Hidden;
            txtnFactor.Visibility = System.Windows.Visibility.Hidden;
            txtTouch.Visibility = System.Windows.Visibility.Hidden;
            inCalc.Visibility = System.Windows.Visibility.Hidden;
            checkingFiles();
        }

        private void btnNvidiaSolo_Click(object sender, RoutedEventArgs e) //
        {
            gpuChoice = 3;
            btnAMD.Visibility = System.Windows.Visibility.Hidden;
            btnNvidia.Visibility = System.Windows.Visibility.Hidden;
            btnNvidiaSolo.Visibility = System.Windows.Visibility.Hidden;
            txtChoose.Visibility = System.Windows.Visibility.Hidden;
            checkingFiles();
            sldrIntensity.Visibility = System.Windows.Visibility.Hidden;
            txtn.Visibility = System.Windows.Visibility.Hidden;
            txtIntensity.Visibility = System.Windows.Visibility.Hidden;
            txtnFactor.Visibility = System.Windows.Visibility.Hidden;
            txtTouch.Visibility = System.Windows.Visibility.Hidden;
            inCalc.Visibility = System.Windows.Visibility.Hidden;
            ComboBox1.IsEnabled = false;
            ComboBox1.SelectedIndex = -1;
            checkingFiles();
        }

        private void btnAMD_Click(object sender, RoutedEventArgs e) //
        {
            gpuChoice = 1;
            btnAMD.Visibility = System.Windows.Visibility.Hidden;
            btnNvidiaSolo.Visibility = System.Windows.Visibility.Hidden;
            btnNvidia.Visibility = System.Windows.Visibility.Hidden;
            txtChoose.Visibility = System.Windows.Visibility.Hidden;
            txtl.Visibility = System.Windows.Visibility.Hidden;
            txtlbox.Visibility = System.Windows.Visibility.Hidden;
            txbxtemp.Visibility = System.Windows.Visibility.Hidden;
            txttemp.Visibility = System.Windows.Visibility.Hidden;
            comboGap.Visibility = System.Windows.Visibility.Hidden;
            txtLook.Visibility = System.Windows.Visibility.Hidden;
            checkingFiles();
        }

        private void shutClose()
        {
            saveConf();
            Close();
            ni.Visible = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) //X-button
        {

            shutClose();
        }
        private void MiniButton_Click(object sender, RoutedEventArgs e) //X-button
        {

            saveConf();
            this.Hide();
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

        private void btnsetx_Click(object sender, RoutedEventArgs e)
        {
            txtsetx.Visibility = System.Windows.Visibility.Visible;
            txtsetx2.Visibility = System.Windows.Visibility.Visible;
            txtsetx3.Visibility = System.Windows.Visibility.Visible;
            txtsetx4.Visibility = System.Windows.Visibility.Visible;
            txtsetx5.Visibility = System.Windows.Visibility.Visible;
            btnsetxSave.Visibility = System.Windows.Visibility.Visible;
        }
        private void btnsetxSave_Click(object sender, RoutedEventArgs e)
        {
            txtsetx.Visibility = System.Windows.Visibility.Hidden;
            txtsetx2.Visibility = System.Windows.Visibility.Hidden;
            txtsetx3.Visibility = System.Windows.Visibility.Hidden;
            txtsetx4.Visibility = System.Windows.Visibility.Hidden;
            txtsetx5.Visibility = System.Windows.Visibility.Hidden;
            btnsetxSave.Visibility = System.Windows.Visibility.Hidden;
        }
        
            private void Refresh_Click(object sender, RoutedEventArgs e)
            {
                checkBalance();
                updateHover();
            }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void StartButton_Click(object sender, RoutedEventArgs e) //"RIP GPU" button which starts cmd
        {
            saveConf();
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
            if (selectedgap == "1")
            {
                strlookup = "--lookup-gap=1";
            }
            else if (selectedgap == "2")
            {
                strlookup = "--lookup-gap=2";
            }
            else if (selectedgap == "3")
            {
                strlookup = "--lookup-gap=3";
            }
            if (gpuChoice == 1)
            {
                strArg = "sgminer --algorithm scrypt-n --nfactor " + strFac + " -o " + strPool + " -u " + strWallet + " -p x -I " + strInt; //Constructing final string to run
            }
            else if (gpuChoice == 0)
            {
                strArg = "ccminer-x64 --algo=scrypt:10 -l " + l + " -o " + strPool + " -u " + strWallet + " " + strlookup + " --max-temp=" + temp + " "; //Constructing final string to run

            }
            else if (gpuChoice == 3)
            {
                cmd5.StartInfo.FileName = "cmd.exe";
                cmd5.StartInfo.Arguments = "/K cd " + appPath + "\\MisasMinerSetup\\ && Run-Network.bat";
                cmd5.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                cmd5.Start();

                strArg = " ccminer.exe --algo=scrypt:10 -l " + l + " -o 127.0.0.1:42068 -u test -p test --no-longpoll " + strlookup + " --no-getwork --no-stratum --coinbase-addr=" + strWallet + " --max-temp=" + temp + " ";

                        }
            System.Windows.MessageBox.Show(strArg);
            Process cmd = new Process();
            //Opening cmd with given arguments
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = "/K cd " + appPath + " && color 02 && setx GPU_MAX_HEAP_SIZE " + setx2 + " && setx GPU_MAX_SINGLE_ALLOC_PERCENT " + setx3 + " && setx GPU_MAX_ALLOC_PERCENT " + setx4 + " && setx GPU_USE_SYNC_OBJECTS " + setx5 + " && " + strArg + "&& pause";
            cmd.Start();

        }

       
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void btnDownload_Click(object sender, EventArgs e) //Install button
        {
            txtwait.Visibility = System.Windows.Visibility.Visible; //Show "Downloading..." text
            btnInstall.IsEnabled = false; //Disable install button
            // Is file downloading yet?
            if (webClient != null)
                return;

            webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedPool);
            if (gpuChoice == 1)
            {
                webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/sgminer-5.5.5.zip"), appPath + "\\sg.zip"); //Downloading sgminer
            }
            else if (gpuChoice == 0)
            {
                webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/ccminer.zip"), appPath + "\\cc.zip"); //Downloading ccminer
            }
            if (gpuChoice == 3)
            {
                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedSolo);

                webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/Wallet.zip"), appPath + "\\soloWallet.zip"); //Downloading Wallet
            }
        }
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void CompletedPool(object sender, AsyncCompletedEventArgs e) //After download completion
        {
            webClient = null;
            unZipPool();                                               //Unzipping sgminer and moving MisasMinerSetup.exe to the new folder
            btnInstall.IsEnabled = true;                           //Enable button if something goes wrong
            txtwait.Visibility = System.Windows.Visibility.Hidden; //Hide "Downloading..." text
            
        }
        private void CompletedSolo(object sender, AsyncCompletedEventArgs e) //After download completion
        {
            webClient = null;
            unZipSolo();                                             

        }
        private void saveConf()
        {
            Properties.Settings.Default.Pool = pool;
            Properties.Settings.Default.Wallet = wallet;
            Properties.Settings.Default.Inten = i;
            Properties.Settings.Default.nFac = n;
            Properties.Settings.Default.l = l;
            Properties.Settings.Default.setx2 = setx2;
            Properties.Settings.Default.setx3 = setx3;
            Properties.Settings.Default.setx4 = setx4;
            Properties.Settings.Default.setx5 = setx5;
            Properties.Settings.Default.temp = temp;
            Properties.Settings.Default.selectedgap = selectedgap;
            Properties.Settings.Default.selectedPool = selectedPool;
            Properties.Settings.Default.Save();  
        }

        private void checkBalance()
        {
            WebClient client = new WebClient();
            if (wallet != "")
            {
                
                string downloadedString = client.DownloadString("https://explorer.grlc-bakery.fun/ext/getbalance/" + wallet);
                balance = Math.Round(Double.Parse(downloadedString, CultureInfo.InvariantCulture), 2);

            }                
                string downloadedWorth = client.DownloadString("https://api.coinmarketcap.com/v1/ticker/garlicoin/");
                string toBeSearched = "\"price_usd\": \"";
                worth = downloadedWorth.Substring(downloadedWorth.IndexOf(toBeSearched) + toBeSearched.Length, 4);
                webClient = null; //ASDASD
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
                else if (gpuChoice == 3)
                {
                    fileCheck = File.Exists(appPath + "\\ccminer.exe");                                                               //Checking if sgminer exists
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

        private void unZipPool() //Unzipping sgminer or ccminer and moving MisasMinerSetup.exe to the new folder
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
            System.Windows.MessageBox.Show("Download completed! You will now find me inside the folder \"MisasMinerSetup\"."); //Hooray!
            Close();                                                                                            //Close application for restart from the new location
            Process.Start(appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");                                  //Restart
        }
        private void unZipSolo() //Unzipping sgminer or ccminer and moving MisasMinerSetup.exe to the new folder
        {

                string zipPath = appPath + "\\soloWallet.zip";
                string extractPath = appPath + "\\MisasMinerSetup";
                ZipFile.ExtractToDirectory(zipPath, extractPath);                                                   
                File.Delete(appPath + "\\soloWallet.zip");                                                                  //Delete old zipfile
            
            File.Move(appPath + "\\MisasMinerSetup.exe", appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");   //Move MisasMinerSetup.exe to the new folder
            System.Windows.MessageBox.Show("Wallet downloaded! Continuing with network and wallet setup"); //Hooray!
            System.Windows.MessageBox.Show("Give me network access to the cmd and do not close it! Just let it be.");
            conNetwork();



        }

        private void conNetwork()
        {
            //Opening cmd with given arguments
            cmd2.StartInfo.FileName = "cmd.exe";
            cmd2.StartInfo.Arguments = "/K cd " + appPath + "\\MisasMinerSetup\\ && Run-Network.bat";
            cmd2.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            cmd2.Start();
            Thread.Sleep(1000);
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appdataPathGarli = appdataPath +"\\Garlicoin\\garlicoin.conf";
            File.Move(appPath + "\\MisasMinerSetup\\garlicoin.conf", appdataPathGarli);
            Thread.Sleep(500);
            System.Windows.MessageBox.Show("I will throw an error now. Just close it as it restarts and just let it be");
            Thread.Sleep(500);
            cmd2.CloseMainWindow();
            System.Windows.MessageBox.Show("Press \"ok\" if you closed the error.");
            cmd2.StartInfo.FileName = "cmd.exe";
            cmd2.StartInfo.Arguments = "/K cd " + appPath + "\\MisasMinerSetup\\ && Run-Network.bat";
            cmd2.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            cmd2.Start();
            string output = string.Empty;
            string error = string.Empty;
            Thread.Sleep(1000);
            System.Windows.MessageBox.Show("This will take a while. We are checking if you have downloaded all the blocks needed.");
            string blockCount = "asd";
            string downloadedBlock = "fgh";
            for (int i = 0; blockCount != downloadedBlock ; i++)
            {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd", "/K cd " + appPath + "\\MisasMinerSetup\\ && garlicoin-cli getblockchaininfo && exit");
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            processStartInfo.UseShellExecute = false;
            Process process = Process.Start(processStartInfo);
            using (StreamReader streamReader = process.StandardOutput)
            {
                output = streamReader.ReadToEnd();
            }
            string toBeSearched = "\"blocks\": ";
            blockCount = output.Substring(output.IndexOf(toBeSearched) + toBeSearched.Length, 5);
            WebClient client = new WebClient();
            downloadedBlock = client.DownloadString("https://explorer.grlc-bakery.fun/api/getblockcount");
            webClient = null;
            System.Windows.MessageBox.Show(blockCount + "/" + downloadedBlock + "\nPress ok to check again. Will probably take some time...");
            }
            System.Windows.MessageBox.Show("FINISHED! Yayyyy that was too long I know...");
            System.Windows.MessageBox.Show("Let's now setup your wallet! Copy the wallet code from the screen and close the wallet cmd. Do not close the 1st cmd that should still be running in the background. You need to run it whenever you mine or use your wallet");
            cmd3.StartInfo.FileName = "cmd.exe";
            cmd3.StartInfo.Arguments = "/K cd " + appPath + "\\MisasMinerSetup\\ && garlicoin-cli getnewaddress";
            cmd3.Start();
            System.Windows.MessageBox.Show("Remember to back up your wallet.dat file! You can find it in %appdata%/Garlicoin/");
            webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedSoloMiner);

            webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/CCSoloMiner.zip"), appPath + "\\solominer.zip"); //Downloading Solominer
        }

        private void CompletedSoloMiner(object sender, AsyncCompletedEventArgs e) //After download completion
        {
            webClient = null;
            string zipPath = appPath + "\\solominer.zip";
            string extractPath = appPath + "\\MisasMinerSetup";
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            File.Delete(appPath + "\\solominer.zip");                                                                  //Delete old zipfile

            System.Windows.MessageBox.Show("Miner Downloaded! You will now find me inside the folder \"MisasMinerSetup\"."); //Hooray!
            Close();                                                                                            //Close application for restart from the new location
            Process.Start(appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");                                  //Restart


            

        }







    }
   
}
