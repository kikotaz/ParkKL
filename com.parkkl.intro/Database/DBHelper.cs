using System;
using Android.Content;
using Android.Database.Sqlite;

namespace com.parkkl.intro.Database
{
    class DBHelper : SQLiteOpenHelper
    {
        public DBHelper(Context context) : base(context, Constants.DATABASE_NAME, null, Constants.DATABASE_VERSION)
        {
        }

        public override void OnCreate(SQLiteDatabase db)
        {
            try
            {
                db.ExecSQL(Constants.CREATE_PARKING_TABLE);
                db.ExecSQL(Constants.CREATE_USER_TABLE);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            db.ExecSQL(Constants.TABLE_PARKING);
            db.ExecSQL(Constants.TABLE_USER);
            OnCreate(db);
        }
    }
}