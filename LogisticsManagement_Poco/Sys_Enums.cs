﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LogisticsManagement_Poco
{
    public enum OrderType
    {
        DeliveryOrder = 1,
        ReturnOrder,
        MiscellaneousOrder,
    }

    public enum TaxToCall
    {
        GST = 1,
        HST,
        PST,
        QST,
        TAX,
        VAT
    }

    public enum AccountType
    {
        Asset = 1,
        Liability,
        Income,
        Expense
    }
}
