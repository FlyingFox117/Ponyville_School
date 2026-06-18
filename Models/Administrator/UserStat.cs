using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonyvilleSchool2._0.Models.Administrator
{
    public class UserStat
    {
        public int user_id { get; set; }
        public string login { get; set; }
        public string username { get; set; }
        public int xp { get; set; }
        public int total_attempts { get; set; }
        public double avg_score { get; set; }
        public int unique_tasks { get; set; }
    }
}
