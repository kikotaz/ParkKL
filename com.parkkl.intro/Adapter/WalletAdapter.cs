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
    class WalletAdapter : BaseAdapter<WalletModel>
    {
        private List<WalletModel> mItems;
        private Context mcontext;

        public WalletAdapter(Context context, List<WalletModel> items)
        {
            mItems = items;
            mcontext = context;
        }
        public override WalletModel this[int position]
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
                row = LayoutInflater.From(mcontext).Inflate(Resource.Layout.wallet_list_view_row, null, false);
            }
            TextView transId = row.FindViewById<TextView>(Resource.Id.trans_id);
            transId.Text = mItems[position].transId;
            TextView transRemarks = row.FindViewById<TextView>(Resource.Id.trans_remarks);
            transRemarks.Text = mItems[position].remarks;
            TextView transDate = row.FindViewById<TextView>(Resource.Id.trans_date);
            transDate.Text = mItems[position].dateTime;
            TextView transAmount = row.FindViewById<TextView>(Resource.Id.trans_amount);
            transAmount.Text = mItems[position].amount;
            TextView transStatus = row.FindViewById<TextView>(Resource.Id.trans_status);
            transStatus.Text = mItems[position].status;
            return row;
        }
    }
}