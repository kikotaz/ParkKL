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
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using com.parkkl.intro.Services;

namespace com.parkkl.intro.ParkingUser
{
    [Activity(Label = "My Wallet", Theme = "@style/MyTheme.Base")]
    public class WalletActivity : Android.Support.V7.App.AppCompatActivity
    {
        //Getting an instance of Azure service client (Singleton instance)
        MobileServiceClient client = SingletonClient.getInstance();
        User user = new User();
        Parking parking = new Parking();
        Wallet wallet = new Wallet();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.wallet);

            user = JsonConvert.DeserializeObject<User>(Intent.GetStringExtra("User"));

            showWalletBalance();

            var topUpBtn = FindViewById<Button>(Resource.Id.btn_top_up);

            topUpBtn.Click += delegate
            {
                onTopUpBtnClick();
            };
        }

        public void onTopUpBtnClick()
        {
            Intent topUpActivityIntent = new Intent(Application.Context,
                    typeof(TopUpActivity));
            topUpActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
            this.StartActivity(topUpActivityIntent);
            Finish();
        }

        public async void showWalletBalance()
        {
            //Check if wallet has enough balance
            IMobileServiceTable<Wallet> walletData = client.GetTable<Wallet>();

            //Getting the Wallet details from the Database
            List<Wallet> currentBalance = await walletData.Where
                (item => item.Email == client.CurrentUser.UserId).ToListAsync();
            

            var balanceTextView = FindViewById<TextView>(Resource.Id.balance);
            if (currentBalance.Count > 0)
            {
                wallet = currentBalance[0];
                balanceTextView.Append("Current Balance: RM " + wallet.Balance + ".00");
            } else
            {
                wallet.Balance = "0";
                balanceTextView.Append("Current Balance: RM " + wallet.Balance + ".00");
            }
        }

    }
}