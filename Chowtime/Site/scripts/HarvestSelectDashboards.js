﻿if (!_key) { var _key = supports_html5_storage() ? localStorage['CT_key'] : readRemember('CT_key'); }

var userID = supports_html5_storage() ? localStorage['CTuserID'] : readRemember('CTuserID');
if (!data) { var data; }
$.support.cors = true;
$.ajaxSetup({ contentType: 'application/json; charset=utf-8', data: data,  statusCode: { 400: function (msg) { hideProgress(); alert("Oops... Something went wrong!"); }, 404: function (msg) { if (msg.responseJSON == "validation failed") { hideProgress(); alert("Validation failed or your session expired. Please login again."); window.location.href = "login.html"; } else { hideProgress(); alert("Oops - there was an error..."); /*window.location.href = "login.html";*/ } }, 500: function () { hideProgress(); alert("AJAX FAIL: 500 Internal Server Error"); } } });
// TODO: how to handle 500/AJAX fail errors?


$(function () {
    logoutControls();
    var currentPage = $(location).attr('href');
    if (currentPage.indexOf("login") > -1) { login(); } else { checkKey(); }
    if ($('.farm-yields').length > 0) { showProgress(); farmYields(); }
    if ($('.weigh-backs').length > 0) { showProgress(); weighBacks(); }
    if ($('.shift-end').length > 0) { showProgress(); shiftEnd(); }
    if ($('.live-sample').length > 0) { showProgress(); liveSample(); }
    if ($('.weekly-report').length > 0) { showProgress(); weeklyReport(); }
    if ($('.op-report').length > 0) { showProgress(); weeklyReport(); }
    if ($('.production').length > 0) { showProgress(); production(); }
});

/* Login page */
function login() {
    if (!rememberCheck()) {
        $('.login form #user').fadeIn(250);
        $('#user input').unbind().focus(function(){
            $('#user').removeClass('has-success, has-error');
            $('#user .help-block').hide();
        });
        $('#user input').unbind().focusout(function(){
            if($(this).val() != '') {
                $('#pass').css('opacity', 1);
                $('#user').addClass('has-success');
            } else {
                $('#user').addClass('has-error');
                $('#user .help-block').show();
            }
        });
        $('#pass input').unbind().focus(function(){
            $('#pass').removeClass('has-success, has-error');
            $('#pass .help-block').hide();
        });
        $('#pass input').unbind().focusout(function(){
            if($(this).val() != '') {
                $('#remember, #login').css('opacity', 1);
                $('#pass').addClass('has-success');
            } else {
                $('#pass').addClass('has-error');
                $('#pass .help-block').show();
            }
        });
    } else {
        $('#pass, #remember, #login').css('opacity', 1);
    }
	
	var passbox = $('#password'), userbox = $('#username');
    // LOGIN / KEY CHECK
    // Cookie is created to keep user/pass filled until browser is closed OR permanently if the checkbox is checked.
    // User Roles are added as classes to the BODY tag; elements are hidden appropriately through CSS.
    $('#login').unbind().click(function(e) {
        e.preventDefault();
        var username = $(userbox).val(), password = $(passbox).val(), dto = { "UserName": username, "Password": password }, data = JSON.stringify(dto);
		showProgress('body');
		$.ajax('../api/Login/ValidateLogin', {
			type: 'POST', data: data,
			statusCode: {
				200: function (msg) {
				    $('#username, #password').val(""); startTimer(msg.Key); localStorage['CT_key'] = msg.Key; var userRoles = "", userID = msg.UserID, companyID = msg.CompanyId; for (var i = 0; i < msg.UserRoles.length; i++) { userRoles += msg.UserRoles[i].RoleDescription + " "; } if (supports_html5_storage()) { localStorage['CTuser'] = username; localStorage['CTpass'] = password; localStorage['CTuserRole'] = userRoles; localStorage['CTuserID'] = userID; localStorage['CTcompanyID'] = companyID; if ($('#remember input').is(':checked')) { localStorage['CTremember'] = true } else { localStorage.removeItem('CTremember'); } } else { createRemember('CTuser', username); createRemember('CTpass', password); createRemember('CTuserRole', userRoles); createRemember('CTuserID', userID); createRemember('CT_key', _key); createRemember('CT_company', companyID); }
					window.location.href = "index.html";
				}, 404: function () {
					$.when(hideProgress()).then(function () { alert("Invalid login credentials: " + username + ":" + password + ". Please enter your information again."); });
				}, 405: function () {
				}, 500: function (msg) {
					hideProgress('login'); if (supports_html5_storage()) { localStorage['CTuser'] = username; localStorage['CTpass'] = password; } else { createRemember('CTuser', username); createRemember('CTpass', password); } alert("Server error. Please notify support. (" + msg.responseJSON.ExceptionMessage + ")"); 
				}
			}
		});
    });
}

//logout
function logoutControls() {
    $('#logout').unbind().click(function (e) {
        e.preventDefault();_key = "";
        if (supports_html5_storage()) {
            if (localStorage['CTremember'] == "false" || typeof localStorage['CTremember'] == 'undefined') {
                localStorage.removeItem('CTuser'); localStorage.removeItem('CTpass');
            }
            localStorage.removeItem('CTuserRole'); localStorage.removeItem('CTcompanyID'); localStorage.removeItem('CTuserID'); localStorage.removeItem('CT_key');
        } else {
            createRemember('CTuserRole', ""); createRemember('CTuserID', ""); createRemember('CT_key', _key);
        }
        window.location.href = "login.html";
    });
}

// COOKIES - SET AND READ; for temp or (if "remember me" is checked) permanent memory of login info)
// Only use cookies if browser doesn't support localStorage
function supports_html5_storage() { try { return 'localStorage' in window && window['localStorage'] !== null; } catch (e) { return false; } }

// Key check for security/validation
function checkKey() { if (!_key) { alert("Session key is undefined; please log in."); window.location.href = "login.html"; } }

var countdown;
function startTimer(newKeyValue) { if (countdown) clearTimeout(countdown); /* set in milliseconds */ var setTimer = 900000; _key = newKeyValue; countdown = setTimeout(function () { var pageName = location.pathname.substring(location.pathname.lastIndexOf("/") + 1); if (pageName != "login.html") { alert("Your session has timed out. Please log in again."); window.location.href = "login.html"; } }, setTimer) }

function refreshKey(username, password) { dto = { "UserName": username, "Password": password }, data = JSON.stringify(dto); $.ajax('../api/Login/ValidateLogin', { type: 'POST', data: data, statusCode: { 200: function (msg) { _key = msg.Key; localStorage['CT_key'] = msg.Key; var userRoles = "", userID = msg.UserID; for (var i = 0; i < msg.UserRoles.length; i++) { userRoles += msg.UserRoles[i].RoleDescription; } if (!supports_html5_storage()) { createRemember('CT_key', _key); } hideProgress(); startTimer(msg.Key);  } }, 404: function () { hideProgress(); alert('Your user credentials are not recognized. Please login again.'); window.location.href = "login.html"; }, 500: function (msg) { $.when(hideProgress()).then(function () { alert("Server error. Please notify support. (" + msg.responseJSON.ExceptionMessage + ")"); window.location.href = "login.html"; }); } }); }

// Cookie check (for 'remember me' box)
function rememberCheck() { var u = supports_html5_storage() ? localStorage['CTuser'] : readRemember('CTuser'), p = supports_html5_storage() ? localStorage['CTpass'] : readRemember('CTpass'); if (u && p) { $('#username').val(u); $('#password').val(p); $('#remember input').attr('checked', true); return true; } }

function createRemember(name, value) { var expires = ";expires=0"; if ($('#rememberMe').is(':checked')) { var date = new Date(); date.setTime(date.getTime() + (31 * 24 * 60 * 60 * 1000)); var expires = ";expires=" + date.toGMTString(); } document.cookie = name + "=" + value + expires; }

function readRemember(name) { var nameEQ = name + "=", ca = document.cookie.split(';'); for(var i=0;i < ca.length;i++) { var c = ca[i]; while (c.charAt(0)==' ') c = c.substring(1,c.length); if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length); } return null; }

// CONTROLS THE 'progress' BALL ANIMATION
function showProgress(targetElement, classIdentifier) { var bgElement = "#lightboxBG." + classIdentifier, layer = '<div class="windows8" id="progressBar"><div class="wBall" id="wBall_1"><div class="wInnerBall"></div></div><div class="wBall" id="wBall_2"><div class="wInnerBall"></div></div><div class="wBall" id="wBall_3"><div class="wInnerBall"></div></div><div class="wBall" id="wBall_4"><div class="wInnerBall"></div></div><div class="wBall" id="wBall_5"><div class="wInnerBall"></div></div></div><div class="modal ' + classIdentifier + '" id="lightboxBG"></div>'; $(targetElement).css('position', 'relative').prepend(layer); centerProgress(targetElement); $('#progressBar').fadeIn(50); $(bgElement).fadeIn(50); }

function hideProgress(classIdentifier) { var bgElement = "#lightboxBG." + classIdentifier; $(bgElement).fadeOut(100); $('#progressBar').fadeOut(100, function () { $('#progressBar').remove(); $(bgElement).remove(); }); }

function centerProgress(container) { if (container == 'body') { container = window }; var containerHeight = $(container).innerHeight(), containerWidth = $(container).innerWidth(), modalHeight = $('#progressBar').innerHeight(), modalWidth = $('#progressBar').innerWidth(); var modalTop = (containerHeight - modalHeight) / 2, modalLeft = (containerWidth - modalWidth) / 2; $('#progressBar').css({ 'top': modalTop, 'left': modalLeft }); }

function getTodaysMonth() {
    $('.row.buttons').css('opacity', 0);
    var today = new Date(), todaysMonth = today.getMonth() + 1, todaysYear = today.getFullYear();
    if (todaysMonth < 10) todaysMonth = "0" + todaysMonth;
    var tdate = new Object();
    tdate.month = todaysMonth;
    tdate.year = todaysYear;
    return tdate;
}

