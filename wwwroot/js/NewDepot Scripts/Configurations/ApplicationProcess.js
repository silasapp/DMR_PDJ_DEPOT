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

   
    // Create Application Process 
    $("#FormCreateProcess").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to create this process?");

        if (msg === true) {

            var ApplicationProcess = {

                PhaseId: $("#txtPhase option:selected").val(),
                RoleId: $("#txtRole").val(),
                Sort: $("#txtSort").val(),
                LocationId: $("#txtLocation option:selected").val(),
              
            };


            $.post("/WorkProccesses/CreateProcess", { "Proccess": ApplicationProcess }, function (response) {
                if ($.trim(response) === "Process Created") {
                    SuccessMessage("#CreateProcessInfo", "Application process created successfuly.");
                    $("#FormCreateProcess")[0].reset();
                    AppProcessTables.ajax.reload();
                }
                else {
                    ErrorMessage("#CreateProcessInfo", response);
                }
            });
        }

    });


    // Edit Application Process
    $("#FormEditProcess").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditProcessID").val() === "") {

            ErrorMessage("#EditProccessInfo", "Please select the process to edit.");
        }
        else {

            var msg = confirm("Are you sure you want to update this process?");

            if (msg === true) {

                var ApplicationProcess = {

                    PhaseId: $("#txtEditPhase option:selected").val(),
                    RoleId: $("#txtEditRolee").val(),
                    Sort: $("#txtEditSort").val(),
                    LocationId: $("#txtEditLocationn option:selected").val(),
                
                };

                $.post("/WorkProccesses/EditProcess",
                    {
                        "processID": $("#txtEditProcessID").val(),
                        "Proccess": ApplicationProcess
                    },
                    function (response) {
                        if ($.trim(response) === "Process Updated") {
                            SuccessMessage("#EditProccessInfo", "Application process edited successfuly.");
                            $("#FormEditProcess")[0].reset();
                            AppProcessTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditProccessInfo", response);
                        }
                    });
            }
        }

    });




    // geting list of application documents
    var AppProcessTables = $("#ApplicationProcessTable").DataTable({

        ajax: {
            url: "/WorkProccesses/GetApplicationProcess",
            type: "POST",
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
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
        stateSave: true,

        columns: [
            { data: "processID", "visible": false, "searchable": false },
            { data: "phaseName" },
            { data: "roleName" },
            { data: "locationName" },
            { data: "sort" },
            { data: "createdAt" },
            //{ data: "createdBy", "orderable": false, "searchable": false },
            { data: "updatedAt" },
            //{ data: "updatedBy", "orderable": false, "searchable": false },
            {
                "render": function (data, type, row) {

                    return "<button class=\"btn btn-xs btn-primary process" + row["processID"] + "\" onclick=\"getProcess(" + row["processID"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteProcess(" + row["processID"] + ")\"> <i class=\"fa fa-trash\"> </i> Bin </button>";
                }
            }
        ]

    });

    
})

function getProcess(id) {

    $.getJSON("/WorkProccesses/GetProcess", { "processID": id }, function (datas) {

        if (datas.processID === 0) {
            alert(datas);
        }
        else {

            $('html, body').animate({
                scrollTop: $("#FormCreateProcess").offset().top
            }, 1000);
            $("#txtEditProcessID").val(id);
          
            $.each(datas,
                function (key, val) {
                    $("#txtEditPhaseVal").val(val.phaseID).change();
                    $("#txtEditPhaseVal").text(val.phaseName).change();
                    $("#txtEditRoleeVal").val(val.roleId).change();
                    $("#txtEditRoleeVal").text(val.roleName).change();
                    $("#txtEditSort").val(val.sort);
                    $("#txtEditLocationnVal").val(val.locationId).change();
                    $("#txtEditLocationnVal").text(val.locationName).change();
                  

                });
        }
    });
}

function DeleteProcess(id) {
    var msg = confirm("Are you sure you want to remove this Application Process ?");
    if (msg === true) {
        $.getJSON("/WorkProccesses/DeleteProcess", { "processID": id }, function (response) {
            if ($.trim(response) === "Process Removed") {
                alert("Application process removed successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}

