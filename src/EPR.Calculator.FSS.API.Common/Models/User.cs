using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.FSS.API.Common.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public Guid? UserId { get; set; }

        [MaxLength(200)]
        [Comment("External Provider Identity ID")]
        public string? ExternalIdpId { get; set; }

        [MaxLength(254)]
        public string? Email { get; set; }
    }
}