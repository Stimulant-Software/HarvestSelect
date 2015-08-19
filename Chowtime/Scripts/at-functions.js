$.when(showProgress('body', 'loadATEntryPage'),checkKey(), pageLabel(), loadFarmsDDL(userID)).then(function () {
    if ($('body').hasClass('entry')) {
        $('#changeFarm').change(function () {
            farmID = $('option:selected', this).val();
            farmName = $('option:selected', this).text();
            $('#currentFarm').empty().text(farmName);
            farmRepeater(farmID);
        });
        hideProgress('loadATEntryPage');
    } else {
        atLoadReport();
        hideProgress('loadATEntryPage');
    }
});

// BEGIN REPORT CODE
function atLoadReport() {
    var farmID, dateEnd;
    $('#changeFarm').unbind().change(function () {
        farmID = $('option:selected', this).val();
        farmName = $$('option:selected', this).text();
        $('#currentFarm').empty().text(farmName);
        if (dateEnd=="") dateEnd = getDate(1); // yesterday - reports shouldn't include "today"
        pondReport(farmID, dateEnd);
    });

    $('#atReportDateEnding').unbind().blur(function () {
        dateEnd = $(this).val(); // may need adjustment for JSONing
        if (!farmID) { alert("Please select a farm") } else { pondReport(farmID, dateEnd); }
    });
}

function pondReport(farmID, dateEnd) {
    showProgress('body', 'farmRepeater');
    if (!dateEnd) { dateEnd = new Date(); }
    var farmPondsHtml = '', resultsPonds = {}, searchQuery = { "key": _key, "userID": userID, "FarmId": farmID }, data = JSON.stringify(searchQuery);
    $.ajax('../api/Pond/PondList', {
        type: 'POST',
        data: data,
        success: function (msg) {
            startTimer(msg['Key']);
            resultsPonds = msg['ReturnData'];
            if (resultsPonds.length < 1) {
                farmPondsHtml += '<section class="pond" ><p>There are no ponds available for this farm.</p></section>';
            } else {
                for (var i = 0; i < resultsPonds.length; i++) {
                    farmPondsHtml += '<section class="pond" id="pond' + resultsPonds[i].PondId + '" data-pondid="' + resultsPonds[i].PondId + '">';
                    farmPondsHtml += reportRepeater(resultsPonds[i]);
                    farmPondsHtml += '</section>';
                }
            }
            var farmTotalHtml = '<section ID="farmPonds">' + farmPondsHtml + '</section>';
            $.when( $('#farmPonds').empty().html(farmTotalHtml) ).then(function () {
                bindButtons();
                searchQuery = { "key": _key, "userID": userID, "FarmId": farmID, "CurrentTime": dateEnd }, data = JSON.stringify(searchQuery);
                $.ajax('../api/Farm/FarmO2Last3Days', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        startTimer(msg['Key']);
                        var pondList = Object.keys(msg['ReturnData']);
                        for (var i = 0; i < pondList.length; i++) {
                            var dateList = Object.keys(msg['ReturnData'][pondList[i]]);
                            for (var j = 0; j < dateList.length; j++) {
                                var resultsList = msg['ReturnData'][pondList[i]][dateList[j]];
                                var pondID = pondList[i], date = dateList[j], element = j;
                                o2LevelsGraph(pondID, date, resultsList, element);
                                hideProgress('farmRepeater');
                            }
                        }
                    }
                });
            });
        }
    });
}

function reportRepeater(pondInfo) {
    var statusClass, pondHtml = '', day = 0;
    switch (pondInfo.StatusId) { case "1": statusClass = "good"; break; case "2": statusClass = "poor"; break; case "3": statusClass = "bad"; break; }
    pondHtml += '<section class="pond-name" data-status="' + statusClass + '"><span class="name">' + pondInfo.PondName + '</span><button class="status-submit" id="status' + pondInfo.PondId + '">Change Status</button></section>';
    pondHtml += '<section class="aeratorsHX"><section class="aeratorHX" id="' + pondInfo.PondId + '_' + (day + 2) + '"></section><section class="aeratorHX" id="' + pondInfo.PondId + '_' + (day + 1) + '"></section><section class="aeratorHX" id="' + pondInfo.PondId + '_' + (day) + '"></section></section>';
    return pondHtml;
}
// END REPORT CODE

