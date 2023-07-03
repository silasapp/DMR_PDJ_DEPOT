// error message
function ErrorMessage(error_id, error_message) {
	$(error_id).fadeIn('fast')
		.html("<div class=\"alert alert-danger\"><i class=\"fa fa-warning\"> </i> <span class=\"\"> " + error_message + " </span> </div>")
		.delay(9000)
		.fadeOut('fast');
	return;
}

// success message
function SuccessMessage(success_id, success_message) {
	$(success_id).fadeIn('fast')
		.html("<div class=\"alert alert-info\"> <i class=\"fa fa-check-circle\"> </i> <span class=\"\">" + success_message + " </span> </div>")
		.delay(10000)
		.fadeOut('fast');
	return;
}

$(document).ready(function () {
	//approve schedule
	
	// staff messages count
	$.post("/Staffs/MyMessageCount", {}, function (response) {
		$("#myMsgCount").text(response);
	});
	// application on staff desk count
	$.post("/Staffs/MyDeskCount", {}, function (response) {
		
		$("#MyDeskCount").text(response);
	});

	// schedules count for staff
	$.post("/Staffs/MyScheduleApprovalCount", {}, function (response) {
		$("#MyPendingScheduleCount").text(response);
	});
	//valid schedules count for staff
	$.post("/Staffs/MyValidScheduleCount", {}, function (response) {
		$("#MyValidScheduleCount").text(response);
	});

	// legacy application count
	$.post("/Staffs/LegacyAppsCount", {}, function (response) {
		$("#LegacyCount").text(response);
		$("#LegacyCount2").text(response);
	});


	$('body').on('focus', '.datePickerY', function () {
		//alert("Pick Year");
		$(this).datepicker({
			format: " yyyy",
			viewMode: "years",
			minViewMode: "years",
			autoclose: true,
			endDate: '+0d'
		});
	});

	$(".msg.closed").each(function(){
		var h = $(this).height();
		if(h >= 74){
			$(this).siblings("a.moreReport").removeClass("hide");
		}
	});

	$("a.moreReport").on("click", function(e){
		e.preventDefault();
		var report = $(this).siblings("p").html();
		var reportDate = $(this).parent().siblings(".foot-note").find(".when").text();
		var reportBy = $(this).parent().siblings(".foot-note").find(".by").text();
		
		$("#modalPopup").on('show.bs.modal', function () {
			var modal = $(this);
			modal.find('.modal-title').text("Report by: " + reportBy);
			modal.find('#modal-body').html(Loading());
			modal.find('#modal-body').html($("#x_modal").html());
			
			modal.find(".modal-body").html(report);
			modal.find(".modal-footer").html('<div class="pull-left">Report Date: ' + reportDate + '</div> <button class="btn" data-dismiss="modal" aria-hidden="true">Close</button>');
			
		});
		$("#modalPopup").modal('show');

		//var target = $(this).data("target");
		//$(target).toggleClass("closed");
		//alert($(this).text());
		//if($(this).text() === "Read more"){
			//$(this).text("Close");
		//}
		//else if($(this).text() === "Close"){
			//$(this).text("Read more");
		//}
	});

	$('body').on('focus', '.datePicker', function () {
		var box = $(this);
		var direction = box.attr("data-datedirection");

		if (direction == 'backward') {
			//alert("Back");
			box.datetimepicker({
				pickTime: false,
				autoclose: true,
				maxDate: new Date
			});
		}
		else if (direction == 'forward') {
			//alert("Forw");
			box.datetimepicker({
				pickTime: false,
				autoclose: true,
				minDate: new Date
			});
		}
		else {
			//alert("No hwere");
			$(this).datetimepicker({
				pickTime: false,
				autoclose: true
			});
		}
	});

	$('body').on('focus', '.dateTimePicker', function () {
		$(this).datetimepicker({
			//pickTime: true,
			//beforeShowDay: function (date) {
			//	return date.valueOf() >= now.valueOf();
			//},
			//autoclose: true,
			minDate: new Date
		});
	})


	$(".ctrl-menu div[data-toggle]").click(function () {
		var elem = $(this).attr("data-toggle");
		//alert(elem);
		if ($(this).attr("data-state") == "open") {
			$(this).attr("data-state", "closed");
			$(this).find("img").removeClass("down");
			$(this).find(".img").removeClass("down");
			$(elem).slideUp(300);
		}
		else {
			$(this).attr("data-state", "open");
			$(this).find("img").addClass("down");
			$(this).find(".img").addClass("down");
			$(elem).slideDown(300);
		}
	});

	$("a[href='#']").click(function (e) { e.preventDefault(); });

	$('body').on('focus', '.datePickerY', function () {
		//alert("Pick Year");
		$(this).datepicker({
			format: " yyyy",
			viewMode: "years",
			minViewMode: "years",
			autoclose: true,
			endDate: '+0d'
		});
	});

	$('body').on('focus', '.datePicker', function () {
		var box = $(this);
		var direction = box.attr("data-datedirection");

		if (direction === 'backward') {
			//alert("Back");
			box.datetimepicker({
				pickTime: false,
				autoclose: true,
				maxDate: new Date
			});
		}
		else if (direction == 'forward') {
			//alert("Forw");
			box.datetimepicker({
				pickTime: false,
				autoclose: true,
				minDate: new Date
			});
		}
		else {
			//alert("No hwere");
			$(this).datetimepicker({
				pickTime: false,
				autoclose: true
			});
		}
	});

	//$('body').on('focus', '.dateTimePicker', function () {
	//	$(this).datetimepicker({
	//		//pickTime: true,
	//		beforeShowDay: function (date) {
	//			return date.valueOf() >= now.valueOf();
	//		},
	//		autoclose: true,
	//		minDate: new Date
	//	});
	//})


	var nowTemp = new Date();
	var now = new Date(nowTemp.getFullYear(), nowTemp.getMonth(), nowTemp.getDate(), 0, 0, 0, 0);

	$(document.body).on("change", ".contrySelect", function (e) {
		//$('.contrySelect').unbind("change").change(function (e) {

		var c = $(this).val();
		var State2Feed = '#' + $(this).attr('data-state');
		//alert(State2Feed);
		var busyBox = $('.stateToFeedLabel');
		busyBox.append(LoadingVSmall_left());

		$.getJSON('/company/GetState', { id: c }, function (states) {
			var StatesSelect = $(State2Feed);
			busyBox.find("span.busy").remove();
			StatesSelect.empty();
			StatesSelect.append($('<option/>', { Value: "", text: "Select State" }));
			$.each(states, function (index, itemData) {

				StatesSelect.append(
					$('<option/>', { value: itemData.Id, text: itemData.Name }));

			});

		});
		e.preventDefault();
	});

	$(document.body).on("change", ".LoadLGAByState", function (e) {
		//$('.contrySelect').unbind("change").change(function (e) {

		var c = $(this).val();
		var Lga2Populate = '#' + $(this).attr('data-state');
		//alert(State2Feed);
		var busyBox = $('.stateToFeedLabel');
		busyBox.append(LoadingVSmall_left());

		$.getJSON('/company/GetState', { id: c }, function (lgas) {
			var Lga = $(Lga2Populate);
			busyBox.find("span.busy").remove();
			Lga.empty();
			Lga.append($('<option/>', { Value: "", text: "Select Local Government" }));
			$.each(lgas, function (index, itemData) {

				Lga.append(
					$('<option/>', { value: itemData.Id, text: itemData.Name }));

			});

		});
		e.preventDefault();
	});

	$('#DiffOPAddDiv').hide();

	if ($('#sameAdd').val() == 1 || $("#add1").val() == undefined) {
		$('#DiffOPAddDiv').hide();
		$("#DiffOPAddDiv [type='text'], #DiffOPAddDiv select").removeAttr("required");
	}
	else {
		//|| $("#opAddLine1").val().length > 0
		$('#DiffOPAddDiv').show();
		$("#DiffOPAddDiv [type='text'], #DiffOPAddDiv select").attr("required");
		$("#DiffOPAddDiv [name='model[1].postal_code']").removeAttr("required");
		$("#DiffOPAddDiv [name='model[1].address_2']").removeAttr("required");
	}

	$('#DiffOpAdd').unbind('change').on('change', function () {
		if (this.checked) {
			$('#DiffOPAddDiv').show();
			$("#DiffOPAddDiv [type='text'], #DiffOPAddDiv select").attr("required", "required");
			$("#DiffOPAddDiv [name='model[1].postal_code']").removeAttr("required");
			$("#DiffOPAddDiv [name='model[1].address_2']").removeAttr("required");
		} else {
			$('#DiffOPAddDiv').hide();
			$("#DiffOPAddDiv [type='text'], #DiffOPAddDiv select").removeAttr("required");

		}
	})

	$('#bizType').unbind("change").on("change", function (e) {
		var value = $(this).val();
		if (value.length != 0) {
			$("#CategoryBlock").removeClass("hide");
		}
		else {
			$("#CategoryBlock").addClass("hide");
		}
	});

	//$("#Category").unbind("change").on("change", function () {
	//    $("#ServiceBlock").html(Loading());
	//    var me = $(this);

	//    $.get("/Application/LoadServiceForMap", { catId: me.val() }, function (data) {
	//        $("#ServiceBlock").html(data);
	//    });
	//});

	$(".deleteItem").click(function (e) {
		if (confirm("Do you really want to delete this item?")) {

		} else {
			e.preventDefault();
		}
	})
	//$(document.body).on("click", "a.serviceClicker", (function (e) {
	//    var me = $(this);
	//    e.preventDefault();
	//    $("#JobSpecificationDiv").html(Loading());
	//    $.get(me.data("url"), { servId: me.data("id"), serviceName: me.data("name"), bizType: $("#bizType").val() }, function (data) {
	//        $("#JobSpecificationDiv").html(data);
	//    });
	//}));

	//$(document.body).on("click", "#find", function () {
	//    var url = "/Application/Renew";
	//    var param = $("#Current_Permit").val();
	//    $('#ServiceDiv').html(Loading());
	//    //alert(param);
	//    $.get(url, { permitno: param }, function (data) {
	//        $('#ServiceDiv').html(data);
	//    });
	//});

	//StatePreload();
});

