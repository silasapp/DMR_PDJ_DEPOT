$(document).ready(function () {

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
  

    /*
     * creating legacy licence
     */
    $("#FormCreateNewLegacy").on('submit', function (event) {

        event.preventDefault();
        var LegacyDocuments = [];

            var LocalID = document.getElementsByName('txtLocalDocID[]');
            var DocSource = document.getElementsByName('txtDocSource[]');
            var CompDocElpsID = document.getElementsByName('txtCompDocElpsID[]');
            var missingCompDocElpsID = document.getElementsByName('missingCompElpsDocID[]');

            if (missingCompDocElpsID.length > 0) {

                // validating missing documents
                for (var i = 0; i < missingCompDocElpsID.length; i++) {
                    if (missingCompDocElpsID[i].value === "0" || missingCompDocElpsID[i].value === "") {
                        ErrorMessage("#divUploadDocInfo", "Please upload all required documents");
                        break;
                    }
                    else {
                        alert("Inputs not found...");
                    }
                }
            }
            else {

                for (var j = 0; j < LocalID.length; j++) {

                    LegacyDocuments.push({
                        "LocalDocID": LocalID[j].value.trim(),
                        "CompElpsDocID": CompDocElpsID[j].value.trim(),
                        "DocSource": DocSource[j].value.trim()
                    });
                }


                if ($("#txtIssuedDate").val() === "" || $("#txtExpiryDate").val() === "") {
                    alert("You need to enter the issued date and the expiry date for the permit.");
                }
                else {

                 
                    var legacy = {
                        LicenseNo: $("#txtLicenceNumber").val(),
                        FacilityName: $("#txtFacilityName").val(),
                        FacilityAddress: $("#txtFacilityAddress").val(),
                        State: $("#txtFacilityState").val(),
                        Lga: $("#txtFacilityLGA option:selected").val(),
                        City: $("#txtFacilityCity").val(),
                        LandMeters: $("#txtMeters").val(),
                        isPipeLine: $("#seltxtPipeLine").val(),
                        isHighTension: $("#seltxtHighTention").val(),
                        IsHighWay: $("#seltxtHighWay").val(),
                        ContactName: $("#txtContactName").val(),
                        ContactName: $("#txtContactName").val(),
                        AppType: $("#seltxtAppTypes").val(),
                        Issue_Date: $("#txtIssuedDate").val(),
                        Exp_Date: $("#txtExpiryDate").val(),
                    };
                    var data = new FormData();
                  
                    data.append("state", $("#txtFacilityState").val());
                    data.append("LicenseNo", $("#txtLicenceNumber").val());
                    data.append("FacilityName", $("#txtFacilityName").val());
                    data.append("FacilityAddress", $("#txtFacilityAddress").val());
                    data.append( "State", $("#txtFacilityState").val());
                    data.append("AppType", $("#seltxtAppTypes").val());
                    data.append("Lga", $("#txtFacilityLGA option:selected").val());
                    data.append("City", $("#txtFacilityCity").val());
                    data.append("LandMeters", $("#txtMeters").val());
                    data.append("isPipeLine", $("#seltxtPipeLine").val());
                    data.append("isHighTension", $("#seltxtHighTention").val());
                    data.append("IsHighWay", $("#seltxtHighWay").val());
                    data.append("ContactName", $("#txtContactName").val());
                    data.append("ContactPhone", $("#txtContactPhone").val());
                    data.append("Issue_Date", $("#txtIssuedDate").val());
                    data.append("Exp_Date", $("#txtExpiryDate").val());

                    debugger;

                    for (var i = 0; i < LegacyDocuments.length; i++) {
                        data.append("legacyDocuments[" + i + "].LocalDocID", LegacyDocuments[i].LocalDocID);
                        data.append("legacyDocuments[" + i + "].CompElpsDocID", LegacyDocuments[i].CompElpsDocID);
                        data.append("legacyDocuments[" + i + "].DocSource", LegacyDocuments[i].DocSource);
                    }
                    $.confirm({
                        text: "Are you sure you want to submit this legacy application ?",
                        confirm: function () {
                            $("#FormCreateNewLegacy").addClass("Submitloader");
                          
                            $.ajax({
                                url: "Create",
                                type: "POST",
                                contentType: false,
                                processData: false,
                                data: data,
                                success: function (response) {
                                    var responsee = $.trim(response);
                                    
                                    if (responsee.indexOf("Legacy Submitted") > -1) {
                                        var LegacyId = $.trim(response).split("|")[1];

                                            alert("Your legacy licence application has been saved successfully.");
                                            SuccessMessage("#FormAddLegacyInfo", "Your legacy licence application has been saved successfully.");
                                            var location = window.location.origin + "/Legacies/UploadLegacyDocument/"+LegacyId;
                                            window.location.href = location;
                                            $("#FormCreateNewLegacy").removeClass("Submitloader");
                                        }
                                        else {
                                            ErrorMessage("#FormAddLegacyInfo", response);
                                            $("#FormCreateNewLegacy").removeClass("Submitloader");
                                        }
                                    }
                                ,
                                failure: function (data) {
                                    alert("Failure");
                                }
                            });
                        },
                        cancel: function () {

                        },
                        confirmButton: "Yes",
                        cancelButton: "No"
                    })
            
                }
            }
        
    });
    $("#FormContinueLegacy").on('submit', function (event) {

        
        event.preventDefault();
        var LegacyDocuments = [];

            var LocalID = document.getElementsByName('txtLocalDocID[]');
            var DocSource = document.getElementsByName('txtDocSource[]');
            var CompDocElpsID = document.getElementsByName('txtCompDocElpsID[]');
            var missingCompDocElpsID = document.getElementsByName('missingCompElpsDocID[]');

            if (missingCompDocElpsID.length > 0) {

                // validating missing documents
                for (var i = 0; i < missingCompDocElpsID.length; i++) {
                    if (missingCompDocElpsID[i].value === "0" || missingCompDocElpsID[i].value === "") {
                        ErrorMessage("#divUploadDocInfo", "Please upload all required documents");
                        break;
                    }
                    else {
                        alert("Inputs not found...");
                    }
                }
            }
            else {

                for (var j = 0; j < LocalID.length; j++) {

                    LegacyDocuments.push({
                        "LocalDocID": LocalID[j].value.trim(),
                        "CompElpsDocID": CompDocElpsID[j].value.trim(),
                        "DocSource": DocSource[j].value.trim()
                    });
                }


                if ($("#txtIssuedDate").val() === "" || $("#txtExpiryDate").val() === "") {
                    alert("You need to enter the issued date and the expiry date for the permit.");
                }
                else {

                 
                    var legacy = {
                        LicenseNo: $("#txtLicenceNumber").val(),
                        FacilityName: $("#txtFacilityName").val(),
                        FacilityAddress: $("#txtFacilityAddress").val(),
                        State: $("#txtFacilityState").val(),
                        Lga: $("#txtFacilityLGA option:selected").val(),
                        City: $("#txtFacilityCity").val(),
                        LandMeters: $("#txtMeters").val(),
                        isPipeLine: $("#seltxtPipeLine").val(),
                        isHighTension: $("#seltxtHighTention").val(),
                        IsHighWay: $("#seltxtHighWay").val(),
                        ContactName: $("#txtContactName").val(),
                        ContactName: $("#txtContactName").val(),
                        AppType: $("#seltxtAppTypes").val(),
                        Issue_Date: $("#txtIssuedDate").val(),
                        Exp_Date: $("#txtExpiryDate").val(),
                    };
                    var data = new FormData();
                  
                    data.append("Id", $("#txtLegacyID").val());
                    data.append("state", $("#txtFacilityState").val());
                    data.append("LicenseNo", $("#txtLicenceNumber").val());
                    data.append("FacilityName", $("#txtFacilityName").val());
                    data.append("FacilityAddress", $("#txtFacilityAddress").val());
                    data.append( "State", $("#txtFacilityState").val());
                    data.append("AppType", $("#seltxtAppTypes").val());
                    data.append("Lga", $("#txtFacilityLGA option:selected").val());
                    data.append("City", $("#txtFacilityCity").val());
                    data.append("LandMeters", $("#txtMeters").val());
                    data.append("isPipeLine", $("#seltxtPipeLine").val());
                    data.append("isHighTension", $("#seltxtHighTention").val());
                    data.append("IsHighWay", $("#seltxtHighWay").val());
                    data.append("ContactName", $("#txtContactName").val());
                    data.append("ContactPhone", $("#txtContactPhone").val());
                    data.append("Issue_Date", $("#txtIssuedDate").val());
                    data.append("Exp_Date", $("#txtExpiryDate").val());

                    var Lga = $("#txtFacilityLGA option:selected").val();

                    debugger;
                    for (var i = 0; i < LegacyDocuments.length; i++) {
                        data.append("legacyDocuments[" + i + "].LocalDocID", LegacyDocuments[i].LocalDocID);
                        data.append("legacyDocuments[" + i + "].CompElpsDocID", LegacyDocuments[i].CompElpsDocID);
                        data.append("legacyDocuments[" + i + "].DocSource", LegacyDocuments[i].DocSource);
                    }
                    $.confirm({
                        text: "Are you sure you want to submit this legacy application ?",
                        confirm: function () {
                            $("#FormContinueLegacy").addClass("Submitloader");
                          
                            $.ajax({
                                url: "/Legacies/Continue",
                                type: "POST",
                                contentType: false,
                                processData: false,
                                data: data,
                                success: function (response) {
                                    var responsee = $.trim(response);
                                    
                                    if (responsee.indexOf("Legacy Submitted") > -1) {
                                        var LegacyId = $.trim(response).split("|")[1];

                                            alert("Your legacy licence application has been saved successfully.");
                                        SuccessMessage("#FormContinueLegacyInfo", "Your legacy licence application has been saved successfully.");
                                            var location = window.location.origin + "/Legacies/UploadLegacyDocument/"+LegacyId;
                                            window.location.href = location;
                                            $("#FormContinueLegacy").removeClass("Submitloader");
                                        }
                                        else {
                                        ErrorMessage("#FormContinueLegacyInfo", response);
                                            $("#FormContinueLegacy").removeClass("Submitloader");
                                        }
                                    }
                                ,
                                failure: function (data) {
                                    alert("Failure");
                                }
                            });
                        },
                        cancel: function () {

                        },
                        confirmButton: "Yes",
                        cancelButton: "No"
                    })
            
                }
            }
        
    });



    /*
    * Editing and Resubmitting legacy licence
    */
    $("#FormEditLegacy").on('submit', function (event) {
        event.preventDefault();

        event.preventDefault();
        var LegacyDocuments = [];

        var LocalID = document.getElementsByName('txtLocalDocID[]');
        var DocSource = document.getElementsByName('txtDocSource[]');
        var CompDocElpsID = document.getElementsByName('txtCompDocElpsID[]');
        var missingCompDocElpsID = document.getElementsByName('missingCompElpsDocID[]');

        if (missingCompDocElpsID.length > 0) {

            // validating missing documents
            for (var i = 0; i < missingCompDocElpsID.length; i++) {
                if (missingCompDocElpsID[i].value === "0" || missingCompDocElpsID[i].value === "") {
                    ErrorMessage("#divUploadDocInfo", "Please upload all required documents");
                    break;
                }
                else {
                    alert("Inputs not found...");
                }
            }
        }
        else {

            for (var j = 0; j < LocalID.length; j++) {

                LegacyDocuments.push({
                    "LocalDocID": LocalID[j].value.trim(),
                    "CompElpsDocID": CompDocElpsID[j].value.trim(),
                    "DocSource": DocSource[j].value.trim()
                });
            }


            if ($("#txtIssuedDate").val() === "" || $("#txtExpiryDate").val() === "") {
                alert("You need to enter the issued date and the expiry date for the permit.");
            }
            else {

                var data = new FormData();

                data.append("state", $("#txtFacilityState").val());
                data.append("LicenseNo", $("#txtLicenceNumber").val());
                data.append("FacilityName", $("#txtFacilityName").val());
                data.append("FacilityAddress", $("#txtFacilityAddress").val());
                data.append("State", $("#txtFacilityState").val());
                data.append("AppType", $("#seltxtAppTypes").val());
                data.append("Lga", $("#txtFacilityLGA option:selected").val());
                data.append("City", $("#txtFacilityCity").val());
                data.append("LandMeters", $("#txtMeters").val());
                data.append("isPipeLine", $("#seltxtPipeLine").val());
                data.append("isHighTension", $("#seltxtHighTention").val());
                data.append("IsHighWay", $("#seltxtHighWay").val());
                data.append("ContactName", $("#txtContactName").val());
                data.append("ContactPhone", $("#txtContactPhone").val());
                data.append("Issue_Date", $("#txtIssuedDate").val());
                data.append("Exp_Date", $("#txtExpiryDate").val());
                data.append("LegacyID", $("#txtLegacyID").val());

                var Lga = $("#txtFacilityLGA option:selected").val();
                debugger;

                for (var i = 0; i < LegacyDocuments.length; i++) {
                    data.append("legacyDocuments[" + i + "].LocalDocID", LegacyDocuments[i].LocalDocID);
                    data.append("legacyDocuments[" + i + "].CompElpsDocID", LegacyDocuments[i].CompElpsDocID);
                    data.append("legacyDocuments[" + i + "].DocSource", LegacyDocuments[i].DocSource);
                }
                $.confirm({
                    text: "Are you sure you want to re-submit this legacy application ?",
                    confirm: function () {
                        $("#FormEditLegacy").addClass("Submitloader");


                      
                        $.ajax({
                            url: "/Legacies/LegacyResubmit",
                            type: "POST",
                            contentType: false,
                            processData: false,
                            data: data,
                            success: function (response) {
                                if ($.trim(response) === "Legacy Resubmitted") {
                                    alert("Your legacy licence application has been re-submitted successfully.");
                                    SuccessMessage("#FormAddLegacyInfo", "Your legacy licence application has been re-submitted successfully.");
                                    var location = window.location.origin + "/Legacies/MyLegacy";
                                    window.location.href = location;
                                    $("#FormEditLegacy").removeClass("Submitloader");
                                }
                                else {
                                    ErrorMessage("#FormAddLegacyInfo", response);
                                    $("#FormEditLegacy").removeClass("Submitloader");
                                }
                            }
                            ,
                            failure: function (data) {
                                alert("Failure");
                            }
                        });
                    },
                    cancel: function () {

                    },
                    confirmButton: "Yes",
                    cancelButton: "No"
                })

            }
        }

    });




function RemoveMistdoStaff(id) {

    var msg = confirm("Are you sure you want to remove this mistdo certified staff?");

    if (msg === true) {

        $("#FormMistdoStaff").addClass("Submitloader");

        $.post("/CompanyApplication/RemoveMistdoStaff", { "MistdoId": id }, function (response) {
            if (response.trim() === "Network Error") {
                $("#Error_" + fileID).html("<span class='text-danger'> No Network. Please check your network. </span>");
                $("#FormMistdoStaff").removeClass("Submitloader");
            }
            else {
                if (response.trim() === "Deleted") {
                    alert("Selected certified mistdo staff removed successfully");
                    $("#MistdoReload").load(location.href + " #MistdoReload");
                    $("#FormMistdoStaff").removeClass("Submitloader");
                }
                else {
                    alert(response);
                    $("#FormMistdoStaff").removeClass("Submitloader");
                }
            }
        });
    }
}


function toInt(str) {
    var i = parseInt(str);
    return i ? i : 0;
}

    $("#CreateLegacyApplication").on('submit', function (e) {
        e.preventDefault();

        if ($("#txtCompanyID").val() === "") {
            ErrorMessage("#CreateLegacyApplicationInfo", "Company not found.");
        }
        else {

            var LegacyDocuments = [];

            var LocalID = document.getElementsByName('txtLocalDocID[]');
            var DocSource = document.getElementsByName('txtDocSource[]');
            var CompDocElpsID = document.getElementsByName('txtCompDocElpsID[]');
            var missingCompDocElpsID = document.getElementsByName('missingCompElpsDocID[]');

            if (missingCompDocElpsID.length > 0) {

                // validating missing documents
                for (var i = 0; i < missingCompDocElpsID.length; i++) {
                    if (missingCompDocElpsID[i].value === "0" || missingCompDocElpsID[i].value === "") {
                        ErrorMessage("#divUploadDocInfo", "Please Upload all required documents");
                        break;
                    }
                    else {
                        alert("Inputs not found...");
                    }
                }
            }
            else {

                for (var j = 0; j < LocalID.length; j++) {

                    LegacyDocuments.push({
                        "LocalDocID": LocalID[j].value.trim(),
                        "CompElpsDocID": CompDocElpsID[j].value.trim(),
                        "DocSource": DocSource[j].value.trim()
                    });
                }

                var msg = confirm("Are you sure you want to create this legacy application?");

                if (msg === true) {

                    var LegacyApplication = {
                        SelfFacility: $("#txtOriginName").val().toUpperCase(),
                        SelfFacilityAddress: $("#txtOriginAddress").val(),
                        DestinationCompany: $("#txtDestinationCompanyName").val().toUpperCase(),
                        DestinationCompanyAddress: $("#txtDestinationCompanyAddress").val(),
                        DestinationFacility: $("#txtDestinationFieldName").val().toUpperCase(),
                        DestinationFacilityAddress: $("#txtDestinationFieldAddress").val(),
                        TransportationMode: $("#txtTransportMode").val(),
                        Qty: $("#txtQuantity").val(),
                        SelfFacilityState: $("#txtOriginState").val(),
                        DestinationFacilityState: $("#txtDestinationFieldState").val(),
                        OriginMesurementInstrument: $("#txtOriginMesurmentInstrument").val(),
                        DestinationMesurementInstrument: $("#txtDestinationMesurmentInstrument").val(),
                        IssuedDate: $("#txtIssuedDate").val(),
                        ExpiryDate: $("#txtExpiryDate").val(),
                        PermitNo: $("#txtLicenceNumber").val().toUpperCase(),
                        AppStageId: $("#txtLStages").val(),
                        IsSubmitted: true,
                        DeletedStatus: false
                    };

                    $("#CreateLegacyLoader").addClass("Submitloader");

                    $.post("/Legacies/CreateLegacyApplication",
                        {
                            "CompanyId": $("#txtCompanyID").val(),
                            "LegacyID": $("#txtLegacyID").val(),
                            "legacy": LegacyApplication,
                            "legacyDocuments": LegacyDocuments
                        }, function (response) {
                            if (response === "Created" || response === "Resubmitted") {
                                SuccessMessage("#CreateLegacyApplicationInfo", "Legacy application " + response + " successfully.");
                                $("#CreateLegacyLoader").removeClass("Submitloader");
                                window.location.href = window.location.origin + "/Legacies/MyLegacy";
                            }
                            else {
                                ErrorMessage("#CreateLegacyApplicationInfo", response);
                                $("#CreateLegacyLoader").removeClass("Submitloader");
                            }
                        });
                }
            }
        }
    });




    $("#btnRejectLegacys").on('click', function (e) {

        if ($("#txtLegacyID").val() === "" ) {
            ErrorMessage("#RejectLegacyModalInfo", "Legacy reference is empty.");
        }
        else if ($("#txtLegacyRejectionComment").val() === "") {
            ErrorMessage("#RejectLegacyModalInfo", "Please enter rejection comment for this application");
        }
        else {

            var msg = confirm("Are you sure you want to reject this legacy application?");

            if (msg === true) {

                $("#RejectLegacyLoader").addClass("Submitloader");

                $.post("/Legacies/RejectLegacy",
                    {
                        "LegacyID": $("#txtLegacyID").val(),
                        "txtComment": $("#txtLegacyRejectionComment").val()
                    }, function (response) {
                        if (response === "Rejected") {
                            SuccessMessage("#RejectLegacyModalInfo", "Legacy application " + response + " successfully.");
                            $("#RejectLegacyLoader").removeClass("Submitloader");
                            window.location.href = window.location.origin + "/Legacies/LegacyDesk";
                        }
                        else {
                            ErrorMessage("#RejectLegacyModalInfo", response);
                            $("#RejectLegacyLoader").removeClass("Submitloader");
                        }
                    });
            }

        }

    });





    $("#btnApproveLegacy").on('click', function (e) {

        if ($("#txtLegacyID").val() === "") {
            ErrorMessage("#ApproveLegacyModalInfo", "Legacy reference is empty.");
        }
        else if ($("#txtApproveLegacyComment").val() === "") {
            ErrorMessage("#ApproveLegacyModalInfo", "Please enter approval comment for this application");
        }
        else {

            var msg = confirm("Are you sure you want to approve this legacy application?");

            if (msg === true) {

                $("#AppApproveLegacyLoader").addClass("Submitloader");

                $.post("/Legacies/AcceptLegacy",
                    {
                        "LegacyID": $("#txtLegacyID").val(),
                        "txtComment": $("#txtApproveLegacyComment").val()
                    }, function (response) {
                        if (response === "Approved") {
                            SuccessMessage("#ApproveLegacyModalInfo", "Legacy application " + response + " successfully.");
                            $("#AppApproveLegacyLoader").removeClass("Submitloader");
                            window.location.href = window.location.origin + "/Legacies/LegacyDesk";
                        }
                        else {
                            ErrorMessage("#ApproveLegacyModalInfo", response);
                            $("#AppApproveLegacyLoader").removeClass("Submitloader");
                        }
                    });
            }

        }

    });

});