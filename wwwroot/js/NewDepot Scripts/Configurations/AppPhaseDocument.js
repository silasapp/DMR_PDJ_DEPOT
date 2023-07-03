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


    // selecting all App Documents 
    $("#txtDocs").ready(function () {
        var html = "";

        $("#txtDocs").html("");

        $.getJSON("/Helpers/GetAppDocs",
            { "deletedStatus": false },
            function (datas) {

                $("#txtDocs").append("<option disabled selected>--Select App Doc--</option>");
                $("#txtEditDocs").append("<option disabled selected>--Select App Doc--</option>");

                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.appDocId + ">" + val.docName + "</option>";
                    });
                $("#txtDocs").append(html);
                $("#txtEditDocs").append(html);
            });
    });


    // selecting all Stages 
    $("#txtStagess").ready(function () {
        var html = "";

        $("#txtStagess").html("");
        $("#txtEditStagess").html("");

        $.getJSON("/Helpers/GetAppStages",
            { "deletedStatus": false },
            function (datas) {

                $("#txtStagess").append("<option disabled selected>--Select Stages--</option>");
                $("#txtEditStagess").append("<option disabled selected>--Select Stages--</option>");

                $.each(datas,
                    function (key, val) {
                        html += "<option value=" + val.appStageId + ">" + val.stageName + "</option>";
                    });
                $("#txtStagess").append(html);
                $("#txtEditStagess").append(html);

            });
    });


    // Create Stage => Document relationship

    $("#FormCreateStageDoc").on('submit', function (event) {

        event.preventDefault();

        var msg = confirm("Are you sure you want to create this Stage => Document relationship?");

        if (msg === true) {
            $.getJSON("/AppStageDocuments/CreateStageDocuments",
                {
                    "DocID": $("#txtDocs").val(), "StageID": $("#txtStagess").val()
                },
                function (response) {
                    if ($.trim(response) === "StageDoc Created") {
                        SuccessMessage("#StageDocInfo", "Stage and Document relationship created successfully");
                        $("#FormCreateStageDoc")[0].reset();
                        StageDocTable.ajax.reload();
                    }
                    else {
                        ErrorMessage("#StageDocInfo", response);
                    }
                });
        }
    });


    // geting list of Zone => State relationship

    var StageDocTable = $("#TableStageDoc").DataTable({

        ajax: {
            url: "/AppStageDocuments/GetStageDoc",
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
            { data: "stageDocID", "visible": false, "searchable": false },
            { data: "stageID", "visible": false, "searchable": false },
            { data: "docID", "visible": false, "searchable": false },
            { data: "stageName" },
            { data: "docName" },
            { data: "docType" },
            { data: "createdAt" },
            { data: "updatedAt" },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-xs btn-primary stagedoc" + row["stageDocID"] + "\" id=\"" + row['docName'] + "|" + row["stageName"] + "|" + row["docID"] + "|" + row["stageID"] + "\" onclick=\"getStageDoc(" + row["stageDocID"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteStageDoc(" + row["stageDocID"] + ")\"> <i class=\"fa fa-trash-o\"> </i> Delete </button>";
                }
            }
        ]
    });



    $("#FormEditStageDocReset").on('click', function (event) {
        event.preventDefault();
        $("#relationshipStatus").html("");
        $("#FormEditStageDoc")[0].reset();
    });


    // Editing Document => Stage relationship

    $("#FormEditStageDoc").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditStageDocID").val() === "") {
            ErrorMessage("#EditStageDocInfo", "Please select a relationship to edit.");
        }
        else {
            var msg = confirm("Are you sure you want to edit this relationship?");

            if (msg === true) {
                $.getJSON("/AppStageDocuments/EditStageDocuments",
                    {
                        "DocID": $("#txtEditDocs").val(),
                        "StageID": $("#txtEditStagess").val(),
                        "StageDocID": $("#txtEditStageDocID").val()
                    },
                    function (response) {

                        if ($.trim(response) === "StageDoc Updated") {
                            SuccessMessage("#EditStageDocInfo", "Updated successfully.");
                            $("#FormEditStageDoc")[0].reset();
                            $("#relationshipStatus").html("");
                            StageDocTable.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditStageDocInfo", response);
                        }
                    });
            }
        }
    });
});

/*
 * Getting Zone => state info for relationship editing
 */
function getStageDoc(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateStageDoc").offset().top
    }, 1000);
    var stagedoc_id = $("#txtEditStageDocID").val(id);
    var typeStage = $(".stagedoc" + id).attr("id").split('|');

    var doc = typeStage[0];
    var stage = typeStage[1];
    var doc_id = typeStage[2];
    var stage_id = typeStage[3];

    var html = "";

    $("#relationshipStatus").html("");
    $("#txtEditDocs").change();
    $("#txtEditStagess").change();

    html = "<span class=\"ui tiny orange label\" >" + stage + "</span> => <span class=\"ui tiny blue label\">" + doc + "</span>";

    $("#relationshipStatus").append(html);
    $("#txtEditDocs").val(doc_id).change();
    $("#txtEditStagess").val(stage_id).change();

}


/*
 * Removing a Stage => Doc.
 */
function DeleteStageDoc(id) {

    var msg = confirm("Are you sure you want to remove this Stage => Document?");

    if (msg === true) {
        $.getJSON("/AppStageDocuments/DeleteStageDocument", { "StageDocID": id }, function (response) {
            if ($.trim(response) === "StageDoc Deleted") {
                alert("Stage => Doc removed successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}