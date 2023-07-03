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



    // Create Application Category 
    $("#FormCreateAppCategory").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtAppCategoryName").val() === "") {
            ErrorMessage("#CreateAppCategoryInfo", "Please enter an app category");
        }
        else {
            var msg = confirm("Are you sure you want to create this app category?");

            if (msg === true) {
                $.getJSON("/ApplicationCategories/CreateCategory",
                    {
                        "CategoryName": $("#txtAppCategoryName").val(),
                        "FriendlyName": $("#txtAppCategoryhortName").val(),
                    },

                    function (response) {
                        if ($.trim(response) === "Category Created") {
                            SuccessMessage("#CreateAppCategoryInfo", "Application Category created successfully");
                            $("#FormCreateAppCategory")[0].reset();
                            AppCategoryTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#CreateAppCategoryInfo", response);
                        }
                    });
            }
        }
    });


    // geting list of application Category
    var AppCategoryTables = $("#TableAppCategory").DataTable({

        ajax: {
            url: "/ApplicationCategories/GetAppCategory",
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
            { data: "catId", "visible": false, "searchable": false },
            { data: "categoryName" },
            { data: "friendlyName" },
            
            { data: "createdAt" },
            { data: "updatedAt" },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-xs btn-primary app" + row["catId"] + "\" id=\"" + row['categoryName'] + "|" + row["friendlyName"] +"\" onclick=\"getAppCategory(" + row["catId"] + ")\"> <i class=\"fa fa-edit\"> </i> Edit </button> <button class=\"btn btn-xs btn-danger\" target=\"\" onclick=\"DeleteAppCategory(" + row["catId"] + ")\"> <i class=\"fa fa-trash-o\"> </i> Delete </button>";
                }
            }
        ]

    });


    // Editing App Type
    $("#FormEditAppCategory").on('submit', function (event) {
        event.preventDefault();

        if ($("#txtEditAppCategoryID").val() === "") {
            ErrorMessage("#EditAppCategoryInfo", "Please select an Application Category to edit.");
        }
        else {
            var msg = confirm("Are you sure you want to edit this application Category?");

            if (msg === true) {

                $.getJSON("/ApplicationCategories/EditCategory",
                    {
                        "CategoryName": $("#txtEditAppCategoryName").val(),
                        "CatId": $("#txtEditAppCategoryID").val(),
                        "FriendlyName": $("#txtEditAppFriendlyName").val(),
                    },
                    function (response) {
                        if ($.trim(response) === "Category Updated") {
                            SuccessMessage("#EditAppCategoryInfo", "Application category updated successfully.");
                            $("#FormEditAppCategory")[0].reset();
                            AppCategoryTables.ajax.reload();
                        }
                        else {
                            ErrorMessage("#EditAppCategoryInfo", response);
                        }
                    });
            }
        }
    });
});


function getAppCategory(id) {
    $('html, body').animate({
        scrollTop: $("#FormCreateAppCategory").offset().top
    }, 1000);
    var app_id = $("#txtEditAppCategoryID").val(id);
    var app = $(".app" + id).attr("id").split("|");
    $("#txtEditAppCategoryName").val(app[0]);
    $("#txtEditAppFriendlyName").val(app[1]);
  
}

/*
 * Removing App Category.
 */
function DeleteAppCategory(id) {
    var msg = confirm("Are you sure you want to remove this Application Category ?");
    if (msg === true) {
        $.getJSON("/ApplicationCategories/DeleteAppCategory", { "CatId": id }, function (response) {
            if ($.trim(response) === "AppCategory Deleted") {
                alert("Application Category removed successfully...");
                window.location.reload();
            }
            else {
                alert(response);
            }
        });
    }
}