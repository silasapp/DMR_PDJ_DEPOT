// --DatePicker Controller--

        $(document).ready(function () {
            var cb = function (start, end, label) {
                $('#reportrange span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));

                $("#startDate").val(start.format('MM/DD/YYYY')); // dd/mm/yy
                $("#endDate").val(end.format('MM/DD/YYYY')); // dd/mm/yy
            };

            var d = new Date();
            var month = d.getMonth() + 1;
            var day = d.getDate();
            var mDate = (month < 10 ? '0' : '') + month + '/' + (day < 10 ? '0' : '') + day + '/' + d.getFullYear();
            var optionSet1 = {
                startDate: moment().subtract(29, 'days'),
                endDate: moment(),
                minDate: '01/01/2016',
                maxDate: mDate, //'12/31/2015',
                dateLimit:
                        {
                            days: 60
                        },
                showDropdowns: true,
                showWeekNumbers: true,
                timePicker: false,
                timePickerIncrement: 1,
                timePicker12Hour: true,
                ranges:
                        {
                            'Today': [moment(), moment()],
                            'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                            'This Month': [moment().startOf('month'), moment().endOf('month')],
                    'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                    'This Year': [moment().startOf('year'), moment().endOf('year')],
                    'Last Year': [moment().subtract(1, 'year').startOf('year'), moment().subtract(1, 'year').endOf('year')]
                        },
                opens: 'left',
                buttonClasses: ['btn btn-default'],
                applyClass: 'btn-sm btn-primary',
                cancelClass: 'btn-sm',
                format: 'MM/DD/YYYY',
                separator: ' to ',
                locale: {
                    applyLabel: 'Submit',
                    cancelLabel: 'Clear',
                    fromLabel: 'From',
                    toLabel: 'To',
                    customRangeLabel: 'Custom',
                    daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                    monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                    firstDay: 1
                }
            };

            //Sets the Date range label witht the default values
            if ($("#startDate").val().length > 0 && $("#endDate").val().length > 0) {
                var mSDate = moment($("#startDate").val()).format('MMMM DD, YYYY');
                var mEDate = moment($("#endDate").val()).format('MMMM DD, YYYY');
                $('#reportrange span').html(mSDate + ' - ' + mEDate);
            }
            else {
                $('#reportrange span').html(moment().subtract(29, 'days').format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));
            }
            $('#reportrange').daterangepicker(optionSet1, cb);
            //$('#reportrange').on('show.daterangepicker', function() {
            //    console.log("show event fired");
            //});
            //$('#reportrange').on('hide.daterangepicker', function() {
            //    console.log("hide event fired");
            //});
            //$('#reportrange').on('apply.daterangepicker', function(ev, picker) {
            //    console.log("apply event fired, start/end dates are " + picker.startDate.format('MMMM D, YYYY') + " to " + picker.endDate.format('MMMM D, YYYY'));
            //});
            //$('#reportrange').on('cancel.daterangepicker', function(ev, picker) {
            //    console.log("cancel event fired");
            //});
            $('#options1').click(function () {
                $('#reportrange').data('daterangepicker').setOptions(optionSet1, cb);
            });
            $('#options2').click(function () {
                $('#reportrange').data('daterangepicker').setOptions(optionSet2, cb);
            });
            $('#destroy').click(function () {
                $('#reportrange').data('daterangepicker').remove();
            });
        });
