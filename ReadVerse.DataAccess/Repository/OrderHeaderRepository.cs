using ReadVerse.DataAccess.Data;
using ReadVerse.DataAccess.Repository.IRepository;
using ReadVerse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadVerse.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly AppDbContext _db;
        public OrderHeaderRepository(AppDbContext db) : base(db)
        {

            _db = db;
        }


        public void Update(OrderHeader orderHeader)
        {
            _db.orderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var OrderFromDb = _db.orderHeaders.FirstOrDefault(u => u.Id == id);
            if (OrderFromDb != null)
            {
                OrderFromDb.OrderStatus = orderStatus;
                if (paymentStatus != null)
                {
                    OrderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var OrderFromDb = _db.orderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                OrderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                OrderFromDb.PaymentIntentId = paymentIntentId;
                OrderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
