using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;
using System.Windows;

namespace MisasMinerSetup
{
    /// <summary>
    /// Class object to define each hardware item we're monitoring from within the application
    /// This will track not only the OpenHardwareMonitor values, but also store the StatGrid to which it will output it's values
    /// </summary>
    public class GPUHardwareNode
    {
        public IHardware Hardware;
        public HardwareType Type;
        public Identifier Ident;
        public String Name;
        private ISensor[] _sensors;
        public StatGrid StatGrid;

        public GPUHardwareNode(IHardware hw, HardwareType hwType, Identifier hwIdent, string hwName, ISensor[] hwSensors, StatGrid statGrid)
        {
            Hardware = hw;
            Type = hwType;
            Ident = hwIdent;
            Name = hwName;
            _sensors = hwSensors;
            StatGrid = statGrid;
            StatGrid.GridObject.Visibility = Visibility.Visible;
            StatGrid.GridObject.ToolTip = hwName;
        }

        public ISensor[] PollSensors()
        {
            Hardware.Update();
            return _sensors;
        }
    }
}
