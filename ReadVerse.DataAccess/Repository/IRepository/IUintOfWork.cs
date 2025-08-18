using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadVerse.DataAccess.Repository.IRepository
{

    public interface IUintOfWork
    {
        ICategoryRepository Category { get; }
        void Save();
    }
}
