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
    public class PayeeController : Controller
    {
        private Lms_PayeeLogic _payeeLogic;
        private readonly LogisticsContext _dbContext;
        IMemoryCache _cache;

        public PayeeController(IMemoryCache cache, LogisticsContext dbContext)
        {
            _dbContext = dbContext;
            _payeeLogic = new Lms_PayeeLogic(_cache, new EntityFrameworkGenericRepository<Lms_PayeePoco>(_dbContext));
        }

        public IActionResult Index()
        {
            var customerList = _payeeLogic.GetList();
            return View();
        }
    }
}