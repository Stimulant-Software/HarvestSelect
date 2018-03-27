(function () {
	var $binLoads_mainContent;
	var $binLoads_currentFarm;
	var $binLoads_changeFarm;
	var $binsContainer;
	var $binLoads_currentTicket;
	var $binLoads_currentPounds;
	var $binLoads_lastDisbursement;
	var $binLoads_lastLoaded;
	var $binLoads_viewHistory;
	var $binData;
	var $binLoads_currentBin;
	var $binLoadData;
	var $alertBox;
	var $binLoads_newTicketNumber;
	var $binLoads_dateLoaded;
	var $binLoads_poundsLoaded;
	var $binLoads_vendor;
	var $binLoads_note;
	var $binLoads_priorHistory;
	var $binLoads_save;
	var $binLoads_historyTable;
	var $binLoads_movePrevious;
	var $binLoads_moveNext;
	var farmList = [];
	var binList = [];
	var binLoads = [];
	var bin = {};
	var loadSkipCount = 0;

	function clearBinLoadInputControls() {
		$binLoads_newTicketNumber.val("");
		$binLoads_dateLoaded.val("");
		$binLoads_poundsLoaded.val("");
		$binLoads_vendor.val("");
		$binLoads_note.val("");
	}

	function clearHistoryTable() {
		$binLoads_historyTable.find("tbody").empty();
	}

	function loadBins(farmId) {
		var html = '<option>Select Bin</option>';
		_(binList).each(function(value) {
			if (value.FarmID.toString() === farmId.toString()) {
				html += '<option value="' + value.BinID + '">' + value.BinName + '</option>';
			}
		});
		$binLoads_currentBin.empty().html(html).off().on("change",
			function () {
				var val = $(this).val();
				$binsContainer.show();
				var bin = _(binList).find(function (value) {
					if (value.BinID === val) {
						return value;
					}
					return false;
				});
				$binLoads_currentTicket.text(bin.CurrentTicket);
				$binLoads_currentPounds.text(bin.CurrentPounds);
				$binLoads_lastDisbursement.text(bin.LastDisbursement);
				$binLoads_lastLoaded.text(bin.LastLoaded);
				$binLoadData.show();
				clearHistoryTable();
			});
	}

	function loadFarms() {
		showProgress($binLoads_mainContent);
		var data = JSON.stringify({
			"Key": localStorage.getItem("CT_key"),
			"StatusId": "1"
		});
		$.when($.ajax('../api/Farm/FarmsForBinLoads', {
				type: 'POST',
				data: data,
				success: function(msg) {
					localStorage['CT_key'] = msg['Key'];
					startTimer(msg['Key']);
					farmList = msg['ReturnData'];
					binList = msg.Bins;
				}
		})).then(function () {
			var html = '<option>Select Farm</option>';
			_(farmList).each(function(value) {
				html += '<option value="' + value.FarmId + '">' + value.FarmName + '</option>';
			});
			$binLoads_changeFarm.empty().html(html);
			$binLoads_changeFarm.off().on("change",
				function () {
					loadBins($(this).val());
					$binData.show();					
					setCurrentFarm($(this).val());
				}
			);
			hideProgress();
		});
	}

	function setCurrentFarm(val) {
		_(farmList).find(function(value) {
			if (value.FarmId.toString() === val.toString()) {
				$binLoads_currentFarm.text(value.FarmName);
			}
		});
	}

	function wireUpEvents() {
		$binLoads_viewHistory.off().on("click",
			function (e) {
				e.preventDefault();
				$binLoads_priorHistory.show();
				showProgress("#binLoads_priorHistory");
				var qry = JSON.stringify({
					"Key": localStorage.getItem("CT_key"),
					"Bin": {
						"BinID": parseInt($binLoads_currentBin.val())
					},
					"Skip": loadSkipCount
				});
				$.when($.ajax('../api/Farm/GetBinLoads', {
					type: 'POST',
					data: qry,
					success: function (msg) {
						localStorage['CT_key'] = msg['Key'];
						startTimer(msg['Key']);
						binLoads = msg["ReturnData"];
					}
				})).then(function () {
					var html = '';
					clearHistoryTable();
					_(binLoads).each(function(value) {
						html += '<tr><td>' + value.BinTicketID + '</td>';
						html += '<td>' + value.BinID + '</td>';
						html += '<td>' + value.TicketNumber + '</td>';
						html += '<td>' + value.DateLoaded + '</td>';
						html += '<td>' + value.PoundsLoaded + '</td>';
						html += '<td>' + value.Vendor + '</td>';
						html += '<td>' + value.Note + '</td>';
						html += '</tr>';
					});
					hideProgress();
					$binLoads_historyTable.find("tbody").html(html);
				});
			}
		);
		$binLoads_save.off().on("click",
			function (e) {
				e.preventDefault();				
				if ($binLoads_newTicketNumber.val().length === 0
					|| parseInt($binLoads_newTicketNumber.val()) <= 0) {
					alertError("Please enter a Ticket Number greater than 0 and try again");
					return;
				}
				if ($binLoads_dateLoaded.val().length === 0) {
					alertError("Please enter a value for 'Date Loaded' and try again");
					return;
				}
				if ($binLoads_poundsLoaded.val().length == 0
					|| parseInt($binLoads_poundsLoaded.val()) <= 0) {
					alertError("Please enter a value for 'Pounds Loaded' greater than 0 and try again");
					return;
				}
				var binId = parseInt($binLoads_currentBin.val());				
				var ticketId = parseInt($binLoads_newTicketNumber.val());
				var qry = JSON.stringify({					
					"Key": localStorage.getItem("CT_key"),
					"BinID": binId,
					"TicketNumber": ticketId,
					"DateLoaded": $binLoads_dateLoaded.val(),
					"PoundsLoaded": parseInt($binLoads_poundsLoaded.val()),
					"Vendor": $binLoads_vendor.val(),
					"Note": $binLoads_note.val()
				});
				$.when($.ajax('../api/Farm/AddBinLoad', {
					type: 'POST',
					data: qry,
					success: function (msg) {
						localStorage['CT_key'] = msg['Key'];
						startTimer(msg['Key']);
						bin = msg["Bins"][0];
						binLoads = msg["ReturnData"];
					}
				})).then(function () {
					var html = '';
					$binsContainer.show();
					$binLoads_currentTicket.text(bin.CurrentTicket);
					$binLoads_currentPounds.text(bin.CurrentPounds);
					$binLoads_lastDisbursement.text(bin.LastDisbursement);
					$binLoads_lastLoaded.text(bin.LastLoaded);
					$binLoadData.show();
					if ($binLoads_priorHistory.is(":visible")) {
						$binLoads_historyTable.find("tbody").empty();
						_(binLoads).each(function (value) {
							html += '<tr><td>' + value.BinTicketID + '</td>';
							html += '<td>' + value.BinID + '</td>';
							html += '<td>' + value.TicketNumber + '</td>';
							html += '<td>' + value.DateLoaded + '</td>';
							html += '<td>' + value.PoundsLoaded + '</td>';
							html += '<td>' + value.Vendor + '</td>';
							html += '<td>' + value.Note + '</td>';
							html += '</tr>';
						});
					}
					hideProgress();
					$binLoads_historyTable.find("tbody").html(html);
					clearBinLoadInputControls();
				});
			}
		);
	}


	(function () {
		$binLoads_mainContent = $("#binLoads_mainContent");
		$binLoads_currentFarm = $("#binLoads_currentFarm");
		$binLoads_changeFarm = $("#binLoads_changeFarm");
		$binsContainer = $("#binsContainer");
		$binLoads_currentTicket = $("#binLoads_currentTicket");
		$binLoads_currentPounds = $("#binLoads_currentPounds");
		$binLoads_lastDisbursement = $("#binLoads_lastDisbursement");
		$binLoads_lastLoaded = $("#binLoads_lastLoaded");
		$binLoads_viewHistory = $("#binLoads_viewHistory");
		$binData = $("#binData");
		$binLoads_currentBin = $("#binLoads_currentBin");
		$binLoadData = $("#binLoadData");
		$binLoads_newTicketNumber = $("#binLoads_newTicketNumber");
		$binLoads_dateLoaded = $("#binLoads_dateLoaded");
		$binLoads_poundsLoaded = $("#binLoads_poundsLoaded");
		$binLoads_vendor = $("#binLoads_vendor");
		$binLoads_note = $("#binLoads_note");
		$binLoads_priorHistory = $("#binLoads_priorHistory");
		$binLoads_priorHistory.hide();
		$binLoads_save = $("#binLoads_save");
		$binLoads_historyTable = $("#binLoads_historyTable");
		$binLoads_movePrevious = $("#binLoads_movePrevious");
		$binLoads_moveNext = $("#binLoads_moveNext");
		$alertBox = $("#alertBox");
		loadFarms();
		wireUpEvents();
	})();
})()