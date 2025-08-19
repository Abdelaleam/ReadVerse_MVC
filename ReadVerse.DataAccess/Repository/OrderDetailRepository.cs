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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly AppDbContext _db;
        public OrderDetailRepository(AppDbContext db) : base(db)
        {

            _db = db;
        }


        public void Update(OrderDetail orderDetail)
        {
            _db.orderDetails.Update(orderDetail);
        }
    }
}
