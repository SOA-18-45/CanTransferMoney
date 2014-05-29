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

        public List<HistoryItem> toHistoryItem(List<History> items)
        {
            List<HistoryItem> output = new List<HistoryItem>();
            for (int i = 0; i < items.Count; i++)
            {
                output.Add(toHistoryItem(items.ElementAt(i)));
            }
            return output;
        }

        public HistoryItem toHistoryItem(History item)
        {
            HistoryItem historyItem = new HistoryItem();
            historyItem.ID = item.ID;
            historyItem.AccountFrom = item.AccountFrom;
            historyItem.AccountTo = item.AccountTo;
            historyItem.TransactionDate = item.TransactionDate;
            historyItem.Value = item.Value;

            return historyItem;
        }

        public List<HistoryItem> GetHistoryByAccountNumber(string AccountNumber)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<History> results = new List<History>();
                results = (List<History>) session.QueryOver<History>().Where(x => (x.AccountFrom == AccountNumber || x.AccountTo == AccountNumber)).List<History>();
                return toHistoryItem(results);
            }
        }

        public List<HistoryItem> GetHistoryBetweenDatesForAccount(DateTime DateFrom, DateTime DateTo, string AccountNumber)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<History> results = new List<History>();
                results = (List<History>) session.QueryOver<History>().Where(x => (x.TransactionDate >= DateFrom && x.TransactionDate <= DateTo && (x.AccountFrom == AccountNumber || x.AccountTo == AccountNumber))).List<History>();
                return toHistoryItem(results);
            }
        }

        public List<HistoryItem> GetHistoryBetweenDates(DateTime DateFrom, DateTime DateTo)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<History> results = new List<History>();
                results = (List<History>)session.QueryOver<History>().Where(x => (x.TransactionDate >= DateFrom && x.TransactionDate <= DateTo)).List<History>();
                return toHistoryItem(results);
            }
        }

    }
}
