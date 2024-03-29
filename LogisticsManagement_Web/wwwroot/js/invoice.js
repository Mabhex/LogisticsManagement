﻿
$(document).ready(function () {

    MaskPhoneNumber('#txtBillingPrimaryPhoneNumber');
    MaskPhoneNumber('#txtMailingPrimaryPhoneNumber');

    var currentDate = new Date();
    currentDate.setDate(currentDate.getDate() - 60);
    $('#txtStartDate').val(ConvertDateToUSFormat(currentDate));
    $('#txtToDate').val(ConvertDateToUSFormat(new Date));

    $('#txtChequeDate').val(ConvertDateToUSFormat(new Date));


    $(document).ajaxStart(function () {
        $("#spinnerLoadingDataTable").css("display", "inline-block");
    });
    $(document).ajaxComplete(function () {
        $("#spinnerLoadingDataTable").css("display", "none");
    });
});

var wayBillNumberArray = [];
var wayBillNumberArrayForInvoicePayment = [];
var employeeNumber;
var orderType = 0; // 1: Delivery, 3: Misc order

$('#chkCheckAllOrders').prop('checked', true);
$('.chkOrderSelected').prop('checked', true);
var wbArrayString = $('#hfWaybillArray').val();
if (wbArrayString != null && wbArrayString !== '') {
    wayBillNumberArray = [];
    var wbArray = wbArrayString.split(',');
    $.each(wbArray, function (i, item) {
        if (item !== '') {
            wayBillNumberArray.push({ wbillNumber: parseInt(item) });
        }
    });
}


$('#pending-list').on('change', '.chkOrderSelected', function (event) {
    event.preventDefault();

    var wbNumber =
    {
        wbillNumber: $(this).data('waybillnumber')
    };

    var isChecked = $(this).is(':checked');

    var index = wayBillNumberArray.findIndex(c => c.wbillNumber === wbNumber.wbillNumber);
    if (index >= 0) {
        wayBillNumberArray.splice(index, 1);
    }

    if (isChecked) {
        wayBillNumberArray.push(wbNumber);
    }
});

$('#pending-list').on('change', '#chkCheckAllOrders', function (event) {
    event.preventDefault();

    var isChecked = $(this).is(':checked');
    if (isChecked === true) {
        $('.chkOrderSelected').prop('checked', true);
        var wbArrayString = $('#hfWaybillArray').val();
        wayBillNumberArray = [];
        var wbArray = wbArrayString.split(',');
        $.each(wbArray, function (i, item) {
            if (item !== '') {
                wayBillNumberArray.push({ wbillNumber: parseInt(item) });
            }
        });
    } else {
        $('.chkOrderSelected').prop('checked', false);
        wayBillNumberArray = [];
    }
});

$('#btnDownloadData').unbind().on('click', function (event) {
    event.preventDefault();
    $('#loadInvoicedDataTable').load('Invoice/PartialViewDataTable');

});

$('#invoiced-list').unbind().on('click', '.btnEdit', function () {
    $('#txtInvoiceNumberToModify').val('');
    $('#txtBillerCustomerName').val('');
    $('#txtWaybillNumbers').val('');
    $('#txtTotalInvoiceAmount').val('');

    var invoiceId = $(this).data('invoiceid');
    var billerName = $(this).data('billername');
    var waybillNumbers = $(this).data('waybills');
    var totalAmount = $(this).data('totalamount');

    $('#txtInvoiceNumberToModify').val(invoiceId);
    $('#txtBillerCustomerName').val(billerName);
    $('#txtWaybillNumbers').val(waybillNumbers);
    $('#txtTotalInvoiceAmount').val(totalAmount);

    $('#modifyInvoice').modal({
        backdrop: 'static',
        keyboard: false
    });

    $('#modifyInvoice').modal('show');


});

