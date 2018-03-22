using System;

using Android.App;
using Android.OS;
using Android.Widget;
using com.parkkl.intro.Models;
using Android.Support.V7.App;
using Microsoft.WindowsAzure.MobileServices;
using com.parkkl.intro.Services;
using System.Collections.Generic;
using com.parkkl.intro.Adapter;
using System.Threading.Tasks;

namespace com.parkkl.intro.ParkingUser
{
    [Activity(Label = "Officer", Theme = "@style/MyTheme.Base")]
    public class OfficerActivity : AppCompatActivity
    {
        private SearchView sv;
        private List<ParkingModel> mList;
        private ListView mListview;
        private ParkingHistoryAdapter adapter;

        //Getting an instance of Azure service client (Singleton instance)
        MobileServiceClient client = SingletonClient.getInstance();
        User user = new User();
        Parking parking = new Parking();
        Penalty penalty = new Penalty();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.officer);

            displayParking();

            var refreshBtn = FindViewById<Button>(Resource.Id.btn_refresh);

            refreshBtn.Click += delegate
            {
                RefreshActivity();
            };

        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        public void RefreshActivity()
        {
            Finish();
            OverridePendingTransition(0, 0);
            StartActivity(Intent);
            OverridePendingTransition(0, 0);
            displayParking();
        }

        public async void displayParking()
        {
            //Find the listview reference
            mListview = FindViewById<ListView>(Resource.Id.parking_list);

            mList = await GetParkingList();

            adapter = new ParkingHistoryAdapter(this, mList);

            //Create our adapter
            mListview.Adapter = adapter;

            //Wire up the click event
            mListview.ItemClick += MListview_ItemClick;

            sv = FindViewById<SearchView>(Resource.Id.sv);

            //EVENTS
            sv.QueryTextChange += Sv_QueryTextChange;
        }

        void MListview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var mList = adapter.GetItemAtPosition(e.Position);

            if (mList.parkingStat != "" && mList.parkingStat != "Penalty issued")
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Confirm Penalty");
                alert.SetMessage("Penalty for " + mList.carPlateNumber + "\n" +
                    "" + "\n" +
                    mList.parkingStat + "\n" +
                    "Total Penalty: " + "RM " + 150 + "\n" +
                    "" + "\n" +
                    "");
                alert.SetPositiveButton("Yes", async (senderAlert, args) =>
                {
                    var parkingDialog = ProgressDialog.Show(this, "Please wait...",
                                "Creating new penalty...", true);
         
                    //Initializing the parking table
                    IMobileServiceTable<Penalty> penaltyTable = client.GetTable<Penalty>();
                    IMobileServiceTable<Parking> parkingTable = client.GetTable<Parking>();

                    var currentParking = await parkingTable.LookupAsync(mList.parkingId);
                    DateTime startTime = DateTime.Parse(Convert.ToString(currentParking.CreatedAt));
                    DateTime currentTime = DateTime.Now;
                    DateTime expectedEnd = startTime.AddHours(Convert.ToDouble(currentParking.ParkedHours));
                    TimeSpan timediff = currentTime.Subtract(expectedEnd);

                    penalty.Email = mList.email;
                    penalty.ParkingId = mList.parkingId;
                    penalty.ExceededHours =  timediff.ToString(@"hh\:mm");
                    penalty.TotaAmount = 150;
                    //Inserting new parking to the Database
                    await penaltyTable.InsertAsync(penalty);

                    currentParking.HasPenalty = true;
                    await parkingTable.UpdateAsync(currentParking);

                    Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                    alertSuccess.SetTitle("Parking Area");
                    alertSuccess.SetMessage("Penalty is issued successful");
                    alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                    {
                        RefreshActivity();
                    });
                    Dialog dialogSuccess = alertSuccess.Create();
                    dialogSuccess.Show();

                });
                alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else if (mList.parkingStat == "Penalty issued")
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Can not issue new penalty");
                alert.SetMessage("This parking has a penalty issued. Can not issue two penalties");
                alert.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                {

                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Parking is legal");
                alert.SetMessage("This parking is still within its legal parking session. "
                    + "Can not issue penalty for this parking");
                alert.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                {

                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        public void Sv_QueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            adapter.Filter.InvokeFilter(e.NewText);
            
        }

        public async Task<List<ParkingModel>> GetParkingList()
        {
            mListview = FindViewById<ListView>(Resource.Id.parking_list);

            mList = new List<ParkingModel>();

            IMobileServiceTable<Parking> parkingData = client.GetTable<Parking>();

            //Getting the Wallet details from the Database
            List<Parking> parkingList = await parkingData.Where
                (item => item.CarPlateNumber != "").ToListAsync();

            if (parkingList.Count > 0)
            {
                foreach (var item in parkingList)
                {
                    DateTime startTime = DateTime.Parse(Convert.ToString(item.CreatedAt));
                    DateTime currentTime = DateTime.Now;
                    string issuePenalty;

                    if (startTime.Date == DateTime.Today)
                    {
                        DateTime expectedEnd = startTime.AddHours(Convert.ToDouble(item.ParkedHours));

                        if (currentTime > expectedEnd)
                        {
                            if (item.HasPenalty == true)
                            {
                                issuePenalty = "Penalty issued";
                            }
                            else
                            {
                                TimeSpan timediff = currentTime.Subtract(expectedEnd);

                                issuePenalty = "Over stay by " + timediff.ToString(@"hh\:mm") +
                                    " hours.";
                            }
                        }
                        else
                        {
                            issuePenalty = "";
                        }

                        mList.Add(new ParkingModel()
                        {
                            parkingId = item.Id,
                            parkingStat = issuePenalty,
                            locationName = item.LocationName,
                            chargePerHour = "Charge Per Hour: RM " + Convert.ToString(item.ChargePerHour) + ".00",
                            parkedHours = "Parked Hours: " + Convert.ToString(item.ParkedHours),
                            date = "Date & Time Started: " + startTime.ToString("HH:mm"),
                            carPlateNumber = "Vehicle Number: " + item.CarPlateNumber,
                            totaAmount = "Total: RM " + Convert.ToString(item.TotaAmount) + ".00",
                            email = item.Email
                        });
                    }
                }
            }
            else
            {
                mList = null;
            }
            return mList;
        }
    }
}