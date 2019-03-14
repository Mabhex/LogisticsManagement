﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogisticsManagement_BusinessLogic;
using LogisticsManagement_DataAccess;
using LogisticsManagement_Poco;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsManagement_Web.Controllers
{
    public class CreditsController : Controller
    {
        private readonly LogisticsContext _dbContext;

        public CreditsController(LogisticsContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}