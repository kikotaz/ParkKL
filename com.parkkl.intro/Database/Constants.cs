using System;

namespace com.parkkl.intro.Database
{
    class Constants
    {
        public static int DATABASE_VERSION = 1;

        public static string DATABASE_NAME = "parkKL";

        public static string TABLE_PARKING = "parking";

        public static string TABLE_USER = "user";

        //COLUMNS
        public static string KEY_ID = "id";
        public static string KEY_PARKING_ID = "parking_id";
        public static string KEY_LOCATION_NAME = "location_name";
        public static string KEY_CHARGE_PER_HOUR = "charge_per_hour";
        public static string KEY_PARKED_HOURS = "parked_hours";
        public static string KEY_CAR_PLATE_NUMBER = "car_plate_number";
        public static string KEY_TOTAL_AMOUNT = "total_amount";
        public static string KEY_DATE = "date";

        public static string KEY_USER_EMAIL = "email";

        //create tb stmt
        public static String CREATE_PARKING_TABLE = "CREATE TABLE " + TABLE_PARKING + "("
                + KEY_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + KEY_PARKING_ID + " TEXT NOT NULL," + KEY_LOCATION_NAME + " TEXT,"
                + KEY_CHARGE_PER_HOUR + " TEXT NOT NULL," + KEY_PARKED_HOURS + " TEXT NOT NULL,"
                + KEY_CAR_PLATE_NUMBER + " TEXT NOT NULL," + KEY_TOTAL_AMOUNT + " TEXT NOT NULL," 
                + KEY_DATE + " TEXT NOT NULL" + ")";

        public static String CREATE_USER_TABLE = "CREATE TABLE " + TABLE_USER + "("
                + KEY_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + KEY_USER_EMAIL + " TEXT NOT NULL" + ")";

        //drop tb stmt
        public static String DROP_TB = "DROP TABLE IF EXISTS " + TABLE_PARKING;
        public static String DROP_TB_USER = "DROP TABLE IF EXISTS " + TABLE_USER;
    }
}