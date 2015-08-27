var SGApp = SGApp || {};
SGApp.reportsMenu = (function(){
    var $button,
        $ddlFarmPonds,
        allData = [],
        farmPonds = [];

    function loadHighCharts() {
        var a = [];
        var weights = [];
        var categories = [];
        var ranges = _(allData).groupBy(function (value) {
            return value.rangeName;
        });
        if (ranges !== undefined) {
            _(ranges).each(function (value) {
                var tot = 0;
                var selectedFP = $ddlFarmPonds.val();
                _(value).each(function (x) {
                    if (x.farmPond === selectedFP) {
                        tot += _(x).reduce(function (memo, val) {
                            return val;
                        }, 0);
                    }
                });
                //var tot = _(value).reduce(function (memo, val) {
                //    return val.weight;
                //}, 1);
                var obj = {
                    name: value[0].rangeValue,
                    y: tot,
                    drilldown: value[0].rangeValue
                };
                weights.push(obj.y);
                a.push(obj);
                categories.push(obj.name);
            });
        }
        var html = "";
        var $keithsDataDiv = $("#keithsDataDiv");
        var pieChart = $("#pieChart");
        $keithsDataDiv.highcharts({
            chart: {
                zoomType: 'xy', alignTicks: false, type: "column"
            },
            title: { text: "Live Fish Sampling" },
            xAxis: [{ categories: categories, labels: { enabled: true } }],
            yAxis: [{ labels: { format: '{value}', style: { color: '#89A54E' } }, title: { text: "weight" } }],
            plotOptions: { column: { stacking: null } },
            series: [{ name: 'Farm Ponds', data:  weights}]
        });
        pieChart.highcharts({
            chart: {plotBackgroundColor: null, plotBorderWidth: null, plotShadow: false, type: 'pie'},
            title: {text: 'Sampling Percentages'},
            tooltip: {pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'},
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true
                }
            },
            series: [{
                name: "Ranges",
                colorByPoint: true,
                data: a
            }]
        });    
    }

    function wireUpEvents(){
        $button.on("click", function () {
            var qry = {
                "Key": "Hello World",
                "startDate": $("#startDateInput").val(),
                "endDate": $("#endDateInput").val()
            };
            var address = "../api/JMremote/GetRemoteData";
            var data = JSON.stringify(qry);
            $.ajax(address, {
                type: 'POST',
                data: data,
                success: function (msg) {
                    var a = [];
                    var fpDDLhtml = "";
                    allData = JSON.parse(msg);
                    a = _(allData).groupBy(function (value) {
                        return value.farmPond;
                    });
                    if (a !== undefined) {
                        _(a).each(function (value) {
                            farmPonds.push(value[0].farmPond);
                            if (fpDDLhtml.length === 0) {
                                fpDDLhtml += "<option selected value='" + value[0].farmPond + "'>" + value[0].farmPond + "</option>";
                            } else {
                                fpDDLhtml += "<option value='" + value[0].farmPond + "'>" + value[0].farmPond + "</option>";
                            }
                        })
                    };
                    $ddlFarmPonds.empty().html(fpDDLhtml);
                    //var groups = _(dta).groupBy(function (value) {
                    //    return value.farmPond + value.rangeName;
                    //});
                    //if (groups !== undefined) {
                    //    _(groups).each(function (value) {
                    //        var tot = _(value).reduce(function (memo, val) {
                    //            return memo + val.weight;
                    //        }, 0);
                    //        var obj = {
                    //            farmPond: value[0].farmPond,
                    //            rangeName: value[0].rangeValue,
                    //            weight: tot
                    //        };
                    //        weights.push(obj.weight);
                    //        a.push(obj);
                    //        categories.push(obj.farmPond + ' ' + obj.rangeName);
                    //    });
                    //}
                    loadHighCharts();
                }
            });
        });
        $ddlFarmPonds.off("change").on("change", function () {
            loadHighCharts();
        });
    }
    (function(){
        $button = $("#getDataButton");
        $ddlFarmPonds = $("#ddlFarmPonds");
        wireUpEvents();
    }());

}());