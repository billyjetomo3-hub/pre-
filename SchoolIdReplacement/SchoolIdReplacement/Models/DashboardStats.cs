namespace SchoolIdReplacement.Models
{
    public class DashboardStats
    {
        public int TotalStudents { get; set; }

        public int ActiveStudents { get; set; }

        public int TotalRequests { get; set; }

        public int Pending { get; set; }

        public int Approved { get; set; }

        public int ReadyForPickup { get; set; }

        public int ReleasedThisMonth { get; set; }

        public decimal CollectedThisMonth { get; set; }

        public int UnpaidApproved { get; set; }
    }
}