/* SHIFT END */
function shiftEnd() {
    var calFlag, date, addOrEdit, date, i, tdate = getTodaysMonth(), startDateMonth = tdate.month, startDateYear = tdate.year;
    $('#shiftDate').unbind().click(function () {
        console.log("clicked");
        if (calFlag != "created") loadCalendar(startDateMonth, startDateYear);
        //else  { $('#calendarModal').modal(); return; }
    });

    function loadCalendar() {
        calFlag = "created";
        //var searchQuery = { "Key": _key }, data = JSON.stringify(searchQuery), shiftEnds = [];

        $('#calendarModal .modal-body').fullCalendar({
            events: function (start, end, timezone, refetchEvents) {
                $.when(hideProgress()).then(function () {
                    showProgress('body');
                    var view = $('#calendarModal .modal-body').fullCalendar('getView');
                    var startDateYear1 = view.calendar.getDate()._d.getFullYear();
                    startDateYear = startDateYear1;
                    //if (view.start._d.getMonth() == 11) { startDateMonth = 1; startDateYear = view.start._d.getFullYear() + 1; } // looking at January
                    //else if (view.start._d.getMonth() == 10) startDateMonth = 12; // looking at December
                    // else 
                    var startDateMonth1 = view.calendar.getDate()._d.getMonth() + 1; // adding one for javascript month representation, 1 for view starting 10 days prior to viewed month
                    startDateMonth = startDateMonth1;

                    var results = [], searchQuery = { "Key": _key, "StartDateMonth": startDateMonth, "StartDateYear": startDateYear }, data = JSON.stringify(searchQuery);
                    $.when($.ajax('../api/ShiftEnd/ShiftEndList', {
                        type: 'POST',
                        data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key'];
                            startTimer(msg.Key);
                            shiftEndList = msg['ReturnData'];
                            if (shiftEndList.length > 0) {
                                for (var i = 0; i < shiftEndList.length; i++) {
                                    var shiftDate = shiftEndList[i].ShiftDate.split(" ")[0];
                                    results.push(shiftDate);
                                }
                            }
                        }
                    })).then(function () {
                        hideProgress();
                        $('#calendarModal').modal();
                        var events = [];
                        for (var event in results) {
                            var obj = {
                                title: "EDIT",
                                start: results[event],
                                end: results[event],
                                allDay: true
                            };
                            events.push(obj);
                        }
                        refetchEvents(events);
                    });
                });
            },
            eventClick: function (event) {
                date = event.start._i, searchQuery = { "Key": _key, "ShiftDate": date }, data = JSON.stringify(searchQuery);
                $.when($.ajax('../api/ShiftEnd/ShiftEndList', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        shiftEndData = msg['ReturnData'][0];
                        $('#rowContainer').empty();
                        $('.date-select h3').remove();
                        $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                        $('#regEmpLate').val(shiftEndData.RegEmpLate);
                        $('#regEmpOut').val(shiftEndData.RegEmpOut);
                        $('#regEmpLeftEarly').val(shiftEndData.RegEmplLeftEarly);
                        $('#tempEmpOut').val(shiftEndData.TempEmpOut);
                        $('#inmateEmpLeftEarly').val(shiftEndData.InmateLeftEarly);
                        $('#empVacation').val(shiftEndData.EmployeesOnVacation);
                        $('#inLateOut').val(shiftEndData.InLateOut);
                        $('#finKill').val(shiftEndData.FinishedKill);
                        $('#finFillet').val(shiftEndData.FinishedFillet);
                        $('#finSkinned').val(shiftEndData.FinishedSkinning);
                        $('#dayFreeze').val(shiftEndData.DayFinishedFreezing);
                        $('#nightFreeze').val(shiftEndData.NightFinishedFreezing);
                        $('#dayFroze').val(shiftEndData.DayShiftFroze);
                        $('#nightFroze').val(shiftEndData.NightShiftFroze);
                        $('#filletScale').val(shiftEndData.FilletScaleReading);
                        $('#downtimeMin').val(shiftEndData.DowntimeMinutes);
                        addOrEdit = shiftEndData.ShiftEndID;
                    }
                })).then(function () {
                    $('#calendarModal').modal('hide');
                    $('.row.fields, .row.buttons').css('opacity', 1);                   
                    $('.shiftForm').css('opacity', 1);
                });
            },
            dayClick: function () {
                $('#rowContainer').empty();
                date = $(this).data('date');
                $('.date-select h3').remove();
                $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                addOrEdit = "-1";
                $('input').val("");
                $('#calendarModal').modal('hide');
                $('.row.fields, .row.buttons').css('opacity', 1);
                $('.shiftForm').css('opacity', 1);
            }
        });
    }

    $('.buttons .reset').unbind().click(function (e) {
        e.preventDefault();
        if (window.confirm("This will permanently delete any information you have entered and not saved.")) {
            document.location.reload(true);
        }
    });

    $('.buttons .save').unbind().click(function (e) {
        e.preventDefault();

        var searchQuery = { "Key": _key, "userID": userID, "ShiftDate": date, "ShiftEndID": addOrEdit, "DayFinishedFreezing": $('#dayFreeze').val(), "DayShiftFroze": $('#dayFroze').val(), "FilletScaleReading": $('#filletScale').val(), "FinishedFillet": $('#finFillet').val(), "FinishedKill": $('#finKill').val(), "FinishedSkinning": $('#finSkinned').val(), "InmateLeftEarly": $('#inmateEmpLeftEarly').val(), "NightFinishedFreezing": $('#nightFreeze').val(), "NightShiftFroze": $('#nightFroze').val(), "RegEmpLate": $('#regEmpLate').val(), "RegEmpOut": $('#regEmpOut').val(), "InLateOut": $('#inLateOut').val(), "EmployeesOnVacation": $('#empVacation').val(), "RegEmplLeftEarly": $('#regEmpLeftEarly').val(), "TempEmpOut": $('#tempEmpOut').val(), "DowntimeMinutes": $('#downtimeMin').val() }, data = JSON.stringify(searchQuery);
        $.when($.ajax('../api/ShiftEnd/ShiftEndAddOrEdit', {
            type: 'PUT',
            data: data,
            success: function (msg) {
                localStorage['CT_key'] = msg['Key'];
                startTimer(msg.Key); 
                farmList = msg['ReturnData'];
                $('.date-select').append("<div>Information Saved!</div>");
            }
        })).then(function () { $('input').val(""); $('.row.fields, .row.buttons').css('opacity', 0); });
    });
}

/* LIVE FISH SAMPLING */
function liveSample() {
    var calFlag, date, addOrEdit, date, i, tdate = getTodaysMonth(), startDateMonth = tdate.month, startDateYear = tdate.year;
    $('#shiftDate').unbind().click(function () {
        if (calFlag != "created") loadCalendar(startDateMonth, startDateYear);
        //else { $('#calendarModal').modal(); return; }
    });

    function loadCalendar() {
        calFlag = "created";
        //var searchQuery = { "Key": _key }, data = JSON.stringify(searchQuery), samplingDates = [];

        $('#calendarModal .modal-body').fullCalendar({
            events: function (start, end, timezone, refetchEvents) {
                $.when(hideProgress()).then(function () {
                    showProgress('body');
                    var view = $('#calendarModal .modal-body').fullCalendar('getView');

                    stateDateYear = view.start._d.getFullYear();
                    if (view.start._d.getMonth() == 11) { startDateMonth = 1; startDateYear = view.start._d.getFullYear() - 1; } // looking at January
                    else if (view.start._d.getMonth() == 10) startDateMonth == 12; // looking at December
                    else startDateMonth = view.start._d.getMonth() + 2; // adding one for javascript month representation, 1 for view starting 10 days prior to viewed month

                    var results = [], searchQuery = { "Key": _key, "StartDateMonth": startDateMonth, "StartDateYear": startDateYear }, data = JSON.stringify(searchQuery);
                    $.when($.ajax('../api/LiveFishSampling/LiveFishSamplingList', {
                        type: 'POST',
                        data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key'];
                            startTimer(msg.Key);
                            sampleList = msg['ReturnData'];
                            for (var i = 0; i < sampleList.length; i++) {
                                var sampleDate = sampleList[i].SamplingDate.split(" ")[0];
                                results.push(sampleDate);
                            }
                        }
                    })).then(function () {
                        hideProgress();
                        $('#calendarModal').modal();
                        var events = [];
                        for (var event in results) {
                            var obj = {
                                title: "EDIT",
                                start: results[event],
                                end: results[event],
                                allDay: true
                            };
                            events.push(obj);
                        }
                        refetchEvents(events);
                    });
                });
            },
            eventClick: function (event) {
                date = event.start._i, searchQuery = { "Key": _key, "SamplingDate": date }, data = JSON.stringify(searchQuery);
                $.when($.ajax('../api/LiveFishSampling/LiveFishSamplingList', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        sampleData = msg['ReturnData'][0];
                        $('#rowContainer').empty();
                        $('.date-select h3').remove();
                        $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                        $('#Pct0_125').val(sampleData.Pct0_125);
                        $('#Avg0_125').val(sampleData.Avg0_125);
                        $('#Pct125_225').val(sampleData.Pct125_225);
                        $('#Avg125_225').val(sampleData.Avg125_225);
                        $('#Pct225_3').val(sampleData.Pct225_3);
                        $('#Avg225_3').val(sampleData.Avg225_3);
                        $('#Pct3_5').val(sampleData.Pct3_5);
                        $('#Avg3_5').val(sampleData.Avg3_5);
                        $('#Pct5_Up').val(sampleData.Pct5_Up);
                        $('#Avg5_Up').val(sampleData.Avg5_Up);
                        addOrEdit = sampleData.SamplingID;
                    }
                })).then(function () {
                    $('#calendarModal').modal('hide');
                    $('.row.fields, .row.buttons').css('opacity', 1);
                });
            },
            dayClick: function () {
                    $('#rowContainer').empty();
                    date = $(this).data('date');
                    $('.date-select h3').remove();
                    $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                    // TODO: add edit function, detected by existing data in calendar
                    // this assumes new/add:
                    searchQuery = { "Key": _key, "ShiftDate": date }, data = JSON.stringify(searchQuery), samplingDates = [];

                    $.ajax('../api/LiveFishSampling/LiveFishSamplingList', {
                        type: 'POST',
                        data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key'];
                            startTimer(msg.Key);
                            sampleList = msg['ReturnData'];
                            for (var i = 0; i < sampleList.length; i++) {
                                var sampleDate = sampleList[i].SamplingDate.split(" ")[0];
                                samplingDates.push(sampleDate);
                            }
                        }
                    });
                    addOrEdit = "-1";
                    $('#calendarModal').modal('hide');
                    $('.row.fields, .row.buttons').css('opacity',1);
                }
        });
    }

    $('.buttons .reset').unbind().click(function (e) {
        e.preventDefault();
        if (window.confirm("This will permanently delete any information you have entered and not saved.")) {
            document.location.reload(true);
        }
    });

    $('.buttons .save').unbind().click(function (e) {
        e.preventDefault();
        var searchQuery = { "Key": _key, "userID": userID, "SamplingDate": date, "SamplingID": addOrEdit, "Pct0_125": $('#Pct0_125').val(), "Avg0_125": $('#Avg0_125').val(), "Pct125_225": $('#Pct125_225').val(), "Avg125_225": $('#Avg125_225').val(), "Pct225_3": $('#Pct225_3').val(), "Avg225_3": $('#Avg225_3').val(), "Pct3_5": $('#Pct3_5').val(), "Avg3_5": $('#Avg3_5').val(), "Pct5_Up": $('#Pct5_Up').val(), "Avg5_Up": $('#Avg5_Up').val() }, data = JSON.stringify(searchQuery);
        $.when($.ajax('../api/LiveFishSampling/LiveFishSamplingAddOrEdit', {
            type: 'PUT',
            data: data,
            success: function (msg) {
                localStorage['CT_key'] = msg['Key'];
                startTimer(msg.Key);
                farmList = msg['ReturnData'];
                $('.date-select').append("<div>Information Saved!</div>");
            }
        })).then(function () { $('input').val(""); $('.row.fields, .row.buttons').css('opacity',1); });
    });
}

