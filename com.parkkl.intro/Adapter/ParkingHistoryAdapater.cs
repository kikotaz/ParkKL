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
using com.parkkl.intro.Models;
using Java.Lang;

namespace com.parkkl.intro.Adapter
{
    class ParkingHistoryAdapter : BaseAdapter<ParkingModel>, IFilterable
    {
        private List<ParkingModel> mItems;
        private List<ParkingModel> original;
        private List<ParkingModel> filteredList;
        private Context mcontext;

        public ParkingHistoryAdapter(Context context, List<ParkingModel> items)
        {
            mItems = items;
            mcontext = context;
            original = items;
            Filter = new ParkingFilter(this);
        }
        public override ParkingModel this[int position]
        {
            get
            {
                return mItems[position];
            }
        }

        public ParkingModel GetItemAtPosition(int position)
        {
            return this.mItems[position];
        }

        public override int Count
        {
            get { return mItems.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            if (row == null)
            {
                row = LayoutInflater.From(mcontext).Inflate(Resource.Layout.parking_history_list_view_row, parent, false);
            }
            TextView parkingStat = row.FindViewById<TextView>(Resource.Id.parking_stat);
            parkingStat.Text = mItems[position].parkingStat;
            TextView parkingLocationName = row.FindViewById<TextView>(Resource.Id.parking_location);
            parkingLocationName.Text = mItems[position].locationName;
            TextView carPlateNumber = row.FindViewById<TextView>(Resource.Id.car_plate_number);
            carPlateNumber.Text = mItems[position].carPlateNumber;
            TextView parkingHours = row.FindViewById<TextView>(Resource.Id.parking_hours);
            parkingHours.Text = mItems[position].parkedHours;
            TextView chargePerHour = row.FindViewById<TextView>(Resource.Id.parking_charge_per_hour);
            chargePerHour.Text = mItems[position].chargePerHour;
            TextView parkingTotalAmount = row.FindViewById<TextView>(Resource.Id.parking_total_amount);
            parkingTotalAmount.Text = mItems[position].totaAmount;
            TextView parkingDate = row.FindViewById<TextView>(Resource.Id.parking_date);
            parkingDate.Text = mItems[position].date;
            TextView userEmail = row.FindViewById<TextView>(Resource.Id.user_email);
            userEmail.Text = mItems[position].email;

            return row;
        }

        public Filter Filter { get; private set; }

        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
        }

        private class ParkingFilter : Filter
        {
            private readonly ParkingHistoryAdapter adapter;
            
            
            public ParkingFilter(ParkingHistoryAdapter _adapter)
            {
                adapter = _adapter;
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                var returnParking = new FilterResults();
                var results = new List<ParkingModel>();

                if(adapter.mItems == null)
                {
                    adapter.mItems = adapter.filteredList;
                }

                if(constraint == null || constraint.Length() == 0)
                {
                    results.Clear();
                    results.AddRange(adapter.original);
                }

                else if(adapter.mItems != null && adapter.mItems.Any())
                {
                    results.AddRange(adapter.mItems.Where(parking => parking.carPlateNumber.ToLower()
                    .Contains(constraint.ToString())));
                }

                returnParking.Values = FromArray(results.Select(p => p.ToJavaObject()).ToArray());

                returnParking.Count = results.Count;

                constraint.Dispose();

                return returnParking;
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                using (var values = results.Values)
                {
                    adapter.filteredList = values.ToArray<Java.Lang.Object>().Select(a =>
                    a.ToNetObject<ParkingModel>()).ToList();
                }

                adapter.mItems = adapter.filteredList;

                adapter.NotifyDataSetChanged();

                constraint.Dispose();
                results.Dispose();
            }
        }
    }
}