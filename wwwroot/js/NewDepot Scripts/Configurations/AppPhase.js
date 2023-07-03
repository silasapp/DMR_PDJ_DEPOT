$(function () {

    // error message
    function ErrorMessage(error_id, error_message) {
        $(error_id).fadeIn('fast')
            .html("<i class=\"fa fa-warning text-danger\"> </i> <span class=\"text-danger\"> " + error_message + " </span>")
            .delay(9000)
            .fadeOut('fast');
        return;
    }

    // success message
    function SuccessMessage(success_id, success_message) {
        $(success_id).fadeIn('fast')
            .html("<i class=\"fa fa-check-circle text-success\"> </i> <span class=\"text-success\">" + success_message + " </span>")
            .delay(10000)
            .fadeOut('fast');
        return;
    }

    // btn to slide to create staff
    $("#btnCreatePhase").on('click', function (event) {
        event.preventDefault();
        $('html, body').animate({
            scrollTop: $("#FormCreateAppPhase").offset().top
        }, 1000);
    });

    // Create Application Phase 
    $("#FormCreateAppPhase").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtAppPhaseName").val() === "") {
            ErrorMessage("#CreateAppPhaseInfo", "Please enter an App Phase");
        }
        else {
            var msg = confirm("Are you sure you want to create this App Phase?");

            if (msg === true) {
                $.getJSON("/ApplicationPhases/CreatePhase",
                    {
                        "PhaseName": $("#txtAppPhaseName").val(),
                        "phsId": $("#txtphsId").val(),
                        "CategoryName": $("#txtAppCategoryName option:selected").val(),
                        "ShortName": $("#txtPhaseShortName").val(),
                        "ServiceCharge": $("#txtServiceCharge").val(),
                        "Price": $("#txtPrice").val(),
                        "PriceByVolume": $("#txtPricePerVolume option:selected").val(),
                        "ProcessingFee": $("#txtProcessingFee").val(),
                        "ProcessingFeeByTank": $("#txtProcessingFeeByTank option:selected").val(),
                        "IssueType": $("#txtIssueType option:selected").val(),
                        "FlowType": $("#txtFlowType option:selected").val(),
                        "Description": $("#txtDescription").val(),
                    },

                    function (response) {
                        if ($.trim(response) === "Phase Created") {
                            SuccessMessage("#CreateAppPhaseInfo", "Application phase created successfully");
                            $("#FormCreateAppPhase")[0].reset();
                            AppPhaseTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#CreateAppPhaseInfo", response);
                        }
                    });
            }
        }
    });


    // geting list of application Phase
    var AppPhaseTables = $("#TableAppPhase").DataTable({

        ajax: {
            url: "/ApplicationPhases/GetAppPhases",
            type: "POST",

        },

        lengthMenu: [[50, 100, 250, 500, 750, 1000], [50, 100, 250, 500, 750, 1000]],

        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            'excelHtml5',
            'csvHtml5',
            'pdfHtml5',
            {
                extend: 'print',
                text: 'Print all',
                exportOptions: {
                    modifier: {
                        selected: null
                    }
                }
            },
            {
                extend: 'colvis',
                collectionLayout: 'fixed two-column'
            }

        ],

        language: {
            buttons: {
                colvis: 'Change columns'
            }
        },
        "processing": true,
        "serverSide": true,
        stateSave: true,

        columns: [
            { data: "phsId", "visible": false, "searchable": false },
            { data: "phaseName" },
            { data: "categoryName" },
            { data: "shortName" },
            { data: "issueType" },{ data: "flowType" },
            { data: "price" },{ data: "priceByVolume" }, { data: "processingFee" }, { data: "processingFeeByTank" },
            { data: "description" },
            { data: "serviceCharge" },
            { data: "createdAt" },
            { data: "updatedAt" },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-xs btn-primary app" + row["phsId"] + "\" id=\"" + row['phaseName'] + "|" + row['categoryName'] + "|"+ row['shortName'] + "|"
                        + row["issueType"] + "|" + row["flowType"] + "|" + row["price"] + "|" + row["priceByVolume"] + "|" + row["processingFee"] + "|" + row["processingFeeByTank"]
                        + "|" + row["serviceCharge"]+ "|" + row["description"] + "\" onclick=\"getAppPhase(" + row["phsId"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteAppPhase(" + row["phsId"] + ")\"> <i class=\"fa fa-trash\"> </i> Delete </button>";

                }
            }
        ]

    });


    // Editing App Type
    $("#FormEditAppPhase").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditAppPhaseID").val() === "") {
            ErrorMessage("#EditAppPhaseInfo", "Please select an application phase to edit.");
        }
        else {
            var msg = confirm("Are you sure you want to edit this application Phase?");

            if (msg === true) {

                $.getJSON("/ApplicationPhases/EditPhase",
                    {
                        "PhaseName": $("#txtEditAppPhaseName").val(),
                        "phsId": $("#txtEditAppPhaseID").val(),
                        "CategoryName": $("#txtEditCategoryName option:selected").val(),
                        "ShortName": $("#txtEditPhaseShortName").val(),
                        "ServiceCharge": $("#txtEditServiceCharge").val(),
                        "Price": $("#txtEditPrice").val(),
                        "PriceByVolume": $("#txtEditPricePerVolume option:selected").val(),
                        "ProcessingFee": $("#txtEditProcessingFee").val(),
                        "ProcessingFeeByTank": $("#txtEditProcessingFeeByTank option:selected").val(),
                        "IssueType": $("#txtEditIssueType option:selected").val(),
                        "FlowType": $("#txtEditFlowType option:selected").val(),
                        "Description": $("#txtEditDescription").val(),

                    },
                    function (response) {
                        if ($.trim(response) === "Phase Updated") {
                            SuccessMessage("#EditAppPhaseInfo", "Application phase updated successfully.");
                            $("#FormEditAppPhase")[0].reset();
                            AppPhaseTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditAppPhaseInfo", response);
                        }
                    });
            }
        }
    });
});


