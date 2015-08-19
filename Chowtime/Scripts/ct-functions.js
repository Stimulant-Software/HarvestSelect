showProgress('body', 'loadCTEntryPage');
$.when(checkKey(), pageLabel(), loadFarmsDDL(userID)).then(function () {
    if ($('body').hasClass('entry')) {
        $('#changeFarm').unbind().change(function () {
            showProgress('body', 'farmRepeater');
            farmID = $('option:selected', this).val();
            farmName = $('option:selected', this).text();
            $('#currentFarm').empty().text(farmName);
            var now = new Date(), searchQueryPonds = { "key": _key, "userID": userID, "FarmId": farmID }, dataPonds = JSON.stringify(searchQueryPonds), searchQueryFeeds = { "key": _key, "userID": userID, "FarmId": farmID, "CurrentTime": now }, dataFeeds = JSON.stringify(searchQueryFeeds), pondList = {}, feedlist = {};
            $.when($.ajax('../api/Pond/PondList', {
                type: 'POST',
                data: dataPonds,
                success: function (msg) {
                    startTimer(msg['Key']);
                    pondList = msg['ReturnData'];
                }
            }), $.ajax('../api/Farm/FarmLast7Feeds', {
                // also available: ../api/Farm/FarmFeedLast7Days
                type: 'POST',
                data: dataFeeds,
                success: function (msg) {
                    startTimer(msg['Key']);
                    feedList = (msg['ReturnData']);
                }
            })).then(function () {
                $('#farmContainer').attr('data-farmid', farmID);
                farmRepeater(7, farmID, pondList, feedList)
            });
        });
    } else {
        var farmID, selectedDate;
        $('#reportEndDate').change(function () {
            showProgress('body', 'farmRepeater');
            selectedDate = $(this).val() == "" ? new Date() : $(this).val();
            if (typeof farmID == "string" && farmID != "") {
                var searchQueryPonds = { "key": _key, "userID": userID, "FarmId": farmID }, dataPonds = JSON.stringify(searchQueryPonds), searchQueryFeeds = { "key": _key, "userID": userID, "FarmId": farmID, "CurrentTime": selectedDate }, dataFeeds = JSON.stringify(searchQueryFeeds), pondList = {}, feedlist = {};
                $.when($.ajax('../api/Pond/PondList', {
                    type: 'POST',
                    data: dataPonds,
                    success: function (msg) {
                        startTimer(msg['Key']);
                        pondList = msg['ReturnData'];
                    }
                }), $.ajax('../api/Farm/FarmFeedLast7Days', {
                    type: 'POST',
                    data: dataFeeds,
                    success: function (msg) {
                        startTimer(msg['Key']);
                        feedList = (msg['ReturnData']);
                    }
                })).then(function () {
                    farmReportRepeater(7, farmID, pondList, feedList, selectedDate);
                });
            } 
        });

        $('#changeFarm').change(function () {
            showProgress('body', 'farmRepeater');
            farmID = $('option:selected', this).val();
            farmName = $$('option:selected', this).text();
            $('#currentFarm').empty().text(farmName);
            if (selectedDate != "") { selectedDate = new Date(); }
            var searchQueryPonds = { "key": _key, "userID": userID, "FarmId": farmID }, dataPonds = JSON.stringify(searchQueryPonds), searchQueryFeeds = { "key": _key, "userID": userID, "FarmId": farmID, "CurrentTime": selectedDate }, dataFeeds = JSON.stringify(searchQueryFeeds), pondList = {}, feedlist = {};
            $.when($.ajax('../api/Pond/PondList', {
                type: 'POST',
                data: dataPonds,
                success: function (msg) {
                    startTimer(msg['Key']);
                    pondList = msg['ReturnData'];
                }
            }), $.ajax('../api/Farm/FarmFeedLast7Days', {
                // also available: ../api/Farm/FarmLast7Feeds
                type: 'POST',
                data: dataFeeds,
                success: function (msg) {
                    startTimer(msg['Key']);
                    feedList = (msg['ReturnData']);
                }
            })).then(function () {
                farmReportRepeater(7, farmID, pondList, feedList, selectedDate);
            });
        });
    }        
    hideProgress('loadCTEntryPage');
});

