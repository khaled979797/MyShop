using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        [ValidateNever]
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public int ProductId { get; set; }


        [Range(1, 100, ErrorMessage = "You Must Enter Value Between 1 To 100")]
        public int Count { get; set; }


        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public string ApplicationUserId { get; set; }
    }
}
