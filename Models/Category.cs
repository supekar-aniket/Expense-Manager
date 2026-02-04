using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Display(Name ="Enter Category Name")]
        public string Name { get; set; }

        [ValidateNever] // This tells model binding not to validate this property.
        public ICollection<Expense> Expenses { get; set; }
    }
}
