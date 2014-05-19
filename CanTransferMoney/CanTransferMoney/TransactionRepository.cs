using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CanTransferMoney.Domain;
using NHibernate;

namespace CanTransferMoney
{
    public class TransactionRepository
    {
        public void Add(Transaction newTransaction)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                using (ITransaction dbtransaction = session.BeginTransaction())
                {
                    session.Save(newTransaction);
                    dbtransaction.Commit();
                }
            }
        }
    }
}