// BEGIN REPORT CODE
function farmReportRepeater(numberOfDays, farmID, pondList, feedList, endDate) {
    if (!endDate) { endDate = new Date() }
    var farmPondsHtml = '', classDays = numberOfDays == 5 ? 'five-day' : 'seven-day', searchQuery = { "key": _key, "FarmId": farmID, "currentTime": endDate }, data = JSON.stringify(searchQuery), headerFeedList = {};
    $.when($.ajax('../api/Farm/FarmFeedLast7DaysTotals', {
        type: 'POST',
        data: data,
        success: function (msg) {
            startTimer(msg['Key']);
            headerFeedList = msg['ReturnData'];
        }
    })).then(function () {
        if (pondList.length < 1) {
            farmPondsHtml += '<section class="pond" ><p>There are no ponds available for this farm.</p></section>';
        } else {
            for (var i = 0; i < pondList.length; i++) {
                // check for suspended feed flag; add class to pond container if flag is on
                var noFeed = "", overlay = "";
                if (pondList[i].NoFeed != "False") {
                    noFeed = " feed-suspended";
                    overlay = '<div class="feed-suspended-overlay"><p>Do Not Feed</p></div>';
                }
                farmPondsHtml += '<section class="pond' + noFeed + '" id="pond' + pondList[i].PondId + '" data-pondid="' + pondList[i].PondId + '">' + overlay;
                farmPondsHtml += pondFeedsReportRepeater(numberOfDays, pondList[i], feedList[pondList[i].PondId], endDate);
                farmPondsHtml += '</section>';
            }
        }

        farmTotalHtml = '<section ID="pondTotal" class="pond clear">' + farmTotalReportHeader(numberOfDays, headerFeedList, endDate) + '</section>';
        farmTotalHtml += '<section ID="farmPonds">' + farmPondsHtml + '</section>';
        $.when(
            $('#farmContainer').empty().html(farmTotalHtml)
        ).then(function () {
            bindButtons();
            hideProgress('farmRepeater');
        });
    });
}

function farmTotalReportHeader(numberOfDays, headerFeedList, endDate) {
    var farmTotalDaysHtml = '', classDays = numberOfDays == 5 ? 'five-day' : 'seven-day', totalFeed = 0, totalPerAcre = 0;

    for (var i = (numberOfDays - 1) ; i >= 0; i--) {
        farmTotalDaysHtml += '<section class="day">';
        if (!headerFeedList[i]) {
            farmTotalDaysHtml += '<header class="date"> -- </header>';
            farmTotalDaysHtml += '<span class="daily-feed">0</span>';
            farmTotalDaysHtml += '<span class="per-acre">0</span>';
        } else {
            farmTotalDaysHtml += '<header class="date">' + headerFeedList[i].FeedDate + '</header>';
            farmTotalDaysHtml += '<span class="daily-feed">' + numberCommas(headerFeedList[i].TotalPoundsFed) + '</span>';
            farmTotalDaysHtml += '<span class="per-acre">' + headerFeedList[i].AveragePoundsFed + '</span>';
            totalFeed += parseInt(headerFeedList[i].TotalPoundsFed, 10);
            totalPerAcre += parseInt(headerFeedList[i].AveragePoundsFed, 10);
        }
        farmTotalDaysHtml += '</section>';
    }
    totalPerAcre = (totalPerAcre / numberOfDays).toFixed(2);
    var farmTotalHtml = '<section class="farm-name"><span class="name">Farm Totals</span><span class="daily-feed-total">Daily Feed Total</span><span class="per-acre-total">Total Feed/Acre</span></section>';
    farmTotalHtml += '<section class="' + classDays + '"><header></header>';
    farmTotalHtml += farmTotalDaysHtml;
    farmTotalHtml += '</section>';
    return farmTotalHtml;
}

