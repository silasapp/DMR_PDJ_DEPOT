
$(function () {


    


    $("a[data-toggle='modal']").on("click", function (e) {
        e.preventDefault();
        var url = $(this).attr("data-url");
        var uid = $(this).attr("data-uid");
        var name = $(this).attr("data-name");
        var xtrPay = $(this).attr("data-xtrPay");
        $("#modal-body").html("");
        var headTitle = "";
        if (uid == "1") {
            headTitle = "Schedule an Inspection with \"" + name + "\"";
            $(".modal-dialog").removeClass("wide-body");
        } else if (uid == "2") {
            headTitle = "Schedule an Inspection with \"" + name + "\"";
            $(".modal-dialog").removeClass("wide-body");
        } else if (uid == "3") {
            var headTitle = "Waivers Request for \"" + name + "'s\" Application";
            $(".modal-dialog").removeClass("wide-body");
        } else if (uid == "4") {
            headTitle = "Assign Inspection Form to \"" + name + "'s\" Application";
            $(".modal-dialog").removeClass("wide-body");
        } else if (uid == "5") {
            //var headTitle = "Confirm Presentation for \"" + name + "'s\" Application";
            headTitle = "Fill Inspection Form for " + name + "'s Application";
            $(".modal-dialog").removeClass("wide-body");
        } else if (uid == "6") {
            //var headTitle = "Approve Inspection Report for \"" + name + "'s\" Application";
            headTitle = "Pass Application for Approval";
            $(".modal-dialog").removeClass("wide-body");
        } else if (uid == "7") {
            headTitle = "Reject " + name + "'s Application";
          
            $(".modal-dialog").addClass("wide-body");
        }
        //else if (uid == "8") {
        //    var headTitle = "Waivers Request for \"" + name + "'s\" Application";
        //    $(".modal-dialog").removeClass("wide-body");
        //} else if (uid == "9") {
        //    var headTitle = "Schedule an Inspection Meeting with \"" + name + "\"";
        //    $(".modal-dialog").removeClass("wide-body");
        //}

        $(".modal-title").text(headTitle);        // find title for changr
        $("#modal-body").html("");
        $("#modal-body").html(Loading());
        //Load the view
        if (xtrPay === "yes") {
            $("#modal-body").html('<p><h2>Please Note that this application can not pass this stage until the<b> Extra Payment </b> attached to it is paid</h2></p>');
        } else {

            $.get(url, function (data) {
                $("#modal-body").html(data);
            });
        }

    });


    $(document).on("click", ".preview", function (e) {
        e.preventDefault();
        var pr = $(this);
        var date = $("#MeetingDate").val();
        var venue = $("#VenueId").val();
        var msg = $("#Message").val();
        var apId = $("#ApplicationId").val();
        // console.log("Date: " + date + "; Venue: " + venue + "; ApId: " + apId + "; Msg: " + msg);

        if (date.length > 0 && parseInt(venue) >= 0 && msg.length > 0) {
            console.log("Enter");

            $.post("/Process/SetPreview", { venue: venue, message: msg, date: date, appid: apId }, function (data) {
                if (data === 1) {
                    //window.open('/process/PreviewSchedule', '_blank');
                    //console.log(data);
                    window.open(pr.attr("href"), '_blank');
                    //$("#btnPreview").click();
                    //return true;
                }
                else {
                    alert("Preview not set properly");
                    return false;
                }
            });

            //$(this).attr("href", href + "?venue=" + venue + "&message=" + msg + "&date=" + date);
        }
        else {
            alert("Please complete all fields.");
            return false;
        }
    });

    $(document).on("click", ".delForm", function (e) {
        e.preventDefault();
        var parent = $(this).parent("li");
        var formName = $(this).data("form-name");
        var ask = confirm("Are you sure you want to remove form \"" + formName + "\" from this Application?")
        if (ask == true) {
            var id = $(this).data("appform-id");
            var url = "/Process/RemoveFormFromApplication?id=" + id;
            //alert(url);

            $.get(url, function (data) {
                if (data == 0) {
                    alert(formName + " removed from Application form successfully.");
                    $(parent).remove();
                    $('#modalPopup').modal('hide');
                }
                else {
                    alert(formName + " cannot be removed from Application. Please try again.");
                }
            });
        }
    });
});

function SetPreview(venue, msg, date, apId) {

    $.post("/Process/SetPreview", { venue: venue, message: msg, date: date, appid: apId }, function (data) {
        if (data === 1) {
            //window.open('/process/PreviewSchedule', '_blank');
            console.log("okay, show preview: " + data)
            return data;
        }
        else {
            return 0;
        }
    });
}