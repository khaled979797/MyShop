using MyShop.DataAccess.Data;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;

namespace MyShop.DataAccess.Implementation
{
    public class OrderHeaderRepository : GenericRepository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly AppDbContext context;

        public OrderHeaderRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(OrderHeader orderHeader)
        {
            context.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string? orderStatus, string? paymentStatus)
        {
            var orderFromDb = context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if(orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                orderFromDb.PaymentDate = DateTime.Now;
                if(paymentStatus != null)
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }
    }
}
