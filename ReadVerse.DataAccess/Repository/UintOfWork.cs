using ReadVerse.DataAccess.Data;
using ReadVerse.DataAccess.Repository.IRepository;
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

        public UintOfWork(AppDbContext db)
        {
                _db = db;
                Category = new CategoryRepository(_db) ;
                Product = new ProductRepository(_db) ;
                Company = new CompanyRepository(_db) ;
        }

        public void Save()
        {
           _db.SaveChanges();
        }
    }
}
