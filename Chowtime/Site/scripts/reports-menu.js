var SGApp = SGApp || {};
SGApp.reportsMenu = (function(){
    var $getDataButton,
        $ddlFarmPonds,
        $keithsDataDiv,
        $pieChart,
        allData = [],
        farmPonds = [],
        farmPondNames = [],
        drillDownData = [];

    function loadDataSources() {
        var a = [],
            weights = [],
            categories = [],
            ranges = _(allData).groupBy(function (value) {
                return value.rangeName;
            }),            
            rangeNames = [];
        var pondSelected = $ddlFarmPonds.val(),
            dataArray = [],
            drillDownSeries = [];
        
        if (ranges !== undefined) {
            rangeNames = _(ranges).keys();
            
            switch (pondSelected) {
                case "All":
                    var totWt = 1;
                    var totPondWt = _(allData).reduce(function (memo, num) { return memo + num.weight; }, 0);
                    if (totPondWt > 0) { totWt = totPondWt };
                    _(rangeNames).each(function (value) {
                        var wt = _(allData).reduce(function (memo, valu) {
                            if (valu.rangeName === value) {
                                return memo + valu.weight;
                            } else {
                                return memo;
                            }
                        }, 0);
                        var wtper = Math.round((wt / totWt) * 100);
                        var obj = {
                            name: value,
                            y: wtper,
                            drilldown: value.toLowerCase()
                        }
                        dataArray.push(obj);
                        dataArray = _.sortBy(dataArray, "name");
                        var ddObjArray = [];
                        _(farmPondNames).each(function (val) {
                            var arr = _(allData).filter(function (valu) {
                                return valu.rangeName === value && valu.farmPond === val;
                            });
                            var wt = 0;
                            if (arr !== undefined) {
                                _(arr).each(function (valu) {
                                    wt += valu.weight;
                                });
                            }
                            if (wt > 0) {
                                var ddo = [
                                    val, wt
                                ]
                                ddObjArray.push(ddo);
                            }
                        });
                        var ddObj = {
                            id: value.toLowerCase(),
                            data: ddObjArray
                        }
                        drillDownSeries.push(ddObj);
                    });
                    $keithsDataDiv.highcharts({
                        chart: {
                            zoomType: 'xy', alignTicks: false, type: "column"
                        },
                        title: { text: "Live Fish Sampling" },
                        xAxis: [{ categories: categories, labels: { enabled: true } }],
                        yAxis: [{ labels: { format: '{value}%', style: { color: '#89A54E' } }, title: { text: "Percent" } }],
                        plotOptions: { column: { stacking: null } },
                        series: [{
                            name: 'Farm Ponds',
                            type: "column",
                            data: dataArray
                        }],
                        drilldown: {
                            series: drillDownSeries
                        }
                    });
                    _(ranges).each(function (value) {
                        var tot = 0;
                        var selectedFP = $ddlFarmPonds.val();
                        if (selectedFP === "All") {
                            _(value).each(function (x) {
                                tot += _(x).reduce(function (memo, val) {
                                    categories.push(val.farmPond);
                                    return val;
                                }, 0);
                            });
                            var obj = {
                                name: value[0].rangeValue,
                                y: tot,
                                drilldown: value[0].farmPond
                            };
                            weights.push(obj.y);
                            a.push(obj);
                            //
                        } else {
                            _(value).each(function (x) {
                                if (x.farmPond === selectedFP) {
                                    tot += _(x).reduce(function (memo, val) {
                                        return val;
                                    }, 0);
                                }
                            });
                            var obj = {
                                name: value[0].rangeValue,
                                y: tot,
                                drilldown: value[0].rangeValue
                            };
                            weights.push(obj.y);
                            a.push(obj);
                            //categories.push(obj.drilldown);
                        }
                    });

                    $pieChart.highcharts({
                        chart: { plotBackgroundColor: null, plotBorderWidth: null, plotShadow: false, type: 'pie' },
                        title: { text: 'Sampling Percentages' },
                        tooltip: { pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>' },
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
                            data: dataArray
                        }],
                        drilldown: {
                            series: drillDownSeries
                        }
                    });
                    break;
                default:
                    var totWt = 1;
                    var totPondWt = _(allData).reduce(function (memo, num) {
                        if (num.farmPond === pondSelected) {
                            return memo + num.weight;
                        } else {
                            return memo;
                        }
                    }, 0);
                    if (totPondWt > 0) { totWt = totPondWt };
                    _(rangeNames).each(function (value) {
                        var wt = _(allData).reduce(function (memo, valu) {
                            if (valu.rangeName === value && valu.farmPond === pondSelected) {
                                return memo + valu.weight;
                            } else {
                                return memo;
                            }
                        }, 0);
                        var wtper = Math.round((wt / totWt) * 100);
                        var obj = {
                            name: value,
                            y: wtper
                        }
                        dataArray.push(obj);
                        dataArray = _.sortBy(dataArray, "name");
                        
                        $keithsDataDiv.highcharts({
                            chart: {
                                zoomType: 'xy', alignTicks: false, type: "column"
                            },
                            title: { text: "Live Fish Sampling" },
                            xAxis: [{ categories: categories, labels: { enabled: true } }],
                            yAxis: [{ labels: { format: '{value} %', style: { color: '#89A54E' } }, title: { text: "Percent" } }],
                            plotOptions: { column: { stacking: null } },
                            series: [{
                                name: 'Farm Ponds',
                                type: "column",
                                data: dataArray
                            }]
                        });
                        //_(ranges).each(function (value) {
                        //    var tot = 0;
                        //    var selectedFP = $ddlFarmPonds.val();
                        //    if (selectedFP === "All") {
                        //        _(value).each(function (x) {
                        //            tot += _(x).reduce(function (memo, val) {
                        //                return val;
                        //            }, 0);
                        //        });
                        //        var obj = {
                        //            name: value[0].rangeValue,
                        //            y: tot,
                        //            drilldown: value[0].farmPond
                        //        };
                        //        weights.push(obj.y);
                        //        a.push(obj);
                        //        categories.push(obj.name);
                        //    } else {
                        //        _(value).each(function (x) {
                        //            if (x.farmPond === selectedFP) {
                        //                tot += _(x).reduce(function (memo, val) {
                        //                    return val;
                        //                }, 0);
                        //            }
                        //        });
                        //        var obj = {
                        //            name: value[0].rangeValue,
                        //            y: tot,
                        //            drilldown: value[0].rangeValue
                        //        };
                        //        weights.push(obj.y);
                        //        a.push(obj);
                        //        categories.push(obj.name);
                        //    }
                        //});

                        $pieChart.highcharts({
                            chart: { plotBackgroundColor: null, plotBorderWidth: null, plotShadow: false, type: 'pie' },
                            title: { text: 'Sampling Percentages' },
                            tooltip: { pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>' },
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
                                data: dataArray
                            }]
                        });
                    });
                    break;
            }
        }
        
    }

    function loadHighCharts() {
        //debugger;
        loadDataSources();
    }

    function wireUpEvents(){
        $getDataButton.on("click", function () {
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
                    var fpDDLhtml = '<option selected value="All">All</option>';
                    allData = JSON.parse(msg);
                    farmPonds = _(allData).groupBy(function (value) {
                        return value.farmPond;
                    });
                    if (farmPonds !== undefined) {
                        var ponds = _(farmPonds).keys();
                        farmPondNames = _.sortBy(ponds, function (item) {
                            return item;
                        });
                        _(farmPondNames).each(function (value) {
                            fpDDLhtml += "<option value='" + value + "'>" + value + "</option>";
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
        $("#rangeCW").on("click", function () {
            
            $("#startDateInput").val(moment().startOf("week").format("YYYY-MM-DD"));
            $("#endDateInput").val(moment().endOf("week").format("YYYY-MM-DD"));
        });
        $("#rangeLW").on("click", function () {            
            $("#startDateInput").val(moment().startOf("week").subtract(1, "weeks").format("YYYY-MM-DD"));
            $("#endDateInput").val(moment().startOf("week").subtract(1, "weeks").endOf("week").format("YYYY-MM-DD"));
        });
        $("#rangeCM").on("click", function () {
            $("#startDateInput").val(moment().startOf("month").format("YYYY-MM-DD"));
            $("#endDateInput").val(moment().endOf("month").format("YYYY-MM-DD"));
        });
        $("#rangeLM").on("click", function () {
            $("#startDateInput").val(moment().startOf("month").subtract(1, 'months').format("YYYY-MM-DD"));
            $("#endDateInput").val(moment().startOf("month").subtract(1, 'months').endOf("month").format("YYYY-MM-DD"));
        });
        $("#rangeLQ").on("click", function () {
            $("#startDateInput").val(moment().subtract(1, 'quarters').startOf("quarter").format("YYYY-MM-DD"));
            $("#endDateInput").val(moment().subtract(1, 'quarters').endOf("quarter").format("YYYY-MM-DD"));
        });
        $("#rangeYTD").on("click", function () {
            $("#startDateInput").val(moment().startOf("year").format("YYYY-MM-01"));
            $("#endDateInput").val(moment().format("YYYY-MM-DD"));
        });
    }
    (function(){
        $getDataButton = $("#getDataButton");
        $ddlFarmPonds = $("#ddlFarmPonds");
        $keithsDataDiv = $("#keithsDataDiv");
        $pieChart = $("#pieChart");
        wireUpEvents();
    }());

}());