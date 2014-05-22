using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CanTransferMoney.Domain;
using NHibernate;
using Contracts;

namespace CanTransferMoney
{
    public class HistoryRepository
    {
        public void Add(History newTransaction)
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

        public List<History> GetHistoryByAccountNumber(string AccountNumber)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<History> results = new List<History>();
                results = (List<History>) session.QueryOver<History>().Where(x => (x.AccountFrom == AccountNumber || x.AccountTo == AccountNumber)).List<History>();
                return results;
            }
        }

        public List<HistoryItem> GetHistoryBetweenDatesForAccount(DateTime DateFrom, DateTime DateTo, string AccountNumber)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<HistoryItem> results = new List<HistoryItem>();
                results = (List<HistoryItem>) session.QueryOver<History>().Where(x => (x.TransactionDate >= DateFrom && x.TransactionDate <= DateTo && (x.AccountFrom == AccountNumber || x.AccountTo == AccountNumber))).List<History>();
                return results;
            }
        }

        public List<HistoryItem> GetHistoryBetweenDates(DateTime DateFrom, DateTime DateTo)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<HistoryItem> results = new List<HistoryItem>();
                results = (List<HistoryItem>)session.QueryOver<History>().Where(x => (x.TransactionDate >= DateFrom && x.TransactionDate <= DateTo)).List<History>();
                return results;
            }
        }

    }
}
