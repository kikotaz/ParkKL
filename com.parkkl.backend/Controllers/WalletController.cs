using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using com.parkkl.backend.Models;
using System.IdentityModel.Tokens;
using System.Collections.Generic;

namespace com.parkkl.backend.Controllers
{
    public class WalletController : TableController<Wallet>
    {
        private Mailer mailer = new Mailer();

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Wallet>(context, Request);
        }

        // GET tables/Wallet
        [Authorize]
        public IQueryable<Wallet> GetBalance()
        {
            return Query(); 
        }

        // GET tables/Wallet/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public SingleResult<Wallet> GetBalance(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Wallet/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public async Task<Wallet> PatchBalance(string id, Delta<Wallet> patch, string userID, string type, string parkingId)
        {
            Wallet newWallet = patch.GetEntity();

            var con = new MobileServiceContext();

            Wallet oldWallet = con.Wallet.Find(newWallet.Id);

            int Amount = newWallet.Balance - oldWallet.Balance;

            if (type == "topUp")
            {
                await mailer.SendTopUpReceipt(newWallet, userID, Amount.ToString());
            }
            else if (type == "parking")
            {
                Amount = Amount * -1;
                await mailer.SendParkingReceipt(newWallet, userID, Amount.ToString(), parkingId);
            }
            else if (type == "extend")
            {
                Amount = Amount * -1;
                int duration = Amount / 2;

                await mailer.SendExtenstionReceipt(newWallet, userID, Amount.ToString(), parkingId,
                    duration.ToString());
            }
            else
            {
                Amount = Amount * -1;
                await mailer.SendPenaltyReceipt(newWallet, parkingId, userID, type);
            }

            return await UpdateAsync(id, patch);
        }

        // POST tables/Wallet
        public async Task<IHttpActionResult> PostBalance(Wallet item, string Id)
        { 
            Wallet current = await InsertAsync(item);
            await mailer.SendTopUpReceipt(item, Id, item.Balance.ToString());
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Wallet/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task DeleteBalance(string id)
        {
             return DeleteAsync(id);
        }

        
    }
}
