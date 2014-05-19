using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using CanTransferMoney.Domain;

namespace CanTransferMoney
{
    class DbTest
    {
        static void Main(string[] args)
        {
            LoadHibernateCfg();

            TransactionRepository repo = new TransactionRepository();

            var Transaction1 = new Transaction
            {
                AccountFrom = "Konto_1",
                AccountTo = "Konto_2",
                Value = 100.0,
                DateTime = DateTime.Now
            };

            repo.Add(Transaction1);

            Console.ReadKey();
        }

        public static void LoadHibernateCfg()
        {
            var cfg = new Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof(TransactionRepository).Assembly);
            new SchemaExport(cfg).Execute(true, true, false);
        }
    }
}
