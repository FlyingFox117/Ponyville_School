using MvvmHelpers;
using PonyvilleSchool2._0.Models.Administrator;
using PonyvilleSchool2._0.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonyvilleSchool2._0.ViewModels.Administrator
{
    public class AnalyticsViewModel : BaseViewModel
    {
        public ObservableCollection<CourseAnalytics> Courses { get; set; } = new();

        private CourseAnalytics _selectedCourse;
        public CourseAnalytics SelectedCourse
        {
            get => _selectedCourse;
            set { _selectedCourse = value; OnPropertyChanged(); }
        }

        public AnalyticsViewModel()
        {
            LoadCourses();
        }

        private async void LoadCourses()
        {
            var result = await AppState.Instance.Supabase.GetCourseAnalytics();
            foreach (var c in result)
                Courses.Add(c);

            SelectedCourse = Courses.FirstOrDefault();
        }
    }
}
