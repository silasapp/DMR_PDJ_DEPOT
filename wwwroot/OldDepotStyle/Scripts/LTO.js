
$(document).ready(function () {

    $('#addTank').unbind('click').click(function (e) {
        e.preventDefault();
        var m = $(this).attr('data-m');
        //count the number of tanks
        var t = $('.tnks').length;
        var h = getTank(t,m);
        //alert(h);
       

    });

    $(document).on('click', '.delete', function () {
        var id = $(this).attr('data-id');
        var tid = $(this).attr('data-tid');
        if (tid == undefined) {
            $('#document' + id).remove();
        } else {
            $.post('/atc/removetank', { id: tid }, function (data) {
               // alert(data);
                if (data === '0') {

                    $('#document' + id).remove();
                } else {
                    alert('Sorry we couldnt remove the Tank')
                }
            })
        };
    });

    $(document).on('click', '.hasAtg', function () {
        var tr = $(this).prop('checked');
        var count = $(".hasAtg:checked").length;
        //check if any of the box is Checked
        //check if the current one is checked or unchecked
        if (tr) {
            $('#hasAtgDiv').show();

        } else {
            if (count > 0) {
                $('#hasAtgDiv').show();
            } else {

                $('#hasAtgDiv').hide();
            }
        }

    });

    $('#addPump').unbind('click').click(function (e) {

        e.preventDefault();
        //count the number of tanks
        var t = $('.pmps').length;
        // alert(t)
        var fid = $('#facilityId').val();
        //alert(t + ':' + fid)
        var h = getPump(t, fid);

    });

});

function getTank(i, m) {
   
    var d = '';
   
    //if (m==='m') {
    //   // d = '<input type="hidden" name="tnks[' + i + '].ModifyType"  value="Addition" />';
    //    d = '<select class="form-control" name="tnks[' + i + '].ModifyType" required >'+
    //        '<option value="Type">Existing</option><option value="Addition"> Addition</option>'+
    //         '<option value="Decommission">Decommission</option><option value="Convert">Convert</option>'+
    //          '<option value="Convert">Upgrade</option></select>'
    //}
    
    // get product form the controller
   
    $.get('/lto/getproducts/' + i, function (data) {

        var t = '';
        var indx = parseInt(i) + 1;
        
        t += '<tr id="document' + i + '" class="tnks"><td style="display: table-cell; vertical-align: middle">' + indx + '</td></td>' +
            '<input type="hidden" name="tnks[' + i + '].AppTank" value="Yes" />' +
            '<td><input type="text" required name="tnks[' + i + '].Name" class="form-control" value="" placeholder="e.g. PMS Tank A" />' +
            '</td><td><input type="number" required name="tnks[' + i + '].MaxCapacity" class="form-control" value="" placeholder="e.g. 33000" />' +
            '</td><td>' + data + '</td><td><input type="text" required name="tnks[' + i + '].Diameter" class="form-control" value="" placeholder="e.g. 3.2" />' +
            '</td><td class="txtcenter" style="display: table-cell; vertical-align: middle">' +
            '<input type="text" required name="tnks[' + i + '].Height" class="form-control" value="" placeholder="e.g. 8" />' +
            '<label for="chk_' + i + '"></label></td><td><input type="checkbox" checked class="AppTank[]" name="@AppTank" value="Yes" /></td> <td><span class="btn btn-danger btn-sm delete" data-id="' + i +
            '"><i class="fa fa-times"></i>Delete</span></td>' +
            '</tr>'
;
        // d = t;
        //alert(t);
        $('#tankContainer').append(t);
        return t;
    });
}


function getPump(i, fid) {
    var d = '';
    // get product form the controller
    //alert(i+':'+ fid)
    $.get('/lto/getTanks/' + i + '?fid=' + fid, function (data) {

        var t = '';

        var indx = parseInt(i) + 1;
        t += '<tr id="document' + i + '" class="pmps"><td>' + indx + '</td>' +
            '<td><input type="text" required name="Pumps[' + i + '].Name" class="form-control" value="" ' +
            'placeholder="e.g. PMS Pump 1" /></td><td>' + data + '</td><td>' +
            '<span class="btn btn-danger btn-sm delete" data-id="' + i + '"><i class="fa fa-times"></i></span></td></tr>';

        $('#pumpContainer').append(t);
        return d;
    });

}