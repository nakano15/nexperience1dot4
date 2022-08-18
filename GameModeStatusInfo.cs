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

        public GameModeStatusInfo(string Name, string Description, string ShortName = "")
        {
            this.Name = Name;
            this.Description = Description;
            this.ShortName = ShortName;
        }

        public GameModeStatusInfo()
        {
            
        }
    }
}
