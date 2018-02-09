using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisasMinerSetup.Configuration
{
    public class MinerConfig
    {
        public string Pool { get; set; }
        public string Wallet { get; set; }
        public int Intensity { get; set; }
        public int NFactor { get; set; }
        public int Setx1 { get; set; }
        public int Setx2 { get; set; }
        public int Setx3 { get; set; }
        public int Setx4 { get; set; }
        public int Setx5 { get; set; }
        public int Temp { get; set; }
        public string SelectedGap { get; set; }
        public string SelectedPool { get; set; }
        public bool FirstRun { get; set; }
        public string LValue { get; set; }       //-l for nvidia   

        public void Save()
        {
            Properties.Settings.Default.Pool = Pool;
            Properties.Settings.Default.Wallet = Wallet;
            Properties.Settings.Default.Inten = Intensity;
            Properties.Settings.Default.nFac = NFactor;
            Properties.Settings.Default.l = LValue;
            Properties.Settings.Default.setx2 = Setx2;
            Properties.Settings.Default.setx3 = Setx3;
            Properties.Settings.Default.setx4 = Setx4;
            Properties.Settings.Default.setx5 = Setx5;
            Properties.Settings.Default.temp = Temp;
            Properties.Settings.Default.selectedgap = SelectedGap;
            Properties.Settings.Default.selectedPool = SelectedPool;
            Properties.Settings.Default.FirstRun = false;
            Properties.Settings.Default.Save();
        }

        public void Load()
        {
            SetDefaultValues();
            
            Pool = Properties.Settings.Default.Pool;                    
            Wallet = Properties.Settings.Default.Wallet;                
            Intensity = Properties.Settings.Default.Inten;              
            NFactor = Properties.Settings.Default.nFac;                 
            Setx2 = Properties.Settings.Default.setx2;                  
            Setx3 = Properties.Settings.Default.setx3;                  
            Setx4 = Properties.Settings.Default.setx4;                  
            Setx5 = Properties.Settings.Default.setx5;                  
            Temp = Properties.Settings.Default.temp;                    
            SelectedGap = Properties.Settings.Default.selectedgap;      
            SelectedPool = Properties.Settings.Default.selectedPool;    
            FirstRun = Properties.Settings.Default.FirstRun;            
        }

        public void SetDefaultValues()
        {
            Intensity = 15;
            NFactor = 11;
            Temp = 85;
            Setx1 = 0;
            Setx2 = 100;
            Setx3 = 100;
            Setx4 = 100;
            Setx5 = 1;
            LValue = "Auto";
        }
    }
}