function pondFeedsReportRepeater(numberOfDays, pondInfo, feedInfo, endDate) {
    // var numberOfDays = 7 is 5 or 7;
    var lastHarvestDate = pondInfo.LastHarvest.split(" ")[0] != "" ? pondInfo.LastHarvest.split(" ")[0] : '--', pondDaysHtml = '', pondFeedsHtml = '', statusClass, feedsTotal = 0, classDays = numberOfDays == 5 ? 'five-day' : 'seven-day';
    switch (pondInfo.HealthStatus) { case "1": statusClass = "good"; break; case "2": statusClass = "poor"; break; case "3": statusClass = "bad"; break; }


    for (var i = (numberOfDays - 1) ; i >= 0; i--) {
        var feedDate, feedID, poundsFed;
        if (feedInfo[i]) {
            feedDate = !feedInfo[i].FeedDate ? "--" : feedInfo[i].FeedDate.split(" ")[0];
            feedID = !feedInfo[i].FeedingId ? "" : feedInfo[i].FeedingId;
            poundsFed = !feedInfo[i].PoundsFed ? 0 : feedInfo[i].PoundsFed;
        } else {
            feedDate = "--";
            feedID = "";
            poundsFed = 0;
        }

        pondDaysHtml += '<section class="day"><header class="date">' + feedDate + '</header><span class="daily-feed">' + poundsFed + '</span></section>';
        feedsTotal += parseInt(poundsFed, 10);
    }

    pondFeedsHtml += '<section class="pond-name"><span class="name" data-status="' + statusClass + '">' + pondInfo.PondName + '</span></section>';
    pondFeedsHtml += '<section class="' + classDays + '"><header><span class="last-seven-days"><strong>Weekly Total Feed:</strong> ' + feedsTotal + '</span><span class="last-harvest"><strong>Weekly Average Feed:</strong> ' + lastHarvestDate + '</span></header>';
    pondFeedsHtml += pondDaysHtml;
    pondFeedsHtml += '<footer><span class="seven-day-avg"><strong>Last Seven Feeds Average:</strong> ' + (feedsTotal / numberOfDays).toFixed(2) + '</span><span class="sale-pounds"><strong>Sale Pounds</strong>: ' + feedList.salePounds + '</span><span class="avg-feed-per-acre"><strong>Average Feed/Acre:</strong> ' + ((feedsTotal / pondInfo.Size) / 7).toFixed(2) + '</span><span class="total-feed-per-acre"><strong>Total Feed/Acre:</strong> ' + (feedsTotal / pondInfo.Size).toFixed(2) + '</span></footer></section>';
    return pondFeedsHtml;
}
// END REPORT CODE

// BEGIN ENTRY CODE
// Repeaters: Farm totals
function farmRepeater(numberOfDays, farmID, pondList, feedList) {
    var now = new Date(), farmPondsHtml = '', classDays = numberOfDays == 5 ? 'five-day' : 'seven-day', searchQuery = { "key": _key, "FarmId": farmID, "currentTime": now }, data = JSON.stringify(searchQuery), headerFeedList = {};
    $.when($.ajax('../api/Farm/FarmFeedLast7FeedsTotals', {
        type: 'POST',
        data: data,
        success: function (msg) {
            startTimer(msg['Key']);
            headerFeedList = msg['ReturnData'];
        }
    })).then(function () {
        if (pondList.length < 1) {
            farmPondsHtml += '<section class="pond" ><p>There are no ponds available for this farm.</p></section>';
        } else {
            for (var i = 0; i < pondList.length; i++) {
                // check for suspended feed flag; add class to pond container if flag is on
                var noFeed = "", overlay = "";
                if (pondList[i].NoFeed != "False") {
                    noFeed = " feed-suspended";
                    overlay = '<div class="feed-suspended-overlay"><p>Do Not Feed</p></div>';
                }
                farmPondsHtml += '<section class="pond' + noFeed + '" id="pond' + pondList[i].PondId + '" data-pondid="' + pondList[i].PondId + '">' + overlay;
                farmPondsHtml += pondFeedsRepeater(numberOfDays, pondList[i], feedList[pondList[i].PondId]);
                farmPondsHtml += '</section>';
            }
        }

        farmTotalHtml = '<section ID="pondTotal" class="pond clear">' + farmTotalHeader(numberOfDays, headerFeedList) + '</section>';
        farmTotalHtml += '<section ID="farmPonds">' + farmPondsHtml + '</section>';
        $.when(
            $('#farmContainer').empty().html(farmTotalHtml)
        ).then(function () {
            bindButtons();
            hideProgress('farmRepeater');
        });
    });
}

