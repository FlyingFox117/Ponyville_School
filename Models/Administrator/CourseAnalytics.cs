using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonyvilleSchool2._0.Models.Administrator
{
    public class CourseAnalytics
    {
        public int course_id { get; set; }
        public string course_name { get; set; }
        public int total_tasks { get; set; }
        public string most_popular_task { get; set; }
        public string least_resultive_task { get; set; }
        public string most_resultive_task { get; set; }
        public double average_progress { get; set; }
    }
}