$('#btnUndoInvoice').unbind().on('click', function () {
    var invoiceId = $('#txtInvoiceNumberToModify').val();
    if (invoiceId !== '') {
        bootbox.confirm("This will remove all associated transactions from the system to allow you to modify order/s. Are you sure you want to continue?", function (result) {
            if (result === true) {
                var resultObject = PerformPostActionWithId('Invoice/UndoInvoicing', invoiceId);
                if (resultObject.length > 0) {
                    bootbox.alert('All relevant transactions have been removed for this invoice. Please regenerate invoice for associated orders.');
                } else {
                    bootbox.alert('Failed! There was an error occurred during this operation. Please check and try again.');
                }
            }
        });
    }
});

$('#btnDeleteInvoice').unbind().on('click', function () {
    var invoiceId = $('#txtInvoiceNumberToModify').val();
    if (invoiceId !== '') {
        bootbox.confirm("This will remove the invoice number along with all associated transactions. This process cannot be undone. Are you sure you want to continue?", function (result) {
            if (result === true) {
                var resultObject = PerformPostActionWithId('Invoice/Remove', invoiceId);
                if (resultObject.length > 0) {
                    bootbox.alert('The invoice number has been removed from the system. Please generate invoice for associated orders.');
                } else {
                    bootbox.alert('Failed! There was an error occurred during this operation. Please check and try again.');
                }
            }
        });
    }
});

$('#frmInvoiceGenerationForm').on('keyup keypress', function (e) {
    var keyCode = e.keyCode || e.which;
    if (keyCode === 13) {
        e.preventDefault();
        return false;
    }
});
$('#frmInvoiceGenerationForm').unbind('submit').submit(function (event) {

    event.preventDefault();

    var dataArray = wayBillNumberArray;

    if (dataArray.length < 1) {
        bootbox.alert('Please select waybill number/s to generate invoice');
        return;
    }

    bootbox.confirm("This will generate invoices for selected customer/s and cannot be undone. Did you see the print preview and found everything ok? ", function (result) {
        if (result === true) {
            PerformPostActionWithObject('Invoice/Add', [dataArray]);
            $('#loadPendingInvoiceDataTable').load('Invoice/PartialPendingInvoiceDataTable');
            wayBillNumberArray = [];
        }
    });
});

$('input[name="orderType"]').on('change', function () {
    $('#pending-list tbody').empty();
    wayBillNumberArray = [];
});

$('#btnFilter').on('click', function (event) {
    event.preventDefault();

    var startDate = $('#txtStartDate').val();
    var toDate = $('#txtToDate').val();
    var selectedCustomer = $('#ddlCustomerId').val();
    orderType = parseInt($('input[name="orderType"]:checked').val());
    var filterData = {
        startDate: startDate,
        toDate: toDate,
        selectedCustomer: selectedCustomer,
        orderType: orderType
    };

    $('#loadPendingInvoiceDataTable').load('Invoice/FilterPendingInvoiceDataTable?filterData=' + JSON.stringify(filterData));
    // doesnt work for some reason. check it later

    event.preventDefault();

});

$('#btnPrintPreview').unbind().on('click', function (event) {
    event.preventDefault();
    if (wayBillNumberArray.length < 1) {
        bootbox.alert('Please select waybill for Print preview');
        return false;
    }

    orderType = parseInt($('input[name="orderType"]:checked').val());

    var printUrl = "";
    if (orderType === 1) {
        printUrl = 'Invoice/PrintDeliveryInvoice';
    } else if (orderType === 3) {
        printUrl = 'Invoice/PrintMiscellaneousInvoice';
    }

    $.ajax({
        'async': false,
        url: printUrl,
        type: 'POST',
        data: JSON.stringify([wayBillNumberArray]),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.length > 0) {
                window.open(result, "_blank");
            }
        },
        error: function (result) {
            bootbox.alert('Error occurred: ' + result);
        }
    });

});

//Payment Colelction
$('#customerdues-list').on('click', '.lnkCollectPayment', function (event) {
    event.preventDefault();

    var customerId = $(this).data('customerid');
    var custName = $(this).data('customername');
    //$('#txtCustomerName').val(custName);
    $('#lblCustomerName').text(custName);
    $('#lblCustomerNo').text(customerId);

    ClearPaymentForm();
    LoadDueInvoicesByCustomer(customerId);

    $('#collectPayment').modal({
        backdrop: 'static',
        keyboard: false
    });
    $('#collectPayment').modal('show');
});

