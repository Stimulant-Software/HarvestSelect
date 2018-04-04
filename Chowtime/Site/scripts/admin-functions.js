﻿var authorized = false, superAuthorized = false, userRoles = localStorage['CTuserRole'].split(' ');
if (userRoles.indexOf('Admin') > -1) authorized = true;
if (userRoles.indexOf('SGAdmin') > -1) { authorized = true; superAuthorized = true; }

if (!authorized) window.location.href = "login.html";

showProgress('body', 'loadAdminPage');
var pageClasses, pageType;

$.when( checkKey(), pageLabel() ).then(function(){
    pageClasses = $('body').attr('class').split(" ");
    for (var i = 0; i < pageClasses.length; i++) {
        switch (pageClasses[i]) {
            case "farm":
                pageType = "farm";
                populateFarms();
                break;
            case "pond":
                pageType = "pond";
				$('#changeFarm').change(function () {
                    farmID = $('option:selected', this).val();
                    populatePonds(farmID);
                });
                break;
            case "user":
                pageType = "user";
                populateUsers();
                break;
            case "email":
                pageType = "email";
                populateEmails();
                resendReport();
                break;
            case "setup":
                if (!superAuthorized) window.location.href = "admin-user.html";
                pagetype = "setup";
                $('#newCompany').css('left', '0').show();
                initialSetup();
            default: break;
        }
    }
    hideProgress('loadAdminPage');
});

function populateFarms() { var searchQuery = { "key": _key, "userID": userID }; data = JSON.stringify(searchQuery); showProgress('body', 'populate-farms'); $.when($.ajax('../api/Farm/FarmList', { type: 'POST', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); farmList = msg['ReturnData']; } })).then(function () { var activeFarms = [], inactiveFarms = [], a = 0, b = 0, activeFarmList = '', inactiveFarmList = ''; for (var i = 0; i < farmList.length; i++) { if (farmList[i].StatusId == "1") { activeFarms[a] = farmList[i]; a++; } else { inactiveFarms[b] = farmList[i]; b++; } } $('ol.active li:not(".header"), ol.inactive li:not(".header")').remove(); for (var i = 0; i < activeFarms.length; i++) { activeFarmList += '<li><span>' + activeFarms[i].FarmName + '</span><button class="change-status" id="activeFarm_' + activeFarms[i].FarmId + '">Deactivate</button><button class="edit" id="editFarm_' + activeFarms[i].FarmId + '">Edit Farm</button></li>'; } for (var i = 0; i < inactiveFarms.length; i++) { inactiveFarmList += '<li><span>' + inactiveFarms[i].FarmName + '</span><button class="change-status" id="inactiveFarm_' + inactiveFarms[i].FarmId + '">Activate</button><button class="edit" id="editFarm_' + inactiveFarms[i].FarmId + '">Edit Farm</button></li>'; } if (activeFarms.length == 0) { activeFarmList += '<li><span>There are no active farms.</span></li>' }; if (inactiveFarms.length == 0) { inactiveFarmList += '<li><span>There are no inactive farms.</span></li>' }; $('ol.active').append(activeFarmList); $('ol.inactive').append(inactiveFarmList); $.when(bindButtons()).then(function () { hideProgress('populate-farms'); }); }); }
function populateEmails() { var searchQuery = { "key": _key, "userID": userID }; data = JSON.stringify(searchQuery); showProgress('body', 'populate-emails'); $.when($.ajax('../api/Email/EmailList', { type: 'POST', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); emailList = msg['ReturnData']; } })).then(function () { var activeEmailList = ''; $('ol.active li:not(".header"), ol.inactive li:not(".header")').remove(); for (var i = 0; i < emailList.length; i++) { activeEmailList += '<li><span style="overflow: visible;">' + emailList[i].EmailAddress + '</span><button class="delete-email" id="activeEmail_' + emailList[i].EmailID + '">Delete</button></li>'; } $('ol.active').append(activeEmailList); $.when(bindButtons()).then(function () { hideProgress('populate-emails'); }); }); }

