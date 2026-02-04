using ExpenseManager.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.Models
{
    public class Expense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter Item Name")]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Enter Amount")]
        [Column(TypeName = "decimal(12,2)")]
        [Display(Name = "Amount")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Enter Date and Time")]
        [Display(Name = "Date & Time")]
        public DateTime DateAndTime { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Enter Description")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Select a Category")]
        [ForeignKey("Category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category? Category { get; set; }

        [ForeignKey("User")]
        [ValidateNever]
        public string? UserId { get; set; }

        [ValidateNever]
        public ApplicationUser? User { get; set; }
    }
}