/* WEEKLY REPORTS */
function weeklyReport() {
    $('#mainContent').hide();
    var calFlag, date, i, tdate = getTodaysMonth(), startDateMonth = tdate.month, startDateYear = tdate.year;
    $('#shiftDate').unbind().click(function () {
        if (calFlag != "created") loadCalendar(startDateMonth, startDateYear);
        //else { $('#calendarModal').modal(); return; }
    });

    function loadCalendar() {
        $('#mainContent').hide();
        $('#calendarModal .modal-body').fullCalendar({
            dayClick: function () {
                showProgress('body');
                $('#rowContainer').empty();
                date = $(this).data('date');
                $('.date-select h3').remove();
                $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                searchQuery = { "Key": _key, "ReportDate": date }, data = JSON.stringify(searchQuery);

                $.ajax('../api/Reports/DailyReport', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        console.log(msg);
                        var $employeesHtml, $employeesData = msg.Employees[0], $finishedHtml = '<ul class="list-unstyled">', $finishedData = msg.Finish[0], $freezingsHtml = '<ul class="list-unstyled clearfix">', $freezingsData = msg.Freezing[0], $headerData = msg.Header[0], $pondsData = msg.Ponds, $pondsHtml = '<ul class="list-unstyled"><li class="row header"><span class="col-xs-3 date">Farm</span><span class="col-xs-3 pond">Pond</span><span class="col-xs-3 pounds">Pounds</span><span class="col-xs-3 yield">Yield</span></li>', $totalPounds = 0, $totalPct = 0;

                        /* EMPLOYEE DATA */
                        var regTotal = parseFloat($employeesData.RegEmpOut) + parseFloat($employeesData.RegEmpLate) + parseFloat($employeesData.RegEmplLeftEarly) + parseFloat($employeesData.EmployeesOnVacation);
                        var inmateTotal = parseFloat($employeesData.InLateOut) + parseFloat($employeesData.InmateLeftEarly);
                        var outTotal = parseFloat($employeesData.RegEmpOut) + parseFloat($employeesData.TempEmpOut) + parseFloat($employeesData.InLateOut);
                        var leftEarlyTotal = parseFloat($employeesData.RegEmplLeftEarly) + parseFloat($employeesData.InmateLeftEarly);
                        var allTotal = regTotal + inmateTotal + parseFloat($employeesData.TempEmpOut); //+outTotal + leftEarlyTotal  + parseFloat($employeesData.RegEmpLate) + parseFloat($employeesData.EmployeesOnVacation);
                        $employeesHtml = '<table><thead><tr><th></th><th>OUT</th><th>LATE</th><th>LEFT EARLY</th><th>VAC</th><th class="total">TOTAL</th></tr></thead>';
                        $employeesHtml += '<tbody><tr><th>REG</th><td class="tablenumbers">' + $employeesData.RegEmpOut + '</td><td class="tablenumbers">' + $employeesData.RegEmpLate + '</td><td class="tablenumbers">' + $employeesData.RegEmplLeftEarly + '</td><td class="tablenumbers">' + $employeesData.EmployeesOnVacation + '</td><td class="tablenumbers total">' + regTotal.toString() + '</td></tr>';
                        $employeesHtml += '<tr><th>TEMP</th><td class="tablenumbers">' + $employeesData.TempEmpOut + '</td><td class="tablenumbers">' + '-' + '</td><td class="tablenumbers">' + '-' + '</td><td class="tablenumbers">' + '-' + '</td><td class="tablenumbers total">' + $employeesData.TempEmpOut + '</td></tr>';
                        $employeesHtml += '<tr><th>INMATE</th><td class="tablenumbers">' + $employeesData.InLateOut + '</td><td class="tablenumbers">' + '-' + '</td><td class="tablenumbers">' + $employeesData.InmateLeftEarly + '</td><td class="tablenumbers">' + '-' + '</td><td class="tablenumbers total">' + inmateTotal.toString() + '</td></tr>';
                        $employeesHtml += '<tr><th class="total">TOTAL</th><td class="tablenumbers total">' + outTotal.toString() + '</td><td class="tablenumbers total">' + $employeesData.RegEmpLate + '</td><td class="tablenumbers total">' + leftEarlyTotal.toString() + '</td><td class="tablenumbers total">' + $employeesData.EmployeesOnVacation + '</td><td class="tablenumbers total">' + allTotal.toString() + '</td></tr><tbody></table>';
                        $('.reports .employees .report-container').append($employeesHtml);

                        /* FREEZINGS */
                        for (var key in $freezingsData) {
                            if ($freezingsData.hasOwnProperty(key)) {
                                function UpperCaseArray(input) {
                                    var result = input.replace(/([A-Z]+)/g, ",$1").replace(/^,/, "").replace(/,/g, ' ');
                                    return result;
                                }
                                $freezingsHtml += '<li class="col-xs-12"><div><strong>' + UpperCaseArray(key) + ':</strong><span>' + $freezingsData[key] + '</span></div></li>';
                            }
                        }
                        $freezingsHtml += "</ul>";
                        $('.reports .freezings .report-container').append($freezingsHtml);

                        
                         var finishcsv = "Date,Distance,Label\n2015-01-01 " + $finishedData.NightFinishedFreezing + ", 1, Night Finished Freezing\n2015-01-01 " + $finishedData.FinishedKill + ", 1, Finished Kill\n2015-01-01 " + $finishedData.FinishedSkinning + ", 1, Finished Skinning\n2015-01-01 " + $finishedData.FinishedFillet + ", 1, Finished Filet\n2015-01-01 " + $finishedData.DayFinishedFreezing + ", 1, Day Finished Freezing";
                        console.log(finishcsv);
                         $('#finishcontainer').highcharts({
                             data: {
                                 csv: finishcsv,
                                 seriesMapping: [{
                                     // x: 0, // X values are pulled from column 0 by default
                                     // y: 1, // Y values are pulled from column 1 by default
                                     label: 2 // Labels are pulled from column 2 and picked up in the dataLabels.format below
                                 }]
                             },
                             chart: {
                                 type: 'line'
                             },
                             title: {
                                 text: ''
                             },
                             xAxis: {
                                 minTickInterval: 36e5,
                                 format: '%H:%M',
                                 startOnTick: false,
                                 endOnTick: false,
                                 minPadding: 0.2,
                                 maxPadding: 0.2
                             },
                             yAxis: {
                                 title: {
                                     enabled: false
                                 },
                                 labels: {
                                     enabled: false
                                 }
                             },
                             legend: {
                                 enabled: false
                             },
                             plotOptions: {
                                 series: {
                                     dataLabels: {
                                         enabled: true,
                                         format: '{point.label}',
                                         rotation: -75
                                     },
                                     tooltip: {
                                         pointFormat: '{point.label}',
                                         headerFormat: '{point.key: %H:%M}<br>',
                                         
                                     }
                                 }
                             }


                        });
                        
                        /* PONDS / FARM YIELDS */
                        for (var pond in $pondsData) {
                            $pondsHtml += '<li class="row"><span class="col-xs-3 farm">' + $pondsData[pond].FarmID + '</span><span class="col-xs-3 pond">' + $pondsData[pond].PondName + '</span><span class="col-xs-3 pounds">' + $pondsData[pond].PoundsYielded + '</span><span class="col-xs-3 yield">' + $pondsData[pond].PercentYield + '</span></li>';
                            $totalPounds += parseFloat($pondsData[pond].PoundsYielded);
                            $totalPct += parseFloat($pondsData[pond].PercentYield);
                        }
                        var $avgYield = $totalPct / $pondsData.length;
                        var $whatToCallMe = $totalPounds * ($avgYield/100);
                        $pondsHtml += '<li class="totals"><span class="col-xs-2 col-xs-offset-3"><strong>Total Pounds:</strong><span>' + $totalPounds + '</span></span>';
                        $pondsHtml += '<span class="col-xs-2"><strong>Avg Yield:</strong><span>' + $avgYield.toFixed(4) + '</span></span>';
                        $pondsHtml += '<span class="col-xs-2"><strong>Sum*Avg Yield:</strong><span>' + Math.round($whatToCallMe) + '</span></span></li><ul>';
                        $('.farm-yields').append($pondsHtml).hide();
                        $('.farm-yields-report').unbind().click(function (e) {
                            e.preventDefault();
                            if($(this).hasClass('open')){
                                $('.farm-yields').slideUp(250);
                                $(this).removeClass('open');
                            } else {
                                $('.farm-yields').slideDown(250);
                                $(this).addClass('open');
                            }
                        });
                        hideProgress();
                        $('#mainContent').show();
                        $('#calendarModal').modal('hide');
                    }
                });
            }
        });
    }
}

