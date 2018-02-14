using System;
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
using System.Management;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;
using System.Timers;
using System.Net.Sockets;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using MisasMinerSetup.Helpers;
using MisasMinerSetup.Configuration;

namespace MisasMinerSetup
{
    public partial class MainWindow : MetroWindow
    {
        #region Properties

        public MinerConfig Config { get; set; }

        public int Intensity
        {
            get { return Config.Intensity; }
            set
            {
                Config.Intensity = value;
                inCalc.Text = value.ToString();
            }
        }
        
        public string SelectedPool
        {
            get { return Config.SelectedPool; }
            set
            {
                Config.SelectedPool = value;
                CheckCustomPool();
            }
        }

        // ToDo: stallone: these should all be capitalized.. didnt have time to fix this yet.

        public string strArg; //Storing arguments
        public string hoverText { get; set; }
        public string cleanTemp { get; set; }
        public string cleanLoad { get; set; }
        public int monGPU { get; set; }
        public string cleanWat { get; set; }
        public string strWorth;
        public System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        public string visi { get; set; }    //Visibility on "Miner not found"
        public Process cmd2 = new Process();
        public Process cmd3 = new Process();
        public Process cmd4 = new Process();
        public Process cmd5 = new Process();
        public bool isMining = false;
        public string strBlocks;
        public string strHash;
        public string OldBlockcount;
        public bool firstCon;
        public bool tempCheck;
        public string strFan;
        public string strUsage;
        public string strInt;
        public string myWallet;
        public string operatingSystem;
        public int blocksFound;
        public int oldBlocksFound;

        public bool bldevice0;
        public bool bldevice1;
        public bool bldevice2;
        public bool bldevice3;
        public bool bldevice4;
        public bool bldevice5;
        
        public string devicePar;
        public string GPUList = Properties.Resources.GPUList;
        public Computer computer;
        public List<GPUHardwareNode> GPUHardwareNodes = new List<GPUHardwareNode>();
        public bool fileCheck { get; set; } //Boolean for checking if sgminer.exe/ccminer.exe exists
        public string appPath;              // Current application path
        public int intcheckLook;
        public string strLookup;        

        private double _balance;
        public double Balance
        {
            get { return _balance; }
            set { _balance = value; }
        }

        private string _garliWorth;
        public string GarliWorth
        {
            get { return _garliWorth; }
            set
            {
                _garliWorth = value;
                txtgarliWorth.Text = GarliWorth;
            }
        }

        #endregion

        #region Private Fields
        
        private int gpuChoice = 2;        
        private string worth;
        private List<StatGrid> _statGrids = new List<StatGrid>();
        private List<TextBlock> _hardwareNames = new List<TextBlock>();
        private List<System.Windows.Controls.CheckBox> _checkBoxes = new List<System.Windows.Controls.CheckBox>();

        #endregion

        #region Notifier
    
