using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using Contracts;

namespace AccountRepository
{
    public class MockAccountDetails : AccountDetails
    {
        public MockAccountDetails(Guid id, string AccountNumber, double Money)
        {
            this.Id = id;
            this.AccountNumber = AccountNumber;
            this.Money = Money;
        }
    }
}
