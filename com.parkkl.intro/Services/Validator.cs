using System.Collections.Generic;
using Android.Widget;
using System;

namespace com.parkkl.intro.Services
{
    /*This class will be providing different validations,
     which shall be used across the application execution*/
    public class Validator
    {
        public Validator() { }

        //This method will check list of fields if any field is missing
        public void checkEmpty(List<EditText> inputs)
        {
            //This loop will go through all the fields and check
            //for empty ones 
            foreach (EditText text in inputs)
            {
                //Getting the index of each member in the inputs
                string inputID = inputs.IndexOf(text).ToString();

                //Validating fields to find empty fields
                if (text.Text.Length == 0)
                {
                    throw new MissingFieldException(inputID);
                }
            }

        }


        //This method will check email field if the format is correct or not
        public void checkValidEmail(EditText input)
        {
            //Validating the email format
            if (!Android.Util.Patterns.EmailAddress.Matcher(input.Text).Matches())
            {
                throw new FormatException();
            }
        }
    }
}
