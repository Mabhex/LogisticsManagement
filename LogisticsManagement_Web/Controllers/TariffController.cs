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
    public class TariffController : Controller
    {
        private Lms_TariffLogic _tariffLogic;
        private App_CityLogic _cityLogic;
        private Lms_DeliveryOptionLogic _deliveryOptionLogic;
        private Lms_VehicleTypeLogic _vehicleTypeLogic;
        private Lms_UnitTypeLogic _unitTypeLogic;
        private Lms_WeightScaleLogic _weightScaleLogic;

        private readonly LogisticsContext _dbContext;
        IMemoryCache _cache;
        SessionData sessionData = new SessionData();

        public TariffController(IMemoryCache cache, LogisticsContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
            _tariffLogic = new Lms_TariffLogic(_cache, new EntityFrameworkGenericRepository<Lms_TariffPoco>(_dbContext));
        }

        public IActionResult Index()
        {
            return View(GetTariffData());
        }

        
        [HttpGet]
        public IActionResult PartialViewDataTable()
        {
            return PartialView("_PartialViewTariffData", GetTariffData());
        }



        [HttpPost]
        public IActionResult Add([FromBody]dynamic tariffData)
        {

            ValidateSession();
            var result = "";

            try
            {
                if (tariffData != null)
                {
                    Lms_TariffPoco poco = JsonConvert.DeserializeObject<Lms_TariffPoco>(JsonConvert.SerializeObject(tariffData));

                    if (poco.Id < 1)
                    {
                        poco.CreatedBy = sessionData.UserId;
                        _tariffLogic.Add(poco);

                        result = "Success";
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult Update([FromBody]dynamic tariffData)
        {
            ValidateSession();

            var result = "";
            try
            {
                var serializedData = JsonConvert.SerializeObject(tariffData);
                Lms_TariffPoco poco = JsonConvert.DeserializeObject<Lms_TariffPoco>(serializedData);
                if (poco.Id > 0)
                {
                    _tariffLogic.Update(poco);
                }

                result = "Success";
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult Remove([FromBody]dynamic tariffData)
        {
            bool result = false;
            try
            {
                var serializedData = JsonConvert.SerializeObject(tariffData);
                Lms_TariffPoco[] pocos = JsonConvert.DeserializeObject<Lms_TariffPoco[]>(serializedData);

                _tariffLogic.Remove(pocos);
                result = true;
            }
            catch (Exception ex)
            {

            }
            return Json(result);
        }

        private TariffViewModel GetTariffData()
        {
            _tariffLogic = new Lms_TariffLogic(_cache, new EntityFrameworkGenericRepository<Lms_TariffPoco>(_dbContext));
            _cityLogic = new App_CityLogic(_cache, new EntityFrameworkGenericRepository<App_CityPoco>(_dbContext));
            _deliveryOptionLogic = new Lms_DeliveryOptionLogic(_cache, new EntityFrameworkGenericRepository<Lms_DeliveryOptionPoco>(_dbContext));
            _vehicleTypeLogic = new Lms_VehicleTypeLogic(_cache, new EntityFrameworkGenericRepository<Lms_VehicleTypePoco>(_dbContext));
            _unitTypeLogic = new Lms_UnitTypeLogic(_cache, new EntityFrameworkGenericRepository<Lms_UnitTypePoco>(_dbContext));
            _weightScaleLogic = new Lms_WeightScaleLogic(_cache, new EntityFrameworkGenericRepository<Lms_WeightScalePoco>(_dbContext));

            TariffViewModel tariffViewModel = new TariffViewModel();

            tariffViewModel.Tariffs = _tariffLogic.GetList();
            tariffViewModel.Cities = _cityLogic.GetList();
            tariffViewModel.DeliveryOptions = _deliveryOptionLogic.GetList();
            tariffViewModel.VehicleTypes = _vehicleTypeLogic.GetList();
            tariffViewModel.UnitTypes = _unitTypeLogic.GetList();
            tariffViewModel.WeightScales = _weightScaleLogic.GetList();

            return tariffViewModel;
        }

        public JsonResult GetTariffById(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var tariff = _tariffLogic.GetSingleById(Convert.ToInt32(id));
                return Json(JsonConvert.SerializeObject(tariff));
            }
            else
            {
                return Json(string.Empty);
            }
        }

        private void ValidateSession()
        {
            if (HttpContext.Session.GetString("SessionData") != null)
            {
                sessionData = JsonConvert.DeserializeObject<SessionData>(HttpContext.Session.GetString("SessionData"));
                if (sessionData == null)
                {
                    Response.Redirect("Login/Index");
                }
            }
            else
            {
                Response.Redirect("Login/InvalidLocation");
            }
        }
    }
}