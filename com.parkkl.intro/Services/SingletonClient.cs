using Microsoft.WindowsAzure.MobileServices;

namespace com.parkkl.intro.Services
{
    /*This class will make sure that there is only one Mobile Service Client handling
    all requests during the whole application execution*/
    public static class SingletonClient
    {
        //Creating new instance of MobielServiceClient
        private static MobileServiceClient instance;

        //Private constructor to ensure that this class can't be instantiated
        static SingletonClient() { }

        //This method will return one instance only of the Mobile Service Client
        public static MobileServiceClient getInstance()
        {
            if (instance == null)
            {
                instance = new MobileServiceClient
            (@"https://testingparkkl.azurewebsites.net/")
                {
                    SerializerSettings = new MobileServiceJsonSerializerSettings()
                    {
                        CamelCasePropertyNames = true
                    }
                };
            }
            return instance;
        }
    }
}