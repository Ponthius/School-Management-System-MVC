namespace School_Management_System.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Stat cards
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public decimal TotalFeesCollected { get; set; }

        // Sub-labels on cards
        public int GradeLevels { get; set; }
        public string StudentRatio { get; set; } = string.Empty;
        public int RoomOccupancyPercent { get; set; }
        public int MonthlyGoalPercent { get; set; }

        // Academic Pulse
        public int DailyAttendancePercent { get; set; }
        public int StaffAvailabilityPercent { get; set; }

        // Recent activity table
        public List<ActivityLog> RecentActivity { get; set; } = new();

        // Upcoming events (static for now)
        public List<UpcomingEvent> UpcomingEvents { get; set; } = new();
    }

    public class UpcomingEvent
    {
        public string Month { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}