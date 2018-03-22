using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using com.parkkl.backend.Models;
using System.IdentityModel.Tokens;

namespace com.parkkl.backend.Controllers
{
    public class ParkingController : TableController<Parking>
    {
        private Mailer mailer = new Mailer();

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Parking>(context, Request);
        }

        // GET tables/Parking
        [Authorize]
        public IQueryable<Parking> GetAll()
        {
            return Query(); 
        }

        // GET tables/Parking/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public SingleResult<Parking> GetSpecific(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Parking/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task<Parking> PatchParking(string id, Delta<Parking> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Parking
        public async Task<IHttpActionResult> PostParking(Parking item)
        {
            Parking current = await InsertAsync(item);
            //await mailer.SendConfirmationEmail(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Parking/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task DeleteParking(string id)
        {
             return DeleteAsync(id);
        }
    }
}