function populatePonds(farmID) {
	var searchQuery = { "key": _key, "userID": userID, "FarmId": farmID }; data = JSON.stringify(searchQuery); showProgress('body', 'populate-ponds');
	debugger;
	$.when($.ajax('../api/Pond/PondList', { type: 'POST', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); pondList = msg['ReturnData']; } })).then(function () { var activePonds = [], inactivePonds = [], a = 0, b = 0, activePondList = '', inactivePondList = ''; for (var i = 0; i < pondList.length; i++) { if (pondList[i].StatusId == "1") { activePonds[a] = pondList[i]; a++; } else { inactivePonds[b] = pondList[i]; b++; } } $('ol.active li:not(".header"), ol.inactive li:not(".header")').remove(); for (var i = 0; i < activePonds.length; i++) { activePondList += '<li><span>' + activePonds[i].PondName + '</span><button class="change-status" id="activePond_' + activePonds[i].PondId + '">Deactivate</button>'; activePondList += activePonds[i].NoFeed == "False" ? '<button class="change-feedme" data-nofeed="' + activePonds[i].NoFeed + '" id="feed_' + activePonds[i].PondId + '">Suspend Feed</button>' : '<button class="change-feedme suspended" data-nofeed="' + activePonds[i].NoFeed + '" id="feed_' + activePonds[i].PondId + '">Resume Feed</button>'; activePondList += '<button class="edit" id="editPond_' + activePonds[i].PondId + '">Edit Pond</button></li>'; } for (var i = 0; i < inactivePonds.length; i++) { inactivePondList += '<li><span>' + inactivePonds[i].PondName + '</span><button class="change-status" id="inactivePond_' + inactivePonds[i].PondId + '">Activate</button>'; inactivePondList += inactivePonds[i].NoFeed == "False" ? '<button class="change-feedme" data-nofeed="' + inactivePonds[i].NoFeed + '" id="feed_' + inactivePonds[i].PondId + '">Suspend Feed</button>' : '<button class="change-feedme suspended" data-nofeed="' + inactivePonds[i].NoFeed + '" id="feed_' + inactivePonds[i].PondId + '">Resume Feed</button>'; inactivePondList += '<button class="edit" id="editPond_' + inactivePonds[i].PondId + '">Edit Pond</button></li>'; } if (activePonds.length == 0) { activePondList += '<li><span>There are no active ponds.</span></li>' }; if (inactivePonds.length == 0) { inactivePondList += '<li><span>There are no inactive ponds.</span></li>' }; $('ol.active').append(activePondList); $('ol.inactive').append(inactivePondList); $.when(bindButtons()).then(function () { hideProgress('populate-ponds'); }); });
}

function populateUsers() {
    var searchQuery = { "key": _key };
    data = JSON.stringify(searchQuery);
    showProgress('body', 'populate-users');
    $.when($.ajax('../api/User/UserList', {
        type: 'POST',
        data: data,
        success: function (msg) {
            localStorage['CT_key'] = msg['Key']; startTimer(msg['Key'])
            userList = msg['ReturnData'];
        }
    })).then(function () {
        var activeUsers = [], inactiveUsers = [], a = 0, b = 0, activeUserList = '', inactiveUserList = '';
        for (var i = 0; i < userList.length; i++) {
            if (userList[i].StatusId == "1") {
                activeUsers[a] = userList[i];
                a++;
            } else {
                inactiveUsers[b] = userList[i];
                b++;
            }
        }

        $('ol.active li:not(".header"), ol.inactive li:not(".header")').remove();
        for (var i = 0; i < activeUsers.length; i++) {

            // new to handle names longer than space permits
            var userFullName = activeUsers[i].LastName + ", " + activeUsers[i].FirstName;
            if (userFullName.length > 25) userFullName = userFullName.substring(0, 17) + "...";

            activeUserList += '<li><span title="' + activeUsers[i].LastName + ", " + activeUsers[i].FirstName + '">' + userFullName + '</span>'; if (activeUsers[i].UserId != localStorage['CTuserID']) { activeUserList += '<button class="change-status" id="activeUser_' + activeUsers[i].UserId + '">Deactivate</button>' } activeUserList += '<button class="edit" id="editUser_' + activeUsers[i].UserId + '">Edit Info</button><button class="edit-roles-farms" id="editUserRoles_' + activeUsers[i].UserId + '">Roles & Farms</button></li>';
        }
        for (var i = 0; i < inactiveUsers.length; i++) {

            // new to handle names longer than space permits
            var userFullName = inactiveUsers[i].LastName + ", " + inactiveUsers[i].FirstName;
            if (userFullName.length > 25) userFullName = userFullName.substring(0, 17) + "...";

            inactiveUserList += '<li><span title="' + inactiveUsers[i].LastName + ", " + inactiveUsers[i].FirstName + '">' + userFullName + '</span><button class="change-status" id="inactiveUser_' + inactiveUsers[i].UserId + '">Activate</button><button class="edit" id="editUser_' + inactiveUsers[i].UserId + '">Edit Info</button><button class="edit-roles-farms" id="editUserRoles_' + inactiveUsers[i].UserId + '">Roles & Farms</button></li>';
        }
        if (activeUsers.length == 0) { activeUserList += '<li><span>There are no active users.</span></li>' };
        if (inactiveUsers.length == 0) { inactiveUserList += '<li><span>There are no inactive users.</span></li>' };
        $('ol.active').append(activeUserList);
        $('ol.inactive').append(inactiveUserList);
        $.when(bindButtons()).then(function () { hideProgress('populate-users'); });
    });
}

