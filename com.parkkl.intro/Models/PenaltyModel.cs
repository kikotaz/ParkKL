using System;

namespace com.parkkl.intro.Models
{
    class PenaltyModel
    {
        public String penaltyId { get; set; }
        public String locationName { get; set; }
        public String carPlateNumber { get; set; }
        public String chargePerHour { get; set; }
        public String bookedTime { get; set; }
        public String excessTime { get; set; }
        public String date { get; set; }
        public String penaltyDueDate { get; set; }
        public String penaltyAmount { get; set; }
        public String penaltyPaid { get; set; }
    }
}