$('#ddlPaymentMethodId').on('change', function () {
    var selectedValue = parseInt($('#ddlPaymentMethodId').val());
    if (selectedValue === 3) {
        $('#ddlBankId').prop('disabled', true);
        $('#ddlBankId').val('0');
        $('#txtChequeNo').prop('disabled', true);
        $('#txtChequeNo').val('');
        $('#txtChequeDate').prop('disabled', true);
        $('#txtChequeDate').val('');
        $('#txtChequeAmount').prop('disabled', true);
        $('#txtChequeAmount').val('');
        $('#txtCashAmount').prop('disabled', false);
    } else if (selectedValue === 2) {
        $('#ddlBankId').prop('disabled', false);
        $('#txtChequeNo').prop('disabled', false);
        $('#txtChequeDate').prop('disabled', false);
        $('#txtChequeAmount').prop('disabled', false);
        $('#txtCashAmount').prop('disabled', false);
    } else {
        $('#ddlBankId').prop('disabled', false);
        $('#txtChequeNo').prop('disabled', false);
        $('#txtChequeDate').prop('disabled', false);
        $('#txtChequeAmount').prop('disabled', false);
        $('#txtCashAmount').prop('disabled', true);
        $('#txtCashAmount').val('');
    }

    CalculatePaymentAppliedAmount();
});

$('#chkPayAllWaybill').unbind().on('change', function (event) {
    event.preventDefault();
    var isChecked = $('#chkPayAllWaybill').is(':checked');
    if (isChecked === true) {
        $('.chkAddToPayment').prop('checked', true);
        $('.chkAddToPayment').trigger('change');
    }
    else {
        $('.chkAddToPayment').prop('checked', false);
        $('.chkAddToPayment').trigger('change');
    }
});

function LoadDueInvoicesByCustomer(customerId) {
    var customerWiseDueInvoices = GetListById('GetDueInvoicesByCustomerId', customerId);

    var invoices = JSON.parse(customerWiseDueInvoices);

    $('#customer-wise-due-invoices tbody').empty();
    $('.customer-wise-due-invoices tbody').empty();

    $('#waybill-list-for-invoice-payment tbody').empty();
    $('.waybill-list-for-invoice-payment tbody').empty();

    $.each(invoices, function (i, item) {

        var paidAmnt = item.PaidAmount == null ? 0 : parseFloat(item.PaidAmount);
        var remainingAmount = (item.TotalInvoiceAmount - item.PaidAmount).toFixed(2);
        var appendString = "";
        appendString += "<tr>";
        appendString += "<td>";
        appendString += "<a style='color:#1b8eb7;cursor:pointer;font-weight:bolder' role='button' id='lnkDisplayInvoice' class='lnkDisplayInvoice' onclick='FillInvoiceData(this)' data-invoicedate='" + item.CreateDate + "' data-paidamount='" + paidAmnt + "' data-remainingamount='" + remainingAmount + "'  data-invoiceid='" + item.Id + "'>" + item.Id + "</a>";
        appendString += "</td>";
        appendString += "<td>";
        appendString += ConvertDateToUSFormat(item.CreateDate);
        appendString += "</td>";
        appendString += "<td>";
        appendString += item.TotalInvoiceAmount;
        appendString += "</td>";
        appendString += "<td>";
        if (item.PaidAmount == null) {
            appendString += 0;
        } else {
            appendString += item.PaidAmount;
        }
        appendString += "</td>";
        appendString += "<td>";
        appendString += remainingAmount;
        appendString += "</td>";
        appendString += "</tr>";
        $('.customer-wise-due-invoices').append(appendString);
    });

}