$('.add-new').unbind().click(function (e) { e.preventDefault(); var objectType = pageType.charAt(0).toUpperCase() + pageType.slice(1); $('body').append('<div id="lightboxBG" class="modal"></div>'); switch (pageType) { case "pond": loadFarmsForPonds(); break; case "user": /* loadFarmsForUsers(), loadRolesForUsers(); */break; case "email": break; default: break; } $('#lightboxBG').fadeIn('100', function () { centerModal('#addNew' + objectType + 'Modal'); $('#addNew' + objectType + 'Modal').fadeIn('100'); bindFormButtons(); }); });

function bindButtons() {
    $('.change-feedme').unbind().click(function (e) { e.preventDefault(); var objectID = getObjectID($(this)), objectStatus = $(this).parent().parent('ol').attr('class'), objectName = $(this).siblings('span').text(); var newFeedStatusID = $(this).data('nofeed') == "True" ? "False" : "True"; dto = { "Key": localStorage['CT_key'],  "PondId": objectID, "PondName": objectName, "NoFeed": newFeedStatusID }, data = JSON.stringify(dto); showProgress('body', 'bind-buttons'); $.when($.ajax('../api/Pond/ChangePondFeedStatus', { type: 'PUT', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); console.log(msg); } })).then(function () { populatePonds(farmID); hideProgress('bind-buttons'); }); });

    $('.change-status').unbind().click(function (e) { e.preventDefault(); var objectID = getObjectID($(this)), objectStatus = $(this).parent().parent('ol').attr('class'), objectName = $(this).siblings('span').text(); var newStatusID = objectStatus == "active" ? "2" : "1"; switch (pageType) { case "farm": dto = { "Key": localStorage['CT_key'], "FarmId": objectID, "FarmName": objectName, "StatusId": newStatusID }, data = JSON.stringify(dto); showProgress('body', 'change-status-farm'); $.when($.ajax('../api/Farm/ChangeFarmStatus', { type: 'PUT', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); } })).then(function () { populateFarms(); hideProgress('change-status-farm'); }); break; case "pond": dto = { "Key": localStorage['CT_key'], "PondId": objectID, "PondName": objectName, "StatusId": newStatusID }, data = JSON.stringify(dto); showProgress('body', 'change-status-pond'); $.when($.ajax('../api/Pond/PondActiveFlag', { type: 'PUT', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); } })).then(function () { populatePonds(farmID); hideProgress('change-status-pond'); }); break; case "user": dto = { "Key": localStorage['CT_key'], "UserId": objectID, "UserName": objectName, "StatusId": newStatusID }, data = JSON.stringify(dto); showProgress('body', 'change-status-user'); $.when($.ajax('../api/User/ChangeUserStatus', { type: 'PUT', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); } })).then(function () { populateUsers(); hideProgress('change-status-user'); }); break; } });
    $('.delete-email').unbind().click(function (e) {
        e.preventDefault();
        var objectID = getObjectID($(this));
        switch (pageType) {
            case "email": dto = { "Key": localStorage['CT_key'], "EmailID": objectID, },
                data = JSON.stringify(dto);
                showProgress('body', 'change-status-email');
                $.when($.ajax('../api/Email/DeleteEmail',
                    {
                        type: 'PUT',
                        data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); }
                    })).then(function () { populateEmails(); hideProgress('change-status-email'); }); break;
            
            
        }
    });
    $('.edit-roles-farms').unbind().click(function (e) {
        e.preventDefault(); var objectID = getObjectID($(this)), objectStatus = $(this).parent().parent('ol').attr('class'), objectName = $(this).siblings('span').text(), userFarms = new Array(), userRoles = new Array();
        ; $('body').append('<div id="lightboxBG" class="modal"></div>'); $('#lightboxBG').fadeIn('100', function () { centerModal('#modifyUserRolesAndFarms'); $('#modifyUserRolesAndFarms').fadeIn('100'); });
        var searchQuery = { "key": _key, "UserId": objectID };
        data = JSON.stringify(searchQuery);
        $.when($.ajax('../api/User/UserDetail', {
            type: 'POST',
            data: data,
            success: function (msg) {
                localStorage['CT_key'] = msg['Key']; startTimer(msg['Key'])
                userInfo = msg['ReturnData'][0];
                if (userInfo.Farms) { userFarms = userInfo.Farms.split(',') } else userFarms = {};
                if (userInfo.Roles) { userRoles = userInfo.Roles.split(',') } else userRoles = {};
            }
        })).then(function () {
            $('#userFirstLast').text(userInfo.FirstName + " " + userInfo.LastName);
            $('#userId').val(userInfo.UserId);
            loadFarmsForUsers(userFarms, userInfo.UserId);
            loadRolesForUsers(userRoles, userInfo.UserId);
        });
        bindFormButtons();
    });


    $('.edit').unbind().click(function (e) {
        e.preventDefault();
        // fill admin-modal inputs appropriately
        var objectID = getObjectID($(this)), objectStatus = $(this).parent().parent('ol').attr('class'), objectName = $(this).siblings('span').text(), objectStatusID = objectStatus == "active" ? "1" : "2", objectType = pageType.charAt(0).toUpperCase() + pageType.slice(1); 

        $('body').append('<div id="lightboxBG" class="modal"></div>');
        $('#lightboxBG').fadeIn('100', function () { centerModal('#addNew' + objectType + 'Modal'); $('#addNew' + objectType + 'Modal').fadeIn('100'); });

        switch (pageType) {
            case "farm": var searchQuery = { "key": _key, "FarmId": objectID }, farmInfo = {}; data = JSON.stringify(searchQuery); $.when($.ajax('../api/Farm/FarmDetail', { type: 'POST', data: data, success: function (msg) { _key = msg['Key']; localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); farmInfo = msg['ReturnData'][0]; } })).then(function () { $('#farmName').val(farmInfo.FarmName); $('#farmID').val(farmInfo.FarmId); $('#statusID').val(farmInfo.StatusId); $('#InnovaName').val(farmInfo.InnovaName); $('#InnovaCode').val(farmInfo.InnovaCode);}); break;
            case "pond": var searchQuery = { "key": _key, "PondId": objectID }, pondInfo = {}; data = JSON.stringify(searchQuery); $.when($.ajax('../api/Pond/PondDetail', { type: 'POST', data: data, success: function (msg) { _key = msg['Key']; localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); pondInfo = msg['ReturnData'][0]; } })).then(function () { $('#PondName').val(pondInfo.PondName); $('#PondId').val(pondInfo.PondId); $('#StatusId').val(pondInfo.StatusId); $('#Size').val(pondInfo.Size); $('#NoFeed').val(pondInfo.NoFeed); $('#InnovaName').val(pondInfo.InnovaName); $('#InnovaCode').val(pondInfo.InnovaCode); loadFarmsForPonds(farmID); }); break;
            case "user":
                var searchQuery = { "key": _key, "UserId": objectID };
                data = JSON.stringify(searchQuery);
                $.when($.ajax('../api/User/UserDetail', {
                    type: 'POST',
                    data: data,
                    success: function (msg) {
                        localStorage['CT_key'] = msg['Key']; startTimer(msg['Key'])
                        userInfo = msg['ReturnData'][0];
                    }
                })).then(function () {
                    $('#firstName').val(userInfo.FirstName);
                    $('#lastName').val(userInfo.LastName);
                    $('#emailAddress').val(userInfo.EmailAddress);
                    $('#phone').val(userInfo.Phone);
                    $('#userID').val(userInfo.UserId);
                    $('#statusID').val(userInfo.StatusId);
                    // fix styling, add "sg" as value somewhere
                    // need userDetail to pass arrays of farms and roles
                    $('#passwordFields').append('<div id="curtain">Click here to reset the password.</div>');
                    $('#curtain').click(function () { $(this).remove(); })
                });
                break;
            default:
                console.log("ERROR: Page has no type");
        }
        bindFormButtons();
    });
}

