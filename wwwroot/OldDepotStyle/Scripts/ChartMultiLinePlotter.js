
$(document).ready(function () {

    var chartData = JSON.parse($('#chatData').val());
    
    PlotMultiLineChart($("#placeholder33x"), chartData);


});

function PlotMultiLineChart(divElement, chartData) {
    var cd = [];
    //define chart clolors ( you maybe add more colors if you want or flot will add it automatic )
    var chartColours = ['#96CA59', '#3F97EB', '#72c380', '#6f7a8a', '#f7cb38', '#5a8022', '#2c7282'];

   
    $.each(chartData, function (i, id) {
        var cd1 = {
            label: '',
            data: []
        }
        cd1.label = id.label;
        $.each(id.data, function (t, y) {
            var dl = [];
            dl.push(moment(y[0]));
            dl.push(y[1]);
            cd1.data.push(dl);
        });
        cd.push(cd1);
        
    });

    var tickSize = [1, "day"];
    var tformat = "%d/%m/%y";

    //graph options
    var options = {
        grid: {
            show: true,
            aboveData: true,
            color: "#3f3f3f",
            labelMargin: 10,
            axisMargin: 0,
            borderWidth: 0,
            borderColor: null,
            minBorderMargin: 5,
            clickable: true,
            hoverable: true,
            autoHighlight: true,
            mouseActiveRadius: 100
        },
        series: {
            lines: {
                show: true,
                fill: true,
                lineWidth: 2,
                steps: true
            },
            points: {
                show: true,
                radius: 4.5,
                symbol: "circle",
                lineWidth: 3.0
            }
        },
        legend: {
            position: "ne",
            margin: [0, -25],
            noColumns: 0,
            labelBoxBorderColor: null,
            labelFormatter: function (label, series) {
                // just add some space to labes
                return label + '&nbsp;&nbsp;';
            },
            width: 40,
            height: 1
        },
        colors: chartColours,
        shadowSize: 0,
        tooltip: true, //activate tooltip
        tooltipOpts: {
            content: "%s: %y.0",
            xDateFormat: "%d/%m",
            shifts: {
                x: -30,
                y: -50
            },
            defaultTheme: false
        },
        yaxis: {
            min: 0
        },
        xaxis: {
            mode: "time",
            minTickSize: tickSize,
            timeformat: tformat
            //,
            //min: chartMinDate,
            //max: chartMaxDate
        }
    };
    var plot = $.plot(divElement,
        cd,
        options);
};

