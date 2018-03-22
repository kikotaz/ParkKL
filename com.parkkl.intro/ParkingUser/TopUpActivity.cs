using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Stripe;
using com.parkkl.intro.Models;
using Microsoft.WindowsAzure.MobileServices;
using com.parkkl.intro.Services;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace com.parkkl.intro.ParkingUser
{
    //Top up activity class which is responsible for adding cash to the user wallet
    [Activity(Label = "Top Up", Theme = "@style/MyTheme.Base")]
    public class TopUpActivity : Android.Support.V7.App.AppCompatActivity
    {
        //Getting an instance of Azure service client (Singleton instance)
        MobileServiceClient client = SingletonClient.getInstance();

        //Creating new instances of user and wallet
        User user = new User();
        Wallet wallet = new Wallet();

        //Method responsible for creating and initializing the activity
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.topup);

            //Deserializing user info that is passed from the wallet activity
            user = JsonConvert.DeserializeObject<User>(Intent.GetStringExtra("User"));

            //Defining the top up btton and the amount field
            var topUpBtn = FindViewById<Button>(Resource.Id.btn_top_up);
            var inputAmount = FindViewById<EditText>(Resource.Id.input_amount);
            
            //Processing user request when top up button is clicked
            topUpBtn.Click += delegate
            {
                string amount = inputAmount.Text;

                //Checking if the amount field has input
                if (!String.IsNullOrEmpty(amount))
                {
                    int iAmount = int.Parse(inputAmount.Text);
                    checkAmount(iAmount);
                }

                //If the amount field is empty an error message will show
                else
                {
                    Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                    alertSuccess.SetTitle("Top Up");
                    alertSuccess.SetMessage("Field is empty");
                    alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                    {

                    });
                    Dialog dialogSuccess = alertSuccess.Create();
                    dialogSuccess.Show();
                }
                
            };
            
        }

        //Method to check the amount is more than 50 RM
        public void checkAmount(int amount)
        {
            if(amount < 50)
            {
                //The error message to be shown
                Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                alertSuccess.SetTitle("Top Up");
                alertSuccess.SetMessage("Minimum amount is RM 50");
                alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                {
                    
                });
                Dialog dialogSuccess = alertSuccess.Create();
                dialogSuccess.Show();
            }
            else
            {
                //Call the top up method that will validate the payment
                onTopUpClick(amount);
            }
        }

        //The top up method that will validate the user payment
        public async void onTopUpClick(int amount)
        {
            //Creating indicator object
            var topUpDialog = ProgressDialog.Show(this, "Please wait...",
                "Validating your payment...", true);

            try
            {
                var amountText = FindViewById<EditText>(Resource.Id.input_amount);

                // Use Stripe's library to make request
                StripeConfiguration.SetApiKey("sk_test_Ep5V2ffFq69TP6cDSEf71fr2");

                var chargeOptions = new StripeChargeCreateOptions()
                {
                    Amount = amount,
                    Currency = "usd",
                    Description = "Top Up",
                    SourceTokenOrExistingSourceId = "tok_amex" // obtained with Stripe.js
                };

                //Creating charge service object that will validate the banking card
                var chargeService = new StripeChargeService();
                StripeCharge charge = chargeService.Create(chargeOptions);

                if (charge.Paid)
                {
                    //Initializing the user table
                    IMobileServiceTable<Wallet> walletTable = client.GetTable<Wallet>();

                    //Getting the Wallet details from the Database
                    List<Wallet> currentBalance = await walletTable.Where
                        (item => item.Email == client.CurrentUser.UserId).ToListAsync();
                    
                    //Checking if the user has a wallet already
                    if (currentBalance.Count > 0)
                    {
                        wallet = currentBalance[0];

                        //Adding the top up to the previous wallet balance
                        wallet.Balance = Convert.ToString(Convert.ToInt32(wallet.Balance) + Convert.ToInt32(amountText.Text));

                        //Creating important extra parameters for the backend request
                        var extraParameters = new Dictionary<string, string>();
                        extraParameters.Add("userId", user.Id);
                        extraParameters.Add("type", "topUp");
                        extraParameters.Add("parkingId", "1");

                        //Sending update wallet request to the backend
                        await walletTable.UpdateAsync(wallet, extraParameters);

                        //Showing success message
                        Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                        alertSuccess.SetTitle("Top Up");
                        alertSuccess.SetMessage("Top is successful");
                        alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                        {
                            Intent walletActivityIntent = new Intent(Application.Context,
                            typeof(WalletActivity));
                            walletActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                            this.StartActivity(walletActivityIntent);
                            Finish();
                        });
                        Dialog dialogSuccess = alertSuccess.Create();
                        dialogSuccess.Show();
                    }

                    //If the user has no wallet, it will be created
                    else
                    {
                        //Assigning wallet Email to the user and creating new balance
                        wallet.Email = user.Email;
                        wallet.Balance = amountText.Text;

                        //Creating necessary extra parameters for the backend request
                        Dictionary<string, string> extraParameters =
                            new Dictionary<string, string>();

                        extraParameters.Add("Id", user.Id);
                        extraParameters.Add("type", "topUp");
                        extraParameters.Add("parkingId", "1");

                        //Inserting new wallet to the Database
                        await walletTable.InsertAsync(wallet, extraParameters);

                        //Creating success message
                        Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                        alertSuccess.SetTitle("Top Up");
                        alertSuccess.SetMessage("Top is successful");
                        alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                        {

                            Intent walletActivityIntent = new Intent(Application.Context,
                            typeof(WalletActivity));
                            walletActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                            this.StartActivity(walletActivityIntent);
                            Finish();
                        });
                        Dialog dialogSuccess = alertSuccess.Create();
                        dialogSuccess.Show();
                    }                  
                }
            }
            //All possible exceptions from Stripe service
            catch (StripeException e)
            {
                switch (e.StripeError.ErrorType)
                {
                    case "card_error":
                        Console.WriteLine("   Code: " + e.StripeError.Code);
                        Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                    case "api_connection_error":
                        break;
                    case "api_error":
                        break;
                    case "authentication_error":
                        break;
                    case "invalid_request_error":
                        break;
                    case "rate_limit_error":
                        break;
                    case "validation_error":
                        break;
                    default:
                        // Unknown Error Type
                        break;
                }
            }
        }
    }
}