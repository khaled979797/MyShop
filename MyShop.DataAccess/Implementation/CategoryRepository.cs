using Microsoft.EntityFrameworkCore;
using MyShop.DataAccess.Data;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.Implementation
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly AppDbContext context;
        public CategoryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(Category category)
        {
            var categoryInDb = context.Categories.FirstOrDefault(x => x.Id == category.Id);
            if (categoryInDb != null)
            {
                categoryInDb.Name = category.Name;
                categoryInDb.Description = category.Description;
                categoryInDb.CreatedTime = DateTime.Now;
            }
        }
    }
}