function bindFormButtons() {
    $('.submit').unbind().click(function (e) {
        e.preventDefault();
        var errorMessage = '', allRequired = true, submitObject = { "key": _key }; 

        // validate fields marked as required:
        $('.admin-modal input[type="text"], .admin-modal select').each(function () {
            $(this).removeAttr('style');
            if ($(this).attr('required')) {
                if ($(this).val() == "") {
                    $(this).attr('placeholder', "** All fields required **").css({ 'border-color': '#b32c31', 'font-weight': 'bold', 'background-color': '#fafaca' });
                    allRequired = false;
                }
            }
        });

        if ($('#userID').length && $('#userID').val() == "-1") {
            if ($('#password').val() != $('#passConfirm').val()) {
                errorMessage += "Password fields do not match<br />";
                $('#password, #passConfirm').val();
                allRequired = false;
            } else if ($('#password').val().length < 8) {
                errorMessage += "Password must be at least 8 characters<br />";
                $('#password, #passConfirm').val();
                allRequired = false;
            }
        }
        //if ($('#emailAddress').length && !validateEmail($("#emailAddress").val())) {
        //    errorMessage += "Email must be a valid address<br />";
        //    allRequired = false;
        //}
        //if ($('#phone').length && !validatePhone($("#phone").val())) {
        //    errorMessage += "Phone must be in the correct format<br />";
        //    allRequired = false;
        //}

        function validatePhone(phone) {
            var re = /(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:(\s*([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]‌​)\s*)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)([2-9]1[02-9]‌​|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})/;
            return re.test(phone);
        }
        function validateEmail(email) {
            var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return re.test(email);
        }

        if (errorMessage != '') alertError(errorMessage);

        // if form validates:
        if(allRequired) {
            showProgress('body', 'submitForm');
            $('.admin-modal input:not(:checkbox), .admin-modal select').each(function () {
                var term = $(this).attr('id'), def = $(this).val();
                if ($(this).attr('type') == 'checkbox') { def = $(this).is(':checked') ? "true" : "false"; }
                if (def != "") { def == "on" ? def = true : def = def; submitObject[term] = def; }
            });
            var data = JSON.stringify(submitObject);

            switch (pageType) {
                case "farm": $.ajax('../api/Farm/FarmAddOrEdit', { type: 'PUT', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); resetFarmModal(); closeAdminModal(); populateFarms(); hideProgress('submitForm'); } }); break;
                case "email": $.ajax('../api/Email/EmailAddOrEdit', { type: 'PUT', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); resetEmailModal(); closeAdminModal(); populateEmails(); hideProgress('submitForm'); } }); break;
                case "pond": $.ajax('../api/Pond/PondAddOrEdit', { type: 'PUT', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); resetPondModal(); closeAdminModal(); populatePonds(farmID); hideProgress('submitForm'); } }); break;
                case "user":
                    $.when($.ajax('../api/User/UserAddOrEdit', {
                        type: 'PUT',
                        data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']);
                            submitObject['userID'] = (msg['UserId']);
                            data = JSON.stringify(submitObject);
                        }
                    })).then(function () {
                        if (data["password"] != "") {
                            $.ajax('../api/User/SetPassword', {
                                type: 'PUT',
                                data: data,
                                success: function (msg) {
                                    localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); resetUserModal(); closeAdminModal(); populateUsers(); hideProgress('submitForm');
                                }
                            });
                        } else {
                            resetUserModal(), closeAdminModal(), populateUsers(); hideProgress('submitForm');
                        }
                    });
                    break;
                default: console.log("ERROR: Page has no type"); break;
            }
        }
    });

    $('.cancel').unbind().click(function (e) { e.preventDefault(); switch (pageType) { case "farm": resetFarmModal(); break; case "pond": resetPondModal(); break; case "user": resetUserModal(); break; default: console.log("ERROR: Page has no type"); break; } closeAdminModal(); }); }