function getAppPhase(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateAppPhase").offset().top
    }, 1000);
    var app_id = $("#txtEditAppPhaseID").val(id);
    var app = $(".app" + id).attr("id").split("|");
  
    
    $("#txtEditAppPhaseName").val(app[0]);
    $("#txtEditCategoryNameVal").text(app[1]);
    $("#txtEditCategoryNameVal").val(app[1]);
    $("#txtEditPhaseShortName").val(app[2]);
    //$("#txtEditIssueTypeVal").append("<option value="+app[3]+">" + app[3] + "</option>").attr("selected", "selected");
    $("#txtEditIssueTypeVal").text(app[3]);
    $("#txtEditIssueTypeVal").val(app[3]);
    $("#txtEditFlowTypeVal").text(app[4]);
    $("#txtEditFlowTypeVal").val(app[4]);
    $("#txtEditPrice").val(parseInt(app[5]));
    $("#txtEditPricePerVolumeVal").val(app[6]);
    $("#txtEditPricePerVolumeVal").text(app[6]);
    $("#txtEditProcessingFee").val(parseInt(app[7]));
    $("#txtEditProcessingFeeByTankVal").text(app[8]);
    $("#txtEditProcessingFeeByTankVal").val(app[8]);
    $("#txtEditServiceCharge").val(parseInt(app[9]));
    $("#txtEditDescription").val(app[10]);
}

/*
 * Removing App Phase.
 */
function DeleteAppPhase(id) {
    var msg = confirm("Are you sure you want to remove this Application Phase ?");
    if (msg === true) {
        $.getJSON("/ApplicationPhases/DeleteAppPhase", { "phsId": id }, function (response) {
            if ($.trim(response) === "AppPhase Deleted") {
                alert("Application Phase removed successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}