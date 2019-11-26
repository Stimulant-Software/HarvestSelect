﻿(function () {
	var $adminBins_modalHeader;
	var $adminBins_header;
	var $adminBinsBody;
	var $addNewBin;
	var $addNewBinModal;
	var $binName;
	var $adminBins_farmSelect;
	var $adminBins_currentTicket;
	var $adminBins_currentPounds;
	var $adminBins_lastDisbursement;
	var $adminBins_lastLoaded;
	var $adminBins_reconciliation;
	var $submitBin;
	var $binReconciliation;
	var $adminBins_cancelBinCreation;
	var $adminBins_noActiveBins;
	var farmList = [];
	var binList = [];
    var bin = {};
    var $farmSelectHTML;
    var $addNewFarmSelect;

	function centerModal($addNewBinModal) {
		var windowHeight = window.innerHeight, windowWidth = window.innerWidth,
			modalHeight = $addNewBinModal.innerHeight(),
			modalWidth = $addNewBinModal.innerWidth();
		var modalTop = (windowHeight - modalHeight) / 2,
			modalLeft = (windowWidth - modalWidth) / 2;
		$addNewBinModal.css({ 'top': /* modalTop */ '100px', 'left': modalLeft });
	}

	function editBin() {
		$adminBins_modalHeader.text("Edit Bin");
		$binName.val(bin.BinName);
        $adminBins_farmSelect.empty();
        var farmIds = bin.FarmIDs;
        farmIds = farmIds.substring(1);
        farmIds = farmIds.substring(0, farmIds.length - 1);
        var res = farmIds.split(",");

        var arrayLength = res.length;
        for (var i = 0; i < arrayLength; i++) {
            var selHTML = '<select class="farmSelect" style="padding: 3px; margin: 8px auto;" id=fselect' + res[i] + '>';
            selHTML += $farmSelectHTML;
            selHTML += '</select>';


            $adminBins_farmSelect.append('<br />');


            $adminBins_farmSelect.append(selHTML);
            $('#fselect' + res[i]).val(res[i]);

        }
		$adminBins_currentTicket.val(bin.CurrentTicket);
		$adminBins_currentPounds.val(bin.CurrentPounds).attr("readonly", true);
		$adminBins_lastDisbursement.val(bin.LastDisbursement);
		$adminBins_lastLoaded.val(bin.LastLoaded);
		$binReconciliation.show();
		showBin();
		$submitBin.off().on("click",
			function(e) {
				e.preventDefault();
				var qry = JSON.stringify({
					"Key": localStorage.getItem("CT_key"),
					"BinID": bin.BinID,
					"BinName": $binName.val(),
					"FarmID": $adminBins_farmSelect.val() > 0 ? parseInt($adminBins_farmSelect.val()) : null,
					"CurrentTicket": $adminBins_currentTicket.val().length > 0 ? parseInt($adminBins_currentTicket.val()) : null,
					"LastDisbursement": $adminBins_lastDisbursement.val().length > 0 ? $adminBins_lastDisbursement.val() : null,
					"LastLoaded": $adminBins_lastLoaded.val().length > 0 ? $adminBins_lastLoaded.val() : null,
					"Reconciliation": $adminBins_reconciliation.val().length > 0 ? parseInt($adminBins_reconciliation.val()): null
				});
				$.ajax('../api/Farm/AddOrEditBin', {
					type: 'POST',
					data: qry,
					success: function (msg) {
						localStorage['CT_key'] = msg['Key'];
						startTimer(msg['Key']);
						bin = {};
						_(binList).find(function(value) {
							if (value.BinID.toString() === msg["BinID"].toString()) {
								value.BinName = msg["BinName"];
								value.FarmID = msg["FarmID"];
								value.CurrentTicket = msg["CurrentTicket"];
								value.CurrentPounds = msg["CurrentPounds"];
								value.LastDisbursement = msg["LastDisbursement"];
								value.LastLoaded = msg["LastLoaded"];
							}
						});
						closeAdminModal();
						reloadBinListItems();
					}
				});
			}
		);
	}

	function getFarmName(farmId) {
		var farmName = "";
		_(farmList).find(function(value) {
			if (value.FarmId.toString() === farmId.toString()) {
				farmName = value.FarmName;
			}
		});
		return farmName;
	}
    function getFarmNameList(farmIds) {
        var farmName = "";
        farmIds = farmIds.substring(1);
        farmIds = farmIds.substring(0, farmIds.length - 1);
        var res = farmIds.split(",");

        var arrayLength = res.length;
        for (var i = 0; i < arrayLength; i++) {
            _(farmList).find(function (value) {
                if (value.FarmId.toString() === res[i].toString()) {
                    if (farmName == "") {
                        farmName = value.FarmName;
                    }
                    else {
                        farmName += ", " + value.FarmName;
                    }

                }
            });
        }

        return farmName;
    }
	function loadBins() {
		var data = JSON.stringify({
			"Key": localStorage.getItem("CT_key"),
			"BinID": "1"
		});
		$.when($.ajax('../api/Farm/GetBinsForAdminPage', {
				type: 'POST',
				data: data,
				success: function(msg) {
					localStorage['CT_key'] = msg['Key'];
					startTimer(msg['Key']);
					binList = msg['Bins'];
				}
		})).then(function () {
			reloadBinListItems();
			//var html = '';
			//if (binList.length > 0) {
			//	$adminBins_noActiveBins.remove();
			//}
			//_(binList).each(function(value) {
			//	html += '<li data-binid="' + value.BinID + '">';
			//	html += '<span>' + value.BinName + '</span>';
			//	html += '<span style="margin-left:10px;width:250px;">' + "Assigned to Farm:  " + getFarmName(value.FarmID) + '</span>';
			//	html += '<input data-binid="' + value.BinID + '" type="button" value="Edit" /></li>';
			//});
			//$('ol.active').append(html);
			//$('ol.active').find("input").each(function () {
			//	var $this = $(this);
			//	$this.off().on("click",
			//		function (e) {
			//			var binId = $this.data("binid");
			//			var qry = JSON.stringify({
			//				"Key": localStorage.getItem("CT_key"),
			//				"BinID": binId
			//			});
			//			$.when($.ajax('../api/Farm/GetBinInfo', {
			//				type: 'POST',
			//				data: qry,
			//				success: function (msg) {
			//					localStorage['CT_key'] = msg['Key'];
			//					startTimer(msg['Key']);
			//					bin = msg.Bins[0];
			//				}
			//			})).then(function () {
			//				editBin();
			//			});
			//		}
			//	);
			//});
		});
	}

	function loadFarms() {
		if (farmList.length === 0) {
			var data = JSON.stringify({
				"Key": localStorage.getItem("CT_key"),
				"StatusId": "1"
			});
			$.when($.ajax('../api/Farm/GetAllFarms', {
				type: 'POST',
				data: data,
				success: function (msg) {
					localStorage['CT_key'] = msg['Key'];
					startTimer(msg['Key']);
					farmList = msg['ReturnData'];
				}
			})).then(function () {
				var html = '<option value="0">Select Farm</option>';
				_(farmList).each(function (value) {
					html += '<option value="' + value.FarmId + '">' + value.FarmName + '</option>';
				});
                $farmSelectHTML = html;
				wireUpEvents();
				loadBins();
			});
		} else {
			var html = '<option>Select Farm</option>';
			_(farmList).each(function (value) {
				html += '<option value="' + value.FarmId + '">' + value.FarmName + '</option>';
			});
            $farmSelectHTML = html;
		}
	}

	function reloadBinListItems() {
		if (binList.length > 0) {
			$adminBins_noActiveBins.remove();
		}
		$("ol.active li").each(function () {
			var $this = $(this);
			if ($this.attr("id") !== "adminBins_header") {
				$this.remove();
			}
		});
		var html = '';
        _(binList).each(function (value) {
            html += '<li data-binid="' + value.BinID + '">';
            html += '<span>' + value.BinName + '</span>';
            html += '<span style="margin-left:10px;width:250px; overflow:auto; max-height: 100px;">' + getFarmNameList(value.FarmIDs) + '</span>';
            html += '<input data-binid="' + value.BinID + '" type="button" value="Edit" /></li>';
        });
		$('ol.active').append(html);
		$('ol.active').find("input").each(function () {
			var $this = $(this);
			$this.off().on("click",
				function (e) {
					var binId = $this.data("binid");
					var qry = JSON.stringify({
						"Key": localStorage.getItem("CT_key"),
						"BinID": binId
					});
					$.when($.ajax('../api/Farm/GetBinInfo', {
						type: 'POST',
						data: qry,
						success: function (msg) {
							localStorage['CT_key'] = msg['Key'];
							startTimer(msg['Key']);
							bin = msg.Bins[0];
						}
					})).then(function () {
						editBin();
					});
				}
			);
		});
	}

	function showBin() {
		$adminBinsBody.append('<div id="lightboxBG" class="modal"></div>');
		$('#lightboxBG').fadeIn('100',
			function () {
				centerModal($addNewBinModal);
				$addNewBinModal.show().fadeIn('100');
			}
		);
	}

	function wireUpEvents() {
		$addNewBin.off().on("click",
			function(e) {
				e.preventDefault();
				$adminBins_modalHeader.text("New Bin");
				$adminBins_currentPounds.attr("readonly", false);
				showBin();				
			}
        );
        $addNewFarmSelect.off().on("click",
            function (e) {
                e.preventDefault();
                var selHTML = '<select class="farmSelect" style="padding: 3px; margin: 8px auto;">';
                selHTML += $farmSelectHTML;
                selHTML += '</select>';
                $adminBins_farmSelect.append(selHTML);
            }
        );
		$submitBin.off().on("click",
			function(e) {
                e.preventDefault();
                var binFarms = [];
                $('.farmSelect').each(function (e) {
                    if ($(this).val() != "0") {
                        binFarms.push($(this).val());
                    }
                });
				var qry = JSON.stringify({
					"Key": localStorage.getItem("CT_key"),
					"BinID": -1,
                    "BinName": $binName.val(),
                    "BinFarms": binFarms,
					"FarmID": $adminBins_farmSelect.val() > 0 ? parseInt($adminBins_farmSelect.val()) : null,
					"CurrentTicket": $adminBins_currentTicket.val().length > 0 ? parseInt($adminBins_currentTicket.val()) : null,
					"CurrentPounds": $adminBins_currentPounds.val().length > 0 ? parseInt($adminBins_currentPounds.val()) : null,
					"LastDisbursement": $adminBins_lastDisbursement.val().length > 0 ? $adminBins_lastDisbursement.val() : null,
					"LastLoaded": $adminBins_lastLoaded.val().length > 0 ? $adminBins_lastLoaded.val() : null
				});
				$.when($.ajax('../api/Farm/AddOrEditBin', {
					type: 'POST',
					data: qry,
					success: function (msg) {
						localStorage['CT_key'] = msg['Key'];
						startTimer(msg['Key']);
						farmList = msg['ReturnData'];
					}
				})).then(function () {
					var html = '<option value="0">Select Farm</option>';
					_(farmList).each(function (value) {
						html += '<option value="' + value.FarmId + '">' + value.FarmName + '</option>';
					});
                    $farmSelectHTML = html;
                    closeAdminModal();
                    reloadBinListItems();
				});
			}
		);
	}

	(function () {
		$adminBinsBody = $("#adminBinsBody");
		$adminBins_header = $("#adminBins_header");
		$adminBins_modalHeader = $("#adminBins_modalHeader");
		$addNewBin = $("#addNewBin");
		$addNewBinModal = $("#addNewBinModal");
		$binName = $("#binName");
        //$adminBins_farmSelect = $("#adminBins_farmSelect");
        $adminBins_farmSelect = $("#farmSelects");
		$adminBins_currentTicket = $("#adminBins_currentTicket");
		$adminBins_currentPounds = $("#adminBins_currentPounds");
		$adminBins_lastDisbursement = $("#adminBins_lastDisbursement");
		$adminBins_lastLoaded = $("#adminBins_lastLoaded");
		$adminBins_reconciliation = $("#adminBins_reconciliation");
		$binReconciliation = $("#binReconciliation");
		$binReconciliation.hide();
        $submitBin = $("#submitBin");
        $addNewFarmSelect = $('#addAnotherFarm');
		$adminBins_cancelBinCreation = $("#adminBins_cancelBinCreation");
		$adminBins_noActiveBins = $("#adminBins_noActiveBins");
		loadFarms();
	})();
})()