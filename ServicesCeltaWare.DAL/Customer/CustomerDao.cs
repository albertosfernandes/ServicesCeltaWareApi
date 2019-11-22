using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServicesCeltaWare.DAL.Customer
{
    public class CustomerDao
    {
        private readonly ServicesCeltaWareContext _context;

        public CustomerDao(ServicesCeltaWareContext context)
        {
            _context = context;
        }

        public IList<ModelCustomer> GetAll()
        {
            return _context.Customer.ToList();
        }
    }
}
