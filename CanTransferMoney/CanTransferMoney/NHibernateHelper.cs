﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using CanTransferMoney.Domain;

namespace CanTransferMoney
{
    class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;

        private static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    var configuration = new Configuration();
                    configuration.Configure(@"C:\Users\Mariusz\Desktop\Studia\SOA\CanTransferMoney\CanTransferMoney\CanTransferMoney\hibernate.cfg.xml");
                    configuration.AddAssembly(typeof(History).Assembly);
                    _sessionFactory = configuration.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
