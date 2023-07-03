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


    // selecting all states to 
    $("#txtLGAState").ready(function () {
        var html = "";
        
        $("#txtLGAState").html("");

        $.getJSON("/Helpers/GetStates",
            { "CountryId": 156},
            function (datas) {

                $("#txtLGAState").append("<option disabled selected>--Select State--</option>");
                $("#txtEditLGAStateName").append("<option disabled selected>--Select State--</option>");
                $.each(datas,
                    function (key, val) {
                        debugger;
                        html += "<option value=" + val.state_id + ">" + val.stateName + "</option>";
                    });
                $("#txtLGAState").append(html);
                $("#txtEditLGAStateName").append(html);
            });
    });



    // Create LGA
    $("#FormCreateLGA").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtLGAName").val() === "") {
            ErrorMessage("#CreateLGAInfo", "Please enter a LGA");
        }
        else {
            var msg = confirm("Are you sure you want to create this LGA?");

            if (msg === true) {
                $.getJSON("/States/CreateLGA", { "StateID": $("#txtLGAState").val(), "Name": $("#txtLGAName").val(), "Code": $("#txtLGACode").val() }, function (response) {
                    if ($.trim(response) === "LGA Created") {
                        SuccessMessage("#CreateLGAInfo", "LGA created successfully");
                        $("#FormCreateLGA")[0].reset();
                        LGAsTables.ajax.reload();
                    }
                    else {
                        ErrorMessage("#CreateLGAInfo", response);
                    }
                });
            }
        }
    });


    // geting list of states
    var LGAsTables = $("#TableLGAs").DataTable({

        ajax: {
            url: "/States/GetLGAs",
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
            { data: "stateId", "visible": false, "searchable":false},
            { data: "lgaId", "visible": false, "searchable": false },
            { data: "lgaName" },
            { data: "lgaCode" },
            { data: "stateName" },

            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-primary LGA" + row["lgaId"] + "\" id=\"" + row['lgaName'] + "\" code=\"" + row['lgaCode'] + "\" state=\"" + row['stateId'] + "\" onclick=\"getLGA(" + row["lgaId"] + ")\"> <i class=\"ui edit icon\"> </i> Edit </button> <button class=\"btn btn-sm btn-danger button\" target=\"\" onclick=\"DeleteLGA(" + row["lgaId"] + ")\"> <i class=\"ui trash icon\"> </i> Delete </button>";
                }
            }

        ]
    
    });


    // Editing States
    $("#FormEditLGA").on('submit', function (event) {
        event.preventDefault();
        
        if ($("#EditStateName").val() === "") {
            ErrorMessage("#EditLGAInfo", "Please select a LGA to edit.");
        }
        else {
            var msg = confirm("Are you sure you want to edit this LGA?");

            if (msg === true) {
                $.getJSON("/States/EditLGA", { "Name": $("#txtEditLGAName").val(),"Code": $("#txtEditLGACode").val(), "ID": $("#txtEditLGAID").val(), "StateID": $("#txtEditLGAStateName").val() }, function (response) {
                    if ($.trim(response) === "LGA Updated") {
                        SuccessMessage("#EditLGAInfo", "LGA updated successfully.");
                        $("#FormEditLGA")[0].reset();
                        LGAsTables.ajax.reload();
                    }
                    else {
                        ErrorMessage("#EditLGAInfo", response);
                    }
                });
            }
        }
    });
});

/*
 * Getting LGA info for editing
 */
function getLGA(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateLGA").offset().top
    }, 1000);
    var LGA = $(".LGA" + id).attr("id");
    var Code = $(".LGA" + id).attr("code");
    var StateID = $(".LGA" + id).attr("state");
    debugger;
    $("#txtEditLGAStateName").val(StateID);
    $("#txtEditLGAID").val(id);
    $("#txtEditLGAName").val(LGA);
    $("#txtEditLGACode").val(Code);
}


/*
 * Removing a LGA.
 */ 
function DeleteLGA(id) {

    var msg = confirm("Are you sure you want to remove this LGA?");

    if (msg === true) {
        $.getJSON("/States/DeleteLGA", { "ID": id }, function (response) {
            if ($.trim(response) === "LGA Deleted") {
                alert("LGA removed successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}