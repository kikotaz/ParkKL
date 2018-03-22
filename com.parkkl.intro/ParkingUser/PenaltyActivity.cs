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
    [Activity(Label = "Penalty", Theme = "@style/MyTheme.Base")]
    public class PenaltyActivity : Android.Support.V7.App.AppCompatActivity
    {
        private List<PenaltyModel> mList;
        private ListView mListview;

        //Getting an instance of Azure service client (Singleton instance)
        MobileServiceClient client = SingletonClient.getInstance();
        User user = new User();
        Penalty penalty = new Penalty();
        Wallet wallet = new Wallet();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.penalty);

            user = JsonConvert.DeserializeObject<User>(Intent.GetStringExtra("User"));

            displayPenalty();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        public async void displayPenalty()
        {
            //Find the listview reference
            mListview = FindViewById<ListView>(Resource.Id.penalty_listview);

            mList = new List<PenaltyModel>();

            IMobileServiceTable<Penalty> penaltyData = client.GetTable<Penalty>();
            IMobileServiceTable<Parking> parkingData = client.GetTable<Parking>();

            //Getting the Wallet details from the Database
            List<Penalty> penaltyIssued = await penaltyData.Where
                (item => item.Email == client.CurrentUser.UserId).ToListAsync();

            foreach (var item in penaltyIssued)
            {
                Parking parking = await parkingData.LookupAsync(item.ParkingId);

                DateTime penaltyDate = DateTime.Parse(Convert.ToString(item.CreatedAt));

                string paymentStatus;
                if (item.PenaltyPaid == false)
                {
                    paymentStatus = "unpaid. Please click to pay";
                }
                else
                {
                    paymentStatus = "paid";
                }
                mList.Add(new PenaltyModel()
                {
                    locationName = "Location: " + parking.LocationName,
                    carPlateNumber = "Plate number: " + parking.CarPlateNumber,
                    date = "Issued on " + penaltyDate,
                    excessTime = "Excess Time: " + item.ExceededHours + " hours",
                    penaltyAmount = "Penalty Amount: " + Convert.ToString(item.TotaAmount),
                    penaltyPaid = "This penalty is " + paymentStatus,
                    penaltyId = item.Id
                });
            }

            //Create our adapter
            mListview.Adapter = new PenaltyAdapter(this, mList);

            //Wire up the click event
            mListview.ItemClick += MListview_ItemClick;
        }

        void MListview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (mList[e.Position].penaltyPaid.Contains("unpaid"))
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Confirm Penalty Payment");
                alert.SetMessage("\n" + mList[e.Position].date + "\n" +
                    mList[e.Position].carPlateNumber + "\n" +
                    mList[e.Position].excessTime + "\n" +
                    "Total Penalty: RM 150" + "\n\n" +
                    "** Penalty is fixed and does not depend on time exceeeded.");
                alert.SetPositiveButton("Yes", async (senderAlert, args) =>
                {
                //Check if wallet has enough balance
                IMobileServiceTable<Wallet> walletData = client.GetTable<Wallet>();

                //Getting the Wallet details from the Database
                List<Wallet> currentBalance = await walletData.Where
                        (item => item.Email == client.CurrentUser.UserId).ToListAsync();

                    if (currentBalance.Count > 0)
                    {
                        wallet = currentBalance[0];

                        if (Convert.ToInt32(wallet.Balance) < 150)
                        {
                            Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                            alertSuccess.SetTitle("Penalty Payment");
                            alertSuccess.SetMessage("Insuficient funds!" + "\n"
                                + "Your wallet is RM " + wallet.Balance + ".00" +
                                "\n" + "Please, top up");
                            alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                            {
                                Intent topUpActivityIntent = new Intent(Application.Context,
                                typeof(TopUpActivity));
                                topUpActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                                this.StartActivity(topUpActivityIntent);
                            });
                            Dialog dialogSuccess = alertSuccess.Create();
                            dialogSuccess.Show();
                        }
                        else
                        {
                            var parkingDialog = ProgressDialog.Show(this, "Please wait...",
                                    "Processing penalty payment...", true);

                        //Initializing the parking table
                        IMobileServiceTable<Penalty> penaltyTable = client.GetTable<Penalty>();

                            penalty = await penaltyTable.LookupAsync(mList[e.Position].penaltyId);

                            penalty.PenaltyPaid = true;

                        //Delete the penalty from the database
                        await penaltyTable.UpdateAsync(penalty);

                            var extraParameters = new Dictionary<string, string>();
                            extraParameters.Add("userId", user.Id);
                            extraParameters.Add("type", penalty.Id);
                            extraParameters.Add("parkingId", penalty.ParkingId);

                        //Update the wallet balance                      
                        wallet.Balance = Convert.ToString(Convert.ToInt32(wallet.Balance) - 150);
                            await walletData.UpdateAsync(wallet, extraParameters);


                            Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                            alertSuccess.SetTitle("Penalty Payment");
                            alertSuccess.SetMessage("Payment is successful");
                            alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                            {
                                RefreshActivity();
                            });
                            Dialog dialogSuccess = alertSuccess.Create();
                            dialogSuccess.Show();
                        }
                    }
                });
                alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else
            {
                Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                alertSuccess.SetTitle("Penalty is already paid");
                alertSuccess.SetMessage("This penalty is paid. You can not pay two times "+
                    "for the same penalty");
                alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                {
                    
                });
                Dialog dialogSuccess = alertSuccess.Create();
                dialogSuccess.Show();
            }
        }

        public void RefreshActivity()
        {
            Finish();
            OverridePendingTransition(0, 0);
            StartActivity(Intent);
            OverridePendingTransition(0, 0);
            displayPenalty();
        }
    }
}