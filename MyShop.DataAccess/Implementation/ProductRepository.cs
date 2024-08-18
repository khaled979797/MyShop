using MyShop.DataAccess.Data;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.Implementation
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(Product product)
        {
            var productInDb = context.Products.FirstOrDefault(x => x.Id == product.Id);
            if(productInDb is not null)
            {
                productInDb.Name = product.Name;
                productInDb.Description = product.Description;
                productInDb.Price = product.Price;
                productInDb.Img = product.Img;
                productInDb.CategoryId = product.CategoryId;
            }
        }
    }
}
