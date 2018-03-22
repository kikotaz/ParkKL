using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace com.parkkl.intro.Models
{
    class WalletModel
    {
        public String transId { get; set; }
        public String amount { get; set; }
        public String dateTime { get; set; }
        public String remarks { get; set; }
        public String status { get; set; }
    }
}