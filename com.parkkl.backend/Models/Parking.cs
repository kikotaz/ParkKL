using System;
using System.ComponentModel;
using Microsoft.Azure.Mobile.Server;

namespace com.parkkl.backend.Models
{
    public class Parking : EntityData
    {
        public string Email { get; set; }
        public string LocationName { get; set; }
        public int ChargePerHour { get; set; }
        public int ParkedHours { get; set; }
        public string CarPlateNumber { get; set; }
        public int TotaAmount { get; set; }

        [DefaultValue(false)]
        public bool HasPenalty { get; set; }
    }
}