function resetFarmModal() { $('#farmName').val(""); $('#statusID').val('1'); $('#farmID').val('-1'); }

function resetEmailModal() { $('#EmailName').val(""); }

function resetPondModal() { $('#PondName').val(""); $('#StatusId').val('1'); $('#PondId').val('-1'); $('#Size').val(""); $('#NoFeed').val('False'); $('#FarmId').empty(); }

function resetUserModal() { $('#firstName').val(""); $('#lastName').val(""); $('#emailAddress').val(""); $('#phone').val(""); $('#userID').val(-1); $('#statusID').val(1); $('adminUserRolesList, #adminUserFarmsList').empty(); $('#password, #passConfirm').val(""); if ($('#curtain').length) { $('#curtain').remove(); } }

function getObjectID(element) { var objectID = element.attr('id').split('_'); return objectID[1]; }

function loadFarmsForPonds(farmID) { var ddlHtml = '<option value="" selected="selected">Select Farm</option>', searchQuery = { "key": _key, "userID": userID }; data = JSON.stringify(searchQuery); $.when($.ajax('../api/Farm/FarmList', { type: 'POST', data: data, success: function (msg) { localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); farmList = msg['ReturnData']; for (var i = 0; i < farmList.length; ++i) { if (farmList[i].StatusId == "1") { if (farmList[i].FarmId == farmID) { ddlHtml += '<option value="' + farmList[i].FarmId + '" selected>' + farmList[i].FarmName + '</option>'; } else { ddlHtml += '<option value="' + farmList[i].FarmId + '">' + farmList[i].FarmName + '</option>'; } } } } })).then(function () { $('#FarmId').empty().html(ddlHtml); }); }

