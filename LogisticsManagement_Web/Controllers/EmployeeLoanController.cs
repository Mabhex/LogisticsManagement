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
    public class EmployeeLoanController : Controller
    {
        private Lms_EmployeeLoanLogic employeeLoanLogic;
        private readonly LogisticsContext _dbContext;

        public EmployeeLoanController(LogisticsContext dbContext)
        {
            _dbContext = dbContext;
            employeeLoanLogic = new Lms_EmployeeLoanLogic(new EntityFrameworkGenericRepository<Lms_EmployeeLoanPoco>(_dbContext));
        }

        public IActionResult Index()
        {
            var customerList = employeeLoanLogic.GetList();
            return View();
        }
    }
}