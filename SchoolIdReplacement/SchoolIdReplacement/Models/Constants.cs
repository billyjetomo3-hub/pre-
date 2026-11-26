namespace SchoolIdReplacement.Models
{
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Student = "Student";
    }

    public static class RequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string ReadyForPickup = "Ready for Pickup";
        public const string Released = "Released";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All =
        {
            Pending,
            Approved,
            ReadyForPickup,
            Released,
            Rejected,
            Cancelled
        };
    }

    public static class PaymentStatus
    {
        public const string Unpaid = "Unpaid";
        public const string Paid = "Paid";
    }

    public static class RequestTypes
    {
        public const string Lost = "Lost ID";
        public const string Damaged = "Damaged ID";
        public const string Stolen = "Stolen ID";
        public const string NameCorrection = "Name Correction";
        public const string InformationUpdate = "Information Update";
        public const string PhotoRetake = "Photo Retake";

        public static readonly string[] All =
        {
            Lost,
            Damaged,
            Stolen,
            NameCorrection,
            InformationUpdate,
            PhotoRetake
        };

        public static decimal FeeFor(string requestType)
        {
            switch (requestType)
            {
                case Lost:
                    return 150m;
                case Damaged:
                    return 100m;
                case Stolen:
                    return 175m;
                case NameCorrection:
                    return 120m;
                case InformationUpdate:
                    return 120m;
                case PhotoRetake:
                    return 80m;
                default:
                    return 100m;
            }
        }

        public static string DescriptionFor(string requestType)
        {
            switch (requestType)
            {
                case Lost:
                    return "The ID was lost. The student must present an affidavit of loss to the registrar.";
                case Damaged:
                    return "The ID is damaged or unreadable. Bring the old card when claiming the replacement.";
                case Stolen:
                    return "The ID was stolen. Attach a police or barangay report number in the reason field.";
                case NameCorrection:
                    return "Correction of a misspelled name. Supporting documents are required.";
                case InformationUpdate:
                    return "Update of course, year level or section details.";
                case PhotoRetake:
                    return "New photograph for the student ID card.";
                default:
                    return string.Empty;
            }
        }
    }
}
