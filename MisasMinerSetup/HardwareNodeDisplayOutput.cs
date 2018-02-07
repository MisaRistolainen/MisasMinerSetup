using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MisasMinerSetup
{
    class HardwareNodeDisplayOutput
    {
        public StatGrid StatGrid;
        public string ReadableTemp;
        public SolidColorBrush BrushColor;
        public int IntTemp;

        public HardwareNodeDisplayOutput(StatGrid statGrid, string readableTemp, SolidColorBrush brushColor, string utilizationPercent, int intTemp)
        {
            StatGrid = statGrid;
            ReadableTemp = readableTemp;
            BrushColor = brushColor;
            IntTemp = intTemp;
        }
    }
}
