using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using com.parkkl.intro.Models;
using com.parkkl.intro.Services;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace com.parkkl.intro.ParkingUser
{
    [Activity(Label = "Sign In")]
    public class SignInActivity : Activity
    {
        //Getting an instance of Azure service client (Singleton instance)
        MobileServiceClient client = SingletonClient.getInstance();
        User user = new User();

        //Default method when the activity page is created
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.sign_in);

            //Setting the E-mail field to be E-mail type for easiness
            var emailText = FindViewById<EditText>(Resource.Id.input_email);
            emailText.InputType = Android.Text.InputTypes.TextVariationEmailAddress;

            //Instantiating Register new account link and login button
            var registerLink = FindViewById<TextView>(Resource.Id.link_signup);
            var loginBtn = FindViewById<Button>(Resource.Id.btn_login);

            //Calling registration method when register link is clicked
            registerLink.Click += delegate
            {
                onRegisterLinkClick();
            };

            //Calling login method when login button is clicked
            loginBtn.Click += delegate
            {
                onLoginClick();
            };
        }


        //This method will show the sign up activity when register link is clicked
        public void onRegisterLinkClick()
        {
            StartActivity(new Intent(Application.Context, typeof(SignUpActivity)));
        }

        //This method will process the login request from the user
        public async void onLoginClick()
        {
            //Declaration of all the text fields in the activity
            var emailText = FindViewById<EditText>(Resource.Id.input_email);
            var passwordText = FindViewById<EditText>(Resource.Id.input_password);

            //Collecting all the fields in one list for validation
            List<EditText> fields = new List<EditText>();
            fields.Clear();
            fields.Add(emailText);
            fields.Add(passwordText);

            //New instance of the validator
            var validator = new Validator();

            //Validating empty fields by validator object
            try
            {
                validator.checkEmpty(fields);
            }
            catch (MissingFieldException mfe)
            {
                //Converting the fieldID from the exception message
                int fieldID = int.Parse(mfe.Message);

                //Show error message at the empty field and focus
                fields[fieldID].SetError("This field can't be empty", null);
                fields[fieldID].RequestFocus();
                return;
            }

            //Validating E-mail format
            try
            {
                validator.checkValidEmail(emailText);
            }
            catch (FormatException)
            {
                //Show error message if the E-mail format is faulty and focus
                emailText.SetError("Please provide valid email", null);
                emailText.RequestFocus();
                return;
            }

            //Creating indicator object
            var loginDialog = ProgressDialog.Show(this, "Please wait...",
                "Loggin in...", true);

            //Setting the inputs after validation
            user.Email = emailText.Text;
            user.Password = passwordText.Text;

            //Authenticating user from Azure Web App Service
            try
            {
                await client.LoginAsync("custom", JObject.FromObject(user));
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                //This exception if the user provided wrong credintials
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    AlertDialog.Builder error = new AlertDialog.Builder(this);
                    error.SetTitle("Wrong Credentials");
                    error.SetNeutralButton("OK",
                        (EventHandler<DialogClickEventArgs>)null);
                    error.SetMessage("Wrong E-mail or Password. Try again!");
                    error.Create().Show();

                    loginDialog.Cancel();
                    return;
                }
                else
                {
                    //This exception if the user e-mail is not confirmed
                    AlertDialog.Builder error = new AlertDialog.Builder(this);
                    error.SetTitle("Unconfirmed");
                    error.SetNeutralButton("OK",
                        (EventHandler<DialogClickEventArgs>)null);
                    error.SetMessage("Your E-mail needs confirmation, "
                        + "please check your E-mail");
                    error.Create().Show();

                    loginDialog.Cancel();
                    return;
                }
            }

            //Initializing the User Table
            IMobileServiceTable<User> userData = client.GetTable<User>();

            //Getting the user details from the Database
            List<User> finalUser = await userData.Where
                (item => item.Email == client.CurrentUser.UserId).ToListAsync();
            user = finalUser[0];

            //Creating an intent with officer details for testing purposes
            //This will check the user Email, if it matches, will show the officer activity
            if(emailText.Text == "karim.mo.saleh@gmail.com")
            {
                Intent mainActivityIntent = new Intent(Application.Context,
                typeof(OfficerActivity));
                mainActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                this.StartActivity(mainActivityIntent);
            }

            //Creating an intent with user details to pass to the main activity
            else
            {
                Intent mainActivityIntent = new Intent(Application.Context,
                typeof(ParkingAreaActivity));
                mainActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                this.StartActivity(mainActivityIntent);
            }
            
            //Ending the login activity page
            loginDialog.Cancel();
            this.Finish();
        }
    }
}