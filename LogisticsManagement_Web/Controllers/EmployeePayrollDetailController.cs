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
    public class EmployeePayrollDetailController : Controller
    {
        private Lms_EmployeePayrollDetailLogic _employeePayrollDetailLogic;
        private readonly LogisticsContext _dbContext;

        public EmployeePayrollDetailController(LogisticsContext dbContext)
        {
            _dbContext = dbContext;
            _employeePayrollDetailLogic = new Lms_EmployeePayrollDetailLogic(new EntityFrameworkGenericRepository<Lms_EmployeePayrollDetailPoco>(_dbContext));
        }

        public IActionResult Index()
        {
            var customerList = _employeePayrollDetailLogic.GetList();
            return View();
        }
    }
}