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
    public class PenaltyController : TableController<Penalty>
    {
        private Mailer mailer = new Mailer();

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Penalty>(context, Request);
        }

        // GET tables/Penalty
        [Authorize]
        public IQueryable<Penalty> GetAll()
        {
            return Query(); 
        }

        // GET tables/Penalty/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public SingleResult<Penalty> GetSpecific(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Penalty/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task<Penalty> PatchPenalty(string id, Delta<Penalty> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Parking
        public async Task<IHttpActionResult> PostPenalty(Penalty item)
        {
            Penalty current = await InsertAsync(item);
            await mailer.SendPenaltyInvoice(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Penalty/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task DeletePenalty(string id)
        {
             return DeleteAsync(id);
        }
    }
}
