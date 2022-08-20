using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nexperience1dot4
{
    public class GameModeStatusInfo
    {
        public string GetShortName { get { if(ShortName != "") return ShortName; return Name; } }
        public string GetName { get { return Name; } }
        public string GetDescription { get { return Description; } }
        private string Name = "", ShortName = "", Description = "";
        private byte InitialStatusPoints = 0;
        private int MaxStatusPoints = int.MaxValue;

        public GameModeStatusInfo(string Name, string Description, string ShortName = "", byte InitialStatusPoints = 0, int MaxStatusPoints = int.MaxValue)
        {
            this.Name = Name;
            this.Description = Description;
            this.ShortName = ShortName;
            this.InitialStatusPoints = InitialStatusPoints;
            this.MaxStatusPoints = MaxStatusPoints;
        }

        public GameModeStatusInfo()
        {
            
        }
    }
}
