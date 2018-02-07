using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MisasMinerSetup
{
    public class StatGrid
    {
        public Grid GridObject;
        public TextBlock Temperature;

        public StatGrid(Grid gridObject, TextBlock temperature)
        {
            GridObject = gridObject;
            Temperature = temperature;
        }
    }
}
