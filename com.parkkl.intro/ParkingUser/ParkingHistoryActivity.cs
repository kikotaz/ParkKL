using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using com.parkkl.intro.Models;
using com.parkkl.intro.Adapter;
using Microsoft.WindowsAzure.MobileServices;
using com.parkkl.intro.Services;
using Newtonsoft.Json;

namespace com.parkkl.intro.ParkingUser
{
    [Activity(Label = "Parking History", Theme = "@style/MyTheme.Base")]
    public class ParkingHistoryActivity : Android.Support.V7.App.AppCompatActivity
    {
        private List<ParkingModel> mList;
        private ListView mListview;

        //Getting an instance of Azure service client (Singleton instance)
        MobileServiceClient client = SingletonClient.getInstance();
        User user = new User();
        Parking parking = new Parking();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.parking_history);

            user = JsonConvert.DeserializeObject<User>(Intent.GetStringExtra("User"));

            displayParking();
        }

        private void MListview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var message = mList[e.Position].parkingId;
            Toast.MakeText(this, message, ToastLength.Long).Show();
        }

        public async void displayParking()
        {
            mListview = FindViewById<ListView>(Resource.Id.parking_history);
            mList = await createListAsync();
            ParkingHistoryAdapter adapter = new ParkingHistoryAdapter(this, mList);
            mListview.Adapter = adapter;
            mListview.ItemClick += MListview_ItemClick;
        }

        private async System.Threading.Tasks.Task<List<ParkingModel>> createListAsync()
        {
            var list = new List<ParkingModel>();

            
            IMobileServiceTable<Parking> parkingData = client.GetTable<Parking>();

            
            List<Parking> parkingHistory = await parkingData.Where
                (item => item.Email == client.CurrentUser.UserId).ToListAsync();

            foreach (var item in parkingHistory)
            {
                DateTime parkingTime = DateTime.Parse(Convert.ToString(item.CreatedAt));
                list.Add(new ParkingModel()
                {
                    locationName = item.LocationName,
                    chargePerHour = "Charge Per Hour: RM " + Convert.ToString(item.ChargePerHour) + ".00",
                    parkedHours = "Parked Hours: " + Convert.ToString(item.ParkedHours),
                    date = "Date & Time Created: " + parkingTime.ToString("g"),
                    carPlateNumber = "Vehicle Number: " + item.CarPlateNumber,
                    totaAmount = "Total: RM " + Convert.ToString(item.TotaAmount) + ".00"
                });
            }

            list.Reverse();

            return list;
        }
    }
}