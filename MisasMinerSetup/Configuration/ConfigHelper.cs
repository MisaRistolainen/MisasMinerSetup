using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisasMinerSetup.Configuration
{
    public class ConfigHelper
    {
        #region Singleton

        private static ConfigHelper instance;

        private ConfigHelper() { }

        public static ConfigHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConfigHelper();
                }

                return instance;
            }
        }

        #endregion

        public MinerConfig LoadConfig()
        {
            var config = new MinerConfig();
            config.SetDefaultValues();

            config.Pool = Properties.Settings.Default.Pool;                    //Loading saved custom pool address
            config.Wallet = Properties.Settings.Default.Wallet;                //Loading saved wallet address
            config.Intensity = Properties.Settings.Default.Inten;              //Loading saved intensity
            config.NFactor = Properties.Settings.Default.nFac;                       //Loading saved nFactor
            config.Setx2 = Properties.Settings.Default.setx2;                  //Loading saved setx values
            config.Setx3 = Properties.Settings.Default.setx3;                  //Loading saved setx values
            config.Setx4 = Properties.Settings.Default.setx4;                  //Loading saved setx values
            config.Setx5 = Properties.Settings.Default.setx5;                  //Loading saved setx values
            config.Temp = Properties.Settings.Default.temp;                    //Loading saved maxtemp
            config.SelectedGap = Properties.Settings.Default.selectedgap;      //Loading saved lookup-gap
            config.SelectedPool = Properties.Settings.Default.selectedPool;    //Loading saved pool
            config.FirstRun = Properties.Settings.Default.FirstRun;            //Loading whether first time running application

            return config;
        }

        public void SaveConfig(MinerConfig config)
        {
            Properties.Settings.Default.Pool = config.Pool;
            Properties.Settings.Default.Wallet = config.Wallet;
            Properties.Settings.Default.Inten = config.Intensity;
            Properties.Settings.Default.nFac = config.NFactor;
            Properties.Settings.Default.l = config.LValue;
            Properties.Settings.Default.setx2 = config.Setx2;
            Properties.Settings.Default.setx3 = config.Setx3;
            Properties.Settings.Default.setx4 = config.Setx4;
            Properties.Settings.Default.setx5 = config.Setx5;
            Properties.Settings.Default.temp = config.Temp;
            Properties.Settings.Default.selectedgap = config.SelectedGap;
            Properties.Settings.Default.selectedPool = config.SelectedPool;
            Properties.Settings.Default.FirstRun = false;
            Properties.Settings.Default.Save();
        }
    }
}