function StatePreload() {
	var state = $("#partial_state");
	var State2Feed = $("#StateId1");
	var stateId = state.data("stateId");
	alert("State Id: " + stateId + "; Country ID: " + state.Country_Id);

	if (parseInt(state.Country_Id) > 0) {
		$.getJSON('/company/GetState', { id: state.Country_Id }, function (states) {
			var StatesSelect = $(State2Feed);
			busyBox.find("span.busy").remove();
			StatesSelect.empty();
			StatesSelect.append($('<option/>', { Value: "", text: "Select State" }));
			$.each(states, function (index, itemData) {

				StatesSelect.append(
					$('<option/>', { value: itemData.Id, text: itemData.Name }));
			});

			$.each("#StateId1 option", function () {
				var _stId = $(this).attr("Value");
				if (_stId == stateId) {
					$(this).attr("selected", "selected");
				}
			});
		});


	}
}

function Loading() {
	var loading = '<div class="busy"><img src="/OldDepotStyle/Content/Images/loading.gif" /></div>';
	return loading;
}

function LoadingSmall() {
	var loading = '<div class="busy"><br /><img style="width: 25px;" src="/OldDepotStyle/Content/Images/loading.gif" /></div>';
	return loading;
}

function LoadingVSmall() {
	var loading = '<div class="busy txtcenter"><img style="width: 15px;" src="/OldDepotStyle/Content/Images/loading.gif" /></div>';
	return loading;
}

