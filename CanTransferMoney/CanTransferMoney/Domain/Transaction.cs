using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanTransferMoney.Domain
{
    public class History
    {
        public virtual Guid ID { get; set; }
        public virtual string AccountFrom { get; set; }
        public virtual string AccountTo { get; set; }
        public virtual double Value { get; set; }
        public virtual DateTime TransactionDate { get; set; }
    }

    /*
    public static class Extensions
    {
        public static historyItem ToHistoryItem(this History h)
        {
            //var h = new History();
            //h.ToHistoryItem();
            return new historyItem();
        }
    }
    */
}