function farmTotalHeader(numberOfDays, headerFeedList) {
    var farmTotalDaysHtml = '', classDays = numberOfDays == 5 ? 'five-day' : 'seven-day', totalFeed = 0, totalPerAcre = 0; 

    for (var i = (numberOfDays - 1) ; i >= 0; i--) {
        farmTotalDaysHtml += '<section class="day">';
        if (!headerFeedList[i]) {
            farmTotalDaysHtml += '<header class="date"> -- </header>';
            farmTotalDaysHtml += '<span class="daily-feed">0</span>';
            farmTotalDaysHtml += '<span class="per-acre">0</span>';
        } else {
            farmTotalDaysHtml += '<header class="date">' + headerFeedList[i].FeedDate + '</header>';
            farmTotalDaysHtml += '<span class="daily-feed">' + numberCommas(headerFeedList[i].TotalPoundsFed) + '</span>';
            farmTotalDaysHtml += '<span class="per-acre">' + headerFeedList[i].AveragePoundsFed + '</span>';
            totalFeed += parseInt(headerFeedList[i].TotalPoundsFed, 10);
            totalPerAcre += parseInt(headerFeedList[i].AveragePoundsFed, 10);
        }
        farmTotalDaysHtml += '</section>';
    }
    totalPerAcre = (totalPerAcre / numberOfDays).toFixed(2);
    
    var farmTotalHtml = '<section class="farm-name"><span class="name">Farm Totals</span><span class="daily-feed-total">Daily Feed Total</span><span class="per-acre-total">Total Feed/Acre</span></section>';
    farmTotalHtml += '<section class="' + classDays + '"><header></header>';
    farmTotalHtml += farmTotalDaysHtml;
    farmTotalHtml += '</section>';
    farmTotalHtml += ' <section class="summary"><header>7 Day Rolling Total</header><span class="daily-feed">' + numberCommas(totalFeed) + '</span><span class="per-acre">' + totalPerAcre + '</span></section>';
    return farmTotalHtml;
}