// ENTRY CODE
// Repeaters: Farm totals
function farmRepeater(farmID) {
    showProgress('body', 'farmRepeater');
    var farmPondsHtml = '', resultsPonds = {}, searchQuery = { "key": _key, "userID": userID, "FarmId": farmID }, data = JSON.stringify(searchQuery);
    $.ajax('../api/Pond/PondList', {
        type: 'POST',
        data: data,
        success: function (msg) {
            startTimer(msg['Key']);
            resultsPonds = msg['ReturnData'];
            if(resultsPonds.length < 1) {
                farmPondsHtml += '<section class="pond" ><p>There are no ponds available for this farm.</p></section>';
            } else {
                for (var i = 0; i < resultsPonds.length; i++) {
                    farmPondsHtml += '<section class="pond" id="pond' + resultsPonds[i].PondId + '" data-pondid="' + resultsPonds[i].PondId + '">';
                    farmPondsHtml += pondRepeater(resultsPonds[i]);
                    farmPondsHtml += '</section>';
                }
            }
            var farmTotalHtml = '<section ID="farmPonds">' + farmPondsHtml + '</section>';
            $.when(
                $('#farmPonds').empty().html(farmTotalHtml)
            ).then(function () {
                bindButtons();
                now = new Date();
                console.log(now.getHours());
                if (now.getHours() < 12) now.setDate(now.getDate() - 1);
                console.log(now);
                searchQuery = { "key": _key, "userID": userID, "FarmId": farmID, "CurrentTime": now.toLocaleDateString() + " " + now.toLocaleTimeString().split(" ")[0] + " " + now.toLocaleTimeString().split(" ")[1] }, data = JSON.stringify(searchQuery);
                $.ajax('../api/Farm/FarmO2Last3Days', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        key = msg['Key'];
                        var pondList = Object.keys(msg['ReturnData']);
                        for (var i = 0; i < pondList.length; i++) {
                            var dateList = Object.keys(msg['ReturnData'][pondList[i]]);
                            for (var j = 0; j < dateList.length; j++) {
                                var resultsList = msg['ReturnData'][pondList[i]][dateList[j]];
                                var pondID = pondList[i], date = dateList[j], element = j;
                                o2LevelsGraph(pondID, date, resultsList, element);
                            }
                        }
                        hideProgress('farmRepeater');
                    }
                });
            });
        }
    });
}

// Repeaters: Pond Feeds
function pondRepeater(pondInfo) {
    //  TODO: resultsFeeds should call based on pondID
    // console.log(pondInfo);
    var statusClass, pondHtml = '', day = 0;
    switch (pondInfo.HealthStatus) { case "1": statusClass = "good"; break; case "2": statusClass = "poor"; break; case "3": statusClass = "bad"; break; }
    pondHtml += '<section class="pond-name" data-status="' + statusClass + '"><span class="name">' + pondInfo.PondName + '</span><button class="status-submit" id="status' + pondInfo.PondId + '">Change Status</button></section>';
    pondHtml += '<section class="aeratorsHX"><section class="aeratorHX" id="' + pondInfo.PondId + '_' + (day+2) + '"></section><section class="aeratorHX" id="' + pondInfo.PondId + '_' + (day + 1) + '"></section><section class="aeratorHX" id="' + pondInfo.PondId + '_' + (day) + '"></section></section>';
    pondHtml += '<section class="at-entry"><fieldset class="pondO2"><label>Current O2:</label><input type="number" step=".1" value="" placeholder="0" min="0" max="15" id="currentO2" /></fieldset><fieldset class="aerators"><label>Active Stationary Aerators:</label><input type="number" step="1" placeholder="0" value="" min="0" max="15" id="currentStationaryAerators" /></fieldset><fieldset class="aerators"><label>Active Portable Aerators:</label><input type="number" step="1" value="" placeholder="0" min="0" max="15" id="currentPortableAerators" /></fieldset><button class="submit">Submit</button><button class="edit">Edit</button></section>';
    return pondHtml;
}
// END ENTRY CODE

