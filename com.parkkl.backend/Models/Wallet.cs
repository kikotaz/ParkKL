namespace com.parkkl.backend.Models
{
    using Microsoft.Azure.Mobile.Server;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Http.OData;

    public class Wallet: EntityData
    {
        public string Email { get; set; }

        public int Balance { get; set; }

        public static implicit operator Wallet(Delta<Wallet> v)
        {
            throw new NotImplementedException();
        }
    }
}