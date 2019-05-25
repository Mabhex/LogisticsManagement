﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
    public class OrderController : Controller
    {

        //private IMemoryCache _cache;  // To do later 

        private Lms_OrderLogic _orderLogic;
        private Lms_OrderStatusLogic _orderStatusLogic;
        private Lms_CustomerLogic _customerLogic;
        private Lms_EmployeeLogic _employeeLogic;
        private App_CityLogic _cityLogic;
        private App_ProvinceLogic _provinceLogic;
        private Lms_DeliveryOptionLogic _deliveryOptionLogic;
        private Lms_UnitTypeLogic _unitTypeLogic;
        private Lms_WeightScaleLogic _weightScaleLogic;
        private Lms_OrderAdditionalServiceLogic _orderAdditionalServiceLogic;
        private Lms_AdditionalServiceLogic _additionalServiceLogic;
        private Lms_ConfigurationLogic _configurationLogic;
        private Lms_TariffLogic _tariffLogic;

        private readonly LogisticsContext _dbContext;
        IMemoryCache _cache;
        SessionData sessionData = new SessionData();

        public OrderController(IMemoryCache cache, LogisticsContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
            _orderLogic = new Lms_OrderLogic(_cache, new EntityFrameworkGenericRepository<Lms_OrderPoco>(_dbContext));
            _orderStatusLogic = new Lms_OrderStatusLogic(_cache, new EntityFrameworkGenericRepository<Lms_OrderStatusPoco>(_dbContext));
        }

        public IActionResult Index()
        {
            ValidateSession();
            return View(GetAllRequiredDataForDispatchBoard());
        }

        [HttpGet]
        public IActionResult LoadOrdersForDispatch()
        {
            ValidateSession();
            return PartialView("_PartialViewOrderData", GetAllRequiredDataForDispatchBoard());
        }

        [HttpGet]
        public IActionResult LoadDispatchedOrdersForDispatchBoard()
        {
            ValidateSession();
            return PartialView("_PartialViewDispatchedOrders", GetAllRequiredDataForDispatchBoard());
        }

        [HttpPost]
        public IActionResult Add([FromBody]dynamic orderData)
        {
            ValidateSession();
            var result = "";

            try
            {
                if (orderData != null)
                {
                    Lms_OrderPoco orderPoco = JsonConvert.DeserializeObject<Lms_OrderPoco>(JsonConvert.SerializeObject(orderData[0]));
                    List<Lms_OrderAdditionalServicePoco> orderAdditionalServices = JsonConvert.DeserializeObject<List<Lms_OrderAdditionalServicePoco>>(JsonConvert.SerializeObject(orderData[1]));

                    if (orderPoco.Id < 1 && orderPoco.BillToCustomerId > 0 && orderPoco.ShipperCustomerId > 0 && orderPoco.ConsigneeCustomerId > 0)
                    {
                        orderPoco.CreatedBy = sessionData.UserId;
                        var orderInfo = _orderLogic.CreateNewOrder(orderPoco, orderAdditionalServices);
                        if (!string.IsNullOrEmpty(orderInfo))
                        {
                            var jObject = JObject.Parse(orderInfo);
                            var returnedObject = (string)jObject.SelectToken("ReturnedValue");

                            if (returnedObject.Length > 0)
                            {
                                result = returnedObject;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult Update([FromBody]dynamic orderData)
        {
            ValidateSession();
            var result = "";

            try
            {
                if (orderData != null)
                {
                    Lms_OrderPoco orderPoco = JsonConvert.DeserializeObject<Lms_OrderPoco>(JsonConvert.SerializeObject(orderData[0]));
                    List<Lms_OrderAdditionalServicePoco> orderAdditionalServices = JsonConvert.DeserializeObject<List<Lms_OrderAdditionalServicePoco>>(JsonConvert.SerializeObject(orderData[1]));

                    var existingOrder = new Lms_OrderPoco();

                    if (orderPoco != null && orderPoco.OrderTypeId > 0)
                    {
                        existingOrder = _orderLogic.GetList().Where(c => c.OrderTypeId == orderPoco.OrderTypeId && c.WayBillNumber == orderPoco.WayBillNumber).FirstOrDefault();
                    }

                    if (existingOrder != null && existingOrder.Id > 0)
                    {
                        existingOrder.ReferenceNumber = orderPoco.ReferenceNumber;
                        existingOrder.CargoCtlNumber = orderPoco.CargoCtlNumber;
                        existingOrder.AwbCtnNumber = orderPoco.CargoCtlNumber;
                        existingOrder.ShipperCustomerId = orderPoco.ShipperCustomerId;
                        existingOrder.ConsigneeCustomerId = orderPoco.ConsigneeCustomerId;
                        existingOrder.BillToCustomerId = orderPoco.BillToCustomerId;
                        existingOrder.ScheduledPickupDate = orderPoco.ScheduledPickupDate;

                        existingOrder.CityId = orderPoco.CityId;
                        existingOrder.DeliveryOptionId = orderPoco.DeliveryOptionId;
                        existingOrder.VehicleTypeId = orderPoco.VehicleTypeId;
                        existingOrder.WeightScaleId = orderPoco.WeightScaleId;
                        existingOrder.WeightTotal = orderPoco.WeightTotal;
                        existingOrder.UnitQuantity = orderPoco.UnitQuantity;
                        existingOrder.OrderBasicCost = orderPoco.OrderBasicCost;
                        existingOrder.BasicCostOverriden = orderPoco.BasicCostOverriden;
                        existingOrder.FuelSurchargePercentage = orderPoco.FuelSurchargePercentage;
                        existingOrder.DiscountPercentOnOrderCost = orderPoco.DiscountPercentOnOrderCost;
                        existingOrder.ApplicableGstPercent = orderPoco.ApplicableGstPercent;
                        existingOrder.TotalOrderCost = orderPoco.TotalOrderCost;
                        existingOrder.TotalAdditionalServiceCost = orderPoco.TotalAdditionalServiceCost;
                        existingOrder.OrderedBy = orderPoco.OrderedBy;
                        existingOrder.ContactName = orderPoco.ContactName;
                        existingOrder.ContactPhoneNumber = orderPoco.ContactPhoneNumber;
                        existingOrder.Remarks = orderPoco.Remarks;


                        _orderAdditionalServiceLogic = new Lms_OrderAdditionalServiceLogic(_cache, new EntityFrameworkGenericRepository<Lms_OrderAdditionalServicePoco>(_dbContext));

                        var orderServices = _orderAdditionalServiceLogic.GetList().Where(c => c.OrderId == existingOrder.Id).ToList();
                        if (orderServices.Count > 0)
                        {
                            foreach (var item in orderServices)
                            {
                                _orderAdditionalServiceLogic.Remove(item);
                            }
                        }

                        if (orderAdditionalServices.Count > 0)
                        {
                            foreach (var item in orderAdditionalServices)
                            {
                                if (item.AdditionalServiceId > 0)
                                {
                                    _orderAdditionalServiceLogic.Add(item);
                                }
                            }
                        }

                        var poco = _orderLogic.Update(existingOrder);

                        result = poco.Id.ToString();

                    }
                    else if (orderPoco.OrderTypeId == 2)
                    {
                        orderPoco.CreatedBy = sessionData.UserId;
                        var orderInfo = _orderLogic.CreateNewOrder(orderPoco, orderAdditionalServices);
                        if (!string.IsNullOrEmpty(orderInfo))
                        {
                            var jObject = JObject.Parse(orderInfo);
                            var returnedObject = (string)jObject.SelectToken("ReturnedValue");

                            if (returnedObject.Length > 0)
                            {
                                result = returnedObject;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult Remove(string id)
        {
            ValidateSession();
            var result = "";
            try
            {
                using (var scope = new TransactionScope())
                {

                    var orders = _orderLogic.GetList().Where(c => c.WayBillNumber == id).ToList();

                    _orderAdditionalServiceLogic = new Lms_OrderAdditionalServiceLogic(_cache, new EntityFrameworkGenericRepository<Lms_OrderAdditionalServicePoco>(_dbContext));
                    _orderStatusLogic = new Lms_OrderStatusLogic(_cache, new EntityFrameworkGenericRepository<Lms_OrderStatusPoco>(_dbContext));

                    foreach (var order in orders)
                    {
                        var orderServices = _orderAdditionalServiceLogic.GetList().Where(c => c.OrderId == order.Id).ToList();
                        if (orderServices.Count > 0)
                        {
                            foreach (var item in orderServices)
                            {
                                _orderAdditionalServiceLogic.Remove(item);
                            }
                        }

                        var orderStatus = _orderStatusLogic.GetList().Where(c => c.OrderId == order.Id).ToList();

                        if (orderStatus.Count > 0)
                        {
                            foreach (var item in orderStatus)
                            {
                                _orderStatusLogic.Remove(item);
                            }
                        }

                        _orderLogic.Remove(order);
                    }

                    scope.Complete();

                    result = "Success";
                }

            }
            catch (Exception ex)
            {

            }
            return Json(result);
        }

        [HttpPost]
        public IActionResult UpdateDispatchStatus([FromBody]dynamic orderData)
        {
            ValidateSession();
            var result = "";

            try
            {
                if (orderData != null)
                {
                    var wayBillNumberList = JArray.Parse(JsonConvert.SerializeObject(orderData[0]));
                    var employeeNumber = Convert.ToInt32(orderData[1]);
                    var dispatchDate = Convert.ToDateTime(orderData[2]);

                    var orders = _orderLogic.GetList();
                    var orderStatuses = _orderStatusLogic.GetList();

                    using (var scope = new TransactionScope())
                    {
                        foreach (var item in wayBillNumberList)
                        {
                            var wbNumber = item.ToString();

                            var filteredOrders = orders.Where(c => c.WayBillNumber == wbNumber).ToList();
                            foreach (var order in filteredOrders)
                            {
                                var status = orderStatuses.Where(c => c.OrderId == order.Id).FirstOrDefault();

                                status.IsDispatched = true;
                                status.DispatchedToEmployeeId = employeeNumber;
                                status.DispatchedDatetime = dispatchDate == null ? DateTime.Now : dispatchDate;
                                status.StatusLastUpdatedOn = DateTime.Now;

                                _orderStatusLogic.Update(status);
                            }

                        }

                        scope.Complete();

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
        public IActionResult UpdatePickupStatus([FromBody]dynamic orderData)
        {
            ValidateSession();
            var result = "";

            try
            {
                if (orderData != null)
                {
                    var waitTime = string.IsNullOrEmpty(Convert.ToString(orderData[1])) == true ? null : Convert.ToDecimal(orderData[1]);
                    var pickupDate = Convert.ToDateTime(orderData[2]);
                    var orderId = Convert.ToInt16(orderData[3]);

                    var orderStatus = _orderStatusLogic.GetList().Where(c => c.OrderId == orderId).FirstOrDefault();
                    if (orderStatus != null)
                    {
                        using (var scope = new TransactionScope())
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(pickupDate)))
                            {
                                orderStatus.IsPickedup = true;
                            }

                            orderStatus.PickupWaitTimeHour = waitTime;
                            orderStatus.PickupDatetime = pickupDate;
                            orderStatus.StatusLastUpdatedOn = DateTime.Now;

                            _orderStatusLogic.Update(orderStatus);
                            scope.Complete();

                            result = "Success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult UpdatePassonStatus([FromBody]dynamic orderData)
        {

            ValidateSession();
            var result = "";

            try
            {
                if (orderData != null)
                {
                    var wayBillNumber = Convert.ToString(orderData[0]);
                    var waitTime = string.IsNullOrEmpty(Convert.ToString(orderData[1])) == true ? null : Convert.ToDecimal(orderData[1]);
                    var passOnToEmployeeId = string.IsNullOrEmpty(Convert.ToString(orderData[2])) == true ? null : Convert.ToInt16(orderData[2]);
                    var passOffDate = Convert.ToDateTime(orderData[3]);
                    var orderId = Convert.ToInt16(orderData[4]);

                    var orderStatus = _orderStatusLogic.GetList().Where(c => c.OrderId == orderId).FirstOrDefault();

                    if (orderStatus != null)
                    {
                        using (var scope = new TransactionScope())
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(passOffDate)))
                            {
                                orderStatus.IsPassedOff = true;
                            }

                            orderStatus.PassOffWaitTimeHour = waitTime;
                            orderStatus.PassOffDatetime = passOffDate;
                            orderStatus.PassedOffFromEmployeeId = orderStatus.DispatchedToEmployeeId;
                            if (orderStatus.PassedOffToEmployeeId != null) {
                                orderStatus.PassedOffFromEmployeeId = orderStatus.PassedOffToEmployeeId;
                            }
                            orderStatus.PassedOffToEmployeeId = passOnToEmployeeId;
                            orderStatus.StatusLastUpdatedOn = DateTime.Now;

                            _orderStatusLogic.Update(orderStatus);
                            scope.Complete();

                            result = "Success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult UpdateDeliveryStatus([FromBody]dynamic orderData)
        {

            ValidateSession();
            var result = "";

            try
            {
                if (orderData != null)
                {
                    var wayBillNumber = Convert.ToString(orderData[0]);
                    var waitTime = string.IsNullOrEmpty(Convert.ToString(orderData[1])) == true ? null : Convert.ToDecimal(orderData[1]);
                    var deliveryDate = Convert.ToDateTime(orderData[2]);
                    var proofNote = Convert.ToString(orderData[3]);
                    var receivedByName = Convert.ToString(orderData[4]);
                    var receivedBySign = Convert.ToString(orderData[5]);
                    var orderId = Convert.ToInt16(orderData[6]);

                    byte[] imageByte = null;
                    if (receivedBySign != null && receivedBySign != "")
                    {
                        var base64Signature = receivedBySign.Split(",")[1];
                        imageByte = string.IsNullOrEmpty(base64Signature) == true ? null : Convert.FromBase64String(base64Signature);
                    }

                    var orderStatus = _orderStatusLogic.GetList().Where(c => c.OrderId == orderId).FirstOrDefault();

                    if (orderStatus != null)
                    {
                        using (var scope = new TransactionScope())
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(deliveryDate)))
                            {
                                orderStatus.IsDelivered = true;
                            }

                            orderStatus.DeliveredDatetime = deliveryDate;
                            orderStatus.DeliveryWaitTimeHour = waitTime;
                            orderStatus.ReceivedByName = receivedByName;
                            orderStatus.ReceivedBySignature = imageByte;
                            orderStatus.ProofOfDeliveryNote = proofNote;
                            orderStatus.StatusLastUpdatedOn = DateTime.Now;

                            _orderStatusLogic.Update(orderStatus);
                            scope.Complete();

                            result = "Success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult RemoveDispatchStatus(string id)
        {
            var result = "";
            try
            {
                var orders = _orderLogic.GetList().Where(c => c.WayBillNumber == id).ToList();
                var dispatchedList = _orderStatusLogic.GetList();

                dispatchedList = (from dispatch in dispatchedList
                                  join order in orders on dispatch.OrderId equals order.Id
                                  select dispatch).ToList();

                using (var scope = new TransactionScope())
                {
                    foreach (var item in dispatchedList)
                    {
                        item.IsDispatched = null;
                        item.DispatchedDatetime = null;
                        item.DispatchedToEmployeeId = null;

                        _orderStatusLogic.Update(item);
                    }

                    scope.Complete();

                    result = "Success";
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult RemovePickupStatus(string id)
        {
            ValidateSession();
            var result = "";

            try
            {
                if (id != null)
                {
                    var orderStatus = _orderStatusLogic.GetList().Where(c => c.OrderId == Convert.ToInt32(id)).FirstOrDefault();
                    if (orderStatus != null)
                    {
                        using (var scope = new TransactionScope())
                        {
                            orderStatus.IsPickedup = null;
                            orderStatus.PickupWaitTimeHour = null;
                            orderStatus.PickupDatetime = null;
                            orderStatus.StatusLastUpdatedOn = DateTime.Now;

                            _orderStatusLogic.Update(orderStatus);
                            scope.Complete();

                            result = "Success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult RemovePassonStatus(string id)
        {

            ValidateSession();
            var result = "";

            try
            {
                if (id != null)
                {
                    var orderStatus = _orderStatusLogic.GetList().Where(c => c.OrderId == Convert.ToInt32(id)).FirstOrDefault();

                    if (orderStatus != null)
                    {
                        using (var scope = new TransactionScope())
                        {
                            orderStatus.IsPassedOff = null;
                            orderStatus.PassOffWaitTimeHour = null;
                            orderStatus.PassOffDatetime = null;
                            orderStatus.PassedOffToEmployeeId = null;
                            orderStatus.StatusLastUpdatedOn = DateTime.Now;

                            _orderStatusLogic.Update(orderStatus);
                            scope.Complete();

                            result = "Success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult RemoveDeliveryStatus(string id)
        {
            ValidateSession();
            var result = "";

            try
            {
                if (id != null)
                {
                    var orderStatus = _orderStatusLogic.GetList().Where(c => c.OrderId == Convert.ToInt32(id)).FirstOrDefault();
                    if (orderStatus != null)
                    {
                        using (var scope = new TransactionScope())
                        {
                            orderStatus.IsDelivered = null;
                            orderStatus.DeliveredDatetime = null;
                            orderStatus.DeliveryWaitTimeHour = null;
                            orderStatus.ReceivedByName = null;
                            orderStatus.ReceivedBySignature = null;
                            orderStatus.ProofOfDeliveryNote = null;
                            orderStatus.StatusLastUpdatedOn = DateTime.Now;

                            _orderStatusLogic.Update(orderStatus);
                            scope.Complete();

                            result = "Success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(result);
        }


        public JsonResult GetTariffCostByParam(string jsonStringParam)
        {
            ValidateSession();

            decimal shipperCost = 0;
            decimal consigneeCost = 0;
            decimal returnedValue = 0;

            var jsonData = JObject.Parse(jsonStringParam);
            var shipperCityId = (int)jsonData.SelectToken("shipperCityId");
            var consigneeCityId = (int)jsonData.SelectToken("consigneeCityId");
            var deliveryOptionId = (int)jsonData.SelectToken("deliveryOptionId");
            var vehicleTypeId = (int)jsonData.SelectToken("vehicleTypeId");
            var unitTypeId = (int)jsonData.SelectToken("unitTypeId");
            var unitQuantity = (int)jsonData.SelectToken("unitQuantity");
            var weightScaleId = (int)jsonData.SelectToken("weightScaleId");
            var weightQuantity = (decimal)jsonData.SelectToken("weightQuantity");

            _tariffLogic = new Lms_TariffLogic(_cache, new EntityFrameworkGenericRepository<Lms_TariffPoco>(_dbContext));
            var tariffList = _tariffLogic.GetList();

            var shipperTariffInfo = tariffList.Where(c =>
                                                          c.CityId == shipperCityId && c.DeliveryOptionId == deliveryOptionId
                                                          && c.VehicleTypeId == vehicleTypeId && c.UnitTypeId == unitTypeId
                                                          && c.UptoWeight >= weightQuantity
                                                          ).ToList().FirstOrDefault();
            if (shipperTariffInfo != null)
            {
                if (unitQuantity > 0)
                {
                    shipperCost = (decimal)shipperTariffInfo.FirstUnitPrice;
                    if (unitQuantity > 1)
                    {
                        shipperCost = shipperCost + (unitQuantity - 1) * (decimal)shipperTariffInfo.PerUnitPrice;
                    }
                }
            }

            var consigneeTariffInfo = tariffList.Where(c =>
                                                          c.CityId == consigneeCityId && c.DeliveryOptionId == deliveryOptionId
                                                          && c.VehicleTypeId == vehicleTypeId && c.UnitTypeId == unitTypeId
                                                          && c.UptoWeight >= weightQuantity
                                                          ).ToList().FirstOrDefault();

            if (consigneeTariffInfo != null)
            {
                if (unitQuantity > 0)
                {
                    consigneeCost = (decimal)consigneeTariffInfo.FirstUnitPrice;
                    if (unitQuantity > 1)
                    {
                        consigneeCost = consigneeCost + (unitQuantity - 1) * (decimal)consigneeTariffInfo.PerUnitPrice;
                    }
                }
            }

            if (consigneeCost >= shipperCost)
            {
                returnedValue = consigneeCost;
            }
            else
            {
                returnedValue = shipperCost;
            }

            return Json(JsonConvert.SerializeObject(returnedValue));
        }

        public JsonResult GetOrderDetailsByWayBillId(string id)
        {
            ValidateSession();

            try
            {
                var orderPocos = _orderLogic.GetList().Where(c => c.WayBillNumber == id).ToList();

                _orderAdditionalServiceLogic = new Lms_OrderAdditionalServiceLogic(_cache, new EntityFrameworkGenericRepository<Lms_OrderAdditionalServicePoco>(_dbContext));
                var orderAdditionalServices = _orderAdditionalServiceLogic.GetList(); //.Where(c => orderPocos.Select(d => d.Id).ToList().Contains(c.OrderId)).ToList();

                orderAdditionalServices = (from addServ in orderAdditionalServices
                                           join order in orderPocos on addServ.OrderId equals order.Id
                                           select addServ).ToList();

                return Json(JsonConvert.SerializeObject(new { orderPocos, orderAdditionalServices }));
            }
            catch (Exception ex)
            {
                return Json("");
            }
        }

        public JsonResult GetOrderStatusByOrderId(string id)
        {
            ValidateSession();

            try
            {
                var orderStatus = _orderStatusLogic.GetList().Where(c => c.OrderId == Convert.ToInt32(id)).FirstOrDefault();

                return Json(JsonConvert.SerializeObject(orderStatus));
            }
            catch (Exception ex)
            {
                return Json("");
            }
        }


        private DeliveryOrderViewModel GetAllRequiredDataForDispatchBoard()
        {
            DeliveryOrderViewModel deliveryOrderViewModel = new DeliveryOrderViewModel();

            #region Get relevant data for a new order

            _cityLogic = new App_CityLogic(_cache, new EntityFrameworkGenericRepository<App_CityPoco>(_dbContext));
            deliveryOrderViewModel.Cities = _cityLogic.GetList();

            _provinceLogic = new App_ProvinceLogic(_cache, new EntityFrameworkGenericRepository<App_ProvincePoco>(_dbContext));
            deliveryOrderViewModel.Provinces = _provinceLogic.GetList();

            _configurationLogic = new Lms_ConfigurationLogic(_cache, new EntityFrameworkGenericRepository<Lms_ConfigurationPoco>(_dbContext));
            deliveryOrderViewModel.Configuration = _configurationLogic.GetList().FirstOrDefault();

            _customerLogic = new Lms_CustomerLogic(_cache, new EntityFrameworkGenericRepository<Lms_CustomerPoco>(_dbContext));
            deliveryOrderViewModel.Customers = _customerLogic.GetList();

            _deliveryOptionLogic = new Lms_DeliveryOptionLogic(_cache, new EntityFrameworkGenericRepository<Lms_DeliveryOptionPoco>(_dbContext));
            deliveryOrderViewModel.DeliveryOptions = _deliveryOptionLogic.GetList();

            _unitTypeLogic = new Lms_UnitTypeLogic(_cache, new EntityFrameworkGenericRepository<Lms_UnitTypePoco>(_dbContext));
            deliveryOrderViewModel.UnitTypes = _unitTypeLogic.GetList();

            _weightScaleLogic = new Lms_WeightScaleLogic(_cache, new EntityFrameworkGenericRepository<Lms_WeightScalePoco>(_dbContext));
            deliveryOrderViewModel.WeightScales = _weightScaleLogic.GetList();

            _additionalServiceLogic = new Lms_AdditionalServiceLogic(_cache, new EntityFrameworkGenericRepository<Lms_AdditionalServicePoco>(_dbContext));
            deliveryOrderViewModel.AdditionalServices = _additionalServiceLogic.GetList();

            _employeeLogic = new Lms_EmployeeLogic(_cache, new EntityFrameworkGenericRepository<Lms_EmployeePoco>(_dbContext));
            deliveryOrderViewModel.Employees = _employeeLogic.GetList();

            if (deliveryOrderViewModel.Configuration.IsSignInRequiredForDispatch != null)
            {
                if ((bool)deliveryOrderViewModel.Configuration.IsSignInRequiredForDispatch)
                {

                    var _timesheetLogic = new Lms_EmployeeTimesheetLogic(_cache, new EntityFrameworkGenericRepository<Lms_EmployeeTimesheetPoco>(_dbContext));
                    var signedInEmployees = _timesheetLogic.GetList().Where(c => c.SignInDatetime != null && c.SignOutDatetime == null).ToList();

                    deliveryOrderViewModel.Employees = (from employees in deliveryOrderViewModel.Employees
                                                        join signedIn in signedInEmployees on employees.Id equals signedIn.EmployeeId
                                                        select employees).ToList();

                }
            }


            #endregion

            #region get datatable for dispatch board

            List<DispatchBoardDataTable> dataList = new List<DispatchBoardDataTable>();
            var orders = _orderLogic.GetList().Where(c => c.IsInvoiced == false).ToList(); //Load all orders 
            var ordersStatus = _orderStatusLogic.GetList();

            var filteredOrdersForDispatchBoard = (from order in orders
                                                  join status in ordersStatus on order.Id equals status.OrderId
                                                  select new { order, status }).ToList();

            foreach (var item in filteredOrdersForDispatchBoard)
            {
                DispatchBoardDataTable data = new DispatchBoardDataTable();
                data.OrderId = item.order.Id;
                data.OrderTypeId = item.order.OrderTypeId;
                data.OrderTypeFlag = data.OrderTypeId == 1 ? "S" : data.OrderTypeId == 2 ? "R" : "";
                data.WayBillNumber = item.order.WayBillNumber;
                data.OrderDateString = item.order.CreateDate.ToString("dd-MMM-yy");
                data.DeliveryOptionId = (int)item.order.DeliveryOptionId;
                data.DeliveryOptionName = deliveryOrderViewModel.DeliveryOptions.Where(c => c.Id == data.DeliveryOptionId).FirstOrDefault().OptionName;
                data.DeliveryOptionCode = deliveryOrderViewModel.DeliveryOptions.Where(c => c.Id == data.DeliveryOptionId).FirstOrDefault().ShortCode;

                data.CustomerRefNumber = item.order.ReferenceNumber;
                data.UnitTypeId = item.order.UnitTypeId;
                data.UnitTypeName = deliveryOrderViewModel.UnitTypes.Where(c => c.Id == data.UnitTypeId).FirstOrDefault().ShortCode;
                data.UnitQuantity = item.order.UnitQuantity;
                data.SpcIns = "";
                data.ShipperCustomerId = (int)item.order.ShipperCustomerId;
                data.ShipperCustomerName = deliveryOrderViewModel.Customers.Where(c => c.Id == data.ShipperCustomerId).FirstOrDefault().CustomerName;
                data.ConsigneeCustomerId = (int)item.order.ConsigneeCustomerId;
                data.ConsigneeCustomerName = deliveryOrderViewModel.Customers.Where(c => c.Id == data.ConsigneeCustomerId).FirstOrDefault().CustomerName;
                data.BillerCustomerId = item.order.BillToCustomerId;
                data.BillerCustomerName = deliveryOrderViewModel.Customers.Where(c => c.Id == data.BillerCustomerId).FirstOrDefault().CustomerName;

                if (item.status.IsDispatched == null || item.status.IsDispatched == false)
                {
                    data.OrderStatus = "0"; // 0 represents not yet dispatched; just the order is created 

                }
                else if (item.status.IsDispatched == true && (item.status.IsPickedup == null || item.status.IsPickedup == false))
                {
                    data.OrderStatus = "WFP"; // WFP - Waiting for pickup
                    data.RowColorCode = "#f9e6e0";
                }
                else if (item.status.IsPickedup == true && (item.status.IsDelivered == null || item.status.IsDelivered == false))
                {
                    data.OrderStatus = "WFD"; // WFD - Waiting for delivery
                    data.RowColorCode = "#fbffbd";
                }
                else if (item.status.IsDelivered == true)
                {
                    data.OrderStatus = "WFB"; // WFB - Waiting for bill
                    data.RowColorCode = "#ccffc6";
                }

                data.DispatchedToEmployeeId = item.status.DispatchedToEmployeeId;
                if (data.DispatchedToEmployeeId != null)
                {
                    if (item.status.PassedOffToEmployeeId != null)
                    {
                        data.DispatchedToEmployeeId = item.status.PassedOffToEmployeeId;
                    }

                    var empInfo = deliveryOrderViewModel.Employees.Where(c => c.Id == data.DispatchedToEmployeeId).FirstOrDefault();
                    data.DispatchedToEmployeeName = empInfo.FirstName + " " + empInfo.LastName;

                    if (!string.IsNullOrEmpty(empInfo.MobileNumber))
                    {
                        data.DispatchedToEmployeeContactNo = empInfo.MobileNumber;
                    }
                    else if (!string.IsNullOrEmpty(empInfo.PhoneNumber))
                    {
                        data.DispatchedToEmployeeContactNo = empInfo.PhoneNumber;
                    }

                    data.DispatchedToEmployeeEmail = empInfo.EmailAddress;
                }

                dataList.Add(data);
            }

            deliveryOrderViewModel.DispatchBoardData = dataList;

            #endregion

            return deliveryOrderViewModel;
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