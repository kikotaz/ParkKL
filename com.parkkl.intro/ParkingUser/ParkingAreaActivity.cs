using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Gms.Maps;
using Android.Locations;
using Android.Runtime;
using Android.Gms.Maps.Model;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Support.V4.View;
using Android.Content;
using Newtonsoft.Json;
using com.parkkl.intro.Models;
using Microsoft.WindowsAzure.MobileServices;
using com.parkkl.intro.Services;
using System.Threading.Tasks;

namespace com.parkkl.intro.ParkingUser
{
    [Activity(Label = "Parking Areas", Theme = "@style/MyTheme.Base")]
    public class ParkingAreaActivity : AppCompatActivity, IOnMapReadyCallback, ILocationListener
    {
        public GoogleMap googleMap;
        private MapFragment mapFragment;
        private Location currentLocation;
        private LocationManager locManager;
        private string locationProvider;
        TextView parkingArea;
        private DrawerLayout drawer;
        private NavigationView navigation;

        //Getting an instance of Azure service client (Singleton instance)
        MobileServiceClient client = SingletonClient.getInstance();
        User user = new User();
        Parking parking = new Parking();
        Wallet wallet = new Wallet();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.parking_area);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuIcon);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            user = JsonConvert.DeserializeObject<User>(Intent.GetStringExtra("User"));

            InitializaLocationManager();

            InitilizeDrawer();

            this.Title = "Welcome " + user.FirstName;

            var navigationView = FindViewById<NavigationView>(Resource.Id.navView);
            navigationView.NavigationItemSelected += NavigationView_NavigationItemSelected;

            CheckParkingSession();

            locManager.RequestLocationUpdates(locationProvider, 1000, 15, this);

            currentLocation = locManager.GetLastKnownLocation(LocationManager.PassiveProvider);

            GetAddress(currentLocation);

            mapFragment = (MapFragment)FragmentManager.FindFragmentById
                (Resource.Id.parking_map);

            mapFragment.GetMapAsync(this);

            var bookingBtn = FindViewById<Button>(Resource.Id.btn_book);
            var extendBtn = FindViewById<Button>(Resource.Id.btn_extend);

            bookingBtn.Click += delegate
            {
                if (bookingBtn.GetTag(Resource.Id.btn_book) == null)
                {
                    onBookingClick();
                }
                else if (bookingBtn.GetTag(Resource.Id.btn_book).ToString().Equals("end_session_btn"))
                {
                    OnEndSessionClick();
                }
            };

            extendBtn.Click += delegate
            {
                OnExtendClick();
            };
        }

        private void NavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.MenuItem.ItemId)
            {
                case (Resource.Id.nav_home):
                    Intent parkingAreaActivityIntent = new Intent(Application.Context,
                    typeof(ParkingAreaActivity));
                    parkingAreaActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                    this.StartActivity(parkingAreaActivityIntent);
                    break;
                case (Resource.Id.nav_wallet):
                    Intent walletActivityIntent = new Intent(Application.Context,
                    typeof(WalletActivity));
                    walletActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                    this.StartActivity(walletActivityIntent);
                    break;
                case (Resource.Id.nav_penalty):
                    Intent penaltyActivityIntent = new Intent(Application.Context,
                    typeof(PenaltyActivity));
                    penaltyActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                    this.StartActivity(penaltyActivityIntent);
                    break;
                case (Resource.Id.nav_parking_history):
                    Intent parkingHistoryActivityIntent = new Intent(Application.Context,
                    typeof(ParkingHistoryActivity));
                    parkingHistoryActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                    this.StartActivity(parkingHistoryActivityIntent);
                    break;
                case (Resource.Id.nav_logout):
                    StartActivity(new Intent(Application.Context, typeof(SignInActivity)));
                    break;
            }
            drawer.CloseDrawer(GravityCompat.Start);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuIcon);
        }

        public void OnMapReady(GoogleMap map)
        {
            googleMap = map;
            googleMap.MyLocationEnabled = true;

            MarkerOptions marker = new MarkerOptions();

            marker.SetPosition(new LatLng
                (currentLocation.Latitude, currentLocation.Longitude));

            marker.SetTitle("Your Parking");
            marker.SetIcon(BitmapDescriptorFactory.DefaultMarker
                (BitmapDescriptorFactory.HueGreen));

            CameraUpdate update = CameraUpdateFactory.NewLatLngZoom(marker.Position, 15);

            googleMap.MoveCamera(update);
            googleMap.AddMarker(marker);
        }

        public void InitializaLocationManager()
        {
            locManager = (LocationManager)GetSystemService(LocationService);
            Criteria locationCriteria = new Criteria
            {
                Accuracy = Accuracy.Fine
            };

            IList<string> locationProviders =
                locManager.GetProviders(locationCriteria, true);

            if (locationProviders.Any())
            {
                locationProvider = locationProviders.First();
            }
            else
            {
                locationProvider = string.Empty;
            }
        }

        public async void GetAddress(Location location)
        {
            Geocoder geocoder = new Geocoder(this);

            IList<Address> addressList = await geocoder.GetFromLocationAsync
                (location.Latitude, location.Longitude, 2);

            Address address = addressList.FirstOrDefault();

            DisplayAddress(address);
        }

        public void DisplayAddress(Address address)
        {
            if (address != null)
            {
                if (!address.AdminArea.Contains("Kuala Lumpur"))
                {
                    Android.App.AlertDialog.Builder locationAlert = 
                        new Android.App.AlertDialog.Builder(this);

                    locationAlert.SetTitle("Not in DBKL Coverage");

                    locationAlert.SetMessage("ParkKL is not supported in your region."
                        + "\nParkKL can only be used in Kuala Lumpur");

                    locationAlert.SetNeutralButton("OK", (senderAlert, args) =>
                    {
                        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                    });

                    Dialog noInternetDialog = locationAlert.Create();
                    noInternetDialog.Show();
                }
                else
                {
                    StringBuilder AddressLines = new StringBuilder();
                    AddressLines.Append(address.Thoroughfare);
                    parkingArea = FindViewById<TextView>(Resource.Id.parking_area_name);
                    parkingArea.Text = AddressLines.ToString();
                }
            }
            else
            {
                RefreshActivity();
            }
        }

        public void OnLocationChanged(Location location)
        {
            currentLocation = location;

            if (currentLocation == null)
            {
                parkingArea.Text = "Couldn't find your address. Please enable "
                    + "Location Services";
                return;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (locationProvider != null)
            {
                locManager.RequestLocationUpdates(locationProvider, 0, 0, this);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            locManager.RemoveUpdates(this);
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            RefreshActivity();
        }

        public void OnProviderDisabled(string provider)
        {
            InitializaLocationManager();
        }

        public void OnProviderEnabled(string provider)
        {
            throw new NotImplementedException();
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            locManager.RemoveUpdates(this);
        }

        public async void onBookingClick()
        {
            var hrsText = FindViewById<EditText>(Resource.Id.input_hr);

            var plateNoText = FindViewById<EditText>(Resource.Id.input_car_plate_number);
            var locationText = FindViewById<TextView>(Resource.Id.parking_area_name);
            string hrsRequested = hrsText.Text;
            string vehicleNo = plateNoText.Text;

            if (!String.IsNullOrEmpty(hrsRequested) || !String.IsNullOrEmpty(vehicleNo))
            {
                int totalParkingAmount = 3 * Convert.ToInt32(hrsText.Text);
                wallet.Balance = "0";

                //Check if wallet has enough balance
                IMobileServiceTable<Wallet> walletData = client.GetTable<Wallet>();

                //Getting the Wallet details from the Database
                List<Wallet> currentBalance = await walletData.Where
                    (item => item.Email == client.CurrentUser.UserId).ToListAsync();

                if (currentBalance.Count > 0)
                {
                    wallet = currentBalance[0];

                    Android.App.AlertDialog.Builder confirmAlert =
                        new Android.App.AlertDialog.Builder(this);

                    confirmAlert.SetTitle("Confirm Payment");
                    confirmAlert.SetMessage("Wallet Balance: " + wallet.Balance + "\n" +
                        "" + "\n" +
                        "Charge Per Hour: " + "RM 3.00" + "\n" +
                        "Hours Reserving: " + hrsText.Text + "\n" +
                        "Total Payment: " + "RM " + totalParkingAmount + "\n" +
                        "" + "\n" +
                        "** penalty will be issued when hours are exceeded.");
                    confirmAlert.SetPositiveButton("Yes", async (senderAlert, args) =>
                    {
                        if (Convert.ToInt32(wallet.Balance) >= totalParkingAmount)
                        {
                            var parkingDialog = ProgressDialog.Show(this, "Please wait...",
                                "Confirming your parking...", true);

                            parking.Email = user.Email;
                            parking.LocationName = locationText.Text;
                            parking.ChargePerHour = 3;
                            parking.ParkedHours = Convert.ToInt32(hrsText.Text);
                            parking.CarPlateNumber = plateNoText.Text;
                            parking.TotaAmount = totalParkingAmount;

                            //Initializing the parking table
                            IMobileServiceTable<Parking> parkingTable = client.GetTable<Parking>();

                            //Inserting new parking to the Database
                            await parkingTable.InsertAsync(parking);

                            //Update the wallet balance                      
                            wallet.Balance = Convert.ToString(Convert.ToInt32(wallet.Balance) -
                                totalParkingAmount);

                            parking = await GetLastParking();

                            var extraParameters = new Dictionary<string, string>();
                            extraParameters.Add("userId", user.Id);
                            extraParameters.Add("type", "parking");
                            extraParameters.Add("parkingId", parking.Id);

                            await walletData.UpdateAsync(wallet, extraParameters);

                            Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                            alertSuccess.SetTitle("Parking Area");
                            alertSuccess.SetMessage("Payment is successful");
                            alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                            {
                                RefreshActivity();
                            });
                            Dialog dialogSuccess = alertSuccess.Create();
                            dialogSuccess.Show();
                        }
                        else
                        {
                            GoToTopUpWallet(Convert.ToInt32(wallet.Balance), totalParkingAmount);
                        }

                    });
                    confirmAlert.SetNegativeButton("Cancel", (senderAlert, args) =>
                    {
                        Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                    });
                    Dialog dialog = confirmAlert.Create();
                    dialog.Show();
                }
                else
                {
                    GoToTopUpWallet(Convert.ToInt32(wallet.Balance), totalParkingAmount);
                }
            }
            else
            {
                Android.App.AlertDialog.Builder alertEmpty = new Android.App.AlertDialog.Builder(this);
                alertEmpty.SetTitle("Parking Area");
                alertEmpty.SetMessage("Fill in the required fields");
                alertEmpty.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                {

                });
                Dialog dialogSuccess = alertEmpty.Create();
                dialogSuccess.Show();
            }

        }

        public void OnEndSessionClick()
        {
            var parkingButton = FindViewById<Button>(Resource.Id.btn_book);
            var extendButton = FindViewById<Button>(Resource.Id.btn_extend);
            var plateNo = FindViewById<EditText>(Resource.Id.input_car_plate_number);
            var parkingLimit = FindViewById<TextView>(Resource.Id.parking_limit);

            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Confirm Starting New Session");
            alert.SetMessage(parkingLimit.Text + "\n\nClick Yes to start a new session." +
                "\n\n**If no new session is issued, the current session will still be active");

            alert.SetPositiveButton("Yes", (senderAlert, args) =>
            {
                plateNo.Text = null;
                plateNo.Enabled = true;

                parkingButton.SetBackgroundResource(Resource.Color.primary);
                parkingButton.Text = "Pay Parking";
                parkingButton.SetTag(Resource.Id.btn_book, null);
                extendButton.Enabled = false;

                parkingLimit.Text = "You have no active parking sessions";

                extendButton.SetBackgroundColor(Android.Graphics.Color.Black);
            });

            alert.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
            });

            Dialog dialogSuccess = alert.Create();
            dialogSuccess.Show();
        }

        public async void OnExtendClick()
        {
            var parkingLimit = FindViewById<TextView>(Resource.Id.parking_limit);
            var extendBtn = FindViewById<Button>(Resource.Id.btn_extend);
            var extendedHours = FindViewById<EditText>(Resource.Id.input_hr);

            string sessionID = extendBtn.GetTag(Resource.Id.btn_extend).ToString();
            Wallet wallet = await GetWalletData();
            Parking parking = await GetLastParking();

            if (String.IsNullOrEmpty(extendedHours.Text))
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Parking Area");
                alert.SetMessage("Fill in the required fields");
                alert.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                {

                });
                Dialog dialogSuccess = alert.Create();
                dialogSuccess.Show();
            }
            else
            {
                int totalParkingAmount = 2 * Convert.ToInt32(extendedHours.Text);

                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Confirm Extending Current Session");
                alert.SetMessage(parkingLimit.Text + "\n\nYou are trying to extend by: "
                    + extendedHours.Text + " Hours" + "\nWallet Balance: " + wallet.Balance +
                        "\nCharge Per Hour: " + "RM 2.00" +
                        "\nTotal Payment: " + "RM " + totalParkingAmount);

                alert.SetPositiveButton("Yes", async (senderAlert, args) =>
                {
                    var parkingDialog = ProgressDialog.Show(this, "Please wait...",
                                "Confirming your extension...", true);

                    if (Convert.ToInt32(wallet.Balance) >= totalParkingAmount)
                    {
                        IMobileServiceTable<Wallet> walletData = client.GetTable<Wallet>();
                        IMobileServiceTable<Parking> parkingData = client.GetTable<Parking>();

                        wallet.Balance = Convert.ToString(Convert.ToInt32(wallet.Balance)
                            - totalParkingAmount);
                        parking.ParkedHours = Convert.ToInt32(parking.ParkedHours) +
                                              Convert.ToInt32(extendedHours.Text);

                        var extraParameters = new Dictionary<string, string>();
                        extraParameters.Add("userId", user.Id);
                        extraParameters.Add("type", "extend");
                        extraParameters.Add("parkingId", parking.Id);

                        await parkingData.UpdateAsync(parking);
                        await walletData.UpdateAsync(wallet, extraParameters);
                        

                        Android.App.AlertDialog.Builder alertSuccess = new Android.App.AlertDialog.Builder(this);
                        alertSuccess.SetTitle("Time Extended");
                        alertSuccess.SetMessage("Payment is successful");
                        alertSuccess.SetPositiveButton("Ok", (senderAlertSucces, argsSuccess) =>
                        {
                            RefreshActivity();
                        });
                        Dialog dialog = alertSuccess.Create();
                        dialog.Show();
                    }
                    else
                    {
                        GoToTopUpWallet(Convert.ToInt32(wallet.Balance), totalParkingAmount);
                    }
                });

                alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                });
                Dialog dialogSuccess = alert.Create();
                dialogSuccess.Show();
            }
        }
        public async void InitilizeDrawer()
        {
            drawer = (DrawerLayout)FindViewById(Resource.Id.drawerLayout);
            navigation = (NavigationView)FindViewById(Resource.Id.navView);
            View headerLayout = navigation.InflateHeaderView(Resource.Menu.header);

            TextView headerName = (TextView)headerLayout.FindViewById(Resource.Id.WelcomeUser);
            TextView headerEmail = (TextView)headerLayout.FindViewById(Resource.Id.UserEmail);
            TextView headerBalance = (TextView)headerLayout.FindViewById(Resource.Id.HeaderWalletBalance);

            Wallet wallet = await GetWalletData();

            if (wallet != null)
            {
                string result = "Balance: RM " + wallet.Balance + ".00";
                headerBalance.Text = result;
            }
            else
            {
                string result = "Balance: RM 0.00";
                headerBalance.Text = result;
            }

            headerName.Text = user.FirstName + " " + user.LastName;
            headerEmail.Text = user.Email;

            drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (drawer.IsDrawerOpen(GravityCompat.Start))
                    {
                        drawer.CloseDrawer(GravityCompat.Start);
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuIcon);
                        return true;
                    }
                    drawer.OpenDrawer(GravityCompat.Start);
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ArrowBack);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
                SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuIcon);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public async void CheckParkingSession()
        {
            var parkingButton = FindViewById<Button>(Resource.Id.btn_book);
            var extendButton = FindViewById<Button>(Resource.Id.btn_extend);
            var plateNo = FindViewById<EditText>(Resource.Id.input_car_plate_number);
            var parkingHours = FindViewById<EditText>(Resource.Id.input_hr);
            var parkingLimit = FindViewById<TextView>(Resource.Id.parking_limit);

            Parking lastParking = await GetLastParking();

            if (lastParking != null)
            {
                DateTime parkingTime = DateTime.Parse(Convert.ToString(lastParking.CreatedAt));
                DateTime expectedEnd = parkingTime.AddHours(lastParking.ParkedHours);

                if (DateTime.Now <= expectedEnd)
                {
                    parkingLimit.Text = "You have active parking session that ends at "
                    + expectedEnd.ToString("hh:mm tt");

                    parkingHours.Text = null;

                    plateNo.Text = lastParking.CarPlateNumber;
                    plateNo.Enabled = false;

                    parkingButton.SetBackgroundColor(Android.Graphics.Color.DimGray);
                    parkingButton.Text = "Start New Session";
                    parkingButton.SetTag(Resource.Id.btn_book, "end_session_btn");

                    extendButton.Enabled = true;
                    extendButton.SetBackgroundResource(Resource.Color.primary);
                    extendButton.SetTag(Resource.Id.btn_extend, lastParking.Id);
                }
                else
                {
                    parkingLimit.Text = "You have no active parking sessions";

                    extendButton.SetBackgroundColor(Android.Graphics.Color.Black);
                    extendButton.Enabled = false;
                }
            }
            else
            {
                parkingLimit.Text = "You have no active parking sessions";

                extendButton.SetBackgroundColor(Android.Graphics.Color.Black);
                extendButton.Enabled = false;
            }
        }

        public async Task<Parking> GetLastParking()
        {
            Parking parking = null;

            IMobileServiceTable<Parking> parkingData = client.GetTable<Parking>();

            List<Parking> parkingList = await parkingData.Where
                (item => item.Email == client.CurrentUser.UserId).ToListAsync();

            if (parkingList.Count == 0)
            {
                return parking;
            }
            else
            {
                parking = parkingList.OrderByDescending(x => x.CreatedAt).ToList().First();

                return parking;
            }
        }
        public async Task<Wallet> GetWalletData()
        {
            Wallet wallet = null;

            //Check if wallet has enough balance
            IMobileServiceTable<Wallet> walletData = client.GetTable<Wallet>();

            //Getting the Wallet details from the Database
            List<Wallet> walletList = await walletData.Where
                (item => item.Email == client.CurrentUser.UserId).ToListAsync();

            if (walletList.Count == 0)
            {
                return wallet;
            }
            else
            {
                wallet = walletList[0];

                return wallet;
            }
        }

        public void GoToTopUpWallet(int balance, int required)
        {

            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Parking Area");
            alert.SetMessage("Insuficient funds!" + "\n"
                + "Your wallet is RM " + balance + ".00" +
                "\n" + "Please, top up");
            alert.SetPositiveButton("Yes", (senderAlertSucces, argsSuccess) =>
            {
                Intent topUpActivityIntent = new Intent(Application.Context,
                typeof(TopUpActivity));
                topUpActivityIntent.PutExtra("User", JsonConvert.SerializeObject(user));
                this.StartActivity(topUpActivityIntent);
            });

            alert.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
            });
            Dialog dialogSuccess = alert.Create();
            dialogSuccess.Show();

        }
        public void RefreshActivity()
        {
            Finish();
            OverridePendingTransition(0, 0);
            StartActivity(Intent);
            OverridePendingTransition(0, 0);
            CheckParkingSession();
        }
    }
}