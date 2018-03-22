namespace com.parkkl.backend.Models
{
    using Microsoft.Azure.Mobile.Server;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class User : EntityData
    {
        [Required]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool EmailConfirmed { get; set; }

        [Required]
        [StringLength(50)]
        public string Password { get; set; }

        [DefaultValue(0.00)]
        public double WalletBalance { get; set; }
    }
}
