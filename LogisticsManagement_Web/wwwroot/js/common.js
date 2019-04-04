﻿

function GetListObject(actionUrl) {

    var returnObject = null;

    $.ajax({
        'async': false,
        url: actionUrl,
        type: 'GET',
        dataType: 'json',
        success: function (result) {
            returnObject = result;
        },
        error: function (result) {
            returnObject = result;
        }
    });

    return returnObject;
}

function GetSingleObjectById(actionUrl, id) {

    $.ajax({
        url: actionUrl + '/' + id,
        type: 'GET',
        dataType: 'json',
        success: function (result) {
            return result;
        },
        error: function (result) {
            return result;
        }
    });
}