////am4core.ready(function () {

////     Application types in bar chart
////    $.ajax({
////        type: "POST",
////        url: "/Reports/GetApplicationTypes?id=" + $.trim($("#txtCategoryName").val()),
////        contentType: false,
////        processData: false,
////        async: false,
////        success: function (response) {
////            am4core.useTheme(am4themes_moonrisekingdom);
////            am4core.useTheme(am4themes_animated);
////             Themes end

////             Create chart instance
////            var chart = am4core.create("GetApplicationTypesDiv", am4charts.XYChart);
////            chart.hiddenState.properties.opacity = 0; // this creates initial fade-in
////            chart.data = response;
////            chart.exporting.menu = new am4core.ExportMenu();

////             Create axes
////            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
////            categoryAxis.dataFields.category = "value";
////            categoryAxis.renderer.grid.template.location = 0;
////            categoryAxis.renderer.minGridDistance = 30;
////            categoryAxis.fontSize = 10;
////            categoryAxis.renderer.labels.template.horizontalCenter = "right";
////            categoryAxis.renderer.labels.template.verticalCenter = "middle";
////            categoryAxis.renderer.labels.template.rotation = 310;
////            categoryAxis.tooltip.disabled = true;
////            categoryAxis.renderer.minHeight = 110;

////            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
////            valueAxis.renderer.minWidth = 50;

////             Create series
////            var series = chart.series.push(new am4charts.ColumnSeries());
////            series.sequencedInterpolation = true;
////            series.dataFields.valueY = "count";
////            series.dataFields.categoryX = "value";
////            series.tooltipText = "{categoryX} : {valueY}";
////            series.columns.template.strokeWidth = 0;

////            series.tooltip.pointerOrientation = "vertical";

////            series.columns.template.column.cornerRadiusTopLeft = 10;
////            series.columns.template.column.cornerRadiusTopRight = 10;
////            series.columns.template.column.fillOpacity = 0.8;

////             on hover, make corner radiuses bigger
////            var hoverState = series.columns.template.column.states.create("hover");
////            hoverState.properties.cornerRadiusTopLeft = 0;
////            hoverState.properties.cornerRadiusTopRight = 0;
////            hoverState.properties.fillOpacity = 1;

////            series.columns.template.adapter.add("fill", function (fill, target) {
////                return chart.colors.getIndex(target.dataItem.index);
////            });

////             Cursor
////            chart.cursor = new am4charts.XYCursor();
////        },
////        error: function (response) {
          
////        },
////        complete: function () {
           
////        }

////    });


////     Application types in pie chart
////    $.ajax({
////        type: "POST",
////        url: "/Reports/GetApplicationTypes?id=" + $.trim($("#txtCategoryName").val()),
////        contentType: false,
////        processData: false,
////        async: false,
////        success: function (response) {

////             Themes begin
////            am4core.useTheme(am4themes_material);
////            am4core.useTheme(am4themes_animated);
////             Themes end

////             Create chart instance
////            var chart = am4core.create("GetApplicationTypesDivPie", am4charts.PieChart);

////             Add and configure Series
////            var pieSeries = chart.series.push(new am4charts.PieSeries());
////            pieSeries.dataFields.value = "count";
////            pieSeries.dataFields.category = "value";

////            chart.exporting.menu = new am4core.ExportMenu();

////            chart.data = response;

////             Let's cut a hole in our Pie chart the size of 30% the radius
////            chart.innerRadius = am4core.percent(30);
////            chart.radius = am4core.percent(70);

////             Put a thick white border around each Slice
////            pieSeries.slices.template.stroke = am4core.color("#fff");
////            pieSeries.slices.template.strokeWidth = 2;
////            pieSeries.slices.template.strokeOpacity = 1;
////            pieSeries.labels.template.fontSize = 6;
////            pieSeries.slices.template
////                 change the cursor on hover to make it apparent the object can be interacted with
////                .cursorOverStyle = [
////                    {
////                        "property": "cursor",
////                        "value": "pointer"
////                    }
////                ];

////            pieSeries.alignLabels = false;
////            pieSeries.labels.template.bent = true;
////            pieSeries.labels.template.radius = 3;
////            pieSeries.labels.template.padding(0, 0, 0, 0);

////            pieSeries.ticks.template.disabled = true;

////             Create a base filter effect (as if it's not there) for the hover to return to
////            var shadow = pieSeries.slices.template.filters.push(new am4core.DropShadowFilter);
////            shadow.opacity = 0;

////             Create hover state
////            var hoverState = pieSeries.slices.template.states.getKey("hover"); // normally we have to create the hover state, in this case it already exists

////             Slightly shift the shadow and make it more prominent on hover
////            var hoverShadow = hoverState.filters.push(new am4core.DropShadowFilter);
////            hoverShadow.opacity = 0.7;
////            hoverShadow.blur = 5;

////             Add a legend
////            chart.legend = new am4charts.Legend();

////            chart.legend.labels.template.fontSize = 8;

////            var markerTemplate = chart.legend.markers.template;
////            markerTemplate.width = 20;
////            markerTemplate.height = 10;
////            chart.legend.align = "right";
////            chart.legend.position = "left";
////            chart.legend.valueLabels.template.text = "{value.value}";
////            chart.legend.valueLabels.template.fontSize = 8;
////        },
////        error: function (response) {

