using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using com.parkkl.backend.Models;
using Microsoft.Azure.Mobile.Server.Login;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;

namespace com.parkkl.backend.Controllers
{
    [Route(".auth/login/custom")]

    //This controller will be responsible for user credintial authentication
    public class CustomAuthController : ApiController
    {
        //Instantiation of database service context and useful variables
        private MobileServiceContext db;
        private string signingKey, audience, issuer;

        //Defining the controller constructor to get important keys from the backend
        public CustomAuthController()
        {
            db = new MobileServiceContext();
            signingKey = Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY");
            var website = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            audience = $"https://testingparkkl.azurewebsites.net/";
            issuer = $"https://testingparkkl.azurewebsites.net/";
        }


        //RESTful post request to receive authentication token from the Azure
        [HttpPost]
        public IHttpActionResult Post([FromBody] User body)
        {
            //Checking if the incoming request has missing data
            if (body == null || body.Email == null || body.Password == null ||
                body.Email.Length == 0 || body.Password.Length == 0)
            {
                return BadRequest(); ;
            }

            //Checking if the user is valid and exists
            if (!IsValidUser(body))
            {
                return Unauthorized();
            }

            //Checking if the user Email is confirmed
            if (!IsEmailConfirmed(body))
            {
                throw new Exception();
            }

            //Creating debugging session in the backend to detect execution issues
            var telemetry = new TelemetryClient();

            try
            {
                //Checking the claims combined with the HTTP request to create token
                var claims = new Claim[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, body.Email)
                };

                JwtSecurityToken token = AppServiceLoginHandler.CreateToken(
                    claims, signingKey, audience, issuer, TimeSpan.FromDays(30));

                //Returning the authentication token
                return Ok(new LoginResult()
                {
                    AuthenticationToken = token.RawData,
                    User = new LoginResultUser { UserId = body.Email }
                });
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return InternalServerError();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //This method will check if the user is registered or not
        private bool IsValidUser(User user)
        {
            return db.Users.Count(u => u.Email.Equals(user.Email) && u.Password.Equals(user.Password)) > 0;
        }

        //This method will check if the user confirmed his Email or not
        private bool IsEmailConfirmed(User user)
        {
            return db.Users.Count(u => u.Email.Equals(user.Email) && u.EmailConfirmed.Equals(true)) > 0;
        }
    }

    //Defining an internal class for the result that will carry the authentication token
    public class LoginResult
    {

        [JsonProperty(PropertyName = "authenticationToken")]
        public string AuthenticationToken { get; set; }

        [JsonProperty(PropertyName = "user")]
        public LoginResultUser User { get; set; }
    }

    //Defining internal class to get the User ID 
    public class LoginResultUser
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}