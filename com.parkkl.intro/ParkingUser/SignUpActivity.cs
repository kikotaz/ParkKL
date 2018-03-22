using Microsoft.WindowsAzure.MobileServices;
using com.parkkl.intro.Models;
using Android.App;
using Android.OS;
using com.parkkl.intro.Services;
using Android.Widget;
using System.Collections.Generic;
using System;
using Android.Content;

namespace com.parkkl.intro.ParkingUser
{
    [Activity(Label = "Sign Up")]
    public class SignUpActivity : Activity
    {
        //Getting an instance of Azure service client (Singleton instance)
        MobileServiceClient client = SingletonClient.getInstance();
        User user = new User();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.sign_up);

            //Setting the E-mail field to be E-mail type for easiness
            var emailText = FindViewById<EditText>(Resource.Id.input_email);
            emailText.InputType = Android.Text.InputTypes.TextVariationEmailAddress;

            var signUpbtn = FindViewById<Button>(Resource.Id.btn_sign_up);
            signUpbtn.Click += delegate
            {
                onSignUpBtnClick();
            };

            var loginLink = FindViewById<TextView>(Resource.Id.link_login);
            loginLink.Click += delegate
            {
                onLoginLinkClick();
            };
        }

        public void onLoginLinkClick()
        {
            StartActivity(new Intent(Application.Context, typeof(SignInActivity)));
        }

        public async void onSignUpBtnClick()
        {
            //Declaration of all the text fields in the activity
            var firstNameText = FindViewById<EditText>(Resource.Id.input_firstname);
            var lastNameText = FindViewById<EditText>(Resource.Id.input_lastname);
            var emailText = FindViewById<EditText>(Resource.Id.input_email);
            var passwordText = FindViewById<EditText>(Resource.Id.input_password);

            //Collecting all the fields in one list for validation
            List<EditText> fields = new List<EditText>();
            fields.Clear();
            fields.Add(firstNameText);
            fields.Add(lastNameText);
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
            var registerDialog = ProgressDialog.Show(this, "Please wait...",
                "Creating new account...", true);

            //Setting user data to be sent to DB
            user.Email = emailText.Text;
            user.FirstName = firstNameText.Text;
            user.LastName = lastNameText.Text;
            user.Password = passwordText.Text;

            //Initializing the user table
            IMobileServiceTable<User> userTable = client.GetTable<User>();

            //Inserting new user to the Database
            await userTable.InsertAsync(user);

            //Creating a new alert to welcome the new user
            AlertDialog.Builder accountCreated = new AlertDialog.Builder(this);
            accountCreated.SetTitle("Welcome " + user.FirstName);
            accountCreated.SetMessage("Please check your email to confirm your account");
            accountCreated.SetPositiveButton("OK", delegate { onLoginLinkClick(); });
            accountCreated.Create().Show();

            registerDialog.Cancel();
        }
    }
}