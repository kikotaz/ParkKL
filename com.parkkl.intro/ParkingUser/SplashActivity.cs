using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using com.parkkl.intro.ParkingUser;
using Java.Net;
using System.Threading.Tasks;

namespace com.parkkl.intro
{
    [Activity(Label = "ParkKL", Theme = "@style/MyTheme.Splash", NoHistory = true, Icon = "@drawable/ic_launcher", MainLauncher = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            try
            {
                if (isOnline() == false)
                {
                    throw new ConnectException();
                }
                StartActivity(new Intent(Application.Context, typeof(SignInActivity)));
            }
            catch (ConnectException)
            {
                AlertDialog.Builder noInternetAlert = new AlertDialog.Builder(this);

                noInternetAlert.SetTitle("No Internet connection");

                noInternetAlert.SetMessage("ParkKL couldn't find internet. "
                    + "Please make sure you have vaild connection and try again");

                noInternetAlert.SetNeutralButton("OK", (senderAlert, args) =>
                {
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                });

                Dialog noInternetDialog = noInternetAlert.Create();
                noInternetDialog.Show();
            }
        }

        //This method will check the internet connectivity
        public bool isOnline()
        {
            //initializing a connectivity manager who will test the connection
            //this connectivity manager uses the local context passed through the constructor
            //if it returns false, it means there is no connection
            ConnectivityManager cm =
                (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            return netInfo != null && netInfo.IsConnectedOrConnecting;
        }
    }
}