function loadRolesForUsers(checkedRoles, userID) {
    roleChecklistHtml = '', searchQuery = { "key": _key, "userID": userID }; data = JSON.stringify(searchQuery); $.when($.ajax('../api/User/AllRoles', {
        type: 'POST', data: data, success: function (msg) {
            localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); rolesList = msg['ReturnData'];
            for (var i = 0; i < rolesList.length; ++i) {
                if(checkedRoles.length > 0) {
                    if (checkedRoles.indexOf(rolesList[i].RoleId) > -1) {
                        roleChecklistHtml += '<label><input type="checkbox" id="' + rolesList[i].RoleId + '" checked>' + rolesList[i].RoleName + '</label><br />';
                    } else {
                        roleChecklistHtml += '<label><input type="checkbox" id="' + rolesList[i].RoleId + '">' + rolesList[i].RoleName + '</label><br />';
                    }
                } else {
                    roleChecklistHtml += '<label><input type="checkbox" id="' + rolesList[i].RoleId + '">' + rolesList[i].RoleName + '</label><br />';
                }
            }
        }
    })).then(function () { $('#adminUserRolesList').empty().html(roleChecklistHtml); editUserRoles(userID); });
}

function loadFarmsForUsers(checkedFarms, userID) {
    farmChecklistHtml = '<input type="checkbox" id="0"><label>Select All</label><br />', searchQuery = { "key": _key, "userID": userID }; data = JSON.stringify(searchQuery); $.when($.ajax('../api/Farm/FarmList', {
        type: 'POST', data: data, success: function (msg) {
            localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']); farmList = msg['ReturnData']; 
            for (var i = 0; i < farmList.length; ++i) {
                if (farmList[i].StatusId == "1") {
                    if(checkedFarms.length > 0) {
                        if (checkedFarms.indexOf(farmList[i].FarmId) > -1) {
                            farmChecklistHtml += '<label><input type="checkbox" id="' + farmList[i].FarmId + '" checked>' + farmList[i].FarmName + '</label><br />';
                        } else {
                            farmChecklistHtml += '<label><input type="checkbox" id="' + farmList[i].FarmId + '">' + farmList[i].FarmName + '</label><br />';
                        }
                    } else {
                        farmChecklistHtml += '<label><input type="checkbox" id="' + farmList[i].FarmId + '">' + farmList[i].FarmName + '</label><br />';
                    }
                }
            }
        }
    })).then(function () { $('#adminUserFarmsList').empty().html(farmChecklistHtml); editUserFarms(userID); });
}