function LoadingVSmall_left() {
	var loading = '<span class="busy"><img style="width: 15px;" src="/OldDepotStyle/Content/Images/loading.gif" /></span>';
	return loading;
}

function Loading2() {
	var loading = '<div class="progress"><div class="progress-bar progress-bar-success progress-striped active" '
		+ 'role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 100%">'
		+ '<span class="sr-only"></span></div></div>';

	return loading;
}

function AlertMessage(title, msg, btn) {
	if (title.length > 0) {
		var titleText = title;
	}
	else
		var titleText = "Alert!";

	$("#modalPopup").on('show.bs.modal', function () {
		var modal = $(this);
		modal.find('.modal-title').text(titleText);
		modal.find('.modal-body').text(msg);
		if (btn == 1) {
			// Ok button
			modal.find('.modal-footer').html('<button class="btn" data-dismiss="modal" aria-hidden="true">Ok</button>');
		}
		else {
			modal.find('.modal-footer').html('<button class="btn" data-dismiss="modal" aria-hidden="true">Continue</button>');
		}
	});
	$("#modalPopup").modal('show');
}

function Loading2() {
	var loading = '<div class="progress"><div class="progress-bar progress-bar-success progress-striped active" '
		+ 'role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 100%">'
		+ '<span class="sr-only"></span></div></div>';

	return loading;
}

function AlertMessage(title, msg, btn) {
	if (title.length > 0) {
		var titleText = title;
	}
	else
		var titleText = "Alert!";

	$("#modalPopup").on('show.bs.modal', function () {
		var modal = $(this);
		modal.find('.modal-title').text(titleText);
		modal.find('.modal-body').text(msg);
		if (btn == 1) {
			// Ok button
			modal.find('.modal-footer').html('<button class="btn" data-dismiss="modal" aria-hidden="true">Ok</button>');
		}
		else {
			modal.find('.modal-footer').html('<button class="btn" data-dismiss="modal" aria-hidden="true">Continue</button>');
		}
	});
	$("#modalPopup").modal('show');
}