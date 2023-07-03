$(function () {

    $("#GetSevenDaysApplications").ready(function () {

        am4core.ready(function () {

            $.ajax({
                type: "POST",
                url: "/Reports/GetSevenDaysApplications",
                contentType: false,
                processData: false,
                async: false,
                success: function (response) {
                    var chartData = [];

                    for (var i = 0; i < response.length; i++) {
                        chartData.push({
                            date: response[i].date,
                            paymentpending: response[i].paymentpending,
                            processing: response[i].processing,
                            approved: response[i].approved,
                        });

                    }

                    // Themes begin
                    am4core.useTheme(am4themes_spiritedaway);
                    //am4core.useTheme(am4themes_moonrisekingdom);
                    // am4core.useTheme(am4themes_animated);
                    // Themes end

                    // Create chart instance
                    var chart = am4core.create("GetSevenDaysApplications", am4charts.XYChart);

                    // Add data
                    chart.data = chartData;

                    // Create axes
                    var dateAxis = chart.xAxes.push(new am4charts.DateAxis());


                    var valueAxis1 = chart.yAxes.push(new am4charts.ValueAxis());
                    valueAxis1.title.text = "Number";

                    //var valueAxis2 = chart.yAxes.push(new am4charts.ValueAxis());
                    //valueAxis2.title.text = "Percentage";
                    //valueAxis2.renderer.opposite = true;
                    //valueAxis2.renderer.grid.template.disabled = true;

                    // Create series
                    var series1 = chart.series.push(new am4charts.ColumnSeries());
                    series1.dataFields.valueY = "approved";
                    series1.dataFields.dateX = "date";
                    series1.yAxis = valueAxis1;
                    series1.name = "Approved";
                    series1.tooltipText = "{name}\n[bold font-size: 20]{valueY}";
                    series1.fill = chart.colors.getIndex(0);
                    series1.strokeWidth = 0;
                    series1.clustered = false;
                    series1.columns.template.width = am4core.percent(40);

                    var series2 = chart.series.push(new am4charts.ColumnSeries());
                    series2.dataFields.valueY = "processing";
                    series2.dataFields.dateX = "date";
                    series2.yAxis = valueAxis1;
                    series2.name = "Processing";
                    series2.tooltipText = "{name}\n[bold font-size: 20]{valueY}";
                    series2.fill = chart.colors.getIndex(0).lighten(0.5);
                    series2.strokeWidth = 0;
                    series2.clustered = false;
                    series2.toBack();

                    var series3 = chart.series.push(new am4charts.LineSeries());
                    series3.dataFields.valueY = "paymentpending";
                    series3.dataFields.dateX = "date";
                    series3.name = "Payment Pending";
                    series3.strokeWidth = 1;
                    series3.tensionX = 0.7;
                    series3.yAxis = valueAxis1;
                    series3.tooltipText = "{name}\n[bold font-size: 20]{valueY}";
                    var bullet3 = series3.bullets.push(new am4charts.CircleBullet());
                    bullet3.circle.radius = 3;
                    bullet3.circle.strokeWidth = 1;
                    bullet3.circle.fill = am4core.color("#fff");


                    // Add cursor
                    chart.cursor = new am4charts.XYCursor();

                    // Add legend
                    chart.legend = new am4charts.Legend();
                    chart.legend.position = "top";

                    // Add scrollbar

                }
            })
        }); // end am4core.ready()

    })

    //am4core.ready(function () {

    //    // Dasboard Graph per day in line chart
    //    $.ajax({
    //        type: "POST",
    //        url: "/Reports/GetDashboardStatus",
    //        contentType: false,
    //        processData: false,
    //        async: false,
    //        success: function (response) {
                
    //            // Themes begin
    //            am4core.useTheme(am4themes_moonrisekingdom);

    //            // Themes end

    //            // Create chart instance
    //            var chart = am4core.create("DashboardGraph", am4charts.XYChart3D);

    //            chart.data = response[0].statusCountModel;

    //            // Create axes
    //            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    //            categoryAxis.dataFields.category = "value";
    //            categoryAxis.renderer.labels.template.rotation = 310;
    //            categoryAxis.renderer.labels.template.hideOversized = false;
    //            categoryAxis.renderer.minGridDistance = 20;
    //            categoryAxis.renderer.labels.template.horizontalCenter = "right";
    //            categoryAxis.renderer.labels.template.verticalCenter = "middle";
    //            categoryAxis.tooltip.label.rotation = 270;
    //            categoryAxis.tooltip.label.horizontalCenter = "right";
    //            categoryAxis.tooltip.label.verticalCenter = "middle";

    //            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    //            valueAxis.title.text = "Status";
    //            valueAxis.title.fontWeight = "bold";

    //            // Create series
    //            var series = chart.series.push(new am4charts.ColumnSeries3D());
    //            series.dataFields.valueY = "count";
    //            series.dataFields.categoryX = "value";
    //            series.name = "Counts";
    //            series.tooltipText = "{categoryX}: [bold]{valueY}[/]";
    //            series.columns.template.fillOpacity = .8;
    //            series.columns.template.propertyFields.fill = "color";

    //            var columnTemplate = series.columns.template;
    //            columnTemplate.strokeWidth = 2;
    //            columnTemplate.strokeOpacity = 1;
    //            columnTemplate.stroke = am4core.color("#FFFFFF");

    //            chart.cursor = new am4charts.XYCursor();
    //            chart.cursor.lineX.strokeOpacity = 0;
    //            chart.cursor.lineY.strokeOpacity = 0;
    //            chart.exporting.menu = new am4core.ExportMenu();

    //            //Now create second chart
    //            var chart2 = am4core.create("PieAppReport", am4charts.PieChart3D);
    //            chart2.hiddenState.properties.opacity = 0;

    //            // Add chart title
    //            var title = chart2.titles.create();
    //            title.fontSize = 20;
    //            title.marginBottom = 30;
                
    //            chart2.data = response;
    //            chart2.legend = new am4charts.Legend();
    //            chart2.exporting.menu = new am4core.ExportMenu();
    //            var series2 = chart2.series.push(new am4charts.PieSeries3D());
    //            series2.dataFields.value = "categoryvalue";
    //            series2.dataFields.category = "category";
    //            series2.dataFields.valueYShow = "categoryvalue";
    //            //pieSeries.dataFields.valueYShow = "totalPercent";

    //            series2.labels.template.text = "{categoryvalue}";

    //            chart2.exporting.menu = new am4core.ExportMenu();

               
    //        },
    //        error: function (response) {

    //        },
    //        complete: function () {

    //        }

    //    });


    //});
    
    //am4core.ready(function () {

    //    $.ajax({
    //        type: "POST",
    //        url: "/Reports/CategoryApplicationChart",
    //        contentType: false,
    //        processData: false,
    //        async: false,
    //        success: function (response) {
    //            // Themes begin
    //            am4core.useTheme(am4themes_spiritedaway);
    //            // am4core.useTheme(am4themes_moonrisekingdom);
    //            // am4core.useTheme(am4themes_animated);
    //            // Themes end

    //            var chartData = [];

    //            for (var i = 0; i < response.length; i++) {

    //                if (response[i].category === "Suitability Inspection") {
    //                    chartData.push({
    //                        category: response[i].category,
    //                        suitabilityvalue: response[i].categoryvalue,
    //                    });
    //                } if (response[i].category === "Take Over") {
    //                    chartData.push({
    //                        category: response[i].category,
    //                        takeovervalue: response[i].categoryvalue,
    //                    });
    //                }
    //                if (response[i].category === "Regularization") {
    //                    chartData.push({
    //                        category: response[i].category,
    //                        regularizationvalue: response[i].categoryvalue,
    //                    });
    //                }
    //                if (response[i].category === "Depot Modification") {
    //                    chartData.push({
    //                        category: response[i].category,
    //                        modivalue: response[i].categoryvalue,
    //                    });
    //                }
    //                if (response[i].category === "Approval To Construct") {
    //                    chartData.push({
    //                        category: response[i].category,
    //                        atcvalue: response[i].categoryvalue,
    //                    });
    //                } if (response[i].category === "License To Operate") {
    //                    chartData.push({
    //                        category: response[i].category,
    //                        ltovalue: response[i].categoryvalue,
    //                    });
    //                }
    //            }

    //            // Create chart instance
    //            var chart = am4core.create("AppReport", am4charts.XYChart);
    //            chart.hiddenState.properties.opacity = 0; // this creates initial fade-in
    //            //chart.data = chartData;
    //            chart.data = chartData;
    //            chart.exporting.menu = new am4core.ExportMenu();
    //            //chart.data = response;

    //            // Add chart title
    //            var title = chart.titles.create();
    //            title.text = "Application Line Chart According To Category";
    //            title.fontSize = 20;
    //            title.marginBottom = 30;


    //            chart.colors.step = 1;


    //            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    //            categoryAxis.dataFields.category = "category";
    //            categoryAxis.renderer.grid.template.location = 0;

    //            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    //            valueAxis.min = 0;
    //            valueAxis.max = 100;
    //            valueAxis.strictMinMax = true;
    //            valueAxis.calculateTotals = true;
    //            valueAxis.renderer.minWidth = 50;

    //            var series1 = chart.series.push(new am4charts.ColumnSeries());
    //            series1.columns.template.width = am4core.percent(40);
    //            series1.columns.template.tooltipText =
    //                "{name}: {valueY.totalPercent.formatNumber('#.00')}% {valueY}";
    //            series1.name = "Site Suitability";
    //            series1.dataFields.categoryX = "category";
    //            series1.dataFields.valueY = "suitabilityvalue";
    //            series1.dataFields.valueYShow = "totalPercent";
    //            series1.dataItems.template.locations.categoryX = 0.5;
    //            series1.stacked = true;
    //            series1.tooltip.pointerOrientation = "vertical";

    //            var bullet1 = series1.bullets.push(new am4charts.LabelBullet());
    //            bullet1.interactionsEnabled = false;
    //            bullet1.label.text = "{valueY.totalPercent.formatNumber('#.00')}%";
    //            bullet1.label.fill = am4core.color("#ffffff");
    //            bullet1.locationY = 0.5;



    //            var series2 = chart.series.push(new am4charts.ColumnSeries());
    //            series2.columns.template.width = am4core.percent(40);
    //            series2.columns.template.tooltipText =
    //                "{name}: {valueY.totalPercent.formatNumber('#.00')}%";
    //            series2.name = "Take Over";
    //            series2.dataFields.categoryX = "category";
    //            series2.dataFields.valueY = "takeovervalue";
    //            series2.dataFields.valueYShow = "totalPercent";
    //            series2.dataItems.template.locations.categoryX = 0.5;
    //            series2.stacked = true;
    //            series2.tooltip.pointerOrientation = "vertical";

    //            var bullet2 = series2.bullets.push(new am4charts.LabelBullet());
    //            bullet2.interactionsEnabled = false;
    //            bullet2.label.text = "{valueY.totalPercent.formatNumber('#.00')}%";
    //            bullet2.locationY = 0.5;
    //            bullet2.label.fill = am4core.color("#ffffff");

    //            var series3 = chart.series.push(new am4charts.ColumnSeries());
    //            series3.columns.template.width = am4core.percent(40);
    //            series3.columns.template.tooltipText =
    //                "{name}: {valueY.totalPercent.formatNumber('#.00')}%";
    //            series3.name = "Depot Modification";
    //            series3.dataFields.categoryX = "category";
    //            series3.dataFields.valueY = "modivalue";
    //            series3.dataFields.valueYShow = "totalPercent";
    //            series3.dataItems.template.locations.categoryX = 0.5;
    //            series3.stacked = true;
    //            series3.tooltip.pointerOrientation = "vertical";

    //            var bullet3 = series3.bullets.push(new am4charts.LabelBullet());
    //            bullet3.interactionsEnabled = false;
    //            bullet3.label.text = "{valueY.totalPercent.formatNumber('#.00')}%";
    //            bullet3.locationY = 0.5;
    //            bullet3.label.fill = am4core.color("#ffffff");
    //            //series ATC
    //            var series4 = chart.series.push(new am4charts.ColumnSeries());
    //            series4.columns.template.width = am4core.percent(40);
    //            series4.columns.template.tooltipText =
    //                "{name}: {valueY.totalPercent.formatNumber('#.00')}%";
    //            series4.name = "Approval To Construct";
    //            series4.dataFields.categoryX = "category";
    //            series4.dataFields.valueY = "atcvalue";
    //            series4.dataFields.valueYShow = "{valueY.totalPercent.formatNumber('#.00')}%";
    //            series4.dataItems.template.locations.categoryX = 0.5;
    //            series4.stacked = true;
    //            series4.tooltip.pointerOrientation = "vertical";

    //            var bullet4 = series4.bullets.push(new am4charts.LabelBullet());
    //            bullet4.interactionsEnabled = false;
    //            bullet4.label.text = "{valueY.totalPercent.formatNumber('#.00')}%";
    //            bullet4.locationY = 0.5;
    //            bullet4.label.fill = am4core.color("#ffffff");




    //            // Add a legend
    //            chart.legend = new am4charts.Legend();

    //            var markerTemplate = chart.legend.markers.template;
    //            markerTemplate.width = 5;
    //            markerTemplate.height = 5;
    //            chart.legend.align = "right";
    //            chart.legend.position = "right";
    //            chart.legend.valueLabels.template.text = "{value.value}";
    //            chart.legend.valueLabels.template.fontSize = 6;
    //            chart.legend.labels.template.fontSize = 6;

    //        }

    //    });

    //});

    //State Application Pie Chart
    //am4core.ready(function () {

    //    $.ajax({
    //        type: "POST",
    //        url: "/Reports/StateApplicationChart",
    //        contentType: false,
    //        processData: false,
    //        async: false,
    //        success: function (response) {
    //            // Themes begin

    //            am4core.useTheme(am4themes_spiritedaway);
                
    //            // Create chart instance
    //            var chart = am4core.create("PieStateAppReport", am4charts.PieChart);

    //            chart.data = response;
    //            chart.exporting.menu = new am4core.ExportMenu();

    //            // Add and configure Series
    //            var pieSeries = chart.series.push(new am4charts.PieSeries());
    //            pieSeries.dataFields.value = "statevalue";
    //            pieSeries.dataFields.valueYShow = "statevalue";
    //            pieSeries.dataFields.category = "state";
    //            pieSeries.labels.template.text = "{statevalue}";


    //            // Let's cut a hole in our Pie chart the size of 30% the radius
    //            chart.innerRadius = am4core.percent(50);
    //            chart.radius = am4core.percent(70);

    //            // Put a thick white border around each Slice
    //            pieSeries.slices.template.stroke = am4core.color("#fff");
    //            pieSeries.slices.template.strokeWidth = 2;
    //            pieSeries.slices.template.strokeOpacity = 1;
    //            pieSeries.labels.template.fontSize = 11;
    //            pieSeries.slices.template
    //                // change the cursor on hover to make it apparent the object can be interacted with
    //                .cursorOverStyle = [
    //                    {
    //                        "property": "cursor",
    //                        "value": "pointer"
    //                    }
    //                ];

    //            pieSeries.alignLabels = false;
    //            pieSeries.labels.template.bent = false;
    //            pieSeries.labels.template.radius = 3;
    //            pieSeries.labels.template.padding(0, 0, 0, 0);

    //            pieSeries.ticks.template.disabled = true;

    //            // Create a base filter effect (as if it's not there) for the hover to return to
    //            var shadow = pieSeries.slices.template.filters.push(new am4core.DropShadowFilter);
    //            shadow.opacity = 0;

    //            // Create hover state
    //            var hoverState = pieSeries.slices.template.states.getKey("hover"); // normally we have to create the hover state, in this case it already exists

    //            // Slightly shift the shadow and make it more prominent on hover
    //            var hoverShadow = hoverState.filters.push(new am4core.DropShadowFilter);
    //            hoverShadow.opacity = 0.7;
    //            hoverShadow.blur = 5;

    //            // Add a legend
    //            chart.legend = new am4charts.Legend();

    //            chart.legend.labels.template.fontSize = 12;

    //            var markerTemplate = chart.legend.markers.template;
    //            markerTemplate.width = 10;
    //            markerTemplate.height = 10;
    //            chart.legend.align = "right";
    //            chart.legend.position = "left";
    //            chart.legend.valueLabels.template.text = "{value.value}";
    //            chart.legend.valueLabels.template.fontSize = 8;
    //        }

    //    });

    //});

    //Status Application Pie and Bar Charts
    //am4core.ready(function () {

    //    $.ajax({
    //        type: "POST",
    //        url: "/Reports/StatusApplicationChart",
    //        contentType: false,
    //        processData: false,
    //        async: false,
    //        success: function (response) {
    //            // Themes begin
    //            //am4core.useTheme(am4themes_kelly);
    //            am4core.useTheme(am4themes_spiritedaway);
    //            // am4core.useTheme(am4themes_moonrisekingdom);
    //            // am4core.useTheme(am4themes_animated);
    //            // Themes end

    //            // Create chart instance
    //            var chart = am4core.create("PieStatusAppReport", am4charts.PieChart);

    //            chart.data = response;
    //            chart.exporting.menu = new am4core.ExportMenu();

    //            // Add and configure Series
    //            var pieSeries = chart.series.push(new am4charts.PieSeries());
    //            pieSeries.dataFields.value = "statusvalue";
    //            pieSeries.dataFields.valueYShow = "statusvalue";
    //            pieSeries.dataFields.category = "status";
    //            pieSeries.labels.template.text = "{statusvalue}";


    //            // Let's cut a hole in our Pie chart the size of 30% the radius
    //            chart.innerRadius = am4core.percent(50);
    //            chart.radius = am4core.percent(70);

    //            // Put a thick white border around each Slice
    //            pieSeries.slices.template.stroke = am4core.color("#fff");
    //            pieSeries.slices.template.strokeWidth = 2;
    //            pieSeries.slices.template.strokeOpacity = 1;
    //            pieSeries.labels.template.fontSize = 11;
    //            pieSeries.slices.template
    //                // change the cursor on hover to make it apparent the object can be interacted with
    //                .cursorOverStyle = [
    //                    {
    //                        "property": "cursor",
    //                        "value": "pointer"
    //                    }
    //                ];

    //            pieSeries.alignLabels = false;
    //            pieSeries.labels.template.bent = false;
    //            pieSeries.labels.template.radius = 3;
    //            pieSeries.labels.template.padding(0, 0, 0, 0);

    //            pieSeries.ticks.template.disabled = true;

    //            // Create a base filter effect (as if it's not there) for the hover to return to
    //            var shadow = pieSeries.slices.template.filters.push(new am4core.DropShadowFilter);
    //            shadow.opacity = 0;

    //            // Create hover state
    //            var hoverState = pieSeries.slices.template.states.getKey("hover"); // normally we have to create the hover state, in this case it already exists

    //            // Slightly shift the shadow and make it more prominent on hover
    //            var hoverShadow = hoverState.filters.push(new am4core.DropShadowFilter);
    //            hoverShadow.opacity = 0.7;
    //            hoverShadow.blur = 5;

    //            // Add a legend
    //            chart.legend = new am4charts.Legend();

    //            chart.legend.labels.template.fontSize = 12;

    //            var markerTemplate = chart.legend.markers.template;
    //            markerTemplate.width = 10;
    //            markerTemplate.height = 10;
    //            chart.legend.align = "right";
    //            chart.legend.position = "left";
    //            chart.legend.valueLabels.template.text = "{value.value}";
    //            chart.legend.valueLabels.template.fontSize = 8;
    //        }

    //    });

    //});


    // Staff Metrics Pie and Bar Charts
    //am4core.ready(function () {

    //    $.ajax({
    //        type: "POST",
    //        url: "/Charts/StaffMetrics",
    //        contentType: false,
    //        processData: false,
    //        async: false,
    //        success: function (response) {
    //            // Themes begin
    //            am4core.useTheme(am4themes_spiritedaway);
    //            //am4core.useTheme(am4themes_animated);
    //            // Themes end

    //            var chart = am4core.create("StaffMetrics", am4charts.XYChart);
    //            chart.hiddenState.properties.opacity = 0; // this creates initial fade-in

    //            chart.exporting.menu = new am4core.ExportMenu();
    //            chart.data = response;

    //            // Add and configure Series
    //            chart.colors.step = 2;
    //            chart.padding(30, 30, 10, 30);

    //            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    //            categoryAxis.dataFields.category = "category";
    //            categoryAxis.renderer.grid.template.location = 0;

    //            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    //            valueAxis.min = 0;
    //            valueAxis.max = 100;
    //            valueAxis.strictMinMax = true;
    //            valueAxis.calculateTotals = true;
    //            valueAxis.renderer.minWidth = 50;


    //            var series1 = chart.series.push(new am4charts.ColumnSeries());
    //            series1.columns.template.width = am4core.percent(40);
    //            series1.columns.template.tooltipText =
    //                "{name}: {valueY.totalPercent.formatNumber('#.00')}% {valueY}";
    //            series1.name = "Approved";
    //            series1.dataFields.categoryX = "category";
    //            series1.dataFields.valueY = "value";
    //            series1.dataFields.valueYShow = "totalPercent";
    //            series1.dataItems.template.locations.categoryX = 0.5;
    //            series1.stacked = true;
    //            series1.tooltip.pointerOrientation = "vertical";

    //            var bullet1 = series1.bullets.push(new am4charts.LabelBullet());
    //            bullet1.interactionsEnabled = false;
    //            bullet1.label.text = "{valueY.totalPercent.formatNumber('#.00')}%";
    //            bullet1.label.fill = am4core.color("#ffffff");
    //            bullet1.locationY = 0.5;



    //            var series2 = chart.series.push(new am4charts.ColumnSeries());
    //            series2.columns.template.width = am4core.percent(40);
    //            series2.columns.template.tooltipText =
    //                "{name}: {valueY.totalPercent.formatNumber('#.00')}%";
    //            series2.name = "Processing";
    //            series2.dataFields.categoryX = "category";
    //            series2.dataFields.valueY = "value";
    //            series2.dataFields.valueYShow = "totalPercent";
    //            series2.dataItems.template.locations.categoryX = 0.5;
    //            series2.stacked = true;
    //            series2.tooltip.pointerOrientation = "vertical";

    //            var bullet2 = series2.bullets.push(new am4charts.LabelBullet());
    //            bullet2.interactionsEnabled = false;
    //            bullet2.label.text = "{valueY.totalPercent.formatNumber('#.00')}%";
    //            bullet2.locationY = 0.5;
    //            bullet2.label.fill = am4core.color("#ffffff");



    //            var series3 = chart.series.push(new am4charts.ColumnSeries());
    //            series3.columns.template.width = am4core.percent(40);
    //            series3.columns.template.tooltipText =
    //                "{name}: {valueY.totalPercent.formatNumber('#.00')}%";
    //            series3.name = "Rejected";
    //            series3.dataFields.categoryX = "category";
    //            series3.dataFields.valueY = "value";
    //            series3.dataFields.valueYShow = "totalPercent";
    //            series3.dataItems.template.locations.categoryX = 0.5;
    //            series3.stacked = true;
    //            series3.tooltip.pointerOrientation = "vertical";

    //            var bullet3 = series3.bullets.push(new am4charts.LabelBullet());
    //            bullet3.interactionsEnabled = false;
    //            bullet3.label.text = "{valueY.totalPercent.formatNumber('#.00')}%";
    //            bullet3.locationY = 0.5;
    //            bullet3.label.fill = am4core.color("#ffffff");


    //            // Add a legend
    //            chart.legend = new am4charts.Legend();

    //            var markerTemplate = chart.legend.markers.template;
    //            markerTemplate.width = 5;
    //            markerTemplate.height = 5;
    //            chart.legend.align = "right";
    //            chart.legend.position = "right";
    //            chart.legend.valueLabels.template.text = "{value.value}";
    //            chart.legend.valueLabels.template.fontSize = 8;
    //            chart.legend.labels.template.fontSize = 8;

    //        },
    //        error: function (response) {
    //        },
    //        complete: function () {

    //        }

    //    });

    //});
    //StaffProcessingRate
    //am4core.ready(function () {

    //    $.ajax({
    //        type: "POST",
    //        url: "/Charts/StaffProcessingRate",
    //        contentType: false,
    //        processData: false,
    //        async: false,
    //        success: function (response) {
    //            // Themes begin
    //            am4core.useTheme(am4themes_spiritedaway);
    //            // am4core.useTheme(am4themes_moonrisekingdom);
    //            // am4core.useTheme(am4themes_animated);

    //            // Themes end

    //            // Create chart instance
    //            var chart = am4core.create("StaffProcessingRate", am4charts.PieChart);

    //            chart.data = response;
    //            chart.exporting.menu = new am4core.ExportMenu();

    //            // Add and configure Series
    //            var pieSeries = chart.series.push(new am4charts.PieSeries());
    //            pieSeries.dataFields.value = "total";
    //            pieSeries.dataFields.valueYShow = "total";
    //            pieSeries.dataFields.category = "category";
    //            pieSeries.labels.template.text = "{value}";


    //            // Let's cut a hole in our Pie chart the size of 30% the radius
    //            chart.innerRadius = am4core.percent(50);
    //            chart.radius = am4core.percent(70);

    //            // Put a thick white border around each Slice
    //            pieSeries.slices.template.stroke = am4core.color("#fff");
    //            pieSeries.slices.template.strokeWidth = 2;
    //            pieSeries.slices.template.strokeOpacity = 1;
    //            pieSeries.labels.template.fontSize = 11;
    //            pieSeries.slices.template
    //                // change the cursor on hover to make it apparent the object can be interacted with
    //                .cursorOverStyle = [
    //                    {
    //                        "property": "cursor",
    //                        "value": "pointer"
    //                    }
    //                ];

    //            pieSeries.alignLabels = false;
    //            pieSeries.labels.template.bent = false;
    //            pieSeries.labels.template.radius = 3;
    //            pieSeries.labels.template.padding(0, 0, 0, 0);

    //            pieSeries.ticks.template.disabled = true;

    //            // Create a base filter effect (as if it's not there) for the hover to return to
    //            var shadow = pieSeries.slices.template.filters.push(new am4core.DropShadowFilter);
    //            shadow.opacity = 0;

    //            // Create hover state
    //            var hoverState = pieSeries.slices.template.states.getKey("hover"); // normally we have to create the hover state, in this case it already exists

    //            // Slightly shift the shadow and make it more prominent on hover
    //            var hoverShadow = hoverState.filters.push(new am4core.DropShadowFilter);
    //            hoverShadow.opacity = 0.7;
    //            hoverShadow.blur = 5;

    //            // Add a legend
    //            chart.legend = new am4charts.Legend();

    //            chart.legend.labels.template.fontSize = 12;

    //            var markerTemplate = chart.legend.markers.template;
    //            markerTemplate.width = 10;
    //            markerTemplate.height = 10;
    //            chart.legend.align = "right";
    //            chart.legend.position = "left";
    //            chart.legend.valueLabels.template.text = "{value.value}";
    //            chart.legend.valueLabels.template.fontSize = 8;
    //        }

    //    });

    //});

    

//    am4core.ready(function () {
//        $.ajax({
//            type: "POST",
//            url: "/Charts/PaymentStateChart",
//            contentType: false,
//            processData: false,
//            async: false,
//            success: function (response) {
//                var chartData = [];

//                for (var i = 0; i < response.length; i++) {
//                    chartData.push({
//                        totalAmount: response[i].totalCatAmount,
//                        stateName: response[i].stateName,

//                    });

//                }

//                am4core.useTheme(am4themes_animated);
//                am4core.useTheme(am4themes_kelly);


//                // Create chart instance
//                var chart2 = am4core.create("paymentReportLocationChart", am4charts.XYChart);
//                chart2.hiddenState.properties.opacity = 0;

//                // Add chart title
//                var title = chart2.titles.create();
//                title.text = "Payment report by location";
//                title.fontSize = 20;
//                title.marginBottom = 30;

//                chart2.data = generateChartData();
//                // Create axes
//                var categoryAxis = chart2.xAxes.push(new am4charts.CategoryAxis());
//                categoryAxis.dataFields.category = "Category";
//                categoryAxis.title.text = "States(s)";
//                categoryAxis.renderer.grid.template.location = 0;
//                categoryAxis.renderer.minGridDistance = 20;

//                var valueAxis = chart2.yAxes.push(new am4charts.ValueAxis());
//                valueAxis.title.text = "Sum Total";
//                valueAxis.calculateTotals = true;
//                valueAxis.min = 0;

//                // Create series
//                var series = chart2.series.push(new am4charts.ColumnSeries());
//                series.dataFields.valueY = "Approved";
//                series.dataFields.categoryX = "Category";
//                series.name = "Approved";
//                series.tooltipText = "{name}: [bold]{valueY}[/]";

//                var series2 = chart2.series.push(new am4charts.ColumnSeries());
//                series2.dataFields.valueY = "Pending";
//                series2.dataFields.categoryX = "Category";
//                series2.name = "Pending";
//                series2.tooltipText = "{name}: [bold]{valueY}[/]";
//                series2.stacked = true;


//                // Add cursor
//                chart2.cursor = new am4charts.XYCursor();

//                // Add legend
//                chart2.legend = new am4charts.Legend();

//            }
//        })
//    })
})

  //series1.dataFields.categoryX = "category";
                            //series1.dataFields.valueY = "categoryvalue";
                            //series1.dataFields.valueYShow = "totalPercent";
                            //series1.dataItems.template.locations.categoryX = 0.5;
                            //series1.stacked = true;
                            //series1.tooltip.pointerOrientation = "vertical";

                            //var bullet1 = series1.bullets.push(new am4charts.LabelBullet());
                            //bullet1.interactionsEnabled = false;
                            //bullet1.label.text = "{valueY.totalPercent.formatNumber('#.00')}%";
                            //bullet1.label.fill = am4core.color("#ffffff");
                            //bullet1.locationY = 0.5;
                            //