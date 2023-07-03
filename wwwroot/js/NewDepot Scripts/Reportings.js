$(document).ready(function () {
    //Section page load report

    $("#SearchTransactionTabless").ready(function () {

        var table = $("#SearchTransactionTabless").DataTable({

            ajax: {
                url: "/Reports/TransactionReport",
                type: "POST",
                data: function (d) {
                },
                dataType: "json",
            },
            lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],

            dom: 'Bfrtip',

            buttons: [
                'pageLength',
                'copyHtml5',
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
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $("td:first", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            "processing": true,
            "serverSide": true,

            'columnDefs': [{
                "targets": [12, 13, 14,15],
                render(v) {
                    return "₦" + formatNumber(Number(v).toFixed(2));
                }
            }],

            "columns": [
                { data: "count" },
                { data: "refNo" },
                { data: "rrr" },
                { data: "companyName" },
                { data: "facilities" },
                { data: "facilityAddress" },
                { data: "category" },
                { data: "type" },
                { data: "channel" },
                { data: "status" },
                { data: "state" },
                { data: "lga" },
                { data: "amount", "orderable": false, "searchable": false },
                { data: "extraAmount", "orderable": false, "searchable": false },
                { data: "serviceCharge", "orderable": false, "searchable": false },
                { data: "totalAmount", "orderable": false, "searchable": false },
                { data: "fieldOffice" },
                { data: "zonalOffice" },
                { data: "transDate" },
            ],

            "footerCallback": function (row, data, start, end, display) {
                var api = this.api();
                nb_cols = api.columns().nodes().length - 1;

                var j = 12;
                while (j < 15) {
                    var pageTotal = api
                        .column(j, { page: 'current' })
                        .data()
                        .sum(function (a, b) {
                            return (Number(a) + Number(b));
                        }, 0);
                    // Update footer

                    $(api.column(j).footer()).html('₦' + formatNumber(pageTotal.toFixed(2)));
                    j++;
                }
            },
       
            rowGroup: {
                endRender: function (rows, group) {

                    var a1 = GetSum(rows, 'amount');
                    var a2 = GetSum(rows, 'extraAmount');
                    var a3 = GetSum(rows, 'serviceCharge');
                    var aSum = GetSum(rows, 'totalAmount');

                    return $('<tr style="">')
                        .append('<td colspan="12" style="font-weight:bold; color:black" class=""><b>Sub Total for ' + group + '</b></td>')
                        .append('<td class="text-danger"><b>₦' + formatNumber(a1.toFixed(2)) + '</b></td>')
                        .append('<td class="text-danger"><b>₦' + formatNumber(a2.toFixed(2)) + '</b></td>')
                        .append('<td class="text-danger"><b>₦' + formatNumber(a3.toFixed(2)) + '</b></td>')
                        .append('<td class="text-danger"><b>₦' + formatNumber(aSum.toFixed(2)) + '</b></td>')
                        .append('<td ></td>')
                        .append('<td ></td>')
                        .append('<td ></td>')
                        .append('</tr>');

                },
                startRender: function (rows, group) {
                    return $('<tr>').append('<td colspan="3" class="text-left" style="background-color:black; color:white"> <b>' + group + '</b></td></tr>');
                },
                dataSrc: 'category'
            },
            orderFixed: [1, 'desc'],
            "bDestroy": true

        });



    });
    $("#SearchPermitTabless").ready(function () {
        $("#SearchPermitTabless").DataTable({

            ajax: {
                url: "/Reports/PermitReports",
                type: "POST",
                data: function (d) {
                },
                dataType: "json",
            },
            lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],

            scrollY: 1000,
            scrollX: true,
            scrollCollapse: true,
            fixedColumns: true,

            dom: 'Bfrtip',
            buttons: [
                'pageLength',
                'copyHtml5',
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
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $("td:first", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            "processing": true,
            "serverSide": true,

            "columns": [
                { data: "count" },
                { data: "permitNo" },
                { data: "reference" },
                { data: "companyName" },
                { data: "facilities" },
                //{ data: "facilityAddress" },
                { data: "category" },
                { data: "type" },
                { data: "state" },
                { data: "lga" },
                { data: "issuedDate" },
                { data: "expiryDate" },
                { data: "fieldOffice" },
                { data: "zonalOffice" },
                { data: "permitType", "orderable": false, "searchable": false },
                { data: "printStatus", "orderable": false, "searchable": false },

            ],

            rowGroup: {
                startRender: function (rows, group) {
                    return $('<tr>').append('<td colspan="15" class="text-left" style="background-color:green; color:white"> <b>' + group + '</b></td></tr>');
                },

                dataSrc: 'type'
            },
            orderFixed: [2, 'desc'],
            "bDestroy": true

        });

    });




    $("#SearchFacilityTabless").ready(function () {

        $("#SearchFacilityTabless").dataTable({

            ajax: {
                url: "/Reports/FacilityReports",
                type: "POST",
                data: function (d) {

                },
                dataType: "json",
            },
            lengthMenu: [[5000, 15000, 25000, 50000], [5000, 15000, 25000, 50000]],


            dom: 'Bfrtip',
            buttons: [
                'pageLength',
                'copyHtml5',
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
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $("td:first", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            'columnDefs': [{
                "targets": [6, 7],
                render(v) {
                    return formatNumber(Number(v));
                }
            }],

            "columns": [
                { data: "count" },
                { data: "companyName" },
                { data: "facilityName" },
                { data: "facilityAddress" },
                { data: "contact" },
                { data: "state" },
                { data: "tanksCount", "orderable": false, "searchable": false },
                { data: "capacity", "orderable": false, "searchable": false },
                { data: "products", "orderable": false, "searchable": false },
                { data: "createdAt" },
            ],

            "footerCallback": function (row, data, start, end, display) {
                var api = this.api();
                nb_cols = api.columns().nodes().length - 2;

                var j = 6;
                while (j < nb_cols) {
                    var pageTotal = api
                        .column(j, { page: 'current' })
                        .data()
                        .sum(function (a, b) {
                            return (Number(a) + Number(b));
                        }, 0);

                    $(api.column(j).footer()).html(formatNumber(pageTotal.toFixed(2)));
                    j++;
                }
            },

            rowGroup: {
                //endRender: function (rows, group) {

                //    var a1 = GetSum(rows, 'tanksCount');
                //    var a2 = GetSum(rows, 'capacity');

                //    return $('<tr>')
                //        .append('<td colspan="6" style="background-color:#88b29196; color:black" class=""><b>Sub Total of Tanks and Capacity</b></td>')
                //        .append('<td class="text-danger"><b>' + formatNumber(a1) + '</b></td>')
                //        .append('<td class="text-danger"><b>' + formatNumber(a2) + '</b></td>')
                //        .append('<td></td>')
                //        .append('<td></td>')
                //        .append('</tr>');

                //},
                startRender: function (rows, group) {
                    return $('<tr>').append('<td colspan="2" class="text-left" style="background-color:#040433; color:white"> <b>' + group + '</b></td></tr>');
                },
                dataSrc: 'state',

            },
            "bDestroy": true

        });

    });

    $("#SearchTabless").ready(function () {

        $("#SearchTabless").DataTable({

            ajax: {
                url: "/Reports/ApplicationReport",
                type: "POST",
                data: function (d) {
                },
                dataType: "json",
            },
            lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],

            scrollY: 1000,
            scrollX: true,
            scrollCollapse: true,
            fixedColumns: true,

            dom: 'Bfrtip',
            buttons: [
                'pageLength',
                'copyHtml5',
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
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $("td:first", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            "processing": true,
            "serverSide": true,

            "columns": [
                { data: "count" },
                { data: "refNo" },
                //{ data: "rrr" },
                { data: "companyName" },
                { data: "facility" },
                { data: "facilityAddress" },
                { data: "category" },
                { data: "type" },
                { data: "year" },
                { data: "status" },
                { data: "products" },
                { data: "currentDesk" },
                { data: "totalDays" },
                { data: "state" },
                { data: "lga" },
                { data: "fieldOffice" },
                { data: "zonalOffice" },
                { data: "dateApplied" },
                { data: "dateSubmitted" },
                {
                    "render": function (data, type, row) {
                        return "<a class=\"btn btn-sm btn-info" + "\" href=\"/Application/ViewApplication/" + row['appId'] + "\"> <i class=\"fa fa-eye\"> </i> View </a>";
                    }
                }
            ],

            rowGroup: {

                startRender: function (rows, group) {
                    return $('<tr>').append('<td colspan="3" class="text-left" style="background-color:#1f231f; color:white"> <b>' + group + '</b></td></tr>');
                },

                dataSrc: 'category'
            },
            orderFixed: [2, 'desc'],
            "bDestroy": true

        });

    })

    //end section


    $("#txtApplicationReportFrom").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0),
        timepicker: false,
        yearEnd: new Date().getFullYear() + 1,
        maxDate: new Date((new Date().getFullYear() + 0) + '/12/31')

    });


    $("#txtApplicationReportTo").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0),
        timepicker: false,
        yearEnd: new Date().getFullYear() + 1,
    });


    $("#TtxtApplicationReportFrom").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0),
        timepicker: false,
        yearEnd: new Date().getFullYear() + 1,
        maxDate: new Date((new Date().getFullYear() + 0) + '/12/31')

    });


    $("#TtxtApplicationReportTo").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0),
        timepicker: false,
        yearEnd: new
            Date().getFullYear() + 1,
    });

    $("#PtxtApplicationReportFrom").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0),
        timepicker: false,
        yearEnd: new Date().getFullYear() + 1,
        maxDate: new Date((new Date().getFullYear() + 0) + '/12/31')

    });


    $("#PtxtApplicationReportTo").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0),
        timepicker: false,
        yearEnd: new Date().getFullYear() + 1,
    });

    $("#FtxtApplicationReportFrom").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0),
        timepicker: false,
        yearEnd: new Date().getFullYear() + 1,
        maxDate: new Date((new Date().getFullYear() + 0) + '/12/31')

    });


    $("#FtxtApplicationReportTo").datetimepicker({
        defaultDate: new Date().setDate(new Date().getDate() + 0),
        timepicker: false,
        yearEnd: new Date().getFullYear() + 1,
    });



    $("#btnFacilityReportSearch").on('click', function (event) {
        event.preventDefault();


        var states = StringToArray($("#txtSelStates"));

        var dateFrom = $("#FtxtApplicationReportFrom").val();
        var dateTo = $("#FtxtApplicationReportTo").val();

        var table = $("#SearchFacilityTabless").DataTable({

            ajax: {
                url: "/Reports/FacilityReports",
                type: "POST",
                data: function (d) {

                    d.states = states,
                        d.dateFrom = dateFrom,
                        d.dateTo = dateTo
                },
                dataType: "json",
            },
            lengthMenu: [[5000, 15000, 25000, 50000], [5000, 15000, 25000, 50000]],


            dom: 'Bfrtip',
            buttons: [
                'pageLength',
                'copyHtml5',
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
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $("td:first", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            language: {
                buttons: {
                    colvis: 'Change columns'
                }
            },

            "processing": true,
            "serverSide": true,

            'columnDefs': [{
                "targets": [6, 7],
                render(v) {
                    return formatNumber(Number(v));
                }
            }],

            "columns": [

                { data: "count" },
                { data: "companyName" },
                { data: "facilityName" },
                { data: "facilityAddress" },
                { data: "contact" },
                { data: "state" },
                { data: "tanksCount", "orderable": false, "searchable": false },
                { data: "capacity", "orderable": false, "searchable": false },
                { data: "products", "orderable": false, "searchable": false },
                { data: "createdAt" },
            ],

            "footerCallback": function (row, data, start, end, display) {
                var api = this.api();
                nb_cols = api.columns().nodes().length - 2;

                var j = 6;
                while (j < nb_cols) {
                    var pageTotal = api
                        .column(j, { page: 'current' })
                        .data()
                        .sum(function (a, b) {
                            return (Number(a) + Number(b));
                        }, 0);

                    $(api.column(j).footer()).html(formatNumber(pageTotal.toFixed(2)));
                    j++;
                }
            },

            rowGroup: {
                endRender: function (rows, group) {

                    //var a1 = GetSum(rows, 'tanksCount');
                    //var a2 = GetSum(rows, 'capacity');

                    //return $('<tr>')
                    //    .append('<td colspan="6" style="background-color:#88b29196; color:black" class=""><b>Sub Total of Tanks and Capacity</b></td>')
                    //    .append('<td class="text-danger"><b>' + formatNumber(a1) + '</b></td>')
                    //    .append('<td class="text-danger"><b>' + formatNumber(a2) + '</b></td>')
                    //    .append('<td></td>')
                    //    .append('<td></td>')
                    //    .append('</tr>');

                },
                startRender: function (rows, group) {
                    return $('<tr>').append('<td colspan="2" class="text-left" style="background-color:#040433; color:white"> <b>' + group + '</b></td></tr>');
                },
                dataSrc: 'state',

            },
            "bDestroy": true

        });

    });




    function GetSum(rows, pluck) {
        var get = rows
            .data()
            .pluck('' + pluck + '')
            .reduce(function (a, b) {
                return a + b;
            });
        return get;
    }


    function GetTotal(column) {
        var total = api
            .column('' + column + '')
            .data()
            .reduce(function (a, b) {
                return intVal(a) + intVal(b);
            }, 0);
        return total;
    }


    function StringToArray(select) {
        var array = [];
        select.each(function () {
            array.push($(this).val());
        });

        return array;
    }


    function formatNumber(num) {
        return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
    }



    $("#btnApplicationReportSearch").on('click', function (event) {
        event.preventDefault();


        var year = StringToArray($("#txtSelYear"));
        var category = StringToArray($("#txtSelCategory"));
        var type = StringToArray($("#txtSelType"));
        var status = StringToArray($("#txtSelStatus"));
        var state = StringToArray($("#txtSelState"));
        var fieldoffice = StringToArray($("#txtSelOffice"));
        var zonaloffice = StringToArray($("#txtSelZone"));
        var dateFrom = $("#txtApplicationReportFrom").val();
        var dateTo = $("#txtApplicationReportTo").val();

        var table = $("#SearchTabless").DataTable({

            ajax: {
                url: "/Reports/ApplicationReport",
                type: "POST",
                data: function (d) {
                    d.category = category,
                        d.year = year,
                        d.type = type,
                        d.office = fieldoffice,
                        d.zone = zonaloffice,
                        d.status = status,
                        d.state = state,
                        d.dateFrom = dateFrom,
                        d.dateTo = dateTo
                },
                dataType: "json",
            },
            lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],

            scrollY: 1000,
            scrollX: true,
            scrollCollapse: true,
            fixedColumns: true,

            dom: 'Bfrtip',
            buttons: [
                'pageLength',
                'copyHtml5',
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
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $("td:first", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            "processing": true,
            "serverSide": true,

            "columns": [

                { data: "count" },
                { data: "refNo" },
                //{ data: "rrr" },
                { data: "companyName" },
                { data: "facility" },
                { data: "facilityAddress" },
                { data: "category" },
                { data: "type" },
                { data: "year" },
                { data: "status" },
                { data: "products" },
                { data: "currentDesk" },
                { data: "totalDays" },
                { data: "state" },
                { data: "lga" },
                { data: "fieldOffice" },
                { data: "zonalOffice" },
                { data: "dateApplied" },
                { data: "dateSubmitted" },
                {
                    "render": function (data, type, row) {
                        return "<a class=\"btn btn-sm btn-info" + "\" href=\"/Application/ViewApplication/" + row['appId'] + "\"> <i class=\"fa fa-eye\"> </i> View </a>";
                    }
                }
            ],


            orderFixed: [2, 'desc'],
            "bDestroy": true

        });


    });



    $("#btnTransactionReportSearch").on('click', function (event) {
        event.preventDefault();

        var type = StringToArray($("#txtSelType"));

        var category = StringToArray($("#TtxtSelCategory"));
        var status = StringToArray($("#txtSelAppStatus"));
        var channel = StringToArray($("#txtSelChannel"));
        var year = StringToArray($("#TtxtSelYear"));
        var state = StringToArray($("#txtSelState"));
        var dateFrom = $("#TtxtApplicationReportFrom").val();
        var dateTo = $("#TtxtApplicationReportTo").val();
        var fieldoffice = StringToArray($("#TtxtSelOffice"));
        var zonaloffice = StringToArray($("#TtxtSelZone"));

        var table = $("#SearchTransactionTabless").DataTable({

            ajax: {
                url: "/Reports/TransactionReport",
                type: "POST",
                data: function (d) {

                    d.type = type,
                        d.office = fieldoffice,
                        d.zone = zonaloffice,
                        d.status = status,
                        d.channel = channel,
                        d.category = category,
                        d.year = year,
                        d.state = state,
                        d.dateFrom = dateFrom,
                        d.dateTo = dateTo
                },
                dataType: "json",
            },
            lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],

            dom: 'Bfrtip',
            buttons: [
                'pageLength',
                'copyHtml5',
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
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $("td:first", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            "processing": true,
            "serverSide": true,

            'columnDefs': [{
                "targets": [12, 13, 14, 15],
                render(v) {
                    return "₦" + formatNumber(Number(v).toFixed(2));
                }
            }],

            "columns": [
                { data: "count" },
                { data: "refNo" },
                { data: "rrr" },
                { data: "companyName" },
                { data: "facilities" },
                { data: "facilityAddress" },
                { data: "category" },
                { data: "type" },
                { data: "channel" },
                { data: "status" },
                { data: "state" },
                { data: "lga" },
                { data: "amount", "orderable": false, "searchable": false },
                { data: "extraAmount", "orderable": false, "searchable": false },
                { data: "serviceCharge", "orderable": false, "searchable": false },
                { data: "totalAmount", "orderable": false, "searchable": false },
                { data: "fieldOffice" },
                { data: "zonalOffice" },
                { data: "transDate" },
            ],

            "footerCallback": function (row, data, start, end, display) {
                var api = this.api();
                nb_cols = api.columns().nodes().length - 1;

                var j = 12;
                while (j < 15) {
                    var pageTotal = api
                        .column(j, { page: 'current' })
                        .data()
                        .sum(function (a, b) {
                            return (Number(a) + Number(b));
                        }, 0);
                    // Update footer

                    $(api.column(j).footer()).html('₦' + formatNumber(pageTotal.toFixed(2)));
                    j++;
                }
            },
            "footerCallback": function (row, data, start, end, display) {
                var api = this.api();
                nb_cols = api.columns().nodes().length - 1;

                var j = 13;
                while (j < 15) {
                    var pageTotal = api
                        .column(j, { page: 'current' })
                        .data()
                        .sum(function (a, b) {
                            return (Number(a) + Number(b));
                        }, 0);
                    // Update footer

                    $(api.column(j).footer()).html('₦' + formatNumber(pageTotal.toFixed(2)));
                    j++;
                }
            },

            rowGroup: {
                endRender: function (rows, group) {


                    var a1 = GetSum(rows, 'amount');
                    var a2 = GetSum(rows, 'extraAmount');
                    var a3 = GetSum(rows, 'serviceCharge');
                    var aSum = GetSum(rows, 'totalAmount');

                    return $('<tr style="">')
                        .append('<td colspan="12" style="font-weight:bold; color:black" class=""><b>Sub Total for ' + group + '</b></td>')
                        .append('<td class="text-danger"><b>₦' + formatNumber(a1.toFixed(2)) + '</b></td>')
                        .append('<td class="text-danger"><b>₦' + formatNumber(a2.toFixed(2)) + '</b></td>')
                        .append('<td class="text-danger"><b>₦' + formatNumber(a3.toFixed(2)) + '</b></td>')
                        .append('<td class="text-danger"><b>₦' + formatNumber(aSum.toFixed(2)) + '</b></td>')
                        .append('<td ></td>')
                        .append('<td ></td>')
                        .append('<td ></td>')
                        .append('</tr>');

                },
                startRender: function (rows, group) {
                    return $('<tr>').append('<td colspan="3" class="text-left" style="background-color:black; color:white"> <b>' + group + '</b></td></tr>');
                },

                dataSrc: 'category',
            },
            orderFixed: [1, 'desc'],
            "bDestroy": true

        });

    });



    $("#btnPermitReportSearch").on('click', function (event) {
        event.preventDefault();

        var year = StringToArray($("#PtxtSelYear"));
        var category = StringToArray($("#PtxtSelCategory"));
        var type = StringToArray($("#PtxtSelType"));
        var dateFrom = $("#PtxtApplicationReportFrom").val();
        var dateTo = $("#PtxtApplicationReportTo").val();

        var table = $("#SearchPermitTabless").DataTable({

            ajax: {
                url: "/Reports/PermitReports",
                type: "POST",
                data: function (d) {
                    d.category = category,
                        d.type = type,
                        d.year = year,
                        d.dateFrom = dateFrom,
                        d.dateTo = dateTo
                },
                dataType: "json",
            },
            lengthMenu: [[1000, 2000, 3000, 4000, 5000], [1000, 2000, 3000, 4000, 5000]],

            scrollY: 1000,
            scrollX: true,
            scrollCollapse: true,
            fixedColumns: true,

            dom: 'Bfrtip',
            buttons: [
                'pageLength',
                'copyHtml5',
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
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $("td:first", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            "processing": true,
            "serverSide": true,

            "columns": [

                { data: "count" }, { data: "permitNo" },
                { data: "reference" },
                { data: "companyName" },
                { data: "facilities" },
                //{ data: "facilityAddress" },
                { data: "category" },
                { data: "type" },
                { data: "state" },
                { data: "lga" },
                { data: "issuedDate" },
                { data: "expiryDate" },
                { data: "fieldOffice" },
                { data: "zonalOffice" },
                { data: "permitType", "orderable": false, "searchable": false },
                { data: "printStatus", "orderable": false, "searchable": false },

            ],

            //rowGroup: {
            //    startRender: function (rows, group) {
            //        return $('<tr>').append('<td colspan="15" class="text-left" style="background-color:green; color:white"> <b>' + group + '</b></td></tr>');
            //    },

            //    dataSrc: 'type'
            //},
            orderFixed: [2, 'desc'],
            "bDestroy": true

        });

    });



    function GetSum(rows, pluck) {
        var get = rows
            .data()
            .pluck('' + pluck + '')
            .reduce(function (a, b) {
                return a + b;
            });
        return get;
    }


    function GetTotal(column) {
        var total = api
            .column('' + column + '')
            .data()
            .reduce(function (a, b) {
                return intVal(a) + intVal(b);
            }, 0);
        return total;
    }


    function StringToArray(select) {
        var array = [];
        select.each(function () {
            array.push($(this).val());
        });

        return array;
    }


    function formatNumber(num) {
        return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
    }

});