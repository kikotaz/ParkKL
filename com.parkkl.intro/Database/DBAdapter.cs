using System;
using Android.Content;
using Android.Database;
using Android.Database.Sqlite;

namespace com.parkkl.intro.Database
{
    class DBAdapter
    {
        private Context c;
        private SQLiteDatabase db;
        private DBHelper helper;

        public DBAdapter(Context c)
        {
            this.c = c;
            helper = new DBHelper(c);
        }

        //OPEN DB
        public void OpenDB()
        {
            try
            {
                db = helper.WritableDatabase;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        //close db
        public void CloseDB()
        {
            try
            {
                helper.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //INSERT DATA
        public bool Add(String parkingId, String locationName,
            String chargePerHr, String parkedHr, String carPlateNo, String totalAmount, String currentDate)
        {
            try
            {
                ContentValues cv = new ContentValues();
                cv.Put(Constants.KEY_PARKING_ID, parkingId);
                cv.Put(Constants.KEY_LOCATION_NAME, locationName);
                cv.Put(Constants.KEY_CHARGE_PER_HOUR, chargePerHr);
                cv.Put(Constants.KEY_PARKED_HOURS, parkedHr);
                cv.Put(Constants.KEY_CAR_PLATE_NUMBER, carPlateNo);
                cv.Put(Constants.KEY_TOTAL_AMOUNT, totalAmount);
                cv.Put(Constants.KEY_DATE, currentDate);

                db.Insert(Constants.TABLE_PARKING, null, cv);
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        //RETRIEVE/FILTER
        public ICursor Retrieve(String searchTerm)
        {
            string[] columns = { Constants.KEY_CAR_PLATE_NUMBER, Constants.KEY_PARKED_HOURS };
            ICursor c = null;

            if (!String.IsNullOrEmpty(searchTerm))
            {
                string sql = "SELECT * FROM " + Constants.TABLE_PARKING + " WHERE " + Constants.KEY_CAR_PLATE_NUMBER + " LIKE '%" + searchTerm + "%'";
                c = db.RawQuery(sql, null);
            }
            else
            {
                c = db.Query(Constants.TABLE_PARKING, columns, null, null, null, null, null);
            }
            return c;
        }

        //INSERT DATA
        public bool AddEmail(String email)
        {
            try
            {
                ContentValues cv = new ContentValues();
                cv.Put(Constants.KEY_USER_EMAIL, email);

                db.Insert(Constants.TABLE_USER, null, cv);
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        //RETRIEVE
        public ICursor RetrieveEmail()
        {
            string[] columns = { Constants.KEY_USER_EMAIL };
            ICursor c = null;

            c = db.Query(Constants.TABLE_USER, columns, null, null, null, null, null);

            return c;
        }
    }
}