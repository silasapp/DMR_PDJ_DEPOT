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



    // Create Application product 
    $("#FormCreateAppProduct").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtAppProductName").val() === "" || $("#txtAppProductShortName").val() === "") {
            ErrorMessage("#CreateAppProductInfo", "Please enter an App product name and short name");
        }
        else {
            var msg = confirm("Are you sure you want to create this App Product?");

            if (msg === true) {

                $.post("/ApplicationProducts/CreateProduct",
                    {
                        "Name": $("#txtAppProductName").val(),
                        "FriendlyName": $("#txtAppProductShortName").val(),
                    },

                    function (response) {
                        if ($.trim(response) === "Created") {
                            SuccessMessage("#CreateAppProductInfo", "Application product created successfully");
                            $("#FormCreateAppProduct")[0].reset();
                            AppProductTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#CreateAppProductInfo", response);
                        }
                    });
            }
        }
    });


    // geting list of application products
    var AppProductTables = $("#TableAppProduct").DataTable({

        ajax: {
            url: "/ApplicationProducts/GetProduct",
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
            { data: "productID", "visible": false, "searchable": false },
            { data: "productName" },
            { data: "shortName" },
            { data: "createdAt" },
            { data: "updatedAt" },
            
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-xs btn-primary app" + row["productID"] + "\" id=\"" + row['productName'] + " | " + row['shortName'] + "\"  onclick=\"getAppProduct(" + row["productID"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteAppProduct(" + row["productID"] + ")\"> <i class=\"fa fa-trash\"> </i> Delete </button>";
                }
            }
        ]

    });


    // Editing App Product
    $("#FormEditAppProduct").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditAppProductID").val() === "") {
            ErrorMessage("#EditAppProductInfo", "Please select an application product to edit.");
        }
        else {
            var msg = confirm("Are you sure you want to edit this application product?");

            if (msg === true) {

                var AppProducts = {

                    Name: $("#txtEditAppProductName").val().toUpperCase().trim(),
                    ShortName: $("#txtEditAppProductShortName").val().toUpperCase().trim(),
                };

                $.post("/ApplicationProducts/EditProduct",
                    {
                        "ProductId": $("#txtEditAppProductID").val(),
                        "Name": $("#txtEditAppProductName").val().toUpperCase().trim(),
                        "FriendlyName": $("#txtEditAppProductShortName").val().toUpperCase().trim(),                    },
                    function (response) {
                        if ($.trim(response) === "Updated") {
                            SuccessMessage("#EditAppProductInfo", "Application product updated successfully.");
                            $("#FormEditAppProduct")[0].reset();
                            AppProductTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditAppProductInfo", response);
                        }
                    });
            }
        }
    });
});


function getAppProduct(id) {
    $('html, body').animate({
        scrollTop: $("#FormEditAppProduct").offset().top
    }, 1000);
    var app_id = $("#txtEditAppProductID").val(id);
    var app = $(".app" + id).attr("id").split("|");
    $("#txtEditAppProductName").val(app[0]);
    $("#txtEditAppProductShortName").val(app[1]);
}

/*
 * Removing App Stage.
 */
function DeleteAppProduct(id) {
    var msg = confirm("Are you sure you want to remove this Application product ?");
    if (msg === true) {
        $.getJSON("/ApplicationProducts/DeleteProduct", { "ProductID": id }, function (response) {
            if ($.trim(response) === "Deleted") {
                alert("Application product removed successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}