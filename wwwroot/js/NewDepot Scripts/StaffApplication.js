
$(function () {

    // error message
    function ErrorMessage(error_id, error_message) {
        $(error_id).fadeIn('fast')
            .html("<div class=\"alert alert-danger\"><i class=\"fa fa-warning\"> </i> <span class=\"text-dark\"> " + error_message + " </span> </div>")
            .delay(9000)
            .fadeOut('fast');
        return;
    }

    // success message
    function SuccessMessage(success_id, success_message) {
        $(success_id).fadeIn('fast')
            .html("<div class=\"alert alert-success\"> <i class=\"fa fa-check-circle\"> </i> <span class=\"text-dark\">" + success_message + " </span> </div>")
            .delay(10000)
            .fadeOut('fast');
        return;
    }





    /*
 * Saving an application Report
 */
    $("#btnSaveReport").on('click', function (event) {


        var txtReport = $("#txtAppReport");
        var txtReportTitle = $("#txtReportTitle");

        if (txtReport.val() === "" || $("#txtAppId").val() === "" || $("#txtReportTitle").val() === "") {
            ErrorMessage("#ReportModalInfo", "Please enter the report title and comment for this report");
        }
        else {
            var data = new FormData();

            data.append("txtReport", txtReport.val());
            data.append("txtReportTitle", txtReportTitle.val());


            $.confirm({
                text: 'Are you sure you want to add a report to this application?',
                columnClass: 'medium',
                closeIcon: true,
                confirm: function () {
                    $("#AppReportLoader").addClass("Submitloader");

                    $.post("/Process/SaveReport",
                        {
                            "AppID": $("#txtAppId").val(),
                            "txtReport": txtReport.val(),
                            "txtReportTitle": $("#txtReportTitle").val(),
                        },
                        function (response) {

                            if ($.trim(response) === "Report Saved") {
                                $("#AppReportLoader").removeClass("Submitloader");
                                txtReport.val("");
                                txtReportTitle.val("")
                                SuccessMessage("#ReportModalInfo", "Report successfully saved.");
                                $("#divReport").load(location.href + " #divReport");

                            }
                            else {
                                ErrorMessage("#ReportModalInfo", response);
                                $("#AppReportLoader").removeClass("Submitloader");
                            }
                        });

                },
                cancel: function () {
                    //$("#AppApproveLoadder").removeClass("Submitloader");

                },
                confirmButton: "Yes",
                cancelButton: "No"
            })


        }
    });

    /*
    * Editing an application Report
    */
    $("#btnEditReport").on('click', function (event) {

        var txtReport = $("#txtEditAppReport");
        var txtReportTitle = $("#txtEditReportTitle");

        if (txtReport.val() === "" || $("#txtEditReportID").val() === "" || txtReportTitle.val() === "") {
            ErrorMessage("#EditReportModalInfo", "Please enter a comment and title for this report");
        }
        else {

            $.confirm({
                text: 'Are you sure you want to edit this report to this application?',
                columnClass: 'medium',
                closeIcon: true,
                confirm: function () {


                    $("#AppEditReportLoadder").addClass("Submitloader");

                    $.post("/Processs/EditReport",
                        {
                            "ReportID": $("#txtEditReportID").val(),
                            "txtReport": txtReport.val(),
                            "txtReportTitle": txtReportTitle.val(),
                        },
                        function (response) {
                            if ($.trim(response) === "Report Edited") {
                                SuccessMessage("#EditReportModalInfo", "Report successfully edited.");
                                txtReport.val("");
                                txtReportTitle.val("");
                                $("#divReport").load(location.href + " #divReport");
                                $("#AppEditReportLoadder").removeClass("Submitloader");
                            }
                            else {
                                ErrorMessage("#EditReportModalInfo", response);
                                $("#AppEditReportLoadder").removeClass("Submitloader");
                            }
                        });
                },
                cancel: function () {
                    //$("#AppApproveLoadder").removeClass("Submitloader");

                },
                confirmButton: "Yes",
                cancelButton: "No"
            })
        }
    });
    //am4charts section


    var OtherDocs = $("#OtherDocsTable").DataTable({

        "paging": false,
        "ordering": false,
        "info": false,
        "searching": false,

        "columnDefs": [
            {
                "orderable": false,
                "targets": 0,
                "width": "20%",
                'checkboxes': {
                    'selectRow': true
                }
            }
        ],

        "select": {
            'style': 'multi'
        }

    });


    var apps = [];
    var myDeskApps = [];
    var staffApps = [];



    $("#btnPushApp").on('click', function (event) {

        event.preventDefault();

        apps.length = 0;

        var rows_selected = AppDropTable.column(0).checkboxes.selected();

        if (rows_selected.length !== 0) {

            $.each(rows_selected, function (index, rowId) {
                apps.push(rowId);
            });

            var str = apps.join(",");
            $("#showpush").val(str);
            $("#btnPushApp2").click();
        }
        else {
            alert("Please select applications to assign");
        }
    });




    /*
     * Displaying all application on a particular staff desk for distribution
     */
    var staffDeskApps = $("#MyDeskAppTable").DataTable({

        'initComplete': function (settings) {
            var api = this.api();

            api.cells(
                api.rows(function (idx, data, node) {

                    var worked = data[6];
                    var status = data[7];
                    //var desk = data[8];
                    return (worked.indexOf("YES") >= 0 || status.indexOf("pproved") >= 0) ? true : false;
                }).indexes(),
                0
            ).checkboxes.disable();
        },

        'columnDefs': [
            {
                'targets': 0,
                'checkboxes': {
                    'selectRow': true
                },
                'createdCell': function (td, cellData, rowData, row, col) {
                    var hasworked = $.trim(rowData[6]);
                    var status = $.trim(rowData[7]);

                    if (hasworked.indexOf("YES") >= 0 || status.indexOf("pproved") >= 0) {
                        this.api().cell(td).checkboxes.disable();
                    }
                }
            }
        ],

        lengthMenu: [[50, 150, 250, 500], [50, 150, 250, 500]],

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

        'deferRender': true,
        "select": {
            'style': 'multi'
        }
    });


    // for staff updating 
    $("#FormUpdateFacilityInformation").on('submit', function (event) {
        event.preventDefault();

        var Facility =
        {
            FacilityId: $("#txtFacilityID").val(),
            ElpsFacilityId: $("#txtFacilityElpsID").val(),
            FacilityName: $("#txtFacilityName").val().toUpperCase(),
            FacilityAddress: $("#txtFacilityAddress").val(),
            Lga: $("#txtFacilityLGA").val().toUpperCase().toUpperCase(),
            City: $("#txtFacilityCity").val().toUpperCase().toUpperCase(),
            LandMeters: $("#txtMeters").val(),
            isPipeLine: $("#seltxtPipeLine").val(),
            isHighTention: $("#seltxtHighTention").val(),
            IsHighWay: $("#seltxtHighWay").val(),
            ContactName: $("#txtContactName").val(),
            ContactPhone: $("#txtContactPhone").val(),
            FacilityPurpose: $("#txtFacilityUse").val(),
        };

        var msg = confirm("Are you sure you want to update this facility information???");

        if (msg === true) {
            $("#AppDiv").addClass("Submitloader");

            $.post("/Company/UpdateFacilityInformation",
                {
                    "CompanyElpsID": $("#txtCompnayElpsID").val(),
                    "State": $("#txtFacilityState").val(),
                    "facilities": Facility,
                },
                function (response) {

                    if (response === "Updated") {
                        alert("Facility information updated successfully...");
                        SuccessMessage("#DivFormUpdateFacilityInfo", "Facility information updated successfully...");
                        location.reload(true);
                        $("#AppDiv").removeClass("Submitloader");
                    }

                    else {
                        $("#AppDiv").removeClass("Submitloader");
                        ErrorMessage("#DivFormUpdateFacilityInfo", res[1].trim());
                    }
                });
        }

    });



    $("#MyAppsTable").dataTable({
        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            {
                extend: 'csvHtml5',
                footer: true
            },
            {
                extend: 'excelHtml5',
                footer: true
            },
            {
                extend: 'pdfHtml5',
                footer: true
            },
            {
                extend: 'print',
                text: 'Print all',
                footer: true,
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

        "aLengthMenu": [50, 100, 150, 200, 250],
        'iDisplayLength': 200,
        columnDefs: [{ orderable: false, "targets": [-1] }],
        "language": {
            "lengthMenu": "Display _MENU_ records per page",
            "zeroRecords": "Nothing found - sorry",
            "infoEmpty": "No records available",
            "infoFiltered": "(filtered from _MAX_ total records)"
        }
    });
    $("#MyCompanyTable").dataTable({
        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            {
                extend: 'csvHtml5',
                footer: true
            },
            {
                extend: 'excelHtml5',
                footer: true
            },
            {
                extend: 'pdfHtml5',
                footer: true
            },
            {
                extend: 'print',
                text: 'Print all',
                footer: true,
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

        "aLengthMenu": [50, 100, 150, 200, 250],
        'iDisplayLength': 200,
        columnDefs: [{ orderable: false, "targets": [-1] }],
        "language": {
            "lengthMenu": "Display _MENU_ records per page",
            "zeroRecords": "Nothing found - sorry",
            "infoEmpty": "No records available",
            "infoFiltered": "(filtered from _MAX_ total records)"
        }
    });
    $("#StaffDeskTable").dataTable({
        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            {
                extend: 'csvHtml5',
                footer: true
            },
            {
                extend: 'excelHtml5',
                footer: true
            },
            {
                extend: 'pdfHtml5',
                footer: true
            },
            {
                extend: 'print',
                text: 'Print all',
                footer: true,
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

        "aLengthMenu": [50, 100, 150, 200, 250],
        'iDisplayLength': 200,
        columnDefs: [{ orderable: false, "targets": [-1] }],
        "language": {
            "lengthMenu": "Display _MENU_ records per page",
            "zeroRecords": "Nothing found - sorry",
            "infoEmpty": "No records available",
            "infoFiltered": "(filtered from _MAX_ total records)"
        }
    });

    var MyDeskTable = $("#MyDeskAppsTable").DataTable({


        ajax: {
            url: "/Application/GetMyDeskApps",
            type: "POST",
            lengthMenu: [[50, 150, 250, 500], [50, 150, 250, 500]],
        },

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

        columns: [
            //{
            //    data: "deskID"
            //},
            {
                "render": function (data, type, row) {
                    return "<b class=\"text-danger\"> " + row["refNo"] + " </b>";
                }
            },

            {
                "render": function (data, type, row) {
                    return "<b class=\"text-primary\"> " + row["companyName"] + " </b>";
                }
            },
            {
                "render": function (data, type, row) {
                    return "<small class=\"\"> " + row["facility"] + " </small>";
                }
            },
            { data: "category" },
            { data: "type" },
            { data: "stage" },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-warning\"> " + row["status"] + " </button>";
                }
            },
            { data: "landedAt" },
            {
                "render": function (data, type, row) {
                    return "<b class=\"text-primary\"> " + row["activity"] + " </b>";
                }
            },
            {
                "render": function (data, type, row) {
                    return "<a class=\"btn btn-sm btn-info\" href=\"/Application/ViewApplication/" + row["deskID"] + "/" + row["processID"] + "\" target=\"_blank\"> <i class=\"fa fa-eye\"> </i> view Application </a>";
                }
            }
        ],

        rowGroup: {

            startRender: function (rows, group) {
                return $('<tr>').append('<td colspan="10" class="text-left" style="background-color:#fabc4eb0; color:red"> <b>' + group + '</b></td></tr>');
            },

            dataSrc: 'category'
        },
        orderFixed: [8, 'desc'],
        "bDestroy": true
    });



    $("#btnInsPushApp").on('click', function (event) {

        var id = $("#txtDeskID");

        if (id.val() !== "") {
            $("#btnInsPushApp2").click();

            $("#DivGetPushStaff").DataTable({

                ajax: {
                    url: "/Application/GetPushStaff?id=" + $("#PushStaffRole").val(),
                    type: "POST",
                },

                lengthMenu: [[5, 10], [5, 10]],

                "processing": true,
                "serverSide": true,
                "bDestroy": true,

                columns: [

                    { data: "fullname" },
                    { data: "email" },
                    { data: "role" },
                    { data: "office" },
                    { data: "deskCount" },
                    {
                        "render": function (data, type, row) {
                            return "<button class=\"btn btn-sm btn-info push\" onclick=\"PushApp(" + row["staffId"] + ")\"> Assign Application </button>";
                        }
                    }
                ],


            });
        }
        else {
            ErrorMessage("#AppViewInfo", "Application link was broken and cannot be Assign");
        }

    });


    /*
     * This is to rerout all application from the desk of one staff to another
     */
    $("#btnStaffRerouteApps").on('click', function (event) {
        event.preventDefault();

        staffApps.length = 0;

        var rows_selected = staffDeskApps.column(0).checkboxes.selected();
        debugger;
        if (rows_selected.length !== 0) {

            $.each(rows_selected, function (index, rowId) {
                staffApps.push(rowId);
            });

            var str = staffApps.join(",");
            $("#Insshowpush").val(str);

            $("#btnGetStaffs").click();
        }
        else {

            alert("Please select staff and applications to re-route");
        }
    });


    $("#btnGetStaffs").on('click', function (event) {

        var RouteApp = $("#TableDistributeApps").DataTable({

            ajax: {
                url: "/Deskes/GetRouteStaff?staff=" + $("#txtOriginalStaffID").val(),
                type: "POST",
                lengthMenu: [[5, 10], [5, 10]]
            },

            "destroy": true,
            "processing": true,
            "serverSide": true,
            "searching": false,
            "info": false,
            "paging": true,
            "lengthChange": false,

            columns: [
                { data: "staffID", "visible": false, },
                { data: "lastName" },
                { data: "firstName" },

                { data: "staffEmail" },
                {
                    "render": function (data, type, row) {
                        return "<button class=\"btn btn-sm btn-info push\" onclick=\"RerouteApps(" + row["staffID"] + ")\"> Re-Route Application </button>";
                    }
                }
            ]
        });

    });





    /*
     * App schdule datetimepicker
     */
    $("#txtSchduleDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });



    /*
     * edit App schdule datetimepicker
     */
    $("#txtEditSchduleDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });



    // Displaying all staff based on location for joint inspection (Create Schedule)
    $("#txtInspectionType").on('change', function (e) {
        e.preventDefault();


        if ($("#txtInspectionType").val() !== "") {

            var JointStaff = $("#NominatedStaffTable").DataTable({

                ajax: {
                    url: "/Application/GetNominatedStaff?appid=" + $("#txtAppId").val() + "&jointInspection=" + $("#txtInspectionType").val(),
                    type: "POST",
                    lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
                },

                "processing": true,
                "serverSide": true,
                "paging": false,
                "ordering": false,
                "info": false,
                "searching": false,
                "bDestroy": true, // important!

                "scrollY": '23vh',
                //"scrollCollapse": true,

                "columnDefs": [
                    {
                        "orderable": false,
                        "targets": 0,
                        "width": "10%",
                        'checkboxes': {
                            'selectRow': true
                        }
                    }
                ],

                "select": {
                    'style': 'multi'
                },

                columns: [
                    { data: "staffId" },
                    { data: "fullname" },
                    { data: "role" },
                    { data: "office" },
                ]
            });
        }



    });




    // Displaying all staff based on location for joint inspection (Edit Schedule)
    $("#txtEditInspectionType").on('change', function (e) {
        e.preventDefault();


        if ($("#txtEditInspectionType").val() !== "") {

            var JointStaff2 = $("#NominatedStaffTable2").DataTable({

                ajax: {
                    url: "/Application/GetNominatedStaff?appid=" + $("#txtAppId").val() + "&jointInspection=" + $("#txtEditInspectionType").val(),
                    type: "POST",
                    lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
                },

                "processing": true,
                "serverSide": true,
                "paging": false,
                "ordering": false,
                "info": false,
                "searching": false,
                "bDestroy": true, // important!

                "scrollY": '23vh',
                //"scrollCollapse": true,

                "columnDefs": [
                    {
                        "orderable": false,
                        "targets": 0,
                        "width": "10%",
                        'checkboxes': {
                            'selectRow': true
                        }
                    }
                ],

                "select": {
                    'style': 'multi'
                },


                columns: [
                    { data: "staffId" },
                    { data: "fullname" },
                    { data: "role" },
                    { data: "office" },
                ]
            });
        }



    });




    /*
     * Create a schdule
     */
    $("#btnSaveSchdule").on('click', function (event) {
        event.preventDefault();

        var staff = [];

        var rows_selected = $("#NominatedStaffTable").DataTable().column(0).checkboxes.selected();

        if (rows_selected.length !== 0) {
            $.each(rows_selected, function (index, rowId) {
                staff.push(rowId);
            });
        }

        if ($("#txtSchduleComment").val() === "" || $("#txtSchduleDate").val() === "") {
            ErrorMessage("#SchduleModalInfo", "Please enter schedule date and comment");
        }
        else if ($("#txtInspectionType").val() === "") {
            ErrorMessage("#SchduleModalInfo", "Please indicate the type of inspection.");
        }
        else if ($("#txtInspectionType").val() === "YES" && staff.length < 1) {
            ErrorMessage("#SchduleModalInfo", "Please select staff for join inspection");
        }
        else {
            msg = confirm("Are you sure you want to schedule this inspection, meeting or presentation?");
            $("#AppSchduleLoadder").addClass("Submitloader");

            if (msg === true) {
                $.post("/Application/CreateSchdule",
                    {
                        "SchduleDate": $("#txtSchduleDate").val(),
                        "SchduleComment": $("#txtSchduleComment").val(),
                        "AppID": $("#txtAppId").val(),
                        "DeskID": $("#txtDeskID").val(), // dask id
                        "SchduleLocation": $("#txtSchduleLocation").val(),
                        "SchduleType": $("#txtSchduleType").val(),
                        "NominatedStaff": staff,
                        "InspectionType": $("#txtInspectionType").val(),
                        "NominatedStaffComment": $("#txtStaffListComment").val()
                    },
                    function (response) {
                        if ($.trim(response) === "Schdule Created") {
                            $("#divSchdule").load(location.href + " #divSchdule");
                            SuccessMessage("#SchduleModalInfo", "Schedule created successfully");
                            $("#txtSchduleComment").val('');
                            $("#txtSchduleDate").val('');
                            $("#AppSchduleLoadder").removeClass("Submitloader");
                        }
                        else {
                            ErrorMessage("#SchduleModalInfo", response);
                            $("#AppSchduleLoadder").removeClass("Submitloader");
                        }
                    });
            }
            else {
                $("#AppSchduleLoadder").removeClass("Submitloader");
            }
        }
    });



    /*
     * Edit a schdule 
     */
    $("#btnEditSchdule").on('click', function (event) {
        event.preventDefault();

        var staff = [];

        var rows_selected = $("#NominatedStaffTable2").DataTable().column(0).checkboxes.selected();

        if (rows_selected.length !== 0) {
            $.each(rows_selected, function (index, rowId) {
                staff.push(rowId);
            });
        }

        if ($("#txtEditSchduleDate").val() === "" || $("#txtEditSchduleID").val() === "" || $("#txtEditSchduleComment").val() === "") {
            ErrorMessage("#EditSchduleModalInfo", "Please enter a comment annd date for this schedule.");
        }
        else if ($("#txtEditInspectionType").val() === "") {
            ErrorMessage("#SchduleModalInfo", "Please indicate the type of inspection.");
        }
        else if ($("#txtEditInspectionType").val() === "YES" && staff.length < 1) {
            ErrorMessage("#SchduleModalInfo", "Please select staff for join inspection");
        }
        else {
            var msg = confirm("Are you sure you want to edit schedule for this inspection, meeting or presentation?");
            $("#AppEditSchduleLoadder").addClass("Submitloader");
            if (msg === true) {

                $.post("/Application/EditSchdule",
                    {
                        "schduleID": $("#txtEditSchduleID").val(),
                        "txtComment": $("#txtEditSchduleComment").val(),
                        "txtSchduleDate": $("#txtEditSchduleDate").val(),
                        "txtSchduleLoaction": $("#txtEditSchduleLocation").val(),
                        "txtSchduleType": $("#txtEditSchduleType").val(),
                        "NominatedStaff": staff,
                        "InspectionType": $("#txtEditInspectionType").val(),
                        "NominatedStaffComment": $("#txtEditStaffListComment").val()
                    },
                    function (response) {
                        if ($.trim(response) === "Schdule Edited") {
                            SuccessMessage("#EditSchduleModalInfo", "Schedule successfully edited.");
                            $("#txtEditSchduleDate").val("");
                            $("#txtEditSchduleComment").val("");
                            $("#divSchdule").load(location.href + " #divSchdule");
                            $("#AppEditSchduleLoadder").removeClass("Submitloader");
                        }
                        else {
                            ErrorMessage("#EditSchduleModalInfo", response);
                            $("#AppEditSchduleLoadder").removeClass("Submitloader");
                        }
                    });
            }
            else {
                $("#AppEditSchduleLoadder").removeClass("Submitloader");
            }
        }

    });




    /*
     * Button for external rejection
     */
    $("#btnReject").on('click', function (event) {

        var txtComment = $("#txtAppExRejectComment");
        var txtDeskID = $("#txtDeskID");
        var docs = [];

        var rows_selected = OtherDocs.column(0).checkboxes.selected();

        if (rows_selected.length !== 0) {
            $.each(rows_selected, function (index, rowId) {
                docs.push(rowId);
            });
        }

        if (txtComment.val() === "") {
            ErrorMessage("#ExRejectModalInfo", "Please enter a comment for rejection.");
        }
        else {

            var msg = confirm("Are you sure you want to reject this application?");

            $("#AppExternalRejectionLoadder").addClass("Submitloader");

            var location = window.location.origin + "/Application/MyDesk";

            if (msg === true) {

                $.post("/Application/Rejection",
                    {
                        "DeskID": txtDeskID.val(),
                        "RequiredDocs": docs,
                        "txtComment": txtComment.val()
                    },
                    function (responses) {

                        var response = responses.split("*");

                        if ($.trim(response[0]) === "External Rejection") {
                            alert("Application has been rejected back to the company.");
                            SuccessMessage("#ExRejectModalInfo", "Application has been rejected back to the company.");

                            window.location.href = location;

                            $("#AppExternalRejectionLoadder").removeClass("Submitloader");
                        }
                        else if ($.trim(response[0]) === "Internal Rejection") {
                            alert("Application has been internally rejected back to " + response[1] + ".");
                            SuccessMessage("#ExRejectModalInfo", "Application has been internally rejected back to " + response[1] + ".");

                            window.location.href = location;
                        }
                        else {
                            $("#AppExternalRejectionLoadder").removeClass("Submitloader");
                            ErrorMessage("#ExRejectModalInfo", response[1]);
                        }
                    });
            }
            else {
                $("#AppExternalRejectionLoadder").removeClass("Submitloader");
            }
        }
    });





    /*
     * Button for dismissal and rejection of an application
     */
    $("#btnDismiss").on('click', function (event) {

        var txtComment = $("#txtAppDismissComment");
        var txtDeskID = $("#txtDeskID");

        if (txtComment.val() === "") {
            ErrorMessage("#DismissModalInfo", "Please enter a comment for rejection and dismissal.");
        }
        else {

            var msg = confirm("Are you sure you want to reject and dismiss this application? The company will need to reapply again.");

            $("#AppDismissLoadder").addClass("Submitloader");

            var location = window.location.origin + "/Application/MyDesk";

            if (msg === true) {

                $.post("/Application/DismissApplication",
                    {
                        "DeskId": txtDeskID.val(),
                        "txtComment": txtComment.val()
                    },
                    function (responses) {

                        var response = responses.trim();

                        if ($.trim(response) === "Dismissed") {
                            alert("Application has been rejected and dismissed back to the company.");
                            SuccessMessage("#DismissModalInfo", "Application has been rejected and dismissed back to the company.");
                            window.location.href = location;
                            $("#AppDismissLoadder").removeClass("Submitloader");
                        }

                        else {
                            $("#AppDismissLoadder").removeClass("Submitloader");
                            ErrorMessage("#DismissModalInfo", response);
                        }
                    });
            }
            else {
                $("#AppDismissLoadder").removeClass("Submitloader");
            }
        }
    });





    /*
     * Reseting application step
     *
     */
    $("#btnResetStep").on('click', function (event) {
        var app_id = $("#btnResetStep").val();

        var msg = confirm("Are you sure you want to reset all step for this application?");

        if (msg === true) {
            $.post("/Application/ResetStep", { "AppID": app_id }, function (response) {
                if ($.trim(response) === "Reset Done") {
                    SuccessMessage("#ResetStepInfo", "All reset done for this application.");
                }
                else {
                    ErrorMessage("#ResetStepInfo", response);
                }
            });
        }
    });


    $("#RecordView").on('click', function (event) {
        $("#CalendarDiv").slideUp();
        $("#SchduleDiv").slideDown();
        $("#CalendarDiv").addClass("hide");
    });

    $("#CalendarView").on('click', function (event) {
        $("#SchduleDiv").slideUp();
        $("#CalendarDiv").slideDown();
        $("#CalendarDiv").removeClass("hide");
    });



    /*
     * Creating and Updating conditional memo
     */
    $("#btnSaveMemo").on('click', function (event) {
        event.preventDefault();

        var MemoConditions = [];

        var Condictions = document.getElementsByName('txtCondiction[]');

        for (var i = 0; i < Condictions.length; i++) {

            MemoConditions.push({
                "Condictions": Condictions[i].value.trim(),
                "AppId": $("#txtAppId").val(),
            });
        }

        var msg = confirm("Are you sure you want save this conditional memo?");

        if (msg === true) {

            $("#FormCreateConditionalMemo").addClass("Submitloader");

            $.post("/Application/CreateMemo",
                {
                    "MemoConditions": MemoConditions,
                    "AppId": $("#txtAppId").val(),
                    "txtMemoSubject": $("#txtMemoSubject").val(),
                    "txtOtherComments": $("#txtOtherComments").val(),
                },
                function (response) {

                    var res = $.trim(response);

                    if (res === "Saved") {

                        SuccessMessage("#MemoInfo", "Conditional memo saved successfully.");
                        alert("Conditional memo saved successfully.");
                        location.reload(true);

                    }
                    else {
                        ErrorMessage("#MemoInfo", res);
                        $("#FormCreateConditionalMemo").removeClass("Submitloader");
                    }
                });
        }
        else {
            $("#FormCreateConditionalMemo").removeClass("Submitloader");
        }

    });





    /*
    * Updating tanks
    */
    $("#FormUpdateTanks").on('submit', function (event) {
        event.preventDefault();

        var Tanks = [];

        var TankName = document.getElementsByName('txtTankName[]');
        var TankCapacity = document.getElementsByName('txtTankCapacity[]');
        var TankProduct = document.getElementsByName('txtTankProduct[]');
        var TankId = document.getElementsByName('txtTankID[]');
        var TankPosition = document.getElementsByName('txtPosition[]');
        var AppID = $("#txtAppID").val();
        var FacilityID = $("#txtFacilityID").val();

        for (var i = 0; i < TankName.length; i++) {

            Tanks.push({
                "FacilityID": FacilityID,
                "Capacity": TankCapacity[i].value.trim(),
                "Product": TankProduct[i].value.trim(),
                "TankId": TankId[i].value.trim(),
                "TankName": TankName[i].value.trim().toUpperCase(),
                "Position": TankPosition[i].value.trim()
            });

        }

        var msg = confirm("Are you sure you want to update these tanks?");


        if (msg === true) {

            $("#tankDivLoader").addClass("Submitloader");

            $.post("/Application/UpdateFacilityTanks", { "Tanks": Tanks }, function (response) {

                var res = $.trim(response).split("|");

                if (res[0] === "1") {

                    SuccessMessage("#SaveTankInfo", "Tanks successfully updated");
                    alert("Tanks successfully updated");
                    location.reload(false);

                }
                else {
                    ErrorMessage("#SaveTankInfo", res[1]);
                    $("#tankDivLoader").removeClass("Submitloader");
                }
            });
        }
        else {
            $("#tankDivLoader").removeClass("Submitloader");
        }
    });






    /*
    * updating application tanks
    */
    $("#btnUpdateAppTanks").on('click', function (event) {
        event.preventDefault();

        var AppTanks = [];

        var TankName = document.getElementsByName('txtTankName[]');
        var TankCapacity = document.getElementsByName('txtTankCapacity[]');
        var TankProduct = document.getElementsByName('txtTankProduct[]');
        var TankId = document.getElementsByName('txtTankID[]');
        var TankPosition = document.getElementsByName('txtPosition[]');

        var AppID = $("#txtAppId").val();
        var FacilityID = $("#txtFacilityID").val();

        for (var i = 0; i < TankName.length; i++) {

            AppTanks.push({
                "FacilityId": FacilityID,
                "AppId": AppID,
                "Capacity": TankCapacity[i].value.trim(),
                "Product": TankProduct[i].value.trim(),
                "Position": TankPosition[i].value.trim(),
                "TankName": TankName[i].value.trim().toUpperCase(),
                "AppTankId": TankId[i].value.trim(),
            });
        }

        var msg = confirm("Are you sure you want to update these tanks?");

        if (msg === true) {

            $.post("/Application/UpdateAppTankss", { "AppTanks": AppTanks }, function (response) {

                var res = $.trim(response);

                if (res === "Updated") {
                    alert("Tanks successfully updated.");
                    location.reload(false);
                }
                else {
                    alert(res);
                }
            });
        }
    });







    $("#btnMergeTanks").on("click", function (event) {

        var msg = confirm("Are you sure you want to merge this tanks?");

        if (msg === true) {

            $("#AppTanksModal").addClass("Submitloader");

            $.post("/Application/MergeTanks", { "FacId": $("#txtFacilityId").val(), "AppId": $("#txtApplicationId").val() }, function (response) {

                var res = $.trim(response);

                if (res === "Done") {
                    alert("Tanks successfully merge");
                    location.reload(false);
                }
                else {
                    alert(response);
                    $("#AppTanksModal").removeClass("Submitloader");
                }
            });
        }

    });





    $("#txtExtraPayType").on('change', function (e) {
        e.preventDefault();

        var txtPayType = $("#txtExtraPayType option:selected");

        var spl = txtPayType.val().split("|");

        var amount = $("#txtExtraPayAmount");
        var txtFine = $("#txtFineId");

        amount.val(spl[1]);
        txtFine.val(spl[0]);
    });



    /*
     * Saving extra payment by staff for customers
     */
    $("#btnSaveExtraPay").on('click', function (event) {
        event.preventDefault();

        var txtFine = $("#txtFineId");
        var txtComment = $("#txtExtraPayComment");
        var txtAppID = $("#txtAppId");

        if (txtFine.val() === "" || txtComment.val() === "") {
            ErrorMessage("#ExtraPayInfo", "Please select a fine to generate and a comment for it.");
        }
        else {

            var msg = confirm("Are you sure you want to generate RRR for this extra payment.");

            if (msg === true) {

                $("#AppExtraPaymentLoader").addClass("Submitloader");

                $.post("/Application/GenerateExtraPayment",
                    {
                        "appid": txtAppID.val(),
                        "fine": txtFine.val(),
                        "Comment": txtComment.val(),
                    },
                    function (response) {
                        if ($.trim(response) === "Generated") {
                            SuccessMessage("#ExtraPayInfo", "Extra payment generated successfully.");
                            $("#divExtraPayment").load(location.href + " #divExtraPayment");
                            txtFine.val("");
                            txtComment.val("");
                            $("#AppExtraPaymentLoader").removeClass("Submitloader");
                        }
                        else {
                            ErrorMessage("#ExtraPayInfo", response);
                            $("#AppExtraPaymentModal").removeClass("Submitloader");
                        }
                    });
            }
            else {
                $("#AppExtraPaymentModal").removeClass("Submitloader");
            }
        }
    });


    var Trans = $("#MyTransactionTable").DataTable({

        lengthMenu: [[50, 150, 250, 500], [50, 150, 250, 500]],
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
        select: true,
        "paging": true,
        "ordering": true,
        "info": true,
        "searching": true,

        "processing": true,


    });


    $('button.toggle-vis').on('click', function (e) {
        e.preventDefault();

        // Get the column API object
        var column = Trans.column($(this).attr('data-column'));

        // Toggle the visibility
        column.visible(!column.visible());
    });

    Trans.columns([16]).visible(false);
    Trans.columns([13]).visible(false);
    Trans.columns([14]).visible(false);




    /*
     * Accepting Legacy Licence
     */
    $("#btnAcceptLegacy").on('click', function (event) {
        event.preventDefault();

        var LegID = $("#txtLegacyID");
        var txtComment = $("#txtAcceptComment");
        var txtState = $("#txtState");

        if (txtComment.val() === "") {
            ErrorMessage("#LegacyModalInfo", "Please enter your approval comment.");
        }
        else if (LegID.val() === "") {
            ErrorMessage("#LegacyModalInfo", "Legacy Application link not found");
        }
        else {
            var msg = confirm("Are you sure you want Approve this legacy licence.");

            if (msg === true) {

                $("#AcceptLegacyLoadder").addClass("Submitloader");

                $.post("/Legacies/AcceptLegacy", { "LegacyID": LegID.val(), "Comment": txtComment.val(), "State": txtState.val() }, function (response) {
                    if ($.trim(response) === "Legacy Approved") {
                        SuccessMessage("#LegacyModalInfo", "Legacy licence added successfully.");
                        alert("Legacy licence added successfully.");
                        $("#AcceptLegacyLoadder").removeClass("Submitloader");
                        var location = window.location.origin + "/Legacies/LegacyApplications/0/Processing";
                        window.location.href = location;
                    }
                    else {
                        ErrorMessage("#LegacyModalInfo", response);
                        $("#AcceptLegacyLoadder").removeClass("Submitloader");
                    }
                });
            }
        }
    });


    /*
     * Rejecting Legacy application
     */
    $("#btnRejectLegacy").on('click', function (event) {
        event.preventDefault();

        var LegID = $("#txtLegacyID");
        var txtComment = $("#txtRejectComment");

        if (txtComment.val() === "") {
            ErrorMessage("#LegacyRejectModalInfo", "Please enter your rejection comment.");
        }
        else if (LegID.val() === "") {
            ErrorMessage("#LegacyRejectModalInfo", "Legacy Application link not found");
        }
        else {
            var msg = confirm("Are you sure you want Reject this legacy licence.");

            if (msg === true) {
                $("#RejectLegacyLoadder").addClass("Submitloader");
                $.post("/Legacies/RejectLegacy", { "LegacyID": LegID.val(), "Comment": txtComment.val() }, function (response) {
                    if ($.trim(response) === "Legacy Rejected") {

                        SuccessMessage("#LegacyRejectModalInfo", "Legacy licence has been rejected successfully.");
                        alert("Legacy licence has been rejected successfully.");
                        $("#RejectLegacyLoadder").removeClass("Submitloader");
                        var location = window.location.origin + "/Legacies/LegacyApplications/0/Processing";
                        window.location.href = location;
                    }
                    else {
                        ErrorMessage("#LegacyRejectModalInfo", response);
                        $("#RejectLegacyLoadder").removeClass("Submitloader");
                    }
                });
            }
        }
    });

    //Edit Legacy Details
    $("#btnEditLegacy").on('click', function (e) {
        var legacyID = $("#LegID").val();
        var legacyLicense = $("#LegacyLicense").val();
        var legacyCompany = $("#LegacyCompany").val();
        var legacyFacility = $("#LegacyFacility").val();
        var legacyAddress = $("#LegacyAddress").val();
        var legacyType = $("#LegacyType option:selected").val();
        var legacyUsage = $("input[name='LegacyUsagee']:checked").val();
        if (legacyUsage === "Yes") {
            legacyUsage = true;
        } else {
            legacyUsage = false;
        }
        debugger;
        if (legacyCompany === "" || legacyCompany === null) {
            $("#LegacyCompany").focus();
            $("#EditLegacyInfo").text("Enter a company name for this legacy.");
        }
        else if (legacyFacility === "" || legacyFacility === null) {
            $("#LegacyFacility").focus();
            $("#EditLegacyInfo").text("Enter a facility name for this legacy.");
        }
        else if (legacyAddress === "" || legacyAddress === null) {
            $("#LegacyAddress").focus();
            $("#EditLegacyInfo").text("Enter an address for this legacy.");
        }
        else if (legacyType === "" || legacyType === null) {
            $("#LegacyType").focus();
            $("#EditLegacyInfo").text("Select a type for this legacy.");
        }
        else {
            debugger;
            $.confirm({
                text: "Are you sure you want to edit this legacy ?",
                confirm: function () {
                    var data = new FormData();

                    data.append("Id", legacyID);
                    data.append("CompName", legacyCompany);
                    data.append("FacilityName", legacyFacility);
                    data.append("FacilityAddress", legacyAddress);
                    data.append("AppType", legacyType);
                    data.append("IsUsed", legacyUsage);
                    data.append("LicenseNo", legacyLicense);
                    //data.append("Issue_Date", legacyUsage);
                    //data.append("Exp_Date", legacyUsage);
                    $("#EditLegacyLoadder").addClass('Submitloader');

                    debugger;
                    $.ajax({
                        url: "/Legacies/EditLegacy",
                        type: "POST",
                        contentType: false,
                        processData: false,
                        data: data,
                        success: function (response) {
                            if (response === "Legacy Updated") {
                                alert("Legacy Information Has Been Successfully Updated.");
                                location.reload(true);
                            }
                            else {
                                alert(response);
                                $("#EditLegacyLoadder").removeClass('Submitloader');

                            }
                        }
                    });


                },
                cancel: function () {
                },
                confirmButton: "Yes",
                cancelButton: "No"
            })
        }
    });


});





/*
 * Push application
 */ 
function PushApp(id) {

    if ($("#txtPushComment").val() === "") {

        alert("Please enter a comment for distribution.");

    }
    else {

        var msg = confirm("Are you sure you want to assign application(s) to this staff?");

        if (msg === true) {

            $(".push").attr("disable", "disabled");

            $.post("/Application/PushApplications", { "staffID": id, "DeskID": $("#txtDeskID").val(), "PushComment": $("#txtPushComment").val() }, function (respons) {
                if (respons.trim() === "Pushed") {

                    alert("Application(s) Assign successfully...");
                    var location = window.location.origin + "/Application/MyDesk";
                    window.location.href = location;
                }
                else {
                    $("#AppDropInfo").text(respons);
                    $(".push").removeAttr("disabled");
                }
            });
        }
    }
}




/*
 * View application
 */ 
function ViewApplication(desk_id, process_id) {

    $.getJSON("/Helpers/GetEncrypt", { "desk_id": desk_id, "process_id": process_id }, function (response) {
        var r = response.split("|");
        var location = window.location.origin + "/Application/ViewApplication/" + r[0] + "/" + r[1];
        window.location.href = location;
    });
}



/*
 * Get applicatioon report for edit
 */
function GetReport(ReportID) {

    $("#txtEditReportID").val(ReportID);
   
    $.getJSON("/Process/GetReport", { "ReportID": ReportID }, function (response) {
        var res = $.trim(response).split("|");

        if (res[0] === "1") {
            $("#txtEditAppReport").val(res[1]);
            $("#txtEditReportTitle").val(res[2]);
        }
        else {
            ErrorMessage("#ReportModalInfo", res[1]);
        }
    });
}


/*
 * Deleteing an application report
 */
function DeleteReport(reportID) {
    var msg = confirm("Are you sure you want to delete this report?");

    if (msg === true) {
        $.getJSON("/Process/DeleteReport", { "ReportID": reportID }, function (response) {
            if (response === "Report Deleted") {
                alert("Application report removed successfully.");
                $("#divReport").load(location.href + " #divReport");
            }
            else {
                ErrorMessage("#AppViewInfo", response);
            }
        });
    }
}



/*
 * Getting schdule
 */
function GetSchdule(schduleID) {

    $("#txtEditSchduleID").val(schduleID);

    $.getJSON("/Application/GetSchdule", { "schduleID": schduleID }, function (response) {
        var res = $.trim(response).split("|");

        if (res[0] === "1") {
            $("#txtEditSchduleComment").val(res[1]);
            $("#txtEditSchduleDate").val(res[2]);
            $("#txtEditSchduleType").val(res[3]).change();
            $("#txtEditInspectionType").val(res[5]).change();
            $("#txtEditSchduleLocation").val(res[4]).change();
            $("#txtEditStaffListComment").val(res[6]);
        }
        else {
            ErrorMessage("#EditSchduleModalInfo", res[1]);
        }
    });
}


/*
 * Delete a schdule
 */
function DeleteSchdule(schduleID) {

    var msg = confirm("Are you sure you want to delete this schedule?");

    if (msg === true) {

        $.getJSON("/Application/DeleteSchdule", { "schduleID": schduleID }, function (response) {

            if (response === "Schdule Deleted") {
                alert("Application schedule removed successfully.");
               
                $("#divSchdule").load(location.href + " #divSchdule");
            }
            else {
                ErrorMessage("#AppViewInfo", response);
            }
        });
    }
}






/*
 * Re route applicationn to another staff
 */
function RerouteApps(id) {
  


    var apps = $("#Insshowpush").val().split(",");

    var previousStaff = $("#txtOriginalStaffID").val();

    var msg = confirm("Are you sure you want to re-route application(s) to this staff?");

    if (msg === true) {

        $(".push").attr("disable", "disabled");

        $.post("/Deskes/RerouteApps", { "staffID": id, "previousStaff": previousStaff, "AppID": apps }, function (respons) {
            var result = respons.trim().split("|");

            if (result[0] === "1") {
                alert(result[1]);
                var location = window.location.origin + "/Deskes/StaffDesk";
                window.location.href = location;
            }
            else {
                ErrorMessage("#InsAppDropInfo", result[1]);
                $(".push").removeAttr("disabled");
            }
        });
    }
}




/*
 * Deleteting extra payment
 */
function DeleteExtraPay(id) {

    var data = new FormData();

    data.append("ExtraPayID", id);

    $.confirm({
        text: "Are you sure you want to delete this extra payment ?",
        columnClass: 'medium',
        closeIcon: true,
        confirm: function () {
            debugger;
            $.ajax({
                url: "/Payment/DeleteExtraPay",
                type: "POST",
                contentType: false,
                processData: false,
                data: data,
                success: function (response) {
                    if (response === "Extra Deleted") {
                        alert("Application extra payment removed successfully.");
                        $("#divExtraPayment").load(location.href + " #divExtraPayment");
                    }
                    else {
                        alert(response);
                    }
                }
            });
          

        },
        cancel: function () {
        },
        confirmButton: "Yes",
        cancelButton: "No"
    })

}


/*
 * Document Validity action
 * 
 */
function ValidityAction(encryptid, option, id) {

    if (encryptid === "" || option === "" || id === "") {
        alert("This operation cannot be performed, some reference parameter needs to be entered.");
    }
    else {
        var msg = confirm("Are you sure you want to mark this document as " + option + "?");

        if (msg === true) {

            $.getJSON("/Application/DocValidity",
                {
                    "SubmittedDocID": encryptid,
                    "Option": option,
                },
                function (response) {

                    if (response === "Done") {
                        alert("This application document successfully marked as " + option + ".");
                        $("#divValidity_" + id).text(option);
                    }
                    else {
                        alert(response);
                    }
                });
        }
    }
}