function editUserFarms(userID) {
    $('#adminUserFarmsList :checkbox').unbind().click(function () {
        var farmID = $(this).attr('id'), state = $(this).is(':checked'), searchQuery = { "key": _key, "userID": userID, "farmId": farmID, "AddRemove": state }; data = JSON.stringify(searchQuery);
        $.ajax('../api/User/UserAddOrRemoveFarm', { 
            type: 'PUT', 
            data: data, 
            success: function (msg) {
                localStorage['CT_key'] = msg['Key']; 
                startTimer(msg['Key']);
            }
        });
    });
}

function editUserRoles(userID) {
    $('#adminUserRolesList :checkbox').unbind().click(function(){
        var roleID = $(this).attr('id'), state = $(this).is(':checked'), searchQuery = { "key": _key, "userID": userID, "roleId": roleID, "AddRemove": state }, data = JSON.stringify(searchQuery);
        $.ajax('../api/User/UserAddOrRemoveRole', { 
            type: 'PUT', 
            data: data, 
            success: function (msg) {
                localStorage['CT_key'] = msg['Key']; 
                startTimer(msg['Key']); 
                console.log(msg);
            }
        });
    });
}

function closeAdminModal() { $('.admin-modal, #lightboxBG').fadeOut('100', function () { $('#lightboxBG').remove(); }); }