////        },
////        complete: function () {

////        }

////    });



   
////     Application status in side bar chart
////    $.ajax({
////        type: "POST",
////        url: "/Reports/GetApplicationStatus?id=" + $.trim($("#txtCategoryName").val()),
////        contentType: false,
////        processData: false,
////        async: false,
////        success: function (response) {

////            am4core.useTheme(am4themes_kelly);
////            am4core.useTheme(am4themes_animated);
////             Themes end

////             Create chart instance
////            var chart = am4core.create("GetApplicationStatusDiv", am4charts.XYChart);
////            chart.hiddenState.properties.opacity = 0; // this creates initial fade-in
////            chart.data = response;
////            chart.exporting.menu = new am4core.ExportMenu();

////            chart.padding(40, 40, 40, 40);

////            var categoryAxis = chart.yAxes.push(new am4charts.CategoryAxis());
////            categoryAxis.renderer.grid.template.location = 0;
////            categoryAxis.dataFields.category = "value";
////            categoryAxis.renderer.minGridDistance = 1;
////            categoryAxis.renderer.inversed = true;
////            categoryAxis.fontSize = 10;
////            categoryAxis.renderer.grid.template.disabled = true;

////            var valueAxis = chart.xAxes.push(new am4charts.ValueAxis());
////            valueAxis.min = 0;

////            var series = chart.series.push(new am4charts.ColumnSeries());
////            series.dataFields.categoryY = "value";
////            series.dataFields.valueX = "count";
////            series.tooltipText = "{valueX.value}"
////            series.columns.template.strokeOpacity = 0;
////            series.columns.template.column.cornerRadiusBottomRight = 5;
////            series.columns.template.column.cornerRadiusTopRight = 5;

////            var labelBullet = series.bullets.push(new am4charts.LabelBullet())
////            labelBullet.label.horizontalCenter = "left";
////            labelBullet.label.dx = 10;
////            labelBullet.label.text = "{values.valueX.workingValue.formatNumber('#.0as')}";
////            labelBullet.locationX = 1;

////             as by default columns of the same series are of the same color, we add adapter which takes colors from chart.colors color set
////            series.columns.template.adapter.add("fill", function (fill, target) {
////                return chart.colors.getIndex(target.dataItem.index);
////            });

            
////        },
////        error: function (response) {

////        },
////        complete: function () {

////        }

////    });


    
////     Application per day in line chart
////    $.ajax({
////        type: "POST",
////        url: "/Reports/GetApplicationPerDay?id=" + $.trim($("#txtCategoryName").val()),
////        contentType: false,
////        processData: false,
////        async: false,
////        success: function (response) {

////            am4core.useTheme(am4themes_moonrisekingdom);
////            am4core.useTheme(am4themes_animated);
          

////             Create chart instance
////            var chart = am4core.create("GetApplicationPerDay", am4charts.XYChart);

////            chart.hiddenState.properties.opacity = 0; // this creates initial fade-in
////            chart.data = response;
////            chart.exporting.menu = new am4core.ExportMenu();


////             Create axes
////            var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
////            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());

////             Create series
////            var series = chart.series.push(new am4charts.LineSeries());
////            series.dataFields.valueY = "value";
////            series.dataFields.dateX = "date";
////            series.tooltipText = "{value}"
////            series.strokeWidth = 2;
////            series.minBulletDistance = 15;

////             Drop-shaped tooltips
////            series.tooltip.background.cornerRadius = 20;
////            series.tooltip.background.strokeOpacity = 0;
////            series.tooltip.pointerOrientation = "vertical";
////            series.tooltip.label.minWidth = 40;
////            series.tooltip.label.minHeight = 40;
////            series.tooltip.label.textAlign = "middle";
////            series.tooltip.label.textValign = "middle";

////             Make bullets grow on hover
////            var bullet = series.bullets.push(new am4charts.CircleBullet());
////            bullet.circle.strokeWidth = 2;
////            bullet.circle.radius = 4;
////            bullet.circle.fill = am4core.color("#fff");

////            var bullethover = bullet.states.create("hover");
////            bullethover.properties.scale = 1.3;

////             Make a panning cursor
////            chart.cursor = new am4charts.XYCursor();
////            chart.cursor.behavior = "panXY";
////            chart.cursor.xAxis = dateAxis;
////            chart.cursor.snapToSeries = series;

////             Create vertical scrollbar and place it before the value axis
////            chart.scrollbarY = new am4core.Scrollbar();
////            chart.scrollbarY.parent = chart.leftAxesContainer;
////            chart.scrollbarY.toBack();

////             Create a horizontal scrollbar with previe and place it underneath the date axis
////            chart.scrollbarX = new am4charts.XYChartScrollbar();
////            chart.scrollbarX.series.push(series);
////            chart.scrollbarX.parent = chart.bottomAxesContainer;

////            dateAxis.start = 0.79;
////            dateAxis.keepSelection = true;

////        },
////        error: function (response) {

////        },
////        complete: function () {

////        }

////    });



////}); // end am4core.ready()




   





