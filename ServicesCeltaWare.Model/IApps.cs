using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public interface IApps
    {
        int CustomersProductsId { get; }
        ModelCustomerProduct CustomerProduct { get; }
        string AddressName { get; }
        string IpAddress { get; }
        string Port { get; }
        string UserName { get; }
        string Password { get; }
        string InstallDirectory { get; }
        bool IsCreated { get; }
    }
}
