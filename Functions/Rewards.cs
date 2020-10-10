using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormBot.Functions
{
    public class Rewards
    {
        public void GiveSilk(string charname, int silk, int gift_silk, int silk_point)
        {
            Program.event_db.ExecuteCommand("exec _GivRewards '" + charname + "', " + silk + ", " + gift_silk + ", " + silk_point + ", 0, 0, 0, 0, 0");
        }

        public void GiveItem(string charname, int item_id, int quantity, int plus)
        {
            Program.event_db.ExecuteCommand("exec _GivRewards " + charname + ", 0, 0, 0, 0, " + item_id + ", " + quantity + ", " + plus + ", 0");
        }

        public void GiveGold(string charname, int gold)
        {
            Program.event_db.ExecuteCommand("exec _GivRewards " + charname + ", 0, 0, 0, " + gold + ", 0, 0, 0, 0");
        }

        public void GiveTitle(string charname, int title)
        {
            Program.event_db.ExecuteCommand("exec _GivRewards " + charname + ", 0, 0, 0, 0, 0, 0, 0, " + title + "");
        }
    }
}