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
    public class UintOfWork : IUintOfWork
    {
        private readonly AppDbContext _db;
        public ICategoryRepository Category{ get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IProductRepository Product {  get; private set; }
        public IShoppingCartRepository ShoppingCart {  get; private set; }
        public IApplicationUserRepository applicationUserRepository {  get; private set; }

        public IOrderDetailRepository orderDetail {  get; private set; }

        public IOrderHeaderRepository orderHeader { get; private set; }

        public UintOfWork(AppDbContext db)
        {
                _db = db;
            orderDetail=new OrderDetailRepository(_db);
            orderHeader=new OrderHeaderRepository(_db);
            applicationUserRepository =new ApplicationUserRepository(_db);
            ShoppingCart=new ShoppingCartRepository(_db);
                Category = new CategoryRepository(_db) ;
                Product = new ProductRepository(_db) ;
            Company = new CompanyRepository(_db);
                
        }

        public void Save()
        {
           _db.SaveChanges();
        }
    }
}
