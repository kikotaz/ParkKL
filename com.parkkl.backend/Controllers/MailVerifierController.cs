using com.parkkl.backend.Models;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace com.parkkl.backend.Controllers
{
    /*This class will verify that the user E-mail address is correct.
     *If the E-mail is correct, the email confirmation status will be changed
     * from False to True in the database, and show a response message*/

    //Configuring the controller route
    [Route("MailVerifier")]
    public class MailVerifierController : ApiController
    {
        //Initializing Database context to deal with the DB
        MobileServiceContext db;
        User claimUser = new User();

        public MailVerifierController()
        {
            db = new MobileServiceContext();
        }

        //Invoking the confirmation token through GET method
        [HttpGet]
        public HttpResponseMessage Get(string token)
        {
            //Creating token handler instance to resolve the JWT Security Token
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var claimToken = handler.ReadToken(token) as JwtSecurityToken;

            //Retrieving the claims included in the Token Payload
            claimUser.Id = claimToken.Claims.First(c => c.Type == "sub").Value;
            claimUser.Email = claimToken.Claims.First(c => c.Type == "email").Value;

            //Creating new response message instance
            var response = new HttpResponseMessage();

            //This will check if the E-mail is already confirmed
            if (IsEmailConfirmed(claimUser))
            {
                response.StatusCode = HttpStatusCode.Created;
                response.Content = new StringContent
                    ("<html><body>This E-mail is already confirmed. "
                    +"Please login normally</body></html>");

                response.Content.Headers.ContentType = 
                    new MediaTypeHeaderValue("text/html");

                return response;
            }

            //This will update the confirmation status to True
            else if (ConfirmEmail(claimUser))
            {
                response.StatusCode = HttpStatusCode.OK;

                response.Content = new StringContent
                    ("<html><body>Email confirmed. Please login normally</body></html>");

                response.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("text/html");

                return response;
            }

            //If the token damaged for any reason, will show error message
            else
            {
                response.StatusCode = HttpStatusCode.NotAcceptable;

                response.Content = new StringContent
                    ("<html><body>This request is not valid anymore. "
                    + "Please register new account.</body></html>");

                response.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("text/html");

                return response;
            }
        }

        //This method will find if the E-mail is confirmed or not
        private bool IsEmailConfirmed(User user)
        {
            return db.Users.Count(u => u.Email.Equals(user.Email) && u.EmailConfirmed.Equals(true)) > 0;
        }

        //This method will change the confirmation status of the E-mail in DB
        private bool ConfirmEmail(User user)
        {
            bool success = false;

            var toUpdate = db.Users.Find(user.Id);
            db.Users.Attach(toUpdate);
            toUpdate.EmailConfirmed = true;
            db.SaveChanges();
            success = true;
            return success;
        }
    }
}
