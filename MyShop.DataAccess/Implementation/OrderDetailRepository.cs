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
    public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
    {
        private readonly AppDbContext context;

        public OrderDetailRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(OrderDetail orderDetail)
        {
            context.OrderDetails.Update(orderDetail);
        }
    }
}
