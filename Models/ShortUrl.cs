using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortener.Models
{
    public class ShortUrl
    {
        public int Id { get; set; }

        [Required]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string OriginalUrl { get; set; }

        [Required]
        [MaxLength(20)]
        public string ShortCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to AspNetUsers
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
} 