function FillInvoiceData(event) {
    var invoiceId = event.dataset.invoiceid;
    var remaining = event.dataset.remainingamount;
    var paidAmount = event.dataset.paidamount;
    var invoiceDate = event.dataset.invoicedate;
    $('#txtInvoiceNo').val(invoiceId);
    $('#txtPaidAmount').val(paidAmount);
    $('#txtDueAmount').val(remaining);
    $('#txtRemainingAmount').val(remaining);
    $('#txtInvoiceDate').val(ConvertDateToUSFormat(new Date(invoiceDate)));

    var invoiceWiseWaybills = GetListById('GetDueWaybillsByInvoiceId', invoiceId);
    var waybills = JSON.parse(invoiceWiseWaybills);

    $('#waybill-list-for-invoice-payment tbody').empty();
    $('.waybill-list-for-invoice-payment tbody').empty();

    $.each(waybills, function (i, item) {
        var appendString = "";
        appendString += "<tr>";
        appendString += "<td>";
        appendString += item.WaybillNumber;
        appendString += "</td>";
        appendString += "<td>";
        appendString += ConvertDateToUSFormat(item.PickupDate);
        appendString += "</td>";
        appendString += "<td>";
        appendString += ConvertDateToUSFormat(item.DeliveryDate);
        appendString += "</td>";
        appendString += "<td>";
        appendString += item.TotalWaybillAmount;
        appendString += "</td>";
        //appendString += "<td>";
        //appendString += item.TotalTaxAmount;
        //appendString += "</td>";

        appendString += "<td>";

        var disabled = '';
        var checked = '';
        if (item.IsCleared === true) {
            disabled = 'disabled';
            checked = 'checked';
        }
        appendString += "<input type='checkbox' class='chkAddToPayment' onchange='AddWayBillToPayment(this)' data-waybillnumber='" + item.WaybillNumber + "' data-totalwaybillamount='" + item.TotalWaybillAmount + "' " + checked + " " + disabled + " />";
        appendString += "</td>";
        appendString += "</tr>";
        $('.waybill-list-for-invoice-payment tbody').append(appendString);

    });
}

function AddWayBillToPayment(event) {

    var waybillAmount = parseFloat(event.dataset.totalwaybillamount);

    var waybillNo = event.dataset.waybillnumber;
    var wbNumber =
    {
        wbillNumber: waybillNo
    };
    var index = wayBillNumberArrayForInvoicePayment.findIndex(c => c.wbillNumber === wbNumber.wbillNumber);
    if (index >= 0) {
        wayBillNumberArrayForInvoicePayment.splice(index, 1);
    }

    var chequeAmount = $('#txtChequeAmount').val() === '' ? 0 : parseFloat($('#txtChequeAmount').val());
    var dueAmount = $('#txtDueAmount').val() === '' ? 0 : parseFloat($('#txtDueAmount').val());

    var isChecked = event.checked;
    if (isChecked === true) {
        chequeAmount = chequeAmount + waybillAmount;
        wayBillNumberArrayForInvoicePayment.push(wbNumber);

    }
    else {
        if (chequeAmount >= waybillAmount) {
            if (chequeAmount > dueAmount) {
                chequeAmount = dueAmount;
            }
            chequeAmount = chequeAmount - waybillAmount;
        } else if (chequeAmount >= dueAmount) {
            chequeAmount = chequeAmount - dueAmount;
        }
    }

    if (chequeAmount > dueAmount) {
        chequeAmount = dueAmount;
    }

    $('#txtChequeAmount').val(chequeAmount.toFixed(2));
    $('#txtChequeAmount').trigger('change');
}

$('#txtChequeAmount').on('change', function () {
    CalculatePaymentAppliedAmount();
});
$('#txtCashAmount').on('change', function () {
    CalculatePaymentAppliedAmount();
});

$('#txtPaymentApplied').on('change', function () {
    var paymentApplied = $('#txtPaymentApplied').val() === '' ? 0 : parseFloat($('#txtPaymentApplied').val());
    var dueAmount = $('#txtDueAmount').val() === '' ? 0 : parseFloat($('#txtDueAmount').val());
    var remainingAmount = 0;
    remainingAmount = dueAmount - paymentApplied;

    $('#txtRemainingAmount').val(remainingAmount.toFixed(2));
});

