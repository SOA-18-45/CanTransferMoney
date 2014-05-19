﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanTransferMoney.Domain
{
    public class Transaction
    {
        public virtual Guid ID { get; set; }
        public virtual string AccountFrom { get; set; }
        public virtual string AccountTo { get; set; }
        public virtual double Value { get; set; }
        public virtual DateTime DateTime { get; set; }
    }
}