// Repeaters: Pond Feeds
function pondFeedsRepeater(numberOfDays, pondInfo, feedInfo){
    // var numberOfDays = 7 is 5 or 7;
    var lastHarvestDate = pondInfo.LastHarvest.split(" ")[0] != "" ? pondInfo.LastHarvest.split(" ")[0] : '--', pondDaysHtml = '', pondFeedsHtml = '', statusClass, feedsTotal = 0, classDays = numberOfDays == 5 ? 'five-day' : 'seven-day', disabled = false;
    var today = new Date(), yyyy = today.getFullYear(), dd = today.getDate(), mm = today.getMonth() + 1, hh = today.getHours(); //January is 0!
    today = mm + "/" + dd + "/" + yyyy;
    
    switch (pondInfo.HealthStatus) { case "1": statusClass = "good"; break; case "2": statusClass = "poor"; break; case "3": statusClass = "bad"; break; }

    for(var i=(numberOfDays-1); i >= 0; i--){
        var feedDate, feedID, poundsFed;
        if (feedInfo[i]) {
            feedDate = !feedInfo[i].FeedDate ? "--" : feedInfo[i].FeedDate.split(" ")[0];
            feedID = !feedInfo[i].FeedingId ? "" : feedInfo[i].FeedingId;
            poundsFed = !feedInfo[i].PoundsFed ? 0 : feedInfo[i].PoundsFed;
        } else {
            feedDate = "--";
            feedID = "";
            poundsFed = 0;
        }

        if (feedDate.toString() == today.toString()) disabled = true;

        pondDaysHtml += '<section class="day">';
        pondDaysHtml += '<header class="date">' + feedDate + '</header>';
        pondDaysHtml += '<span class="daily-feed">' + poundsFed + '</span>';

        if (feedID != "") {
            pondDaysHtml += '<button class="edit-feed" data-feedid="' + feedID + '" data-pondid="' + pondInfo.PondId + '">Edit</button>';
        }
        pondDaysHtml += '</section>';
        feedsTotal += parseInt(poundsFed, 10);
    }
    pondFeedsHtml += '<section class="pond-name"><span class="name" data-status="' + statusClass + '">' + pondInfo.PondName + '</span><button class="harvest-pond" data-pondid="' + pondInfo.PondId + '">Harvest Pond</button></section>';
    pondFeedsHtml += '<section class="' + classDays + '"><header><span class="last-seven-days"><strong>Last 7 Days Feeding:</strong> ' + feedsTotal + '</span><span class="last-harvest"><strong>Last Harvest Date:</strong> ' + lastHarvestDate + '</span></header>';
    pondFeedsHtml += pondDaysHtml;
    pondFeedsHtml += '<footer><span class="seven-day-avg"><strong>7 Day Feed Average:</strong> ' + (feedsTotal / numberOfDays).toFixed(2) + '</span><span class="sale-pounds"><strong>Sale Pounds</strong>: ' + pondInfo.SalesPoundsSinceHarvest + '</span><span class="avg-feed-per-acre"><strong>Average Feed/Acre:</strong> ' + ((feedsTotal / pondInfo.Size) / 7).toFixed(2) + '</span><span class="total-feed-per-acre"><strong>Total Feed/Acre:</strong> ' + (feedsTotal / pondInfo.Size).toFixed(2) + '</span></footer></section>';
    pondFeedsHtml += disabled ? '<section class="summary"><header>Fed Today:</header><form><input type="number" disabled class="feed-amount" placeholder="N/A" title="Fed Today" required /><button class="submit-feed" disabled title="Only One Feed Per Day Allowed" data-pondid="' + pondInfo.PondId + '">Submit</button></form></section>' : '<section class="summary"><header>Fed Today:</header><form><input type="number" class="feed-amount" placeholder="0" title="Fed Today" required /><button class="submit-feed" data-pondid="' + pondInfo.PondId + '">Submit</button></form></section>';
    return pondFeedsHtml;
}

function bindButtons(){
    $('.harvest-pond').unbind().click(function(e){
        e.preventDefault();
        var pondID = $(this).data('pondid'), now = new Date(), currentSection = $(this).parent().parent('.pond'), numberOfDays = $(this).parent().parent().hasClass('five-day') ? "5" : "7", pondName = $(this).prev().text();
        if (confirm("Please confirm that you wish to harvest pond " + pondName)) {
            showProgress(currentSection, 'submitCTHarvest');
            var searchQuery = { "key": _key, "PondId": pondID, "HarvestDate": now.toLocaleDateString() + " " + now.toLocaleTimeString().split(" ")[0] + " " + now.toLocaleTimeString().split(" ")[1] }, data = JSON.stringify(searchQuery);
            $.ajax('../api/Pond/HarvestPond', {
                type: 'PUT', data: data,
                success: function (msg) {
                    startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
                    reloadPond(pondID, currentSection, 'submitCTHarvest');
                }
            });
        }
    });

    $('.edit-feed').unbind().click(function (e) {
        e.preventDefault();
        var feedID = $(this).data('feedid'), pondID = $(this).data('pondid'), currentSection = $(this).parents('.pond'), numberOfDays = $(this).parent().parent().hasClass('five-day') ? "5" : "7";
        
        $('body').append('<div id="lightboxBG" class="modal"></div>');
        $('#lightboxBG').fadeIn('100', function () { centerModal('#editFeedModal'); $('#editFeedModal').fadeIn('100'); });
        
        var searchQuery = { "key": _key, "FeedingId": feedID }, data = JSON.stringify(searchQuery);
        $.ajax('../api/Pond/FeedById', {
            type: 'POST', data: data,
            success: function (msg) {
                startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
                feedInfo = msg['ReturnData'][0];
                feedDate = cleanDate(feedInfo.FeedDate);
                $('#feedDate').val(feedDate); $('#feedID').val(feedID); $('#poundsFed').val(feedInfo.PoundsFed); $('#pondID').val(pondID);
                bindEditButtons(numberOfDays, currentSection);
            }
        });
    });

    $('.submit-feed').unbind().click(function (e) {
        e.preventDefault();
        var pondID = $(this).data('pondid'), now = new Date(), currentSection = $(this).parent().parent().parent('.pond'), feedAmount = $(this).prev('.feed-amount').val(), pondInfo = {}, feedInfo = {}, numberOfDays = $(this).parent().parent().prev('section').hasClass('five-day') ? 5 : 7;
        showProgress(currentSection, 'submitCTEntry');

        var searchQuery = { "key": _key, "FeedDate": now.toLocaleDateString() + " " + now.toLocaleTimeString().split(" ")[0] + " " + now.toLocaleTimeString().split(" ")[1], "FeedingId": -1, "PondId": pondID, "PoundsFed": feedAmount }, data = JSON.stringify(searchQuery);
        $.ajax('../api/Pond/FeedAddOrEdit', {
            type: 'PUT', data: data,
            success: function (msg) {
                startTimer(msg['Key']); localStorage['CT_key'] = msg['Key']; 
                reloadPond(pondID, currentSection, 'submitCTEntry');
            }
        });
    });

    $('.cancel').unbind().click(function (e) { e.preventDefault(); resetFeedModal(); closeAdminModal(); }); 
}