/* PRODUCTION */
function production() {
    /* CALENDAR */
    var calFlag, date, i, tdate = getTodaysMonth(), startDateMonth = tdate.month, startDateYear = tdate.year, chosenDate;
    $('#shiftDate').unbind().click(function () {
        if (calFlag != "created") loadCalendar(startDateMonth, startDateYear);
        else $('#calendarModal').modal();
    });

    function loadCalendar() {
        calFlag = "created";
        $('#calendarModal .modal-body').fullCalendar({
            events:
                function (start, end, timezone, refetchEvents) {
                    $.when(hideProgress()).then(function () {
                        showProgress('body');
                        var view = $('#calendarModal .modal-body').fullCalendar('getView');

                        var startDateYear1 = view.calendar.getDate()._d.getFullYear();
                        startDateYear = startDateYear1;
                        var startDateMonth1 = view.calendar.getDate()._d.getMonth() + 1; // adding one for javascript month representation, 1 for view starting 10 days prior to viewed month
                        startDateMonth = startDateMonth1;

                        var results = [], searchQuery = { "Key": _key, "StartDateMonth": startDateMonth, "StartDateYear": startDateYear }, data = JSON.stringify(searchQuery);
                        $.when($.ajax('../api/Production/ProductionDates', { 
                            type: 'POST',
                            data: data,
                            success: function (msg) {
                                localStorage['CT_key'] = msg['Key'];
                                startTimer(msg.Key);
                                yieldList = msg['ReturnData'];
                                if (yieldList.length > 0) {
                                    var lastdate = yieldList[0].ProductionDate.split(" ")[0];
                                    for (var i = 0; i < yieldList.length; i++) {
                                        var shiftDate = yieldList[i].ProductionDate.split(" ")[0];
                                        if (i == 0) { results.push(shiftDate); }
                                        else if (shiftDate != lastdate) {
                                            results.push(shiftDate);
                                            lastdate = shiftDate;
                                        }
                                    };
                                }
                            }
                        })).then(function () {
                            hideProgress();
                            $('#calendarModal').modal();
                            var events = [];
                            for (var event in results) {
                                var obj = {
                                    title: "EDIT",
                                    start: results[event],
                                    end: results[event],
                                    allDay: true
                                };
                                events.push(obj);
                            }
                            refetchEvents(events);
                        });
                    });
                },
            dayClick: function () {
                showProgress();
                chosenDate = $(this).data('date');
                $.when(loadPondList(chosenDate)).then(function () {
                    $('#calendarModal').modal('hide');
                });
            },
            eventClick: function (calEvent) {
                chosenDate = calEvent.start._i;
                $.when(loadPondList(chosenDate)).then(function () {
                    $('#calendarModal').modal('hide');
                });
            }
        });
    }

    /* FROM FARM YIELDS: 
    function loadCalendar() {
        calFlag = "created";
        $('#calendarModal .modal-body').fullCalendar({
            events:
                function (start, end, timezone, refetchEvents) {
                    $.when(hideProgress()).then(function () {
                        showProgress('body');
                        var view = $('#calendarModal .modal-body').fullCalendar('getView');
                        
                        var startDateYear1 = view.calendar.getDate()._d.getFullYear();
                        startDateYear = startDateYear1;
                        var startDateMonth1 = view.calendar.getDate()._d.getMonth() + 1; // adding one for javascript month representation, 1 for view starting 10 days prior to viewed month
                        startDateMonth = startDateMonth1;
                        
                        var results = [], searchQuery = { "Key": _key, "StartDateMonth": startDateMonth, "StartDateYear": startDateYear }, data = JSON.stringify(searchQuery);
                        $.when($.ajax('../api/FarmYield/FarmYieldDates', {
                            type: 'POST',
                            data: data,
                            success: function (msg) {
                                localStorage['CT_key'] = msg['Key'];
                                startTimer(msg.Key);
                                yieldList = msg['ReturnData'];
                                if(yieldList.length>0) {
                                    var lastdate = yieldList[0].YieldDate.split(" ")[0];
                                    for (var i = 0; i < yieldList.length; i++) {
                                        var shiftDate = yieldList[i].YieldDate.split(" ")[0];
                                        if (i == 0) { results.push(shiftDate); }
                                        else if (shiftDate != lastdate) {
                                            results.push(shiftDate);
                                            lastdate = shiftDate;
                                        }
                                    };
                                }
                            }
                        })).then(function () { 
                            hideProgress();
                            $('#calendarModal').modal();
                            var events = [];
                            for (var event in results) {
                                var obj = {
                                    title: "EDIT",
                                    start: results[event],
                                    end: results[event],
                                    allDay: true
                                };
                                events.push(obj);
                            }
                            refetchEvents(events);
                        });
                    });
                },
            dayClick: function () {
                var chosenDate = $(this).data('date');
                $.when(loadEditFarmYields(chosenDate)).then(function () {
                    $('#calendarModal').modal('hide');
                });
                bindYieldButtons();
            },
            eventClick: function (calEvent) {
                var chosenDate = calEvent.start._i;
                $.when(loadEditFarmYields(chosenDate)).then(function () {
                    $('#calendarModal').modal('hide');
                });
                bindYieldButtons();
            }
        });
    }*/

    /* FROM WEIGHBACKS: 
    function loadCalendar() {
        calFlag = "created";
        $('#calendarModal .modal-body').fullCalendar({
            events:
                function (start, end, timezone, refetchEvents) {
                    $.when(hideProgress()).then(function () {
                        showProgress('body');
                        var view = $('#calendarModal .modal-body').fullCalendar('getView');

                        var startDateYear1 = view.calendar.getDate()._d.getFullYear();
                        startDateYear = startDateYear1;
                        //if (view.start._d.getMonth() == 11) { startDateMonth = 1; startDateYear = view.start._d.getFullYear() + 1; } // looking at January
                        //else if (view.start._d.getMonth() == 10) startDateMonth = 12; // looking at December
                        // else 
                        var startDateMonth1 = view.calendar.getDate()._d.getMonth() + 1; // adding one for javascript month representation, 1 for view starting 10 days prior to viewed month
                        startDateMonth = startDateMonth1;

                        var results = [], searchQuery = { "Key": _key, "StartDateMonth": startDateMonth, "StartDateYear": startDateYear }, data = JSON.stringify(searchQuery);
                        $.when($.ajax('../api/WeighBack/WeighBackDates', {
                            type: 'POST',
                            data: data,
                            success: function (msg) {
                                localStorage['CT_key'] = msg['Key'];
                                startTimer(msg.Key);
                                yieldList = msg['ReturnData'];
                                if (yieldList.length > 0) {
                                    var lastdate = yieldList[0].WBDateTime.split(" ")[0];
                                    for (var i = 0; i < yieldList.length; i++) {
                                        var shiftDate = yieldList[i].WBDateTime.split(" ")[0];
                                        if (i == 0) { results.push(shiftDate); }
                                        else if (shiftDate != lastdate) {
                                            results.push(shiftDate);
                                            lastdate = shiftDate;
                                        }
                                    };
                                }
                            }
                        })).then(function () {
                            hideProgress();
                            $('#calendarModal').modal();
                            var events = [];
                            for (var event in results) {
                                var obj = {
                                    title: "EDIT",
                                    start: results[event],
                                    end: results[event],
                                    allDay: true
                                };
                                events.push(obj);
                            }
                            refetchEvents(events);
                        });
                    });
                },
            dayClick: function () {
                var chosenDate = $(this).data('date');
                $.when(loadEditWeighBacks(chosenDate)).then(function () {
                    $('#calendarModal').modal('hide');
                });
                bindYieldButtons();
            },
            eventClick: function (calEvent) {
                var chosenDate = calEvent.start._i;
                $.when(loadEditWeighBacks(chosenDate)).then(function () {
                    $('#calendarModal').modal('hide');
                });
                bindYieldButtons();
            }
        });
    } */

    /* FROM PLANT WEIGHTS
    function loadCalendar() {
        calFlag = "created";

        $('#calendarModal .modal-body').fullCalendar({
            events: function (start, end, timezone, refetchEvents) {
                $.when(hideProgress()).then(function () {
                    showProgress('body');
                    var view = $('#calendarModal .modal-body').fullCalendar('getView');
                    var startDateYear1 = view.calendar.getDate()._d.getFullYear();
                    startDateYear = startDateYear1;
                    var startDateMonth1 = view.calendar.getDate()._d.getMonth() + 1; // adding one for javascript month representation, 1 for view starting 10 days prior to viewed month
                    startDateMonth = startDateMonth1;

                    var results = [], searchQuery = { "Key": _key, "StartDateMonth": startDateMonth, "StartDateYear": startDateYear }, data = JSON.stringify(searchQuery);
                    $.when($.ajax('../api/PlantPondWeight/PlantPondWeightList', {
                        type: 'POST',
                        data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key'];
                            startTimer(msg.Key);
                            plantPondWeightList = msg['ReturnData'];
                            if (plantPondWeightList.length > 0) {
                                for (var i = 0; i < plantPondWeightList.length; i++) {
                                    var plantPondWeightDate = plantPondWeightList[i].plantPondWeightDate.split(" ")[0];
                                    results.push(plantPondWeightDate);
                                }
                            }
                        }
                    })).then(function () {
                        hideProgress();
                        $('#calendarModal').modal();
                        var events = [];
                        for (var event in results) {
                            var obj = {
                                title: "EDIT",
                                start: results[event],
                                end: results[event],
                                allDay: true
                            };
                            events.push(obj);
                        }
                        refetchEvents(events);
                    });
                });
            },
            eventClick: function (event) {
                date = event.start._i, searchQuery = { "Key": _key, "plantPondWeightDate": date }, data = JSON.stringify(searchQuery);
                $.when($.ajax('../api/plantPondWeight/plantPondWeightList', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        plantPondWeightData = msg['ReturnData'];
                        console.log(plantPondWeightData);
                        //$('#rowContainer').empty();
                        //$('.date-select h3').remove();
                        //$('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                        //for (var i = 0; i < farmYieldData.length; i++) {
                        //    var savedOrNot = farmYieldData[i].YieldId == "-1" ? "save" : "check";

                        //    var newRowHtml = '<section class="row row' + farmYieldData[i].YieldId + ' data" data-rownum="' + farmYieldData[i].YieldId + '" data-yieldid="' + farmYieldData[i].YieldId + '" data-pondid="' + farmYieldData[i].PondID + '"><div class="col-xs-4"><label id="farms' + farmYieldData[i].YieldId + '" class="farmDDLLabel">' + farmYieldData[i].PondName + '</label></div><div class="col-xs-6"><input placeholder="(Pond Weight)" id="pounds' + farmYieldData[i].YieldId + '" class="pounds table-numbers" type="text" value="' + farmYieldData[i].PoundsYielded + '"><input placeholder="(Headed Weight)" id="headedpounds' + farmYieldData[i].YieldId + '" class="headedpounds table-numbers" type="text" value="' + farmYieldData[i].PoundsHeaded + '"><input placeholder="(% Yield1)" id="pctyield1_' + farmYieldData[i].YieldId + '" class="pctyield1 table-numbers" type="text" value="' + farmYieldData[i].PercentYield + '"><input placeholder="(% Yield2)" id="pctyield2_' + farmYieldData[i].YieldId + '" class="pctyield2 table-numbers" type="text" value="' + farmYieldData[i].PercentYield2 + '"></div><div class="col-xs-2"><a href="#" class="save-row"><img src="content/images/' + savedOrNot + '.png"></a></div></section>';

                        //    $.when($('#rowContainer').append(newRowHtml)).then(function () {
                        //        $('.yield-labels, .ponds, .pounds, .plantpounds, .headedpounds, .pctyield1, .pctyield2, .add-row, .delete-row').css('opacity', 1);
                        //    });
                        //}

                        //var addButton = '<section class="row row0 data" data-rownum="0" data-yieldid="-1"><div class="col-xs-2"><a href="#" class="add-new-row"><img src="content/images/plus.png"></a></div></section>';
                        //$('#rowContainer').append(addButton)
                        //bindYieldButtons();
                    }
                })).then(function () {
                    $('#calendarModal').modal('hide');
                    $('.row.fields, .row.buttons').css('opacity', 1);
                    $('.plantWeightForm').css('opacity', 1);
                });
            },
            dayClick: function () {
                $('#rowContainer').empty();
                date = $(this).data('date');
                $('.date-select h3').remove();
                $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                addOrEdit = "-1";
                $('input').val("");
                $('#calendarModal').modal('hide');
                $('.row.fields, .row.buttons').css('opacity', 1);
                $('.plantWeightForm').css('opacity', 1);
            }
        });
    } */

    function loadPondList(date) {
        $('#pondListContainer').empty();
        $('.date-select h3, .date-select div').remove();

        var searchQuery = { "Key": _key, "ProductionDate": date }, data = JSON.stringify(searchQuery);
        $.ajax('../api/Production/ProductionTotals', { 
            type: 'POST',
            data: data,
            success: function (msg) {
                localStorage['CT_key'] = msg['Key'];
                startTimer(msg.Key);
                pondData = msg['ReturnData'];
                $('#pondListContainer').empty();
                $('.date-select h3').remove();
                $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                for (var i = 0; i < pondData.length; i++) {
                    // add test to determine which buttons are green and red
                    var $pondBtn = pondData[i].PondWeight != "---" ? "btn-success" : "btn-danger", $plantBtn = pondData[i].PlantWeight != "---" ? "btn-success" : "btn-danger", $backsBtn = pondData[i].WeighBacks != "---" ? "btn-success" : "btn-danger", $yieldsBtn = pondData[i].AverageYield != "---" ? "btn-success" : "btn-danger";
                    var newRowHtml = '<section id="pond' + pondData[i].PondID + '" class="row pond-container"><section class="pond-buttons"><section class="col-md-2"><button class="btn btn-label">' + pondData[i].PondName + '</button></section><section class="col-md-2"><button id="pond' + pondData[i].PondID + '_pondWeight" data-column="pond-weight" class="btn ' + $pondBtn + '">' + pondData[i].PondWeight + '</button></section><section class="col-md-2"><button id="pond' + pondData[i].PondID + '_plantWeight" class="btn ' + $plantBtn + '" data-column="plant-weight">' + pondData[i].PlantWeight + '</button></section><section class="col-md-2"><button id="pond' + pondData[i].PondID + '_weighbacks" class="btn ' + $backsBtn + '" data-column="weighbacks">' + pondData[i].WeighBacks + '</button></section><section class="col-md-2"><button id="pond' + pondData[i].PondID + '_yield" class="btn ' + $yieldsBtn + '" data-column="yield">' + pondData[i].AverageYield + '</button></section><section class="col-md-2"><button id="pond' + pondData[i].PondID + '_headedWeight" class="btn btn-label">' + pondData[i].HeadedWeight + '</button></section></section><section class="form-container col-md-12"></section></section>';

                    $.when($('#pondListContainer').append(newRowHtml)).then(function () {
                        bindPondButtons();
                        $('.pondlist').show();
                        hideProgress();
                    });
                }
            }
        });
    }

    bindPondButtons();
    function bindPondButtons() {
        $('.pond-buttons button').unbind().click(function () {
            // RUN ERROR CHECK TO MAKE SURE THEY'RE NOT FORGETTING TO SUBMIT INFORMATION
            var $status = $(this).attr('class').split("-")[1], $id = $(this).attr('id').split("d")[1].split('_')[0], $activeButton = $(this), $activeRow = $activeButton.data('column'), $formContainer = $activeButton.parent().parent().next('.form-container');
            if ($activeRow != undefined) {
                showProgress('body');
                $('.open-tab').removeClass('open-tab');
                if ($('.form-container.active').length > 0) {
                    $('.form-container.active').slideUp(500);
                }
                switch ($activeRow) {
                    case 'pond-weight': loadEditPondWeights($id, $status); break;
                    case 'plant-weight': loadEditPlantWeights($id, $status); break;
                    case 'weighbacks': loadEditWeighBacks($id, $status); break;
                    case 'yield': loadEditFarmYields($id, $status); break;
                }
                // AJAX call to get appropriate information & populate fields goes here - $.when().then(function(){});
                $activeButton.parent().addClass('open-tab').parent().next('.form-container').addClass('active').slideDown(500);
                // BIND NEW ENTRY BUTTON?
            }
        });
    }

    function loadEditPondWeights($id, $status) {
        if ($status == "danger") {
            var formHtml = '<section id="pond-weight-' + $id + '" class="row form-inline" data-row="pond-weight"><header class="col-md-12">Pond Weights</header><section class="col-md-2"><p>(time will be stamped)</p></section><section class="col-md-10"><label>Pond Weight:</label><input type="text" id="pondweightID" class="form-control" value=""><button class="btn btn-default">Edit</button></section><section class="row buttons"><button id="addNewPondWeight" class="btn btn-default">Add New Pond Weight</button></section></section>';
        } else {
            var formHtml = '<section id="pond-weight-' + $id + '" class="row form-inline" data-row="pond-weight"><header class="col-md-12">Pond Weights</header><section class="col-md-2"><p>(time will be stamped)</p></section><section class="col-md-10"><label>Pond Weight:</label><input type="text" id="pondweightID" class="form-control" value="pondWeightValue"><button class="btn btn-default">Edit</button></section><section class="row buttons"><button id="addNewPondWeight" class="btn btn-default">Add New Pond Weight</button></section></section>';
        }
        $.when($('.form-container').append(formHtml)).then(function () { hideProgress(); });
    }

    function loadEditPlantWeights($id, $status) {
        if ($status == "danger") {
            var formHtml = '<section id="pond-weight-' + $id + '" class="row form-inline" data-row="pond-weight"><header class="col-md-12">Pond Weights</header><section class="col-md-2"><p></p></section><section class="col-md-10"><label>Pond Weight:</label><input type="text" id="pondweightID" class="form-control" value=""><button class="btn btn-default">Edit</button></section><section class="row buttons"><button id="addNewPondWeight" class="btn btn-default">Add New Pond Weight</button></section></section>';
        } else {
            var formHtml = '<section id="pond-weight-' + $id + '" class="row form-inline" data-row="pond-weight"><header class="col-md-12">Pond Weights</header><section class="col-md-2"><p></p></section><section class="col-md-10"><label>Pond Weight:</label><input type="text" id="pondweightID" class="form-control" value="pondWeightValue"><button class="btn btn-default">Edit</button></section><section class="row buttons"><button id="addNewPondWeight" class="btn btn-default">Add New Pond Weight</button></section></section>';
        }
        $.when($('.form-container.active').empty().append(formHtml)).then(function () { hideProgress(); });
    }

    function loadEditFarmYields($id, $status) {
        $('#rowContainer').empty();
        $('.date-select h3, .date-select div').remove();
        $('#plantpounds').val();
        $('#plantPoundsID').val("-1");
        $('#weighbacks').val();
        var searchQuery = { "Key": _key, "YieldDate": chosenDate }, data = JSON.stringify(searchQuery);
        $.ajax('../api/FarmYieldHeader/FarmYieldHeaderList', {
            type: 'POST',
            data: data,
            success: function (msg) {
                localStorage['CT_key'] = msg['Key'];
                startTimer(msg.Key);
                plantPoundsData = msg['ReturnData'];
                var plantWeight = typeof plantPoundsData[0] !== "undefined" ? plantPoundsData[0].PlantWeight : 0;
                var weighBacks = typeof plantPoundsData[0] !== "undefined" ? plantPoundsData[0].WeighBacks : 0;
                var farmYieldHeaderID = typeof plantPoundsData[0] !== "undefined" ? plantPoundsData[0].FarmYieldHeaderID : -1;
                $('#plantpounds').val(plantWeight);
                $('#plantPoundsID').val(farmYieldHeaderID);
                $('#weighbacks').val(weighBacks);
            }
        });
        $.ajax('../api/FarmYield/FarmYieldsFromSamplings', {
            type: 'POST',
            data: data,
            success: function (msg) {
                localStorage['CT_key'] = msg['Key'];
                startTimer(msg.Key);
                farmYieldData = msg['ReturnData'];
                $('#rowContainer').empty();
                $('.date-select h3').remove();
                $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                for (var i = 0; i < farmYieldData.length; i++) {
                    var savedOrNot = farmYieldData[i].YieldId == "-1" ? "save" : "check";

                    var newRowHtml = '<section class="row row' + farmYieldData[i].YieldId + ' data" data-rownum="' + farmYieldData[i].YieldId + '" data-yieldid="' + farmYieldData[i].YieldId + '" data-pondid="' + farmYieldData[i].PondID + '"><div class="col-xs-4"><label id="farms' + farmYieldData[i].YieldId + '" class="farmDDLLabel">' + farmYieldData[i].PondName + '</label></div><div class="col-xs-6"><input placeholder="(Pond Weight)" id="pounds' + farmYieldData[i].YieldId + '" class="pounds table-numbers" type="text" value="' + farmYieldData[i].PoundsYielded + '"><input placeholder="(Headed Weight)" id="headedpounds' + farmYieldData[i].YieldId + '" class="headedpounds table-numbers" type="text" value="' + farmYieldData[i].PoundsHeaded + '"><input placeholder="(% Yield1)" id="pctyield1_' + farmYieldData[i].YieldId + '" class="pctyield1 table-numbers" type="text" value="' + farmYieldData[i].PercentYield + '"><input placeholder="(% Yield2)" id="pctyield2_' + farmYieldData[i].YieldId + '" class="pctyield2 table-numbers" type="text" value="' + farmYieldData[i].PercentYield2 + '"></div><div class="col-xs-2"><a href="#" class="save-row"><img src="content/images/' + savedOrNot + '.png"></a></div></section>';

                    $.when($('#rowContainer').append(newRowHtml)).then(function () {
                        $('.yield-labels, .ponds, .pounds, .plantpounds, .headedpounds, .pctyield1, .pctyield2, .add-row, .delete-row').css('opacity', 1);
                    });
                }

                var addButton = '<section class="row row0 data" data-rownum="0" data-yieldid="-1"><div class="col-xs-2"><a href="#" class="add-new-row"><img src="content/images/plus.png"></a></div></section>';
                $('#rowContainer').append(addButton)
                bindYieldButtons();
            }
        });

        function bindYieldButtons() {
            $('.farmDDL').unbind().change(function () {
                var rowID = $(this).attr('id').replace('farms', ''), farmID = $(this).val();
                loadPondsDDL(rowID, farmID);
            });

            $('.pondsDDL').unbind().change(function () {
                $(this).parent().next().find('input').css('opacity', 1);
            });

            $('.pounds').unbind().focusout(function () {
                if (!$(this).val() == "") {
                    $(this).parent().parent().find('.add-row').css('opacity', 1);
                }
            });
            $('.pounds').unbind().change(function () {
                if (!$(this).val() == "") {
                    $(this).parent().parent().find('.save-row').children().attr('src', 'content/images/save.png');
                    $(this).parent().parent().find('.add-row').css('opacity', 1);
                }
            });

            $('#plantLbsSave').unbind().click(function (e) {
                showProgress('body');
                e.preventDefault();
                var date = $('.date-select h3 strong').text(), weighBacks = $('#weighbacks').val(), plantPounds = $('#plantpounds').val(), plantPoundsID = $('#plantPoundsID').val(), searchQuery = { "Key": _key, "YieldDate": date, "PlantWeight": plantPounds, "FarmYieldHeaderID": plantPoundsID, "WeighBacks": weighBacks }, data = JSON.stringify(searchQuery);
                $.ajax('../api/FarmYieldHeader/FarmYieldHeaderAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        hideProgress();
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        $('.date-select').append("<div>Plant Weight Saved.</div>");
                    }
                })
            });

            $('.data .add-row').unbind().click(function (e) {
                e.preventDefault();
                var remove = "0", date = $('.date-select h3 strong').text(), yieldID = $(this).parent().parent().data('yieldid'), pondID = $(this).parent().parent().find('.pondsDDL').val(), pondYield = $(this).parent().parent().find('.pounds').val(), headPounds = $(this).parent().parent().find('.headedpounds').val(), pctYield = $(this).parent().parent().find('.pctyield1').val(), pctYield2 = $(this).parent().parent().find('.pctyield2').val(), searchQuery = { "Key": _key, "YieldDate": date, "YieldID": yieldID, "PondID": pondID, "PoundsYielded": pondYield, "PercentYield": pctYield, "PercentYield2": pctYield2, "PoundsHeaded": headPounds, "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().find('.add-row').css('opacity', 0);
                $(this).parent().parent().find('.delete-row').css('opacity', 1);
                i = parseInt($(this).parent().parent().attr('data-rownum')) + 1;
                var justadded = ".row" + (i - 1);
                $.when($.ajax('../api/FarmYield/FarmYieldAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        yieldID = msg['YieldID'];
                        $(this).parent().parent().addClass('complete');
                        $(justadded).attr('data-yieldid', yieldID);
                        if (yieldID != -1) { loadEditFarmYields(date); }
                        // ?? What's this? Or - is it necessary?
                        addOrEdit = yieldID;
                    }
                })).then(function () {
                    if (yieldID == -1) {
                        newRowHtml = '<section class="row row' + i + ' data" data-rownum="' + i + '" data-yieldid="-1"><div class="col-xs-2"><select id="farms' + i + '" class="farmDDL"></select></div><div class="col-xs-2"><select id="ponds' + i + '" class="pondsDDL"><option>(Pond)</option></select></div><div class="col-xs-6"><input placeholder="(Pond Weight)" id="pounds' + i + '" class="pounds table-numbers" type="text"><input placeholder="(Headed Weight)" id="headedpounds' + i + '" class="headedpounds table-numbers" type="text"><input placeholder="(% Yield1)" id="pctyield1_' + i + '" class="pctyield table-numbers" type="text"><input placeholder="(% Yield2)" id="pctyield2_' + i + '" class="pctyield2 table-numbers" type="text"></div><div class="col-xs-2"><a href="#" class="add-row"><img src="content/images/plus.png"></a><a href="#" class="delete-row"><img src="content/images/close.png"></a></div></section>';

                        $.when($('#rowContainer').append(newRowHtml)).then(function () { loadFarmsDDL(i); });
                        bindYieldButtons();
                    }
                });

            });
            $('.data .add-new-row').unbind().click(function (e) {
                e.preventDefault();
                var newRowHtml = '<section class="row row0 data" data-rownum="0" data-yieldid="-1"><div class="col-xs-2"><select id="farms0" class="farmDDL"></select></div><div class="col-xs-2"><select id="ponds0" class="pondsDDL"><option>(Pond)</option></select></div><div class="col-xs-6"><input placeholder="(Pond Weight)" id="pounds0" class="pounds table-numbers" type="text"><input placeholder="(Headed Weight)" id="headedpounds0" class="headedpounds table-numbers" type="text"><input placeholder="(% Yield1)" id="pctyield1_0" class="pctyield1 table-numbers" type="text"><input placeholder="(% Yield2)" id="pctyield2_0" class="pctyield2 table-numbers" type="text"></div><div class="col-xs-2"><a href="#" class="add-row"><img src="content/images/plus.png"></a><a href="#" class="delete-row"><img src="content/images/close.png"></a></div></section>';

                $.when($('#rowContainer').append(newRowHtml)).then(function () {
                    loadFarmsDDL(0);
                    $('.row.buttons').show();
                });
                i = 1;
                bindYieldButtons();

            });
            $('.data .save-row').unbind().click(function (e) {
                e.preventDefault();
                var remove = "0", date = $('.date-select h3 strong').text(), yieldID = $(this).parent().parent().data('yieldid'), pondID = $(this).parent().parent().data('pondid'), pondYield = $(this).parent().parent().find('.pounds').val(), headPounds = $(this).parent().parent().find('.headedpounds').val(), pctYield = $(this).parent().parent().find('.pctyield1').val(), pctYield2 = $(this).parent().parent().find('.pctyield2').val(), searchQuery = { "Key": _key, "YieldDate": date, "YieldID": yieldID, "PondID": pondID, "PoundsYielded": pondYield, "PercentYield": pctYield, "PercentYield2": pctYield2, "PoundsHeaded": headPounds, "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().find('.save-row').children().attr('src', 'content/images/check.png');
                $.when($.ajax('../api/FarmYield/FarmYieldAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                    }
                })).then(function () {
                    if (yieldID == -1) {
                    }
                });

            });

            $('.data .delete-row').unbind().click(function (e) {
                e.preventDefault();
                // TO DO: prevent removing sole empty row or replace with empty row
                var remove = "1", searchQuery = { "Key": _key, "YieldID": $(this).parent().parent().attr('data-yieldid'), "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().remove();
                $.when($.ajax('../api/FarmYield/FarmYieldAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        yieldList = msg['ReturnData'];
                    }
                })).then(function () {
                    if (!$('.data').length > 0) {
                        var newRowHtml = '<section class="row row0 data" data-rownum="0" data-yieldid="-1"><div class="col-xs-2"><select id="farms0" class="farmDDL"></select></div><div class="col-xs-2"><select id="ponds0" class="pondsDDL"><option>(Pond)</option></select></div><div class="col-xs-6"><input placeholder="(Pond Weight)" id="pounds0" class="pounds table-numbers" type="text"><input placeholder="(Headed Weight)" id="headedpounds0" class="headedpounds table-numbers" type="text"><input placeholder="(% Yield1)" id="pctyield!_0" class="pctyield table-numbers" type="text"><input placeholder="(% Yield2)" id="pctyield2_0" class="pctyield2 table-numbers" type="text"></div><div class="col-xs-2"><a href="#" class="add-row"><img src="content/images/plus.png"></a><a href="#" class="delete-row"><img src="content/images/close.png"></a></div></section>';

                        $.when($('#rowContainer').append(newRowHtml)).then(function () { loadFarmsDDL(0); });
                        i = 1;
                        bindYieldButtons();
                    }
                });
            })
        }
    }

    function loadEditWeighBacks($id, $status) {
        $('#plantpounds').val();
        $('#plantPoundsID').val("-1");
        $('#weighbacks').val();
        var searchQuery = { "Key": _key, "WBDateTime": chosenDate }, data = JSON.stringify(searchQuery);

        $.ajax('../api/WeighBack/WeighBacksFromSamplings', {
            type: 'POST',
            data: data,
            success: function (msg) {
                localStorage['CT_key'] = msg['Key'];
                startTimer(msg.Key);
                weighBackData = msg['ReturnData'];
                $('#rowContainer').empty();
                $('.date-select h3').remove();
                $('.date-select').append("<h3><strong>" + date + "</strong></h3>");
                for (var i = 0; i < weighBackData.length; i++) {
                    var savedOrNot = weighBackData[i].WeightBackID == "-1" ? "save" : "check";

                    var newRowHtml = '<section class="row row' + weighBackData[i].WeightBackID + ' data" data-rownum="' + weighBackData[i].WeightBackID + '" data-weighbackid="' + weighBackData[i].WeightBackID + '" data-pondid="' + weighBackData[i].PondID + '"><div class="col-xs-4"><label id="farms' + weighBackData[i].WeightBackID + '" class="farmDDLLabel">' + weighBackData[i].PondName + '</label></div><form class="col-xs-6 input-labels form-inline"><fieldset class="form-group"><label>Turtle</label><input placeholder="Turtles" id="turtles' + weighBackData[i].WeightBackID + '" class="turtles table-numbers" value="' + weighBackData[i].Turtle + '" type="text"></fieldset><fieldset class="form-group"><label>Trash</label><input placeholder="Trash" id="trash' + weighBackData[i].WeightBackID + '" class="trash table-numbers" value="' + weighBackData[i].Trash + '" type="text"></fieldset><fieldset class="form-group"><label>Shad</label><input placeholder="Shad" id="shad' + weighBackData[i].WeightBackID + '" class="shad table-numbers" value="' + weighBackData[i].Shad + '" type="text"></fieldset><fieldset class="form-group"><label>Carp</label><input placeholder="Carp" id="carp' + weighBackData[i].WeightBackID + '" class="carp table-numbers" value="' + weighBackData[i].Carp + '" type="text"></fieldset><fieldset class="form-group"><label>Bream</label><input placeholder="Bream" id="bream' + weighBackData[i].WeightBackID + '" class="bream table-numbers" value="' + weighBackData[i].Bream + '" type="text"></fieldset><fieldset class="form-group"><label>Live Disease</label><input placeholder="Live Disease" id="livedisease' + weighBackData[i].WeightBackID + '" class="livedisease table-numbers" value="' + weighBackData[i].LiveDisease + '" type="text"></fieldset><fieldset class="form-group"><label>Dressed Disease</label><input placeholder="Dressed Disease" id="dresseddisease' + weighBackData[i].WeightBackID + '" class="dresseddisease table-numbers" value="' + weighBackData[i].DressedDisease + '" type="text"></fieldset><fieldset class="form-group"><label>~~Backs</label><input placeholder="~~Backs" id="backs' + weighBackData[i].WeightBackID + '" class="backs table-numbers" value="' + weighBackData[i].Backs + '" type="text"></fieldset><fieldset class="form-group"><label>Red Fillet</label><input placeholder="Red Fillet" id="redfillet' + weighBackData[i].WeightBackID + '" class="redfillet table-numbers" value="' + weighBackData[i].RedFillet + '" type="text"></fieldset><fieldset class="form-group"><label>Big Fish</label><input placeholder="Big Fish" id="bigfish' + weighBackData[i].WeightBackID + '" class="bigfish table-numbers" value="' + weighBackData[i].BigFish + '" type="text"></fieldset><fieldset class="form-group"><label>DOAs</label><input placeholder="DOAs" id="doas' + weighBackData[i].WeightBackID + '" class="doas table-numbers" value="' + weighBackData[i].DOAs + '" type="text"></fieldset></form><div class="col-xs-2"><fieldset class="form-group"><label>Red Fillet @ 36% Yield</label><input placeholder="" id="redfilletpct' + weighBackData[i].WeightBackID + '" class="redfilletpct table-numbers" disabled></fieldset><fieldset class="form-group"><label>Dressed Disease @ 60% Yield</label><input placeholder="" id="dresseddiseasepct' + weighBackData[i].WeightBackID + '" class="dresseddiseasepct table-numbers" disabled></fieldset><a href="#" class="save-row"><img src="content/images/' + savedOrNot + '.png"></a></div></section></div></section>';

                    $.when($('#rowContainer').append(newRowHtml)).then(function () {
                        if (weighBackData[i].RedFillet != "") { $('#redfilletpct' + weighBackData[i].WeightBackID).val(weighBackData[i].RedFillet / .36); }
                        if (weighBackData[i].DressedDisease != "") { $('#dresseddiseasepct' + weighBackData[i].WeightBackID).val(weighBackData[i].DressedDisease / .6); }
                    });
                }

                var addButton = '<section class="row row0 data" data-rownum="0" data-weighbackid="-1"><div class="col-xs-2"><a href="#" class="add-new-row"><img src="content/images/plus.png"></a></div></section>';
                $('#rowContainer').append(addButton);
                bindYieldButtons();
            }
        });

        function bindYieldButtons() {
            $('.farmDDL').unbind().change(function () {
                var rowID = $(this).attr('id').replace('farms', ''), farmID = $(this).val();
                loadPondsDDL('0', farmID)
            });

            $('.pondsDDL').unbind().change(function () {
                $('#newValues').css('opacity', 1);
                $('#newButtons').css('opacity', 1);
            });

            $('.dresseddisease').unbind().blur(function () {
                var redfilletid = $(this).attr('id').replace('dresseddisease', ''), actual = $(this).val(), pct = actual / .6;
                $('#dresseddiseasepct' + redfilletid).val(pct);
            });

            $('.redfillet').unbind().blur(function () {
                var redfilletid = $(this).attr('id').replace('redfillet', ''), actual = $(this).val(), pct = actual / .36;
                $('#redfilletpct' + redfilletid).val(pct);
            });

            $('.data .add-row').unbind().click(function (e) {
                showProgress('body');
                e.preventDefault();
                var remove = "0", date = $('.date-select h3 strong').text(), WeightBackID = -1, pondID = $(this).parent().parent().find('#ponds0').val(), Backs = $(this).parent().parent().find('.backs').val(), BigFish = $(this).parent().parent().find('.bigfish').val(), Bream = $(this).parent().parent().find('.bream').val(), Carp = $(this).parent().parent().find('.carp').val(), DOAs = $(this).parent().parent().find('.doas').val(), DressedDisease = $(this).parent().parent().find('.dresseddisease').val(), LiveDisease = $(this).parent().parent().find('.livedisease').val(), RedFillet = $(this).parent().parent().find('.redfillet').val(), Shad = $(this).parent().parent().find('.shad').val(), Trash = $(this).parent().parent().find('.trash').val(), Turtles = $(this).parent().parent().find('.turtles').val(), searchQuery = { "Key": _key, "WBDateTime": date, "WeightBackID": WeightBackID, "PondID": pondID, "Backs": Backs, "BigFish": BigFish, "Bream": Bream, "Carp": Carp, "DOAs": DOAs, "DressedDisease": DressedDisease, "LiveDisease": LiveDisease, "RedFillet": RedFillet, "Shad": Shad, "Trash": Trash, "Turtle": Turtles, "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().find('.save-row').children().attr('src', 'content/images/check.png');
                $.when($.ajax('../api/WeighBack/WeighBackAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        hideProgress();
                    }
                })).then(function () {
                    if (WeightBackID == -1) {
                        loadEditWeighBacks(date);
                    }
                });
            });
            $('.data .delete-row').unbind().click(function (e) {
                $('.row0').empty().append('<div class="col-xs-2"><a href="#" class="add-new-row"><img src="content/images/plus.png"></a></div>');
                bindYieldButtons();

            });
            $('.data .add-new-row').unbind().click(function (e) {
                if ($('#newValues').length < 1) {
                    $('.row0').empty();
                    e.preventDefault();
                    var newRowHtml = '<section class="row row0 data" data-rownum="0" data-weighbackid="-1"><div class="col-xs-4"><select id="farms0" class="farmDDL"></select><select id="ponds0" class="pondsDDL"><option>(Pond)</option></select></div><form id="newValues" class="col-xs-6 input-labels form-inline"><fieldset class="form-group"><label>Turtle</label><input placeholder="Turtles" id="turtles0" class="turtles table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Trash</label><input placeholder="Trash" id="trash0" class="trash table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Shad</label><input placeholder="Shad" id="shad0" class="shad table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Carp</label><input placeholder="Carp" id="carp0" class="carp table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Bream</label><input placeholder="Bream" id="bream0" class="bream table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Live Disease</label><input placeholder="Live Disease" id="livedisease0" class="livedisease table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Dressed Disease</label><input placeholder="Dressed Disease" id="dresseddisease0" class="dresseddisease table-numbers" type="text"></fieldset><fieldset class="form-group"><label>~~Backs</label><input placeholder="~~Backs" id="backs0" class="backs table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Red Fillet</label><input placeholder="Red Fillet" id="redfillet0" class="redfillet table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Big Fish</label><input placeholder="Big Fish" id="bigfish0" class="bigfish table-numbers" type="text"></fieldset><fieldset class="form-group"><label>DOAs</label><input placeholder="DOAs" id="doas0" class="doas table-numbers" type="text"></fieldset></form><div id="newButtons" class="col-xs-2"><fieldset class="form-group"><label>Red Fillet @ 36% Yield</label><input placeholder="" id="redfilletpct0" class="redfilletpct table-numbers" disabled></fieldset><fieldset class="form-group"><label>Dressed Disease @ 60% Yield</label><input placeholder="" id="dresseddiseasepct0" class="dresseddiseasepct table-numbers" disabled></fieldset><a href="#" class="add-row"><img src="content/images/plus.png"></a><a href="#" class="delete-row"><img src="content/images/close.png"></a></div></section>';

                    $.when($('.row0').append(newRowHtml)).then(function () {
                        loadFarmsDDL(0);
                    });
                    bindYieldButtons();
                }
            });
            $('.data .save-row').unbind().click(function (e) {
                e.preventDefault();
                var remove = "0", date = $('.date-select h3 strong').text(), WeightBackID = $(this).parent().parent().data('weighbackid'), pondID = $(this).parent().parent().data('pondid'), Backs = $(this).parent().parent().find('.backs').val(), BigFish = $(this).parent().parent().find('.bigfish').val(), Bream = $(this).parent().parent().find('.bream').val(), Carp = $(this).parent().parent().find('.carp').val(), DOAs = $(this).parent().parent().find('.doas').val(), DressedDisease = $(this).parent().parent().find('.dresseddisease').val(), LiveDisease = $(this).parent().parent().find('.livedisease').val(), RedFillet = $(this).parent().parent().find('.redfillet').val(), Shad = $(this).parent().parent().find('.shad').val(), Trash = $(this).parent().parent().find('.trash').val(), Turtles = $(this).parent().parent().find('.turtles').val(), searchQuery = { "Key": _key, "WBDateTime": date, "WeightBackID": WeightBackID, "PondID": pondID, "Backs": Backs, "BigFish": BigFish, "Bream": Bream, "Carp": Carp, "DOAs": DOAs, "DressedDisease": DressedDisease, "LiveDisease": LiveDisease, "RedFillet": RedFillet, "Shad": Shad, "Trash": Trash, "Turtle": Turtles, "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().find('.save-row').children().attr('src', 'content/images/check.png');
                $.when($.ajax('../api/WeighBack/WeighBackAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                    }
                })).then(function () {
                    if (WeightBackID == -1) {
                        console.log("!")
                    }
                });
            });
        }
    }


    
    /* FARM YIELDS */
    function farmYields() {
        function bindYieldButtons() {
            $('.farmDDL').unbind().change(function () {
                var rowID = $(this).attr('id').replace('farms', ''), farmID = $(this).val();
                loadPondsDDL(rowID, farmID);
            });

            $('.pondsDDL').unbind().change(function () {
                $(this).parent().next().find('input').css('opacity', 1);
            });

            $('.pounds').unbind().focusout(function () {
                if (!$(this).val() == "") {
                    $(this).parent().parent().find('.add-row').css('opacity', 1);
                }
            });
            $('.pounds').unbind().change(function () {
                if (!$(this).val() == "") {
                    $(this).parent().parent().find('.save-row').children().attr('src', 'content/images/save.png');
                    $(this).parent().parent().find('.add-row').css('opacity', 1);
                }
            });

            $('#plantLbsSave').unbind().click(function (e) {
                showProgress('body');
                e.preventDefault();
                var date = $('.date-select h3 strong').text(), weighBacks = $('#weighbacks').val(), plantPounds = $('#plantpounds').val(), plantPoundsID = $('#plantPoundsID').val(), searchQuery = { "Key": _key, "YieldDate": date, "PlantWeight": plantPounds, "FarmYieldHeaderID": plantPoundsID, "WeighBacks": weighBacks }, data = JSON.stringify(searchQuery);
                $.ajax('../api/FarmYieldHeader/FarmYieldHeaderAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        hideProgress();
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        $('.date-select').append("<div>Plant Weight Saved.</div>");
                    }
                })
            });

            $('.data .add-row').unbind().click(function (e) {
                e.preventDefault();
                var remove = "0", date = $('.date-select h3 strong').text(), yieldID = $(this).parent().parent().data('yieldid'), pondID = $(this).parent().parent().find('.pondsDDL').val(), pondYield = $(this).parent().parent().find('.pounds').val(), headPounds = $(this).parent().parent().find('.headedpounds').val(), pctYield = $(this).parent().parent().find('.pctyield1').val(), pctYield2 = $(this).parent().parent().find('.pctyield2').val(),  searchQuery = { "Key": _key, "YieldDate": date, "YieldID": yieldID, "PondID": pondID, "PoundsYielded": pondYield, "PercentYield": pctYield, "PercentYield2": pctYield2, "PoundsHeaded": headPounds, "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().find('.add-row').css('opacity', 0);
                $(this).parent().parent().find('.delete-row').css('opacity', 1);
                i = parseInt($(this).parent().parent().attr('data-rownum')) + 1;
                var justadded = ".row" + (i - 1);
                $.when($.ajax('../api/FarmYield/FarmYieldAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key); 
                        yieldID = msg['YieldID'];
                        $(this).parent().parent().addClass('complete');
                        $(justadded).attr('data-yieldid', yieldID);
                        if (yieldID != -1) { loadEditFarmYields(date); }
                        // ?? What's this? Or - is it necessary?
                        addOrEdit = yieldID;
                    }
                })).then(function () {
                    if (yieldID == -1) {
                        newRowHtml = '<section class="row row' + i + ' data" data-rownum="' + i + '" data-yieldid="-1"><div class="col-xs-2"><select id="farms' + i + '" class="farmDDL"></select></div><div class="col-xs-2"><select id="ponds' + i + '" class="pondsDDL"><option>(Pond)</option></select></div><div class="col-xs-6"><input placeholder="(Pond Weight)" id="pounds' + i + '" class="pounds table-numbers" type="text"><input placeholder="(Headed Weight)" id="headedpounds' + i + '" class="headedpounds table-numbers" type="text"><input placeholder="(% Yield1)" id="pctyield1_' + i + '" class="pctyield table-numbers" type="text"><input placeholder="(% Yield2)" id="pctyield2_' + i + '" class="pctyield2 table-numbers" type="text"></div><div class="col-xs-2"><a href="#" class="add-row"><img src="content/images/plus.png"></a><a href="#" class="delete-row"><img src="content/images/close.png"></a></div></section>';

                        $.when($('#rowContainer').append(newRowHtml)).then(function () { loadFarmsDDL(i); });
                        bindYieldButtons();
                    }
                });
            
            });
            $('.data .add-new-row').unbind().click(function (e) {
                e.preventDefault();
                var newRowHtml = '<section class="row row0 data" data-rownum="0" data-yieldid="-1"><div class="col-xs-2"><select id="farms0" class="farmDDL"></select></div><div class="col-xs-2"><select id="ponds0" class="pondsDDL"><option>(Pond)</option></select></div><div class="col-xs-6"><input placeholder="(Pond Weight)" id="pounds0" class="pounds table-numbers" type="text"><input placeholder="(Headed Weight)" id="headedpounds0" class="headedpounds table-numbers" type="text"><input placeholder="(% Yield1)" id="pctyield1_0" class="pctyield1 table-numbers" type="text"><input placeholder="(% Yield2)" id="pctyield2_0" class="pctyield2 table-numbers" type="text"></div><div class="col-xs-2"><a href="#" class="add-row"><img src="content/images/plus.png"></a><a href="#" class="delete-row"><img src="content/images/close.png"></a></div></section>';

                $.when($('#rowContainer').append(newRowHtml)).then(function () {
                    loadFarmsDDL(0);
                    $('.row.buttons').show();
                });
                i = 1;
                bindYieldButtons();

            });
            $('.data .save-row').unbind().click(function (e) {
                e.preventDefault();
                var remove = "0", date = $('.date-select h3 strong').text(), yieldID = $(this).parent().parent().data('yieldid'), pondID = $(this).parent().parent().data('pondid'), pondYield = $(this).parent().parent().find('.pounds').val(), headPounds = $(this).parent().parent().find('.headedpounds').val(), pctYield = $(this).parent().parent().find('.pctyield1').val(), pctYield2 = $(this).parent().parent().find('.pctyield2').val(), searchQuery = { "Key": _key, "YieldDate": date, "YieldID": yieldID, "PondID" : pondID, "PoundsYielded": pondYield, "PercentYield": pctYield, "PercentYield2": pctYield2, "PoundsHeaded": headPounds, "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().find('.save-row').children().attr('src', 'content/images/check.png');
                $.when($.ajax('../api/FarmYield/FarmYieldAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                    }
                })).then(function () {
                    if (yieldID == -1) {
                    }
                });

            });

            $('.data .delete-row').unbind().click(function (e) {
                e.preventDefault();
                // TO DO: prevent removing sole empty row or replace with empty row
                var remove = "1", searchQuery = { "Key": _key, "YieldID": $(this).parent().parent().attr('data-yieldid'), "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().remove();
                $.when($.ajax('../api/FarmYield/FarmYieldAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key); 
                        yieldList = msg['ReturnData'];
                    }
                })).then(function () {
                    if (!$('.data').length > 0) {
                        var newRowHtml = '<section class="row row0 data" data-rownum="0" data-yieldid="-1"><div class="col-xs-2"><select id="farms0" class="farmDDL"></select></div><div class="col-xs-2"><select id="ponds0" class="pondsDDL"><option>(Pond)</option></select></div><div class="col-xs-6"><input placeholder="(Pond Weight)" id="pounds0" class="pounds table-numbers" type="text"><input placeholder="(Headed Weight)" id="headedpounds0" class="headedpounds table-numbers" type="text"><input placeholder="(% Yield1)" id="pctyield!_0" class="pctyield table-numbers" type="text"><input placeholder="(% Yield2)" id="pctyield2_0" class="pctyield2 table-numbers" type="text"></div><div class="col-xs-2"><a href="#" class="add-row"><img src="content/images/plus.png"></a><a href="#" class="delete-row"><img src="content/images/close.png"></a></div></section>';

                        $.when($('#rowContainer').append(newRowHtml)).then(function () { loadFarmsDDL(0); });
                        i = 1;
                        bindYieldButtons();
                    }
                });
            })
        }

        function loadFarmsDDL(rowID, farmID) {
            var ddlHtml = '<option value="">Select Farm</option>', searchQuery = { "Key": _key, "userID": userID }; data = JSON.stringify(searchQuery); $.when($.ajax('../api/Farm/FarmList', {
                type: 'POST', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg.Key);  farmList = msg['ReturnData']; for (var i = 0; i < farmList.length; ++i) { if (farmList[i].StatusId == "1") { if (typeof farmID !== "undefined" && farmList[i].FarmId == farmID) { ddlHtml += '<option value="' + farmList[i].FarmId + '" selected>' + farmList[i].FarmName + '</option>'; } else { ddlHtml += '<option value="' + farmList[i].FarmId + '">' + farmList[i].FarmName + '</option>'; } } } } })).then(function () { $('#farms' + rowID).empty().html(ddlHtml); }); }

        function loadPondsDDL(rowID, farmID, pondID) {
            var ddlHtml = '<option value="">Select Pond</option>', searchQuery = { "Key": _key, "userID": userID, "FarmId": farmID }; data = JSON.stringify(searchQuery); $.when($.ajax('../api/Pond/PondList', { type: 'POST', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg.Key); pondList = msg['ReturnData']; for (var i = 0; i < pondList.length; ++i) { if (pondList[i].StatusId == "1") { if (typeof pondID !== "undefined" && pondList[i].PondId == pondID) { ddlHtml += '<option value="' + pondList[i].PondId + '" selected>' + pondList[i].PondName + '</option>'; } else { ddlHtml += '<option value="' + pondList[i].PondId + '">' + pondList[i].PondName + '</option>'; } } } } })).then(function () { $('#ponds' + rowID).empty().html(ddlHtml).css('opacity', 1); });
        }
    }

    /* WEIGH BACKS */
    function weighBacks() {
        function bindYieldButtons() {
            $('.farmDDL').unbind().change(function () {
                var rowID = $(this).attr('id').replace('farms', ''), farmID = $(this).val();
                loadPondsDDL('0', farmID)
            });

            $('.pondsDDL').unbind().change(function () {
                $('#newValues').css('opacity', 1);
                $('#newButtons').css('opacity', 1);
            });

            $('.dresseddisease').unbind().blur(function () {
                var redfilletid = $(this).attr('id').replace('dresseddisease', ''), actual = $(this).val(), pct = actual / .6;
                $('#dresseddiseasepct' + redfilletid).val(pct);
            });

            $('.redfillet').unbind().blur(function () {
                var redfilletid = $(this).attr('id').replace('redfillet', ''), actual = $(this).val(), pct = actual / .36;
                $('#redfilletpct' + redfilletid).val(pct);
            });

            $('.data .add-row').unbind().click(function (e) {
                showProgress('body');
                e.preventDefault();
                var remove = "0", date = $('.date-select h3 strong').text(), WeightBackID = -1, pondID = $(this).parent().parent().find('#ponds0').val(), Backs = $(this).parent().parent().find('.backs').val(), BigFish = $(this).parent().parent().find('.bigfish').val(), Bream = $(this).parent().parent().find('.bream').val(), Carp = $(this).parent().parent().find('.carp').val(), DOAs = $(this).parent().parent().find('.doas').val(), DressedDisease = $(this).parent().parent().find('.dresseddisease').val(), LiveDisease = $(this).parent().parent().find('.livedisease').val(), RedFillet = $(this).parent().parent().find('.redfillet').val(), Shad = $(this).parent().parent().find('.shad').val(), Trash = $(this).parent().parent().find('.trash').val(), Turtles = $(this).parent().parent().find('.turtles').val(), searchQuery = { "Key": _key, "WBDateTime": date, "WeightBackID": WeightBackID, "PondID": pondID, "Backs": Backs, "BigFish": BigFish, "Bream": Bream, "Carp": Carp, "DOAs": DOAs, "DressedDisease": DressedDisease, "LiveDisease": LiveDisease, "RedFillet": RedFillet, "Shad": Shad, "Trash": Trash, "Turtle": Turtles, "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().find('.save-row').children().attr('src', 'content/images/check.png');
                $.when($.ajax('../api/WeighBack/WeighBackAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                        hideProgress();
                    }
                })).then(function () {
                    if (WeightBackID == -1) {
                        loadEditWeighBacks(date);
                    }
                });
            });
            $('.data .delete-row').unbind().click(function (e) {
                $('.row0').empty().append('<div class="col-xs-2"><a href="#" class="add-new-row"><img src="content/images/plus.png"></a></div>');
                bindYieldButtons();

            });
            $('.data .add-new-row').unbind().click(function (e) {
                if ($('#newValues').length < 1) {
                    $('.row0').empty();
                    e.preventDefault();
                    var newRowHtml = '<section class="row row0 data" data-rownum="0" data-weighbackid="-1"><div class="col-xs-4"><select id="farms0" class="farmDDL"></select><select id="ponds0" class="pondsDDL"><option>(Pond)</option></select></div><form id="newValues" class="col-xs-6 input-labels form-inline"><fieldset class="form-group"><label>Turtle</label><input placeholder="Turtles" id="turtles0" class="turtles table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Trash</label><input placeholder="Trash" id="trash0" class="trash table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Shad</label><input placeholder="Shad" id="shad0" class="shad table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Carp</label><input placeholder="Carp" id="carp0" class="carp table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Bream</label><input placeholder="Bream" id="bream0" class="bream table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Live Disease</label><input placeholder="Live Disease" id="livedisease0" class="livedisease table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Dressed Disease</label><input placeholder="Dressed Disease" id="dresseddisease0" class="dresseddisease table-numbers" type="text"></fieldset><fieldset class="form-group"><label>~~Backs</label><input placeholder="~~Backs" id="backs0" class="backs table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Red Fillet</label><input placeholder="Red Fillet" id="redfillet0" class="redfillet table-numbers" type="text"></fieldset><fieldset class="form-group"><label>Big Fish</label><input placeholder="Big Fish" id="bigfish0" class="bigfish table-numbers" type="text"></fieldset><fieldset class="form-group"><label>DOAs</label><input placeholder="DOAs" id="doas0" class="doas table-numbers" type="text"></fieldset></form><div id="newButtons" class="col-xs-2"><fieldset class="form-group"><label>Red Fillet @ 36% Yield</label><input placeholder="" id="redfilletpct0" class="redfilletpct table-numbers" disabled></fieldset><fieldset class="form-group"><label>Dressed Disease @ 60% Yield</label><input placeholder="" id="dresseddiseasepct0" class="dresseddiseasepct table-numbers" disabled></fieldset><a href="#" class="add-row"><img src="content/images/plus.png"></a><a href="#" class="delete-row"><img src="content/images/close.png"></a></div></section>';

                    $.when($('.row0').append(newRowHtml)).then(function () {
                        loadFarmsDDL(0);
                    });
                    bindYieldButtons();
                }
            });
            $('.data .save-row').unbind().click(function (e) {
                e.preventDefault();
                var remove = "0", date = $('.date-select h3 strong').text(), WeightBackID = $(this).parent().parent().data('weighbackid'), pondID = $(this).parent().parent().data('pondid'), Backs = $(this).parent().parent().find('.backs').val(), BigFish = $(this).parent().parent().find('.bigfish').val(), Bream = $(this).parent().parent().find('.bream').val(), Carp = $(this).parent().parent().find('.carp').val(), DOAs = $(this).parent().parent().find('.doas').val(), DressedDisease = $(this).parent().parent().find('.dresseddisease').val(), LiveDisease = $(this).parent().parent().find('.livedisease').val(), RedFillet = $(this).parent().parent().find('.redfillet').val(), Shad = $(this).parent().parent().find('.shad').val(), Trash = $(this).parent().parent().find('.trash').val(), Turtles = $(this).parent().parent().find('.turtles').val(), searchQuery = { "Key": _key, "WBDateTime": date, "WeightBackID": WeightBackID, "PondID": pondID, "Backs": Backs, "BigFish": BigFish, "Bream": Bream, "Carp": Carp, "DOAs": DOAs, "DressedDisease": DressedDisease, "LiveDisease": LiveDisease, "RedFillet": RedFillet, "Shad": Shad, "Trash": Trash, "Turtle": Turtles, "Remove": remove }, data = JSON.stringify(searchQuery);
                $(this).parent().parent().find('.save-row').children().attr('src', 'content/images/check.png');
                $.when($.ajax('../api/WeighBack/WeighBackAddOrEdit', {
                    type: 'PUT',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key'];
                        startTimer(msg.Key);
                    }
                })).then(function () {
                    if (WeightBackID == -1) {
                        console.log("!")
                    }
                });
            });
        }

        function loadFarmsDDL(rowID, farmID) {
            showProgress('body');
            var ddlHtml = '<option value="">Select Farm</option>', searchQuery = { "Key": _key, "userID": userID }; data = JSON.stringify(searchQuery); $.when($.ajax('../api/Farm/FarmList', {
                type: 'POST', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg.Key); farmList = msg['ReturnData']; for (var i = 0; i < farmList.length; ++i) { if (farmList[i].StatusId == "1") { if (typeof farmID !== "undefined" && farmList[i].FarmId == farmID) { ddlHtml += '<option value="' + farmList[i].FarmId + '" selected>' + farmList[i].FarmName + '</option>'; } else { ddlHtml += '<option value="' + farmList[i].FarmId + '">' + farmList[i].FarmName + '</option>'; } } } }
            })).then(function () { hideProgress(); $('#farms' + rowID).empty().html(ddlHtml).css('opacity', 1); });
        }

        function loadPondsDDL(rowID, farmID, pondID) {
            showProgress('body');
            var ddlHtml = '<option value="">Select Pond</option>', searchQuery = { "Key": _key, "userID": userID, "FarmId": farmID }; data = JSON.stringify(searchQuery); $.when($.ajax('../api/Pond/PondList', { type: 'POST', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg.Key); pondList = msg['ReturnData']; for (var i = 0; i < pondList.length; ++i) { if (pondList[i].StatusId == "1") { if (typeof pondID !== "undefined" && pondList[i].PondId == pondID) { ddlHtml += '<option value="' + pondList[i].PondId + '" selected>' + pondList[i].PondName + '</option>'; } else { ddlHtml += '<option value="' + pondList[i].PondId + '">' + pondList[i].PondName + '</option>'; } } } } })).then(function () { hideProgress(); $('#ponds' + rowID).empty().html(ddlHtml).css('opacity', 1); });
        }
    }

    /* PLANT WEIGHTS */
    function plantWeights() {
        $('.buttons .reset').unbind().click(function (e) {
            e.preventDefault();
            if (window.confirm("This will permanently delete any information you have entered and not saved.")) {
                document.location.reload(true);
            }
        });

        $('.buttons .save').unbind().click(function (e) {
            e.preventDefault();

            var searchQuery = { "Key": _key, "userID": userID, "ShiftDate": date, "ShiftEndID": addOrEdit, "DayFinishedFreezing": $('#dayFreeze').val(), "DayShiftFroze": $('#dayFroze').val(), "FilletScaleReading": $('#filletScale').val(), "FinishedFillet": $('#finFillet').val(), "FinishedKill": $('#finKill').val(), "FinishedSkinning": $('#finSkinned').val(), "InmateLeftEarly": $('#inmateEmpLeftEarly').val(), "NightFinishedFreezing": $('#nightFreeze').val(), "NightShiftFroze": $('#nightFroze').val(), "RegEmpLate": $('#regEmpLate').val(), "RegEmpOut": $('#regEmpOut').val(), "InLateOut": $('#inLateOut').val(), "EmployeesOnVacation": $('#empVacation').val(), "RegEmplLeftEarly": $('#regEmpLeftEarly').val(), "TempEmpOut": $('#tempEmpOut').val(), "DowntimeMinutes": $('#downtimeMin').val() }, data = JSON.stringify(searchQuery);
            $.when($.ajax('../api/ShiftEnd/ShiftEndAddOrEdit', {
                type: 'PUT',
                data: data,
                success: function (msg) {
                    localStorage['CT_key'] = msg['Key'];
                    startTimer(msg.Key);
                    farmList = msg['ReturnData'];
                    $('.date-select').append("<div>Information Saved!</div>");
                }
            })).then(function () { $('input').val(""); $('.row.fields, .row.buttons').css('opacity', 0); });
        });
    }
}

