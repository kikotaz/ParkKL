using System;
using System.ComponentModel;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;

namespace com.parkkl.backend.Models
{
    public class Penalty : EntityData
    {
        public string Email { get; set; }
        public string ExceededHours { get; set; }
        public int TotaAmount { get; set; }

        [DefaultValue(false)]
        public bool PenaltyPaid { get; set; }
        public string ParkingId { get; set; }

        public static implicit operator Penalty(Delta<Penalty> v)
        {
            throw new NotImplementedException();
        }
    }
}