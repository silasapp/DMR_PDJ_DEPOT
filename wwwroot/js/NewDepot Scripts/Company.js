
$(function () {

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





    var appTable = $("#MyAppsTable").DataTable({

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
        "ordering": false,
        "info": true,
        "searching": true,
        "processing": true,

    });


    var permitTable = $("#MyPermitsTable").DataTable({

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

        "paging": true,
        "ordering": false,
        "info": true,
        "searching": true,
        "processing": true,

    });


    var FacilityTable = $("#MyFacilityTable").DataTable({

        lengthMenu: [[50, 150, 250, 500], [50, 150, 250, 500]],
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

        language: {
            buttons: {
                colvis: 'Change columns'
            }
        },

        "paging": true,
        "ordering": false,
        "info": true,
        "searching": true,
        "processing": true,

    });




    var FacilityTable2 = $("#MyFacilityTable2").DataTable({

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

        language: {
            buttons: {
                colvis: 'Change columns'
            }
        },

        "paging": true,
        "ordering": true,
        "info": true,
        "searching": true,
        "processing": true,

    });



    /*
     * Get company profile
     */
    $("#CompanyProfile").ready(function () {

        $("#CompanyDiv").addClass("loader");

        $.getJSON("/Companies/GetCompanyProfile", { "CompanyId": $("#CompId").val() }, function (data) {
            if (data === "Network Error") {
                ErrorMessage("#FormCompanyProfileInfo", "A Network error has occured. Please check your network.");
                $("#CompanyDiv").removeClass("loader");
            }
            else {
                
                var datas = $.parseJSON($.parseJSON(data));
                
                $("#txtId").val(datas.id);
                //$("#txtCompId").val(datas.id);
                $("#txtCompanyName").val(datas.name);
                $("#txtBusinessType").val(datas.business_Type);
                $("#txtCompanyEmail").val(datas.user_Id);
                $("#txtContactFirstName").val(datas.contact_FirstName);
                $("#txtContactLastName").val(datas.contact_LastName);
                $("#txtContactNo").val(datas.contact_Phone);
                $("#txtRegNo").val(datas.rC_Number);
                $("#txtTinNo").val(datas.tin_Number);
                $("#txtYear").val(datas.year_Incorporated);
                $("#txtRegAddressId").val(datas.registered_Address_Id);
                $("#txtOperationalAddressId").val(datas.operational_Address_Id);
                $("#txtTotalAssets").val(datas.total_Asset);
                $("#txtStaffNo").val(datas.no_Staff);
                $("#txtYearlyRevenue").val(datas.yearly_Revenue);
                $("#txtExpatriateNo").val(datas.no_Expatriate);

                $("#txtNationality").append('<option value="' + datas.nationality + '" selected="selected">' + datas.nationality + '</option>');

                $("#CompanyDiv").removeClass("loader");
            }
        });

    });




    // updating company profile 
    $("#FormCompanyProfile").on('submit', function (event) {

        event.preventDefault();

        var msg = confirm("Are you sure you want to update this company information?");

        $("#CompanyProfile").addClass('Submitloader');


        if (msg === true) {

            var data = new FormData();

            data.append("id", $("#txtId").val()); // company id
            data.append("user_Id", $("#txtCompanyEmail").val());
            data.append("name", $("#txtCompanyName").val());
            data.append("business_Type", $("#txtBusinessType").val());
            data.append("contact_FirstName", $("#txtContactFirstName").val().toUpperCase());
            data.append("contact_LastName", $("#txtContactLastName").val().toUpperCase());
            data.append("contact_Phone", $("#txtContactNo").val());
             data.append("rC_Number", $("#txtRegNo").val());
             data.append("tin_Number", $("#txtTinNo").val());
             data.append("year_Incorporated", $("#txtYear").val());
             data.append("nationality", $("#txtNationality").val());
            data.append("registered_Address_Id", $("#txtRegAddressId").val());
            data.append("operational_Address_Id", $("#txtOperationalAddressId").val());
            data.append("total_Asset", $("#txtTotalAssets").val());
            data.append("no_Staff", $("#txtStaffNo").val());
            data.append("yearly_Revenue", $("#txtYearlyRevenue").val());
            data.append("no_Expatriate", $("#txtExpatriateNo").val());
            
            debugger;
            $.ajax({
                url: "/Companies/UpdateCompanyProfile",
                type: "POST",
                data: data,
                contentType: false,
                processData: false,
                dataType: "json",
                cache: false,
                success: function (response) {

                    if ($.trim(response) === "Company Updated") {
                        $("#CompanyProfile").removeClass('Submitloader');
                        SuccessMessage("#FormCompanyProfileInfo", "Company's profile updated successfully.");
                    }
                    else {
                        ErrorMessage("#FormCompanyProfileInfo", response);
                        $("#CompanyProfile").removeClass('Submitloader');

                    }
                },
                failure: function (data) {
                    alert("Failure");
                }
            });
        }
    });



    // checking for registered and operational address 
    $("#divProfile").on('click', function (event) {
        event.preventDefault();

        if (($("#txtRegAddressId").val() === "" && $("#txtOperationalAddressId").val() === "") || ($("#txtRegAddressId").val() === "0" && $("#txtOperationalAddressId").val() === "0")) {
            $("#OpenRegAddressModal").click();
        }
        else {
            $("#CompanyAddress").addClass("loader");

            var address_id = "";
            var addressType = "";

            if ($("#txtRegAddressId").val() === "") {

                if ($("#txtOperationalAddressId").val() === "") {
                    address_id = "";
                    addressType = "operational";
                }
                else {
                    address_id = $("#txtOperationalAddressId").val();
                    addressType = "operational";
                }
            }
            else {
                address_id = $("#txtRegAddressId").val();
                addressType = "registered";
            }

            $.getJSON("/Helpers/GetCompanyAddress",
                {
                    "address_id": address_id
                },
                
                function (data) {
                    
                    if (data !== "Empty") {
                        var datas = $.parseJSON($.parseJSON(data));

                        $("#txtUpAddressID").val(datas.id);
                        $("#txtUpAddressType").val(addressType);
                        $("#txtUpStreet").val(datas.address_1);
                        $("#txtUpCity").val(datas.city);
                        $("#txtUpPostalCode").val(datas.postal_code);

                        $("#txtUpCountry").append('<option value="' + datas.country_Id + '" selected="selected">' + datas.countryName + '</option>');
                        $("#txtUpState").append('<option value="' + datas.stateId + '" selected="selected">' + datas.stateName + '</option>');
                        $("#CompanyAddress").removeClass("loader");
                    }
                    else {

                        if (datas === "Empty") {
                            ErrorMessage("#FormCompanyAddressInfo", "No Address found. Address is empty, please try again.");
                        }
                        else if (datas === "Network Error") {
                            ErrorMessage("#FormCompanyAddressInfo", "A network error occured, please check your network.");
                        }
                        else {
                            ErrorMessage("#FormCompanyAddressInfo", datas);
                        }
                        $("#CompanyAddress").removeClass("loader");
                    }

                    $("#CompanyAddress").removeClass("loader");
                });
        }
    });


    // get sates function by country id
    function GetStates(country_id, state_id, loader_id) {

        var html = "";
        $(state_id).html("");
        $(loader_id).text("Getting States....");

        $.getJSON("/Helpers/GetStates", { 'CountryId': $(country_id).val() }, function (datas) {

            debugger;

            $(state_id).append("<option disabled selected>--Select States--</option>");
            $.each(datas,
                function (key, val) {
                    html += "<option value=" + val.state_id + ">" + val.stateName + "</option>";
                });

            $(state_id).append(html);

            $(loader_id).text("");
        });
    }


    // geting states on country change - creating address
    $("#txtARegCountry").on('change', function (event) {
        event.preventDefault();
        GetStates("#txtARegCountry", "#txtARegState", "#StateLoader");
    });

    // geting states on country change - updating address
    $("#txtUpCountry").on('change', function (event) {
        event.preventDefault();
        GetStates("#txtUpCountry", "#txtUpState", "#UStateLoader");
    });

    // geting states on country change - creating director
    $("#txtRegDCountry").on('change', function (event) {
        event.preventDefault();
        GetStates("#txtRegDCountry", "#txtRegDState", "#RegDStateLoader");
    });

    // geting states on country change - geting director
    $("#txtDCountry").on('change', function (event) {
        event.preventDefault();
        GetStates("#txtDCountry", "#txtDState", "#DStateLoader");
    });




    //Creating a new Address for a company
    $("#FormRegAddress").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to save this new address ?");

        if (msg === true) {

                var data = new FormData();

                data.append("CompanyId", $("#txtId").val());
                data.append("address_1", $("#txtAAddress1").val());
                data.append("city", $("#txtACity").val());
                data.append("postal_code", $("#txtAPostalCode").val());
                data.append("stateId", $("#txtARegState").val());
                data.append("country_Id", $("#txtARegCountry").val());
                data.append("type", "registered");

                debugger;
                $.ajax({
                    url: "/Companies/CreateCompanyAddress",
                    type: "POST",
                    data: data,
                    contentType: false,
                    processData: false,
                    dataType: "json",
                    cache: false,
                    success: function (response) {


                        if ($.trim(response) === "Created Address") {
                            SuccessMessage("#FormRegAddressInfo", "Company's address created successfully.");
                            window.location.reload(true);
                        }
                        else {
                            ErrorMessage("#FormRegAddressInfo", response);
                        }
                    },
                    failure: function (data) {
                        ErrorMessage("#FormRegAddressInfo", "Process failed...");
                    }
                });

            }
    });


    /*
     * Updating Company Address
     */
    $("#FormAddress").on('submit', function (event) {
        event.preventDefault();

       
            $.confirm({
                text: "Are you sure you want to update this address ?",
                confirm: function () {

                    var address = new FormData();
                    address.append("id", $("#txtUpAddressID").val());
                    address.append("address_1", $("#txtUpStreet").val());
                    address.append("city", $("#txtUpCity").val());
                    address.append("postal_code", $("#txtUpPostalCode").val());
                    address.append("stateId", $("#txtUpState").val());
                    address.append("country_Id", $("#txtUpCountry").val());
                    address.append("type", $("#txtUpAddressType").val());
                    

                    $("#CompanyAddress").addClass('Submitloader');

                    
                    $.ajax({
                        url: "/Companies/UpdateCompanyAddress",
                        type: "POST",
                        data: address,
                        contentType: false,
                        processData: false,
                        dataType: "json",
                        cache: false,
                        success: function (response) {
                            if ($.trim(response) === "Address Updated") { 
                                $("#CompanyAddress").removeClass('Submitloader');
                                SuccessMessage("#FormCompanyAddressInfo", "Company's address updated successfully.");
                            }
                            else {
                                $("#CompanyAddress").removeClass('Submitloader');
                                ErrorMessage("#FormCompanyAddressInfo", response);
                            }
                        },
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


    });


    /*
     * Get Directors names 
     */
    $("#divDirectors").on('click', function (event) {
        event.preventDefault();

        $("#CompanyDirectors").addClass("loader");

        var html = "";
        $("#DirectorNames").html("");

        $.getJSON("/Companies/GetDirectorsNames", { "CompanyId": $("#txtId").val() }, function (datas) {

            if (datas === "Network Error") {
                ErrorMessage("#CompanyDirectorsInfo", "A Network error has occured. Please check your network.");
                $("#CompanyDirectors").removeClass("loader");
            }
            else if (datas === null || datas === "" || datas.length === 0) {
                ErrorMessage("#CompanyDirectorsInfo", "No directors found. Please register your director(s)");
                $("#CompanyDirectors").removeClass("loader");
            }
            else {
                
                 datas = $.parseJSON($.parseJSON(datas));

                $.each(datas,
                    function (key, val) {
                        html += "<span class=\"btn btn-dark btn-block\" onclick=\"getDirectors(" + val.id + ")\"> <i class=\"ti-user\"> </i> " + val.lastName + " " + val.firstName + "</span>";
                    });

                $("#DirectorNames").append(html);
                $("#CompanyDirectors").removeClass("loader");
            }
        });

    });


    /*
     * Creating company directors
     */
    $("#FormRegDirectors").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to register this director to this company?");

        if (msg === true) {


            var data = new FormData();

            data.append("CompanyId", $("#txtId").val());
            data.append("company_Id", $("#txtId").val());
            data.append("firstName", $("#txtRegDFirstName").val().toUpperCase());
            data.append("lastName", $("#txtRegDLastName").val().toUpperCase());
            data.append("telephone", $("#txtRegDPhone").val().toUpperCase());
            data.append("nationality", $("#txtRegDNationality").val());

            data.append("address_1", $("#txtRegDAddress").val());
            data.append("city", $("#txtRegDCity").val());
            data.append("postal_Code", $("#txtRegDPostalCode").val());
            data.append("stateId", $("#txtRegDState").val());
            data.append("country_Id", $("#txtRegDCountry").val());
            
         
            debugger;
            $.ajax({
                url: "/Companies/CreateCompanyDirectors",
                type: "POST",
                data: data,
                contentType: false,
                processData: false,
                dataType: "json",
                cache: false,
                success: function (response) {

                    if ($.trim(response) === "Director Created") {
                        SuccessMessage("#FormRegDirectorInfo", "Company's director created successfully.");
                        window.location.reload(true);
                    }
                    else {
                        ErrorMessage("#FormRegDirectorInfo", response);
                    }
                },
                failure: function (data) {
                    ErrorMessage("#FormRegAddressInfo", "Process failed...");
                }
            });

        }
    });


    /*
     * Updating company's director information 
     */
    $("#FormUpdateDirectors").on('submit', function (event) {
        event.preventDefault();

        var msg = confirm("Are you sure you want to update this director information?");

        $("#CompanyDirectors").addClass('Submitloader');


        if (msg === true) {

            var Directors = [
                {
                    id: $("#txtUpDDirectorId").val(),
                    company_Id: $("#txtUpDCompanyId").val(),
                    firstName: $("#txtUpDFirstName").val().toUpperCase(),
                    lastName: $("#txtUpDLastName").val().toUpperCase(),
                    telephone: $("#txtUpDPhoneNo").val(),
                    nationality: $("#txtUpDNationality").val(),
                    address_id: $("#txtUpDAddressId").val(),
                    address:
                    {
                        address_1: $("#txtUpDAddress").val(),
                        city: $("#txtUpDCity").val(),
                        postal_Code: $("#txtUpDPostalCode").val(),
                        country_Id: $("#txtDCountry").val(),
                        stateId: $("#txtDState").val(),
                        id: $("#txtUpDAddressId").val()
                    }
                }
            ];

            $.post("/Companies/UpdateCompanyDirectors", { Directors }, function (response) {

                if ($.trim(response) === "Director Updated") {

                    $("#CompanyDirectors").removeClass('Submitloader');
                    SuccessMessage("#DirectorsInfo", "Company's director updated successfully.");
                }
                else {
                    $("#CompanyDirectors").removeClass('Submitloader');
                    ErrorMessage("#DirectorsInfo", response);
                }
            });
        }
    });






});



function getDirectors(id) {

    if (id === null || id === "" || id.length === 0 || id <= 0) {
        ErrorMessage("#CompanyDirectorsInfo", "No reccord found for this director.");
    }
    else {
        $("#divRegDirector").addClass("loader");

        $.getJSON("/Companies/GetDirectors", { "DirectorID": id }, function (datas) {

            if (datas === "Network Error") {
                ErrorMessage("#CompanyDirectorsInfo", "A Network error has occured. Please check your network.");
                $("#divRegDirector").removeClass("loader");
            }
            else if (datas === null || datas === "" || datas.length === 0) {
                ErrorMessage("#CompanyDirectorsInfo", "No directors found. Please register your director(s)");
                $("#divRegDirector").removeClass("loader");
            }
            else {
                
                var datas = $.parseJSON($.parseJSON(datas));

                $("#txtUpDCompanyId").val(datas.company_Id);
                $("#txtUpDAddressId").val(datas.address_Id);
                $("#txtUpDDirectorId").val(datas.id);
                $("#txtUpDFirstName").val(datas.firstName);
                $("#txtUpDLastName").val(datas.lastName);
                $("#txtUpDPhoneNo").val(datas.telephone);
                $("#txtUpDNationality").val(datas.nationality).change();
                $("#txtUpDAddress").val(datas.address.address_1);
                $("#txtUpDCity").val(datas.address.city);
                $("#txtDCountry").val(datas.address.country_Id).change();
                $("#txtUpDPostalCode").val(datas.address.postal_Code);
                $("#txtDState").val(datas.address.stateId).change();

                $("#divRegDirector").removeClass("loader");
            }
        });
    }
}

