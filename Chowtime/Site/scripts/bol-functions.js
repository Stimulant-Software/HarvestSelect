var authorized = false, superAuthorized = false, userRoles = localStorage['CTuserRole'].split(' ');
if (userRoles.indexOf('BOL') > -1) authorized = true;
if (userRoles.indexOf('BOL') > -1) { authorized = true; superAuthorized = true; }

console.log(userRoles); debugger;

if (!authorized) window.location.href = "login.html";

/* BOL */
function bol() {
    $('#submitBOL').unbind().click(function (e) {
        e.preventDefault();
        openBolReport();
    }
    );
    function openBolReport() {
        showProgress('body');
        date = $(this).data('date');
        orderNumber = $('#ordernumber').val();
        searchQuery = { "Key": _key, "OrderCode": orderNumber }, data = JSON.stringify(searchQuery);

        $.ajax('../api/Utilities/GetBOLReport', {
            type: 'POST',
            data: data,
            success: function (msg) {
                var uri = 'data:application/pdf;charset=utf-8;base64,' + msg;
                var downloadLink = document.createElement("a");
                downloadLink.href = uri;
                downloadLink.target = "_blank";
                downloadLink.download = "BillOfLading.pdf";// "data.csv"; SHOULD BE  Project Name + Quote Number.pdf
                document.body.appendChild(downloadLink);
                downloadLink.click();
                document.body.removeChild(downloadLink);
                hideProgress();
            }
        });
    }

}