function o2LevelsGraph( pond, date, readings, element ) {
    var titleText = "O2 readings for night of " + date, pondDOM = "#" + pond + "_" + element, portableArray = [], stationaryArray = [], o2Array = [], timestampArray = [];
    if (readings.length == 0) {
        $(pondDOM).append("<p>There are no results for " + date + "</p>");
    } else {
        for (var i = 0; i < readings.length; i++) { portableArray[i] = parseInt(readings[i]['PortableCount'], 10); stationaryArray[i] = parseInt(readings[i]['StaticCount'], 10); o2Array[i] = parseFloat(readings[i]['O2Level']); timestampArray[i] = formatDateForHighcharts(readings[i]['ReadingDate']); }
        $(pondDOM).highcharts({
            chart: { zoomType: 'xy', alignTicks: false },
            title: { text: titleText },
            xAxis: [{ type: 'datetime', dateTimeLabelFormats: { hour: '%H:%M' }, labels: { enabled: false }, categories: timestampArray  }],
            yAxis: [ { labels: { format: '{value}', style: { color: '#89A54E' } }, title: { text: 'O2 Levels', style: { color: '#89A54E' } }, minRange: 14, min: 0, max: 14, tickInterval: 4 }, // Primary yAxis - O2 level at reading
                { gridLineWidth: 0, title: { text: '# Active Aerators', style: { color: '#4572A7' } }, labels: { format: '{value}', style: { color: '#4572A7' } }, minRange: 6, min: 0, max: 6, opposite: true, tickInterval: 2 }], // Secondary yAxis - aerator count, both stationary and portable
            tooltip: { shared: true },
            legend: { layout: 'vertical', align: 'left', x: 120, verticalAlign: 'top', y: 100, floating: true, backgroundColor: '#FFFFFF', enabled: false },
            credits: { enabled: false },
            plotOptions: { column: { stacking: 'normal' } },
            series: [
                { name: '# Portable Aerators', color: '#AA1919', type: 'column', yAxis: 1, data: portableArray },
                { name: '# Stationary Aerators', color: '#4572A7', type: 'column', yAxis: 1, data: stationaryArray },
                { name: 'O2 Levels', color: '#89A54E', type: 'spline', data: o2Array, minRange: 0 }
            ]
        });
    }
}

function bindButtons() {
    $('.at-entry .submit').unbind().click(function (e) {
        e.preventDefault();
        var pondID = $(this).parent().parent().data('pondid'), o2Input = $(this).parent().find('#currentO2'), o2Reading = o2Input.val() == "" ? "0" : o2Input.val(), aeratorsStationaryInput = $(this).parent().find('#currentStationaryAerators'), aeratorsStationary = aeratorsStationaryInput.val() == "" ? "0" : aeratorsStationaryInput.val(), aeratorsPortableInput = $(this).parent().find('#currentPortableAerators'), aeratorsPortable = aeratorsPortableInput.val() == "" ? "0" : aeratorsPortableInput.val(), currentPond = $(this).parent().parent().find('.aeratorsHX section:last'), now = new Date(), currentSection = $(this).parent().parent();
        showProgress(currentSection, 'submitATEntry');
        var searchQuery = { "key": _key, "ReadingDate": now.toLocaleDateString() + " " + now.toLocaleTimeString().split(" ")[0] + " " + now.toLocaleTimeString().split(" ")[1], "ReadingId": -1, "PondId": pondID, "O2Level": o2Reading, "StaticCount": aeratorsStationary, "PortableCount": aeratorsPortable }, data = JSON.stringify(searchQuery);
        $.ajax('../api/Pond/O2AddOrEdit', {
            type: 'PUT', data: data,
            success: function (msg) {
                startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
                o2Input.val("");
                aeratorsStationaryInput.val("");
                aeratorsPortableInput.val("");
                searchQuery = { "key": _key, "PondId": pondID, "ReadingDate": now }, data = JSON.stringify(searchQuery);
                $.ajax('../api/Pond/PondO2ByDate', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        key = msg['Key'];
                        resultsList = msg['ReturnData'];
                        console.log(now.getHours());
                        if (now.getHours() < 12) { today = getDate(1); } else { today = getDate(); }
                        o2LevelsGraph(pondID, today, resultsList, 0); // 0 refers to numeric part of DOM ID for pond
                    }
                });
                hideProgress('submitATEntry');
            }
        });
    })

    $('.at-entry .edit').unbind().click(function (e) {
        e.preventDefault();
        var pondID = $(this).parent().parent().data('pondid'), currentPond = $(this).parent().prev('aeratorsHX').find('aeratorHX:last');
        bindEditButtons(currentPond);
        $('body').append('<div id="lightboxBG" class="modal"></div>');
        $('#lightboxBG').fadeIn('100', function () { centerModal('#editEntryModal'); $('#editEntryModal').fadeIn('100'); });
        var searchQuery = { "key": _key, "PondId": pondID }, readingInfo = {}; data = JSON.stringify(searchQuery);
        $.ajax('../api/Pond/GetLastPondReading', {
            type: 'POST',
            data: data,
            success: function (msg) {
                startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
                readingInfo = msg['ReturnData'][0];
                $('#readingDateEdit').val(readingInfo.ReadingDate); $('#pondIDEdit').val(readingInfo.PondId); $('#readingIDEdit').val(readingInfo.ReadingId); $('#currentO2Edit').val(readingInfo.O2Level); $('#currentStationaryAeratorsEdit').val(readingInfo.StaticCount); $('#currentPortableAeratorsEdit').val(readingInfo.PortableCount); $('#readingNoteEdit').val(readingInfo.Note);
            }
        });
    });

    $('.pond-name .status-submit').unbind().click(function(e){
        containerElement = $(this).parent().parent();
        showProgress(containerElement, 'changePondStatus');
        e.preventDefault();
        var pondID = $(this).parent().parent().data('pondid'), statusDiv = $(this).parent('.pond-name'), currentStatus = statusDiv.attr('data-status'), newStatus, newStatusClass;
        switch(currentStatus) {
            case "good": newStatus = "2", newStatusClass = "poor"; break; // 2 = poor
            case "poor": newStatus = "3", newStatusClass = "bad"; break; // 3 = bad
            case "bad": newStatus = "1", newStatusClass = "good"; break; // 1 = good
        }

        var submitObject = { "key": _key, "CompanyID": "1", "PondId": pondID, "HealthStatus" : newStatus }, data = JSON.stringify(submitObject);
        $.ajax('../api/Pond/PondHealthStatus', {
            type: 'PUT', data: data,
            success: function (msg) {
                startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
                statusDiv.attr('data-status', newStatusClass);
                hideProgress('changePondStatus');
            }
        });
    });
}

