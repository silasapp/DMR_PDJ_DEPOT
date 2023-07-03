function PlotLineChart(element, label, data) {
	var chartData = [];
	//chartData = data;
	$.each(data, function (i, iData) {
		chartData.push([moment(iData.Key), iData.Value]);
	});

	//define chart clolors ( you maybe add more colors if you want or flot will add it automatic )
	var chartColours = ['#96CA59', '#3F97EB', '#72c380', '#6f7a8a', '#f7cb38', '#5a8022', '#2c7282'];
	
	//$.each(chartData, function (i, iData) {
	//	console.log(iData);
	//});

	//console.log(chartData[0][0]);
	var chartMinDate = chartData[0][0]; //first day
	var chartMaxDate = chartData[29][0];
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
				steps: false
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
		tooltip: true,
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
			timeformat: tformat,
			min: chartMinDate,
			max: chartMaxDate
		}
	};

	var plot = $.plot(element, [{
		label: label,
		data: chartData,
		lines: {
			fillColor: "rgba(150, 202, 89, 0.12)" 
		}, //#96CA59 rgba(150, 202, 89, 0.42)
		points: {
			fillColor: "#fff"
		}
	}], options);
}
