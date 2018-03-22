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

namespace com.parkkl.intro.Adapter
{
    class PenaltyAdapter : BaseAdapter<PenaltyModel>
    {
        private List<PenaltyModel> mItems;
        private Context mcontext;

        public PenaltyAdapter(Context context, List<PenaltyModel> items)
        {
            mItems = items;
            mcontext = context;
        }
        public override PenaltyModel this[int position]
        {
            get
            {
                return mItems[position];
            }
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
                row = LayoutInflater.From(mcontext).Inflate(Resource.Layout.penalty_list_view_row, null, false);
            }
            TextView penaltyId = row.FindViewById<TextView>(Resource.Id.penalty_id);
            penaltyId.Text = mItems[position].penaltyId;
            TextView locationName = row.FindViewById<TextView>(Resource.Id.penalty_location);
            locationName.Text = mItems[position].locationName;
            TextView carPlateNumber = row.FindViewById<TextView>(Resource.Id.penalty_number_plate);
            carPlateNumber.Text = mItems[position].carPlateNumber;
            TextView date = row.FindViewById<TextView>(Resource.Id.penalty_date);
            date.Text = mItems[position].date;
            TextView excessTime = row.FindViewById<TextView>(Resource.Id.excess_time);
            excessTime.Text = mItems[position].excessTime;
            TextView penaltyAmount = row.FindViewById<TextView>(Resource.Id.penalty_total_amount);
            penaltyAmount.Text = mItems[position].penaltyAmount;
            TextView penaltyPaid = row.FindViewById<TextView>(Resource.Id.penalty_payment);
            penaltyPaid.Text = mItems[position].penaltyPaid;
            if (penaltyPaid.Text.Contains("unpaid"))
            {
                penaltyPaid.SetTextColor(Android.Graphics.Color.Red);
            }
            else
            {
                penaltyPaid.SetTextColor(Android.Graphics.Color.LightGreen);
            }

            return row;
        }
    }
}