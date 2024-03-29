﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogisticsManagement_BusinessLogic;
using LogisticsManagement_DataAccess;
using LogisticsManagement_Poco;
using LogisticsManagement_Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace LogisticsManagement_Web.Controllers
{
    public class TransactionController : Controller
    {
        private Lms_TransactionLogic _transactionLogic;
        private readonly LogisticsContext _dbContext;
        IMemoryCache _cache;

        public TransactionController(IMemoryCache cache, LogisticsContext dbContext)
        {
            _dbContext = dbContext;
            _transactionLogic = new Lms_TransactionLogic(_cache, new EntityFrameworkGenericRepository<Lms_TransactionPoco>(_dbContext));
        }

        public IActionResult Index()
        {
            var customerList = _transactionLogic.GetList();
            return View();
        }
    }
}