function bindEditButtons(currentPond) {
    $('.cancel').unbind().click(function (e) { e.preventDefault(); resetATEntryModal(); closeAdminModal(); });

    $('#editEntryModal .submit').unbind().click(function (e) {
        e.preventDefault();
        var pondID = $('#pondIDEdit').val(), readingID = $('#readingIDEdit').val(), o2Reading = $('#currentO2Edit').val(), aeratorsStationary = $('#currentStationaryAeratorsEdit').val() == "" ? "0" : $('#currentStationaryAeratorsEdit').val(), aeratorsPortable = $('#currentPortableAeratorsEdit').val() == "" ? "0" : $('#currentPortableAeratorsEdit').val(), readingDate = $('#readingDateEdit').val(), readingNote = $('#readingNoteEdit').val();
        showProgress('body', 'submitATEntryEdit');
        var searchQuery = { "key": _key, "ReadingDate": readingDate, "ReadingId": readingID, "PondId": pondID, "O2Level": o2Reading, "StaticCount": aeratorsStationary, "PortableCount": aeratorsPortable, "Note": readingNote }, data = JSON.stringify(searchQuery);
        $.ajax('../api/Pond/O2AddOrEdit', {
            type: 'PUT', data: data,
            success: function (msg) {
                startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
                closeAdminModal();
                today = getDate();
                searchQuery = { "key": _key, "PondId": pondID, "ReadingDate": today }, data = JSON.stringify(searchQuery);
                $.ajax('../api/Pond/PondO2ByDate', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        startTimer(msg['Key']);
                        resultsList = msg['ReturnData'];
                        o2LevelsGraph(pondID, today, resultsList, 0);
                        hideProgress('submitATEntryEdit');
                    }
                });
            }
        });
    });
}

function resetATEntryModal() { $('#readingDateEdit').val(""); $('#readingIDEdit').val(""); $('#currentO2Edit').val(""); $('#currentStationaryAeratorsEdit').val(""); $('#currentPortableAeratorsEdit').val(""); $('#readingNoteEdit').val(""); }

function closeAdminModal() { $('.admin-modal, #lightboxBG').fadeOut('100', function () { $('#lightboxBG').remove(); }); }

function formatDateForHighcharts(date) {
    var timestamp = date.split(" ");
    return timestamp[1]+timestamp[2];
}