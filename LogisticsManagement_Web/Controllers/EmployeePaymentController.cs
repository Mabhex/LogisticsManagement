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
    public class EmployeePaymentController : Controller
    {
        private Lms_EmployeePaymentLogic _employeePaymentLogic;
        private readonly LogisticsContext _dbContext;

        public EmployeePaymentController(LogisticsContext dbContext)
        {
            _dbContext = dbContext;
            _employeePaymentLogic = new Lms_EmployeePaymentLogic(new EntityFrameworkGenericRepository<Lms_EmployeePaymentPoco>(_dbContext));
        }

        public IActionResult Index()
        {
            var customerList = _employeePaymentLogic.GetList();
            return View();
        }
    }
}