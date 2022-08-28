using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebRentServer.NETCore.Models.Entities
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        [Required]
        public string Review { get; set; }
        [Required]
        public DateTime PostedDate { get; set; }
        [Required]
        public int Grade { get; set; }
        public virtual Order Order { get; set; }
    }
}