        public Notifier notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 0, 40); //Creating notifiers using ToastNotifications

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = System.Windows.Application.Current.Dispatcher;
        });

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            this.AllowsTransparency = true;
            DataContext = this;

            SetDefaults();
            
            txbxWallet.Text = Config.Wallet;

            ConfigureUI();
            CatalogGPUHardware();
            PollHardware();
            CheckBalance();
            SetMenus();
            SetHandlers();
            UpdateHover();
            MinerInfo();
            MinerInfo2();
            CheckBlocksFound();
            GetGPUType();
        }

        #endregion

        private void SetDefaults()
        {
            Config = new MinerConfig();
            Config.Load();

            Balance = 0;
            monGPU = 1;
            tempCheck = false;
            worth = "N/A";            
            strBlocks = "0";
            strHash = "0";
            firstCon = true;
            fileCheck = false;
            txtDonatos.IsReadOnly = true;                //Donation box       

        }
        
        private void SetMenus()
        {
            ni.Icon = Properties.Resources.Myicon;
            ni.Visible = true;
            System.Windows.Forms.ContextMenu mn = new System.Windows.Forms.ContextMenu(); //Setting up icontray icon
            ni.ContextMenu = mn;
            mn.MenuItems.Add("Copy active wallet", WalletTray);
            mn.MenuItems.Add("Exit", ExitApplication);
        }

        private void SetHandlers()
        {
            MouseDown += Window_MouseDown;
            
        }

        private void UpdateHover() //Handling doubleclick and tooltip updates
        {
            Double doubleWorth = Double.Parse(worth, CultureInfo.InvariantCulture) * Balance;
            UpdateHover2();

            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        private void UpdateHover2() //Constructing new tooltip
        {
            Double doubleWorth = Double.Parse(worth, CultureInfo.InvariantCulture) * Balance;
            strWorth = doubleWorth.ToString();
            strWorth = strWorth.Split(',')[0];
            hoverText = "Balance: " + Balance + "\nGPU temp:" + cleanTemp + "\n" + strHash + "Kh/s";
            ni.Text = hoverText;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }                

        private void ExitApplication(object sender, EventArgs e)
        {
            ShutClose();
        }

        private void WalletTray(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(Config.Wallet); //Copy current wallet address to clipboard
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) //Moving the window
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SelectedNvidia() //Person selected Nvidia
        {
            gpuChoice = 0;
            btnNvidiaSolo.Visibility = Visibility.Hidden;
            txtChoose.Visibility = Visibility.Hidden;
            btnMonitor.Visibility = Visibility.Hidden;
            checkingFiles();
        }        

        private void SelectedAMD() //Person selected AMD
        {
            gpuChoice = 1;
            btnNvidiaSolo.Visibility = Visibility.Hidden;
            txtChoose.Visibility = Visibility.Hidden;
            btnMonitor.Visibility = Visibility.Hidden;
            checkingFiles();
            
        }        

        private void ShutClose() //Closing the app
        {
            Config.Save();

            System.Windows.Application.Current.Shutdown();
            ni.Visible = false;
        }

        #region UI Events

        private void BtnNvidiaSolo_Click(object sender, RoutedEventArgs e) //Person selected Nvidia Solo mining
        {
            gpuChoice = 3;
            btnNvidiaSolo.Visibility = Visibility.Hidden;
            txtChoose.Visibility = Visibility.Hidden;
            checkingFiles();
            sldrIntensity.Visibility = Visibility.Hidden;
            txtIntensity.Visibility = Visibility.Hidden;
            inCalc.Visibility = Visibility.Hidden;
            btnMonitor.Visibility = Visibility.Hidden;
            ComboBox1.IsEnabled = false;
            ComboBox1.SelectedIndex = -1;
            checkingFiles();
        }

        private void BtnMonitor_Click(object sender, RoutedEventArgs e)
        {
            txtMonAMD.Visibility = Visibility.Visible;
            txtMonNvidia.Visibility = Visibility.Visible;
            sldrGPU.Visibility = Visibility.Visible;
            btnNvidiaSolo.Visibility = Visibility.Hidden;
            txtChoose.Visibility = Visibility.Hidden;
            sldrIntensity.Visibility = Visibility.Hidden;
            txtIntensity.Visibility = Visibility.Hidden;
            inCalc.Visibility = Visibility.Hidden;
            btnMonitor.Visibility = Visibility.Hidden;
            btnInstall.Visibility = Visibility.Hidden;
            btnsetx.Visibility = Visibility.Hidden;
            txtsetx.Visibility = Visibility.Hidden;
            txbxPool.Visibility = Visibility.Hidden;
            btnStart.Visibility = Visibility.Hidden;
            txttemp.Visibility = Visibility.Hidden;
            TempCheck.Visibility = Visibility.Hidden;
            txtninstall.Visibility = Visibility.Hidden;
            txtninstall2.Visibility = Visibility.Hidden;
            txbxtemp.Visibility = Visibility.Hidden;
            isMining = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) //X-button click
        {
            ShutClose();
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e) //Minimalize click
        {
            Config.Save();

            this.Hide();
        }

        private void BtnTesti_Click(object sender, RoutedEventArgs e) //Testimonials click
        {
            txtTestimonials.Visibility = Visibility.Visible;
            btnCloseTest.Visibility = Visibility.Visible;
            pngAbby.Visibility = Visibility.Visible;
        }

        private void BtnCloseTest_Click(object sender, RoutedEventArgs e)  //Close testimonials click
        {

            txtTestimonials.Visibility = Visibility.Hidden;
            btnCloseTest.Visibility = Visibility.Hidden;
            pngAbby.Visibility = Visibility.Hidden;
        }

        private void BtnDonate_Click(object sender, RoutedEventArgs e) //Show wallet addresses for donations
        {
            txtDonatos.Visibility = Visibility.Visible;
            btnCloseDonate.Visibility = Visibility.Visible;
        }

        private void BtnCloseDonate_Click(object sender, RoutedEventArgs e) //Close donation window
        {
            txtDonatos.Visibility = Visibility.Hidden;
            btnCloseDonate.Visibility = Visibility.Hidden;
        }

        private void Btnsetx_Click(object sender, RoutedEventArgs e) //Open setx window
        {
            txtsetx.Visibility = Visibility.Visible;
            txtsetx2.Visibility = Visibility.Visible;
            txtsetx3.Visibility = Visibility.Visible;
            txtsetx4.Visibility = Visibility.Visible;
            txtsetx5.Visibility = Visibility.Visible;
            btnsetxSave.Visibility = Visibility.Visible;
        }

        private void BtnsetxSave_Click(object sender, RoutedEventArgs e) //Close setx window
        {
            txtsetx.Visibility = Visibility.Hidden;
            txtsetx2.Visibility = Visibility.Hidden;
            txtsetx3.Visibility = Visibility.Hidden;
            txtsetx4.Visibility = Visibility.Hidden;
            txtsetx5.Visibility = Visibility.Hidden;
            btnsetxSave.Visibility = Visibility.Hidden;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) //Update icon click
        {
            CheckBalance();
            UpdateHover();
        }

        private void TempCheck_Checked(object sender, RoutedEventArgs e) //Temperature alert checkbox selected
        {
            tempCheck = true;
        }

        private void TempCheck_Unchecked(object sender, RoutedEventArgs e) //Temperature alert checkbox selected
        {
            tempCheck = false;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) //"RIP GPU" button which starts cmd
        {
            string devices = checkDevices();
            isMining = true;
            
            Config.Save();

            string strPool;
            if (SelectedPool == "Custom")
            {
                strPool = Config.Pool;
            }
            else                                                         //Checking what pool to use and storing it
            {
                strPool = "stratum+tcp://" + SelectedPool;
            }
            string strFac = Config.NFactor.ToString(); 
            string strWallet = Config.Wallet;                      //Storing wallet
            string strCustom = Config.Custom;


                strInt = Intensity.ToString();                   //Storing intensity
                                                         //Storing nFactor
                if (Config.SelectedGap == "1")
                {
                    strLookup = "--lookup-gap=1";
                }
                else if (Config.SelectedGap == "2")
                {
                    strLookup = "--lookup-gap=2";               //Checking what lookup-gap option was selected
                }
                else if (Config.SelectedGap == "3")
                {
                    strLookup = "--lookup-gap=3";
                
            }
            if (gpuChoice == 1) //If AMD GPU was chosen
            {
                strArg = "sgminer --api-listen -d " + devices + " --temp-cutoff " + Config.Temp + " -k allium -o " + strPool + " -u " + strWallet + " -p x -I " + strInt + " " + strCustom; //Constructing final string to run
            }
            else if (gpuChoice == 0) //If Nvidia GPU was chosen
            {
                if (operatingSystem == "64bit")
                {
                    strArg = "ccminer-x64 -b --api-remote --api-bind=4028 --api-allow=127.0.0.1 -d " + devices + " --algo=allium -o " + strPool + " -u " + strWallet + " --max-temp=" + Config.Temp + " -i " + strInt + " " + strCustom; //Constructing final string to run
                }
                else if (operatingSystem == "32bit")
                {
                    strArg = "ccminer -b --api-remote --api-bind=4028 --api-allow=127.0.0.1 -d " + devices + " --algo=allium -o " + strPool + " -u " + strWallet + " --max-temp=" + Config.Temp + " -i " + strInt + " " + strCustom; //Constructing final string to run

                }
            }
            else if (gpuChoice == 3) //If Nvidia solomining was chosen
            {
                cmd5.StartInfo.FileName = "cmd.exe";
                cmd5.StartInfo.Arguments = "/K cd " + appPath + "\\MisasMinerSetup\\ && Run-Network.bat"; //Running network needed to solomine
                cmd5.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                cmd5.Start();

                strArg = " ccminer.exe --algo=scrypt:10 -l " + Config.LValue + " -o 127.0.0.1:42068 -u test -p test --no-longpoll " + strLookup + " --no-getwork --no-stratum --coinbase-addr=" + strWallet + " --max-temp=" + Config.Temp + " "; //Constructing final string to run. EXPERIMENTAL!

            }
            System.Windows.MessageBox.Show(strArg);
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = "/K cd " + appPath + " && color 02 && setx GPU_MAX_HEAP_SIZE " + Config.Setx2 + " && setx GPU_MAX_SINGLE_ALLOC_PERCENT " + Config.Setx3 + " && setx GPU_MAX_ALLOC_PERCENT " + Config.Setx4 + " && setx GPU_USE_SYNC_OBJECTS " + Config.Setx5 + " && " + strArg + "&& pause"; //Final argument given to cmd
            cmd.Start();            //Opening cmd with given arguments

        }
        
        private void btnDownload_Click(object sender, EventArgs e) //Install button
        {
            downloadProgress.Visibility = Visibility.Visible; //Show "Downloading..." text
            btnInstall.IsEnabled = false; //Disable install button

            using (var webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedPool);
                if (gpuChoice == 1) //AMD
                {
                    webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/sgminer-5.5.5.zip"), appPath + "\\sg.zip"); //Downloading sgminer
                }
                else if (gpuChoice == 0) //NVIDIA
                {
                    if (operatingSystem == "64bit")
                    {
                        webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/ccminer64.zip"), appPath + "\\cc.zip"); //Downloading ccminer64bit
                    }
                    else if (operatingSystem == "32bit")
                    {
                        webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/ccminer32.zip"), appPath + "\\cc.zip"); //Downloading ccminer32bit
                    }
                    
                }

                if (gpuChoice == 3) //NVIDIA SOLO
                {
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedSolo);
                    webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/Wallet.zip"), appPath + "\\soloWallet.zip"); //Downloading Wallet
                }
            }
        }

        private void btnDevices_Click(object sender, RoutedEventArgs e)
        {
            txtDevices.Visibility = Visibility.Visible;
            device0.Visibility = Visibility.Visible;
            device1.Visibility = Visibility.Visible;
            device2.Visibility = Visibility.Visible;
            device3.Visibility = Visibility.Visible;
            device4.Visibility = Visibility.Visible;
            device5.Visibility = Visibility.Visible;
            btnDeviceSave.Visibility = Visibility.Visible;
        }

        private void btnDeviceSave_Click(object sender, RoutedEventArgs e)
        {
            txtDevices.Visibility = Visibility.Hidden;
            device0.Visibility = Visibility.Hidden;
            device1.Visibility = Visibility.Hidden;
            device2.Visibility = Visibility.Hidden;
            device3.Visibility = Visibility.Hidden;
            device4.Visibility = Visibility.Hidden;
            device5.Visibility = Visibility.Hidden;
            btnDeviceSave.Visibility = Visibility.Hidden;
        }

        private void txbxWallet_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CheckBalance();
        }

        #endregion

        private void CompletedPool(object sender, AsyncCompletedEventArgs e) //After download completion
        {
            UnZipPool();                                           //Unzipping sg/ccminer and moving MisasMinerSetup.exe to the new folder
            btnInstall.IsEnabled = true;                           //Enable button if something goes wrong
            downloadProgress.Visibility = Visibility.Hidden; //Hide "Downloading..." text

        }

        private void CompletedSolo(object sender, AsyncCompletedEventArgs e) //After solo download completion
        {
            UnZipSolo();                                           //Unzipping solo files.
        }

       

        private string checkDevices()
        {
            string deviceList = "";
            if (bldevice0 == true)
            {
                if (deviceList == "")
                {
                    deviceList += "0";
                }
                else { deviceList += ",0"; }
            }
            if (bldevice1 == true)
            {
                if (deviceList == "")
                {
                    deviceList += "1";
                }
                else { deviceList += ",1"; }
            }
            if (bldevice2 == true)
            {
                if (deviceList == "")
                {
                    deviceList += "2";
                }
                else { deviceList += ",2"; }
            }
            if (bldevice3 == true)
            {
                if (deviceList == "")
                {
                    deviceList += "3";
                }
                else { deviceList += ",3"; }
            }
            if (bldevice4 == true)
            {
                if (deviceList == "")
                {
                    deviceList += "4";
                }
                else { deviceList += ",4"; }
            }
            if (bldevice5 == true)
            {
                if (deviceList == "")
                {
                    deviceList += "5";
                }
                else { deviceList += ",5"; }
            }
            return deviceList;
        }

        private void MinerInfo() //Get minerinfo from miner API
        {
            if (isMining == true) //Is the user mining
            {
                if (gpuChoice == 1 || monGPU == 0) //AMD
                {

                    System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();

                    while (true) //Try to connect to the API
                    {
                        if (monGPU != 0 && gpuChoice == 2)
                        { break; }
                        try
                        {
                            clientSocket.Connect("127.0.0.1", 4028);
                            break;
                        }
                        catch
                        {
                        }

                    }
                    if (gpuChoice == 1 || monGPU == 0) //AMD
                    {
                        string get_menu_request = "summary"; //Request summary
                        NetworkStream serverStream = clientSocket.GetStream();
                        byte[] outStream = System.Text.Encoding.ASCII.GetBytes(get_menu_request);
                        serverStream.Write(outStream, 0, outStream.Length);
                        serverStream.Flush();
                        byte[] inStream = new byte[65556];
                        serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                        string _returndata = System.Text.Encoding.ASCII.GetString(inStream);
                        serverStream.Close();
                        int hashStart = _returndata.IndexOf("KHS av=") + "KHS av=".Length; //Find hashrate
                        int hashEnd = _returndata.LastIndexOf(",KHS");
                        strHash = _returndata.Substring(hashStart, hashEnd - hashStart);
                    }
                }
                if (gpuChoice == 0 || monGPU == 2) //nvidia
                {

                    System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();

                    while (true) //Try to connect to the API
                    {
                        if (monGPU != 2 && gpuChoice == 2)
                        { break; }
                        try
                        {
                            clientSocket.Connect("127.0.0.1", 4028);
                            break;
                        }
                        catch
                        {
                        }

                    }
                    if (gpuChoice == 0 || monGPU == 2) //nvidia
                    {
                        string get_menu_request = "summary";  //Request summary
                        NetworkStream serverStream = clientSocket.GetStream();
                        byte[] outStream = System.Text.Encoding.ASCII.GetBytes(get_menu_request);
                        serverStream.Write(outStream, 0, outStream.Length);
                        serverStream.Flush();
                        byte[] inStream = new byte[65556];
                        serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                        string _returndata = System.Text.Encoding.ASCII.GetString(inStream);
                        serverStream.Close();
                        int hashStart = _returndata.IndexOf("KHS=") + "KHS=".Length; //Find hashrate
                        int hashEnd = _returndata.LastIndexOf(";SOLV=");
                        strHash = _returndata.Substring(hashStart, hashEnd - hashStart);

                    }
                }
            }
        }

        private void MinerInfo2()
        {
            if (isMining == true) //Is the user mining
            {
                if (gpuChoice == 1 || monGPU == 0) //AMD
                {

                    System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();

                    while (true) //Try to connect to the API
                    {
                        if (monGPU != 0 && gpuChoice == 2)
                        { break; }
                        try
                        {
                            clientSocket.Connect("127.0.0.1", 4028);
                            break;
                        }
                        catch
                        {
                        }

                    }
                    if (gpuChoice == 1 || monGPU == 0) //AMD
                    {
                        string get_menu_request = "devs";
                        NetworkStream serverStream = clientSocket.GetStream();
                        byte[] outStream = System.Text.Encoding.ASCII.GetBytes(get_menu_request);
                        serverStream.Write(outStream, 0, outStream.Length);
                        serverStream.Flush();
                        byte[] inStream = new byte[65556];
                        serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                        string _returndata = System.Text.Encoding.ASCII.GetString(inStream);
                        serverStream.Close();
                        int hashStart = _returndata.IndexOf("Fan Percent=") + "Fan Percent=".Length; //Find fanspeed. NOT CURRENTLY WORKING!
                        int hashEnd = _returndata.LastIndexOf(",GPU Clock=");
                        strFan = _returndata.Substring(hashStart, hashEnd - hashStart);
                        int activityStart = _returndata.IndexOf("GPU Activity=") + "GPU Activity=".Length; //Find fanspeed. NOT CURRENTLY WORKING!
                        int activityEnd = _returndata.LastIndexOf(",Powertune=");
                        strUsage = _returndata.Substring(activityStart, activityEnd - activityStart);
                    }
                }

                if (gpuChoice == 0 || monGPU == 2) //nvidia
                {

                    System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();

                    while (true) //Try to connect to the API
                    {
                        if (monGPU != 2 && gpuChoice == 2)
                        { break; }
                        try
                        {
                            clientSocket.Connect("127.0.0.1", 4028);
                            break;
                        }
                        catch
                        {
                        }

                    }
                    if (gpuChoice == 0 || monGPU == 2) //nvidia
                    {
                        string get_menu_request = "threads";
                        NetworkStream serverStream = clientSocket.GetStream();
                        byte[] outStream = System.Text.Encoding.ASCII.GetBytes(get_menu_request);
                        serverStream.Write(outStream, 0, outStream.Length);
                        serverStream.Flush();
                        byte[] inStream = new byte[65556];
                        serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                        string _returndata = System.Text.Encoding.ASCII.GetString(inStream);
                        serverStream.Close();
                        int basicStart = _returndata.IndexOf(";FREQ=") + ";FREQ=".Length; //Find power. NOT CURRENTLY WORKING!
                        int basicEnd = _returndata.LastIndexOf(";MEMFREQ=");
                        int usedStart = _returndata.IndexOf(";GPUF=") + ";GPUF=".Length; //Find power. NOT CURRENTLY WORKING!
                        int usedEnd = _returndata.LastIndexOf(";MEMF=");
                        string strUsageBasic = _returndata.Substring(basicStart, basicEnd - basicStart);
                        string strUsageUsed = _returndata.Substring(usedStart, usedEnd - usedStart);
                        var usageB = float.Parse(strUsageBasic);
                        var usageU = float.Parse(strUsageUsed);
                        var usagePerc = usageU / usageB * 100f;
                        strUsage = usagePerc.ToString("000.00");
                        int fanStart = _returndata.IndexOf(";FAN=") + ";FAN=".Length; //Find fanspeed. NOT CURRENTLY WORKING!
                        int fanEnd = _returndata.LastIndexOf(";RPM=");
                        strFan = _returndata.Substring(fanStart, fanEnd - fanStart);

                    }
                }
            }
        }

        private void CheckBalance() //Check worth 
        {
            worth = GarlicoinHelper.GetCurrentPrice();
            GarliWorth = String.Format("${0} USD", worth);

            Config.Wallet = txbxWallet.Text;
            if (!String.IsNullOrWhiteSpace(Config.Wallet))
            {
                notifier.ShowSuccess($"Wallet set\r\n{Config.Wallet}");

                double balance;
                if (GarlicoinHelper.GetBalance(Config.Wallet, out balance))
                    Balance = balance;
                else
                    notifier.ShowWarning("Wallet not found.");
            }
            else
            {
                notifier.ShowInformation("No wallet set.");
            }

            var myWorth = float.Parse(worth) * Balance;
            txtBal2.Text = $"{Balance} GRLC (${myWorth:0.00} USD)";
        }

        private void CheckBlocksFound() //Check blocks founds 
        {
            if (SelectedPool == "happy.garlicoin.fun:3210")
            {
                using (WebClient client = new WebClient())
                {
                    string downloadedBlocks = client.DownloadString("http://happy.garlicoin.fun/api/stats/"); //Checks for blocks
                    int blockStart = downloadedBlocks.IndexOf("\"validBlocks\":\"") + "\"validBlocks\":\"".Length; //Find fanspeed. NOT CURRENTLY WORKING!
                    int blocksEnd = downloadedBlocks.LastIndexOf("\",\"inva");
                    blocksFound = Int32.Parse(downloadedBlocks.Substring(blockStart, blocksEnd - blockStart));
                    if (blocksFound != oldBlocksFound)
                    {
                        notifier.ShowSuccess("Your Pool hit a block!");
                        oldBlocksFound = blocksFound;
                    }
                }
            }
        }

        private void CheckCustomPool() //Checking if Custom is selected on pool and opening/closing textbox for custom pool
        {
            if (SelectedPool == "Custom")
            { txbxPool.Visibility = Visibility.Visible; }
            else { txbxPool.Visibility = Visibility.Hidden; }
            
        }

        private void checkingFiles() //Checking if I am in the right folder with miner files
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
                    if (operatingSystem == "64bit")
                    {
                        fileCheck = File.Exists(appPath + "\\ccminer-x64.exe");                                                               //Checking if ccminer exists
                    }
                    else if (operatingSystem == "32bit")
                    {
                        fileCheck = File.Exists(appPath + "\\ccminer.exe");
                    }
                    
                }
                else if (gpuChoice == 3)
                {
                    fileCheck = File.Exists(appPath + "\\ccminer.exe");                                                               //Checking if ccminer for solomining exists
                }
                if (fileCheck == true)
                {
                    txtFileCheck.Text = "Miner found✔";
                    visi = "Hidden";
                    txtninstall.Visibility = Visibility.Hidden;
                    txtninstall2.Visibility = Visibility.Hidden;
                    btnInstall.Visibility = Visibility.Hidden;
                    btnStart.Visibility = Visibility.Visible;
                    btnStart.IsEnabled = true;
                }
                else
                {
                    txtFileCheck.Text = "";
                    visi = "Visible";
                    btnStart.Visibility = Visibility.Hidden;
                }
            }
            
        }

      

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) //Allowing only numbers for nFactor
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void UnZipPool() //Unzipping sgminer or ccminer and moving MisasMinerSetup.exe to the new folder
        {
            if (gpuChoice == 1) //AMD
            {
                string zipPath = appPath + "\\sg.zip";
                string extractPath = appPath + "\\MisasMinerSetup";
                ZipFile.ExtractToDirectory(zipPath, extractPath);                                                   //Extracting sg.zip into new folder called MisasMinerSetup
                File.Delete(appPath + "\\sg.zip");                                                                  //Delete old zipfile
            }
            else if (gpuChoice == 0) //NVIDIA
            {
                string zipPath = appPath + "\\cc.zip";
                string extractPath = appPath + "\\MisasMinerSetup";
                ZipFile.ExtractToDirectory(zipPath, extractPath);                                                   //Extracting cc.zip into new folder called MisasMinerSetup
                File.Delete(appPath + "\\cc.zip");                                                                  //Delete old zipfile
            }
            File.Move(appPath + "\\MisasMinerSetup.exe", appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");   //Move MisasMinerSetup.exe to the new folder
            System.Windows.MessageBox.Show("Download completed! You will now find me inside the folder \"MisasMinerSetup\"."); //Hooray!
            System.Windows.Application.Current.Shutdown();                                                                                         //Close application for restart from the new location
            Process.Start(appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");                                  //Restart
        }

        private void UnZipSolo() //Unzipping sgminer or ccminer and moving MisasMinerSetup.exe to the new folder
        {

            string zipPath = appPath + "\\soloWallet.zip";
            string extractPath = appPath + "\\MisasMinerSetup";
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            File.Delete(appPath + "\\soloWallet.zip");                                                                  //Delete old zipfile

            File.Move(appPath + "\\MisasMinerSetup.exe", appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");               //Move MisasMinerSetup.exe to the new folder
            System.Windows.MessageBox.Show("Wallet downloaded! Continuing with network and wallet setup");                  //Hooray!
            System.Windows.MessageBox.Show("Give me network access to the cmd and do not close it! Just let it be.");
            ConNetwork();
        }

        private void ConNetwork() //Opening network and installing needed files
        {
            //Opening cmd with given arguments
            cmd2.StartInfo.FileName = "cmd.exe";
            cmd2.StartInfo.Arguments = "/K cd " + appPath + "\\MisasMinerSetup\\ && Run-Network.bat";
            cmd2.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            cmd2.Start();       //Open network
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            System.Windows.MessageBox.Show("Press \"ok\"");
            string appdataPathGarli = appdataPath + "\\Garlicoin\\garlicoin.conf";
            File.Move(appPath + "\\MisasMinerSetup\\garlicoin.conf", appdataPathGarli); //Moving config file to the right place in %Appdata%
            Thread.Sleep(500);
            System.Windows.MessageBox.Show("I will throw an error now. Just close it as it restarts and just let it be"); //Didn't make the network files so I cant do anything about the errors. Is what it is.
            Thread.Sleep(500);
            cmd2.CloseMainWindow();
            System.Windows.MessageBox.Show("Press \"ok\" if you closed the error.");
            cmd2.StartInfo.FileName = "cmd.exe";
            cmd2.StartInfo.Arguments = "/K cd " + appPath + "\\MisasMinerSetup\\ && Run-Network.bat";
            cmd2.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            cmd2.Start();                           //Restart network to start downloading blocks
            string output = string.Empty;
            string error = string.Empty;
            Thread.Sleep(1000);
            System.Windows.MessageBox.Show("This will take a while. We are checking if you have downloaded all the blocks needed.");
            string blockCount = "asd";
            string downloadedBlock = "fgh";
            for (int i = 0; blockCount != downloadedBlock; i++) //Checking blockcount and comparing to downloaded blocks
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

                using (WebClient client = new WebClient())
                {
                    downloadedBlock = client.DownloadString("https://explorer.grlc-bakery.fun/api/getblockcount");
                    System.Windows.MessageBox.Show(blockCount + "/" + downloadedBlock + "\nPress ok to check again. Will probably take some time..."); //Showing user how many blocks are downloaded from the needed amount.
                }
            }
            System.Windows.MessageBox.Show("FINISHED! Yayyyy that was too long I know...");
            System.Windows.MessageBox.Show("Let's now setup your wallet! Copy the wallet code from the screen and close the wallet cmd. Do not close the 1st cmd that should still be running in the background. You need to run it whenever you mine or use your wallet");
            cmd3.StartInfo.FileName = "cmd.exe";
            cmd3.StartInfo.Arguments = "/K cd " + appPath + "\\MisasMinerSetup\\ && garlicoin-cli getnewaddress"; //Get local wallet address
            cmd3.Start();
            System.Windows.MessageBox.Show("Remember to back up your wallet.dat file! You can find it in %appdata%/Garlicoin/");
            using (var webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedSoloMiner);
                webClient.DownloadFileAsync(new Uri("http://139.59.147.231/some/CCSoloMiner.zip"), appPath + "\\solominer.zip"); //Downloading Solominer
            }
        }

        private void CompletedSoloMiner(object sender, AsyncCompletedEventArgs e) //After download completion
        {
            string zipPath = appPath + "\\solominer.zip";
            string extractPath = appPath + "\\MisasMinerSetup";
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            File.Delete(appPath + "\\solominer.zip");                                                                  //Delete old zipfile

            System.Windows.MessageBox.Show("Miner Downloaded! You will now find me inside the folder \"MisasMinerSetup\"."); //Hooray!
            System.Windows.Application.Current.Shutdown();                                                                                          //Close application for restart from the new location
            Process.Start(appPath + "\\MisasMinerSetup\\MisasMinerSetup.exe");                                  //Restart
        }

        private void GetGPUType()
        {
            foreach (var g in GPUHardwareNodes)
            {
                string gpuType = g.Type.ToString();
                if (gpuType == "GpuAti")
                {
                    gpuChoice = 1;
                    SelectedAMD();

                }
                else if (gpuType == "GpuNvidia")
                {
                    gpuChoice = 0;
                    SelectedNvidia();
                }
            }
        }

       

        /// <summary>
        /// Used to configure any UI elements that need to be set at time of application start
        /// </summary>
        private void ConfigureUI()
        {
            StatGrid0Temp.Text = "";
            StatGrid1Temp.Text = "";
            StatGrid2Temp.Text = "";
            StatGrid3Temp.Text = "";
            StatGrid4Temp.Text = "";
            StatGrid5Temp.Text = "";
            StatGrid0.Visibility = Visibility.Hidden;
            StatGrid1.Visibility = Visibility.Hidden;
            StatGrid2.Visibility = Visibility.Hidden;
            StatGrid3.Visibility = Visibility.Hidden;
            StatGrid4.Visibility = Visibility.Hidden;
            StatGrid5.Visibility = Visibility.Hidden;
            _statGrids.Add(new StatGrid(StatGrid0, StatGrid0Temp));
            _statGrids.Add(new StatGrid(StatGrid1, StatGrid1Temp));
            _statGrids.Add(new StatGrid(StatGrid2, StatGrid2Temp));
            _statGrids.Add(new StatGrid(StatGrid3, StatGrid3Temp));
            _statGrids.Add(new StatGrid(StatGrid4, StatGrid4Temp));
            _statGrids.Add(new StatGrid(StatGrid5, StatGrid5Temp));
            _hardwareNames.Add(device0Name);
            _hardwareNames.Add(device1Name);
            _hardwareNames.Add(device2Name);
            _hardwareNames.Add(device3Name);
            _hardwareNames.Add(device4Name);
            _hardwareNames.Add(device5Name);
            _checkBoxes.Add(device0);
            _checkBoxes.Add(device1);
            _checkBoxes.Add(device2);
            _checkBoxes.Add(device3);
            _checkBoxes.Add(device4);
            _checkBoxes.Add(device5);
        }

        #region Hardware Monitor

        /// <summary>
        /// Handles cataloging the hardware present on the machine
        /// </summary>
        private void CatalogGPUHardware()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                operatingSystem = "64bit";
            }
            else
            {
                operatingSystem = "32bit";
            }
            computer = new Computer() { GPUEnabled = true };
            computer.Open();

            var hardwareCounter = 0;

            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();

                try
                {
                    GPUHardwareNodes.Add(new GPUHardwareNode(hardware, hardware.HardwareType, hardware.Identifier, hardware.Name, hardware.Sensors, _statGrids[hardwareCounter]));
                    _hardwareNames[hardwareCounter].Text = hardware.Name;
                }
                catch (Exception)
                {
                    // Empty catch - simply ignoring additional hardware for now and setting a hard limit to 6
                }

                hardwareCounter++;
            }

            foreach (var g in GPUHardwareNodes)
            {
                notifier.ShowInformation($"Hardware Found\r\n{g.Name}");
            }

        }      
        
        private void PollHardware() //Checking temperature using OpenHardwareMonitor. Probably going to get this information from the miner API later.
        {
            List<HardwareNodeDisplayOutput> nodeDisplayUpdate = new List<HardwareNodeDisplayOutput>();

            var timer = new System.Timers.Timer() { Enabled = true, Interval = 1000 };

            timer.Elapsed += delegate(object sender, ElapsedEventArgs e)
            {
                CheckBlocksFound();
                MinerInfo();
                MinerInfo2();
                UpdateHover();
                nodeDisplayUpdate.Clear();

                foreach (var hardwareNode in GPUHardwareNodes)
                {
                    var hardwareSensors = hardwareNode.PollSensors();
                    var readableTemp = "";
                    int intTemp = 0;
                    var readableUtilization = ""; //$"{utilizationPercent}%";
                    var brushColor = System.Windows.Media.Brushes.LightBlue;

                    foreach (var sensor in hardwareSensors)
                    {
                        switch (sensor.SensorType)
                        {
                            case SensorType.Voltage:
                                break;
                            case SensorType.Clock:
                                break;
                            case SensorType.Temperature:
                                var sensorTemp = sensor.Value;
                                intTemp = (int)sensorTemp;
                                readableTemp = $"{sensorTemp}°C";
                                break;
                            case SensorType.Load:
                                break;
                            case SensorType.Fan:
                                break;
                            case SensorType.Flow:
                                break;
                            case SensorType.Control:
                                break;
                            case SensorType.Level:
                                break;
                            case SensorType.Factor:
                                break;
                            case SensorType.Power:
                                break;
                            case SensorType.Data:
                                break;
                            case SensorType.SmallData:
                                break;
                        }
                    }

                    nodeDisplayUpdate.Add(new HardwareNodeDisplayOutput(hardwareNode.StatGrid, readableTemp, brushColor, readableUtilization, intTemp));
                }

                Dispatcher.Invoke(() =>
                {
                    foreach (var n in nodeDisplayUpdate)
                    {
                        try
                        {
                            if (n.IntTemp >= Config.Temp - 5)
                            {
                                n.BrushColor = System.Windows.Media.Brushes.Red;
                                if (tempCheck == true)
                                    notifier.ShowWarning("GPU TEMPERATURE WARNING! CURRENT TEMPERATURE " + n.ReadableTemp); //Show temperature warning if user opted in.
                            }
                            n.StatGrid.Temperature.Text = n.ReadableTemp;
                            n.StatGrid.Temperature.Foreground = n.BrushColor;
                        }
                        catch (Exception ex)
                        {
                            // This is an empty catch - intended if the user has more GPUs than is currently supported by the GUI
                            Debug.WriteLine(ex.Message);
                        }
                    }
                });
            };
            for (int i = 0; i < GPUHardwareNodes.Count; i++)
            {
                _checkBoxes[i].IsChecked = true;
            }
        }
        
        #endregion

        #region Hotkey

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HotkeyHelper.Instance.RegisterHotKey(this);
            HotkeyHelper.Instance.OnHotkeyPressed += Instance_OnHotkeyPressed;
        }        

        protected override void OnClosed(EventArgs e)
        {            
            base.OnClosed(e);

            HotkeyHelper.Instance.UnregisterHotKey(this);
        }

        private void Instance_OnHotkeyPressed(object sender, EventArgs e)
        {
            
            notifier.ShowInformation(strHash + "Kh/s");
            notifier.ShowInformation("GPU Fan Speed: " + strFan + "%");
            notifier.ShowInformation(GarlicoinHelper.GetProfitability(strHash) + "$ in 1h");
        }

        #endregion

        #region Checkbox Stuff     

        private void device0_Unchecked(object sender, RoutedEventArgs e)
        {
            bldevice0 = false;
        }

        private void device0_Checked(object sender, RoutedEventArgs e)
        {
            bldevice0 = true;
        }

        private void device1_Checked(object sender, RoutedEventArgs e)
        {
            bldevice1 = true;
        }

        private void device1_Unchecked(object sender, RoutedEventArgs e)
        {
            bldevice1 = false;
        }

        private void device2_Checked(object sender, RoutedEventArgs e)
        {
            bldevice2 = true;
        }

        private void device2_Unchecked(object sender, RoutedEventArgs e)
        {
            bldevice2 = false;
        }

        private void device3_Checked(object sender, RoutedEventArgs e)
        {
            bldevice3 = true;
        }

        private void device3_Unchecked(object sender, RoutedEventArgs e)
        {
            bldevice3 = false;
        }

        private void device4_Checked(object sender, RoutedEventArgs e)
        {
            bldevice4 = true;
        }

        private void device4_Unchecked(object sender, RoutedEventArgs e)
        {
            bldevice4 = false;
        }

        private void device5_Checked(object sender, RoutedEventArgs e)
        {
            bldevice5 = true;
        }

        private void device5_Unchecked(object sender, RoutedEventArgs e)
        {
            bldevice5 = false;
        }

        #endregion
    }
}