function CalculatePaymentAppliedAmount() {
    var chequeAmnt = 0;
    var cashAmnt = 0;
    var paymentApplied = 0;

    chequeAmnt = $('#txtChequeAmount').val() === '' ? 0 : parseFloat($('#txtChequeAmount').val());
    cashAmnt = $('#txtCashAmount').val() === '' ? 0 : parseFloat($('#txtCashAmount').val());
    paymentApplied = $('#txtPaymentApplied').val() === '' ? 0 : parseFloat($('#txtPaymentApplied').val());

    paymentApplied = chequeAmnt + cashAmnt;

    $('#txtPaymentApplied').val(paymentApplied.toFixed(2));
    $('#txtPaymentApplied').trigger('change');

}

function ClearPaymentForm() {
    $('#txtInvoiceNo').val('');
    $('#txtPaidAmount').val('');
    $('#txtDueAmount').val('');
    $('#txtInvoiceDate').val('');


    //$('#ddlPaymentMethodId').val('0');
    $('#ddlBankId').val('0');
    $('#txtChequeNo').val('');
    $('#txtChequeAmount').val('');
    $('#txtChequeDate').val('');
    $('#txtCashAmount').val('');
    $('#txtPaymentApplied').val('');
    $('#txtRemainingAmount').val('');
    $('#txtPaymentRemarks').val('');

    $('#ddlBankId').prop('disabled', false);
    $('#txtChequeNo').prop('disabled', false);
    $('#txtChequeDate').prop('disabled', false);
    $('#txtChequeAmount').prop('disabled', false);
    $('#txtCashAmount').prop('disabled', false);

    $('#ddlPaymentMethodId').trigger('change');
}

$('#btnMakePayment').unbind().on('click', function (event) {
    event.preventDefault();

    var data = {
        invoiceNo: $('#txtInvoiceNo').val() === '' ? 0 : parseInt($('#txtInvoiceNo').val()),
        billerCustomerId: $('#lblCustomerNo').text() === '' ? 0 : parseInt($('#lblCustomerNo').text()),
        paymentAmount: $('#txtPaymentApplied').val() === '' ? 0 : parseFloat($('#txtPaymentApplied').val()),
        ddlBankId: $('#ddlBankId').val() === '0' ? 0 : parseInt($('#ddlBankId').val()),
        chequeNo: $('#txtChequeNo').val(),
        chequeDate: $('#txtChequeDate').val(),
        chequeAmount: $('#txtChequeAmount').val() === '' ? 0 : parseFloat($('#txtChequeAmount').val()),
        cashAmount: $('#txtCashAmount').val() === '' ? 0 : parseFloat($('#txtCashAmount').val()),
        paymentRemarks: $('#txtPaymentRemarks').val()
    };

    if (data.invoiceNo <= 0) {
        bootbox.alert('Please select invoice number to make a payment.');
        return;
    }

    if (data.paymentAmount === '' || data.paymentAmount <= 0) {
        bootbox.alert('Payment amount is required. Please check and try again.');
        return;
    }

    if (data.ddlBankId > 0 || data.chequeNo !== '' || data.chequeDate !== '' || data.chequeAmount > 0) {
        if (data.ddlBankId < 1) {
            bootbox.alert('Please select bank information');
            return;
        }
        if (data.chequeNo === '') {
            bootbox.alert('Please enter cheque number');
            return;
        }
        if (data.chequeDate === '') {
            bootbox.alert('Please enter cheque date');
            return;
        }
        if (data.chequeAmount === '' || data.chequeAmount <= 0) {
            bootbox.alert('Please enter cheque amount');
            return;
        }
    }

    var result = PerformPostActionWithObject('MakePayment', [data, wayBillNumberArrayForInvoicePayment]);
    if (result.length > 0) {
        LoadDueInvoicesByCustomer(data.billerCustomerId);
        bootbox.alert('Payment successfully made.');

        wayBillNumberArrayForInvoicePayment = [];

    } else {
        bootbox.alert('Failed! There was an error occurred while making the payment.');
        return;
    }

    ClearPaymentForm();

    var isApplyToNextInvoice = $('#chkKeepBankingInformation').is(':checked');
    if (isApplyToNextInvoice === true) {
        $('#ddlBankId').val(data.ddlBankId);
        $('#txtChequeNo').val(data.chequeNo);
        $('#txtChequeDate').val(data.chequeDate);
    }
});



