using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Models
{
    public class Product
    {
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        [DisplayName("Image")]
        [ValidateNever]
        public string Img { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [ForeignKey("Category")]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        public virtual Category? Category { get; set; }
    }
}