var newCompanyID, newFarmID;
function initialSetup() {
    $('.submit').unbind().click(function (e) {
        e.preventDefault();
        var allRequired = true, submitObject = { "key": _key }, formType = $(this).attr('id'), currentForm = $(this).parents('form'), errorMessage = ''; 

        if(currentForm.is('#newUser')) {
            // validate fields marked as required:
            $('input[type="text"], select', currentForm).each(function () {
                $(this).removeAttr('style');
                if ($(this).attr('required')) {
                    if ($(this).val() == "") {
                        $(this).attr('placeholder', "** All fields required **").css({ 'border-color': '#b32c31', 'font-weight': 'bold', 'background-color': '#fafaca' });
                        allRequired = false;
                    }
                }
            });

            if ($('#userID').length && $('#userID').val() == "-1") {
                if ($('#password').val() != $('#passConfirm').val()) {
                    errorMessage += "Password fields do not match<br />";
                    $('#password, #passConfirm').val();
                    allRequired = false;
                } else if ($('#password').val().length < 8) {
                    errorMessage += "Password must be at least 8 characters<br />";
                    $('#password, #passConfirm').val();
                    allRequired = false;
                }
            }
            if ($('#emailAddress').length && !validateEmail($("#emailAddress").val())) {
                errorMessage += "Email must be a valid address<br />";
                allRequired = false;
            }
            if ($('#phone').length && !validatePhone($("#phone").val())) {
                errorMessage += "Phone must be in the correct format<br />";
                allRequired = false;
            }

            function validatePhone(phone) {
                var re = /(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:(\s*([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]‌​)\s*)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)([2-9]1[02-9]‌​|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})/;
                return re.test(phone);
            }
            function validateEmail(email) {
                var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                return re.test(email);
            }

            if(errorMessage != '') alertError(errorMessage);
        }

        // if form validates:
        if (allRequired) {
            showProgress('body', 'submitForm');
            $('input:not(:checkbox), select', currentForm).each(function () {
                var term = $(this).attr('id'), def = $(this).val();
                if ($(this).attr('type') == 'checkbox') { def = $(this).is(':checked') ? "true" : "false"; }
                if (def != "") { def == "on" ? def = true : def = def; submitObject[term] = def; }
            });
            if (formType != "submitCompany") submitObject['CompanyId'] = newCompanyID;
            if (formType == "submitFarm") {
                submitObject['StatusId'] = $('#farmStatusID').val();
                submitObject['InnovaName'] = $('#innovaName').val();
                submitObject['InnovaCode'] = $('#innovaCode').val();
            }
            if (formType == "submitPond") {
                submitObject['StatusId'] = $('#pondStatusID').val();
                submitObject['FarmID'] = newFarmID;
                submitObject['InnovaName'] = $('#innovaName').val();
                submitObject['InnovaCode'] = $('#innovaCode').val();
            }
            var data = JSON.stringify(submitObject);

            switch (formType) {
                case "submitCompany":
                    $.ajax('../api/Utilities/CompanyAddOrEdit', {
                        type: 'PUT', data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key'];
                            newCompanyID = msg['CompanyId'];
                            startTimer(msg['Key']);
                            $('#newCompany').animate({ 'left': '-960px' }, function () {
                                $('#newUser').show().animate({ 'left': '0' });
                                $('#newCompany').hide().css('left', '960px');
                            });
                            hideProgress('submitForm');
                        }
                    }); break;
                case "submitFarm":
                    $.ajax('../api/Farm/FarmAddOrEdit', {
                        type: 'PUT', data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key'];
                            newFarmID = msg['FarmId'];
                            startTimer(msg['Key']);
                            $('#newFarm').animate({ 'left': '-960px' }, function () {
                                $('#newPond').show().animate({ 'left': '0' });
                                $('#newFarm').hide().css('left', '960px');
                            });
                            hideProgress('submitForm');
                        }
                    }); break;
                case "submitPond":
                    $.ajax('../api/Pond/PondAddOrEdit', {
                        type: 'PUT', data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key'];
                            startTimer(msg['Key']);
                            window.location = "admin-user.html";
                            hideProgress('submitForm');
                        }
                    }); break;
                case "submitUser":
                    $.when($.ajax('../api/User/UserAddOrEdit', {
                        type: 'PUT',
                        data: data,
                        success: function (msg) {
                            localStorage['CT_key'] = msg['Key']; startTimer(msg['Key']);
                            submitObject['userID'] = (msg['UserId']);
                            data = JSON.stringify(submitObject);
                        }
                    })).then(function () {
                        if (data["password"] != "") {
                            $.ajax('../api/User/SetPassword', {
                                type: 'PUT',
                                data: data,
                                success: function (msg) {
                                    localStorage['CT_key'] = msg['Key']; 
                                    startTimer(msg['Key']);
                                    $('#newUser').animate({ 'left': '-960px' }, function () {
                                        $('#newFarm').show().animate({ 'left': '0' });
                                        $('#newUser').hide().css('left', '960px');
                                    });
                                    hideProgress('submitForm');
                                }
                            });
                        } else {
                            resetUserModal(); closeAdminModal(); populateUsers(); hideProgress('submitForm');
                        }
                    });
                    break;
                default: console.log("ERROR: Form has no type: " + formType); hideProgress('submitForm'); break;
            }
        }
    });
}

function resendReport() {
    var calFlag, date, addOrEdit, date, i, tdate = getTodaysMonth(), startDateMonth = tdate.month, startDateYear = tdate.year, chosenDate;
    $('#reportDate').unbind().click(function () {
        if (calFlag != "created") loadCalendar(startDateMonth, startDateYear);
    });

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
                    hideProgress();
                    $('#calendarModal').modal();
                });
            },
            dayClick: function () {
                showProgress('body');
                chosenDate = $(this).data('date').replace(/-/g, '');
                var sDate = moment($(this).data('date')).subtract(1, "days").format("YYYYMMDD");
                $.get('http://dashboards.harvestselect.com/api/utilities/EmailDailyReport?sDate='+sDate+'&eDate='+chosenDate).done(function() {
                    alert( "Report successfully sent." );
                })
                .fail(function() {
                    alert( "Error occurred while sending report." );
                })
                .always(function() {
                    $('#calendarModal').modal('hide');
                    hideProgress();
                });
            }
        });
    }


    function getTodaysMonth() {
        $('.row.buttons').css('opacity', 0);
        var today = new Date(), todaysMonth = today.getMonth() + 1, todaysYear = today.getFullYear();
        if (todaysMonth < 10) todaysMonth = "0" + todaysMonth;
        var tdate = new Object();
        tdate.month = todaysMonth;
        tdate.year = todaysYear;
        return tdate;
    }
}