using System;

namespace SchoolIdReplacement.Models
{
    public class IdRequest
    {
        public int Id { get; set; }

        public string RequestNo { get; set; }

        public int StudentId { get; set; }

        public string StudentNo { get; set; }

        public string StudentName { get; set; }

        public string RequestType { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; }

        public decimal FeeAmount { get; set; }

        public string PaymentStatus { get; set; }

        public string OrNumber { get; set; }

        public string Remarks { get; set; }

        public DateTime DateRequested { get; set; }

        public DateTime? DateUpdated { get; set; }

        public DateTime? DateReleased { get; set; }

        public string ProcessedBy { get; set; }
    }
}
