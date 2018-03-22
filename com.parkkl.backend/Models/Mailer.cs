using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace com.parkkl.backend.Models
{
    /*This Class will be responsible for sending different types
     * of E-mails that is required during the mobile app execution.
     *It will use a web API called SendGrid*/
    public class Mailer
    {
        public Mailer() { }
        MobileServiceContext context = new MobileServiceContext();

        //This method will send account registration confirmation
        public async Task SendConfirmationEmail(User item)
        {
            //This key is for authentication in SendGrid API
            var sendGridAPIKey = Environment.GetEnvironmentVariable("SENDGRID_KEY");

            //Initializing new instance of the API client
            var mailClient = new SendGridClient(sendGridAPIKey);

            //Initializing new message
            var message = new SendGridMessage()
            {
                From = new EmailAddress("admin@parkkl.com", "ParkKL"),
            };

            //Initializing claims that will be included with the E-mail
            var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, item.Id),
                    new Claim(JwtRegisteredClaimNames.Email, item.Email)
                };

            //Creating new JWT Security Token
            ClaimsIdentity claimsID = new ClaimsIdentity(claims);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateToken(null, null, claimsID,
                null, null, null);
            string encodedToken = handler.WriteToken(token);

            //Configuring the E-mail message to fit a saved template
            message.SetTemplateId("503f2077-dd34-43c1-994f-be61adbe6fd7");
            message.AddSubstitution("-EMAIL-", item.Email);
            message.AddSubstitution("-FIRST-", item.FirstName);
            message.AddSubstitution("-TOKEN-", encodedToken);
            message.AddTo(new EmailAddress(item.Email, "User"));

            //Sending the E-mail message
            var response = await mailClient.SendEmailAsync(message);
        }

        public async Task SendTopUpReceipt(Wallet wallet, string Id, string Amount)
        {
            User user = await context.Users.FindAsync(Id);

            DateTime paymentTime = DateTime.Now;

            //This key is for authentication in SendGrid API
            var sendGridAPIKey = Environment.GetEnvironmentVariable("SENDGRID_KEY");

            //Initializing new instance of the API client
            var mailClient = new SendGridClient(sendGridAPIKey);

            //Initializing new message
            var message = new SendGridMessage()
            {
                From = new EmailAddress("admin@parkkl.com", "ParkKL"),
            };

            //Configuring the E-mail message to fit a saved template
            message.SetTemplateId("7f25958d-a1bb-47e8-95aa-59a0e308373e");
            message.AddSubstitution("-FIRST-", user.FirstName);
            message.AddSubstitution("-DATE-", paymentTime.ToString("g"));
            message.AddSubstitution("-AMOUNT-", Amount);
            message.AddSubstitution("-BALANCE-", wallet.Balance.ToString());
            message.AddTo(new EmailAddress(wallet.Email, "User"));

            //Sending the E-mail message
            var response = await mailClient.SendEmailAsync(message);
        }

        public async Task SendParkingReceipt(Wallet wallet, string Id, string Amount, string parkingId)
        {
            User user = await context.Users.FindAsync(Id);
            Parking parking = await context.Parking.FindAsync(parkingId);

            DateTime paymentTime = DateTime.Now;

            //This key is for authentication in SendGrid API
            var sendGridAPIKey = Environment.GetEnvironmentVariable("SENDGRID_KEY");

            //Initializing new instance of the API client
            var mailClient = new SendGridClient(sendGridAPIKey);

            //Initializing new message
            var message = new SendGridMessage()
            {
                From = new EmailAddress("admin@parkkl.com", "ParkKL"),
            };

            //Configuring the E-mail message to fit a saved template
            message.SetTemplateId("12ee3b1e-cca1-4844-ad8b-d0f78fd87299");
            message.AddSubstitution("-FIRST-", user.FirstName);
            message.AddSubstitution("-DATE-", paymentTime.ToString("g"));
            message.AddSubstitution("-LOCATION-", parking.LocationName);
            message.AddSubstitution("-PLATE-", parking.CarPlateNumber);
            message.AddSubstitution("-DURATION-", parking.ParkedHours.ToString());
            message.AddSubstitution("-AMOUNT-", Amount);
            message.AddSubstitution("-BALANCE-", wallet.Balance.ToString());
            message.AddTo(new EmailAddress(wallet.Email, "User"));

            //Sending the E-mail message
            var response = await mailClient.SendEmailAsync(message);
        }

        public async Task SendExtenstionReceipt(Wallet wallet, string Id, string Amount, string parkingId, string duration)
        {
            User user = await context.Users.FindAsync(Id);
            Parking parking = await context.Parking.FindAsync(parkingId);

            DateTime paymentTime = DateTime.Now;

            //This key is for authentication in SendGrid API
            var sendGridAPIKey = Environment.GetEnvironmentVariable("SENDGRID_KEY");

            //Initializing new instance of the API client
            var mailClient = new SendGridClient(sendGridAPIKey);

            //Initializing new message
            var message = new SendGridMessage()
            {
                From = new EmailAddress("admin@parkkl.com", "ParkKL"),
            };

            //Configuring the E-mail message to fit a saved template
            message.SetTemplateId("88487b3b-6a5c-44aa-8c54-2265d5f274df");
            message.AddSubstitution("-FIRST-", user.FirstName);
            message.AddSubstitution("-DATE-", paymentTime.ToString("g"));
            message.AddSubstitution("-LOCATION-", parking.LocationName);
            message.AddSubstitution("-PLATE-", parking.CarPlateNumber);
            message.AddSubstitution("-DURATION-", parking.ParkedHours.ToString());
            message.AddSubstitution("-AMOUNT-", Amount);
            message.AddSubstitution("-BALANCE-", wallet.Balance.ToString());
            message.AddTo(new EmailAddress(wallet.Email, "User"));

            //Sending the E-mail message
            var response = await mailClient.SendEmailAsync(message);
        }

        public async Task SendPenaltyInvoice(Penalty penalty)
        {
            Parking parking = await context.Parking.FindAsync(penalty.ParkingId);

            DateTime penaltyTime = DateTime.Now;
            DateTime startTime = DateTime.Parse(Convert.ToString(parking.CreatedAt));
            DateTime expectedEnd = startTime.AddHours(Convert.ToDouble(parking.ParkedHours));

            //This key is for authentication in SendGrid API
            var sendGridAPIKey = Environment.GetEnvironmentVariable("SENDGRID_KEY");

            //Initializing new instance of the API client
            var mailClient = new SendGridClient(sendGridAPIKey);

            //Initializing new message
            var message = new SendGridMessage()
            {
                From = new EmailAddress("admin@parkkl.com", "ParkKL"),
            };

            message.SetTemplateId("a1049d0f-08ed-4a78-8b3e-d1e890b41ec0");
            message.AddSubstitution("-PLATE-", parking.CarPlateNumber);
            message.AddSubstitution("-LOCATION-", parking.LocationName);
            message.AddSubstitution("-ISSUETIME-", penaltyTime.ToString("g"));
            message.AddSubstitution("-ENDTIME-", expectedEnd.ToString("g"));
            message.AddSubstitution("-EXCEED-", penalty.ExceededHours);

            message.AddTo(new EmailAddress(penalty.Email, "User"));

            //Sending the E-mail message
            var response = await mailClient.SendEmailAsync(message);
        }

        public async Task SendPenaltyReceipt(Wallet wallet, string parkingId, string Id, string penaltyId)
        {
            User user = await context.Users.FindAsync(Id);
            Parking parking = await context.Parking.FindAsync(parkingId);
            Penalty penalty = await context.Penalty.FindAsync(penaltyId);

            DateTime paymentTime = DateTime.Now;
            DateTime startTime = DateTime.Parse(Convert.ToString(parking.CreatedAt));
            DateTime expectedEnd = startTime.AddHours(Convert.ToDouble(parking.ParkedHours));

            //This key is for authentication in SendGrid API
            var sendGridAPIKey = Environment.GetEnvironmentVariable("SENDGRID_KEY");

            //Initializing new instance of the API client
            var mailClient = new SendGridClient(sendGridAPIKey);

            //Initializing new message
            var message = new SendGridMessage()
            {
                From = new EmailAddress("admin@parkkl.com", "ParkKL"),
            };

            //Configuring the E-mail message to fit a saved template
            message.SetTemplateId("a18f4e89-4212-4820-b7bd-7ad5ee52d410");
            message.AddSubstitution("-FIRST-", user.FirstName);
            message.AddSubstitution("-DATE-", paymentTime.ToString("g"));
            message.AddSubstitution("-LOCATION-", parking.LocationName);
            message.AddSubstitution("-PLATE-", parking.CarPlateNumber);
            message.AddSubstitution("-DURATION-", expectedEnd.ToString("g"));
            message.AddSubstitution("-EXCESS-", penalty.ExceededHours);
            message.AddSubstitution("-BALANCE-", wallet.Balance.ToString());
            message.AddTo(new EmailAddress(wallet.Email, "User"));

            //Sending the E-mail message
            var response = await mailClient.SendEmailAsync(message);
        }
    }
}