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
    public class EmployeePayrollController : Controller
    {
        private Lms_EmployeePayrollLogic _employeePayrollLogic;
        private readonly LogisticsContext _dbContext;

        public EmployeePayrollController(LogisticsContext dbContext)
        {
            _dbContext = dbContext;
            _employeePayrollLogic = new Lms_EmployeePayrollLogic(new EntityFrameworkGenericRepository<Lms_EmployeePayrollPoco>(_dbContext));
        }

        public IActionResult Index()
        {
            var customerList = _employeePayrollLogic.GetList();
            return View();
        }
    }
}