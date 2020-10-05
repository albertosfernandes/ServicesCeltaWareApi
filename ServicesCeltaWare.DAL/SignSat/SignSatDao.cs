using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.DAL.SignSat
{
    public class SignSatDao 
    {
        private ModelSignSat modelSignSat = new ModelSignSat();
        private readonly ServicesCeltaWareContext _context;

        public SignSatDao(ServicesCeltaWareContext context)
        {
            _context = context;
        }
        
    }
}