function bindEditButtons(numberOfDays, currentSection) {
    $('#submitEditFeed').unbind().click(function (e) {
        e.preventDefault();
        var now = new Date(), pondID = $('#pondID').val(), feedDate = $('#feedDate').val(), feedID = $('#feedID').val(), feedAmount = $('#poundsFed').val(), pondInfo = {};
        showProgress('#editFeedModal', 'submitCTEdit');

        var searchQuery = { "key": _key, "FeedDate": feedDate, "FeedingId": feedID, "PoundsFed": feedAmount, "PondId": pondID }, data = JSON.stringify(searchQuery);
        $.ajax('../api/Pond/FeedAddOrEdit', {
            type: 'PUT', data: data,
            success: function (msg) {
                startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
                reloadPond(pondID, currentSection, 'submitCTEdit');
                resetFeedModal();
            }
        });
    });
}

function reloadPond(pondID, currentSection, progressID) {
    var nowTime = new Date(), farmID = $('#farmContainer').data('farmid'), searchQuery = { "key": _key, "CurrentTime": nowTime, "PondId": pondID }, data = JSON.stringify(searchQuery), totalSearchQuery = { "key": _key, "FarmId": farmID, "currentTime": nowTime }, dataTotal = JSON.stringify(totalSearchQuery), numberOfDays = $(this).parent().parent().hasClass('five-day') ? "5" : "7";
    $.when($.ajax('../api/Pond/PondFeedLast7Feeds', {
        type: 'POST', data: data,
        success: function (msg) {
            startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
            feedInfo = msg['ReturnData'];
        }
    }), $.ajax('../api/Pond/PondDetail', {
        type: 'POST', data: data,
        success: function (msg) {
            startTimer(msg['Key']); localStorage['CT_key'] = msg['Key'];
            pondInfo = msg['ReturnData'][0];
        }
    }), $.ajax('../api/Farm/FarmFeedLast7FeedsTotals', {
        type: 'POST',
        data: dataTotal,
        success: function (msg) {
            startTimer(msg['Key']);
            headerFeedList = msg['ReturnData'];
        }
    })).then(function () {
        currentSection.empty().html(pondFeedsRepeater(numberOfDays, pondInfo, feedInfo));
        $('#pondTotal').empty().html(farmTotalHeader(numberOfDays, headerFeedList))
        hideProgress(progressID);
        closeAdminModal();
        bindButtons();
    });
}

function resetFeedModal() { $('#feedDate').val(''); $('#feedID').val(''); $('#pondID').val(''); $('#poundsFed').val(''); }

function closeAdminModal() { $('.admin-modal, #lightboxBG').fadeOut('100', function () { $('#lightboxBG').remove(); $('.admin-modal').removeAttr('style'); }); }
// END ENTRY CODE