/*****************************************************************
<!-- APPEND TO FORM CONTAINER -->
<!-- Static for PondWeight -->

<!-- END -->

<!-- Static for PlantWeight -->
<section id="plant-weight-' + plantweightID + '" class="row form-inline" data-row="plant-weight">
<header class="col-md-12">Plant Weights</header>
	<section class="col-md-2"><p>(TIME) 6:13AM</p></section><section class="col-md-10"><label>Plant Weight:</label><input type="text" id="plantweightID" class="form-control" value="plantWeightValue"><button class="btn btn-default">Edit</button></section>
	<section class="row buttons"><button id="addNewPlantWeight" class="btn btn-default">Add New Plant Weight</button></section>
</section>
<!-- END -->

<!-- Static for Weighbacks -->
<section id="weighbacks-' + weighbacksID + '" class="row" data-row="weighbacks">
<header class="col-md-12">Weighbacks</header>
	<section class="col-md-6"><p>(TIME) 6:13AM</p></section><section class="col-md-6"><input type="text" id="plantweightID" value="plantWeightValue"><button class="btn btn-default">Edit</button></section>
	<section class="row buttons"><button id="addNewWeighback" class="btn btn-default">Add New Weighback</button></section>
</section>
<!-- END -->

<!-- Static for FarmYields -->
<section id="farm-yields-' + farm-yieldID + '" class="row"  data-row="yield">
<header class="col-md-12">Farm Yields</header>
	<section class="col-md-6"><p>(TIME) 6:13AM</p></section><section class="col-md-6"><input type="text" id="plantweightID" value="plantWeightValue"><button class="btn btn-default">Edit</button></section>
	<section class="row buttons"><button id="addNewFarmYield" class="btn btn-default">Add New Farm Yield</button></section>
</section>
<!-- END -->
**************************************************************/