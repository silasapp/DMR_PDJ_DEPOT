$(document).ready(function () {

    $('#addNewDirector').unbind('click').click(function (e) {

        //$.get('/Company/GetDirector', function (data) {
        $.get('/Company?id=Directors', function (data) {
            $('#formContainer').html(data);
        });
        e.preventDefault();
    });

    $('.EditDirector').unbind('click').click(function (e) {
        //alert('yep');
        var did = $(this).attr('data-dirid');

        $.get('/Company/GetDirector', { id: did }, function (data) {
            $('#formContainer').html(data);
        });
        e.preventDefault();
    });

    $('.appOnDirectorEdit').unbind('click').click(function (e) {
        //alert('yep');
        var did = $(this).attr('data-dirid');

        $.get('/Company/GetDirector', { id: did, appOn: 'Yes' }, function (data) {
            $('#formContainer').html(data);
        });
        e.preventDefault();
    });

    ////////// Staff section addNewStaff
    $(document.body).on("click", "#sCertAddButton", function (e) {
        var noOfNtifs = $('.sCert').length;
        $('#sCertDiv').append(GetStaffDiv(noOfNtifs))

    });
    $(document.body).on("click", ".sCertDelete", function (e) {
        if (confirm('Do you really want to remove this?')) {
            var divToremove = $(this).attr('data-DivId');

            $('#' + divToremove).remove();
        }
        //if ($('.sCert').length > 1) {
            
        //}
        //else {
        //    alert('this section can not be empty');
        //}
        //e.preventdefault();
    });

    $('#addNewStaff').unbind('click').click(function (e) {

        $.get('/Company/GetStaff', function (data) {
            $('#StaffFormContainer').html(data);
        });
        e.preventDefault();
    });

    $('.EditStaff').unbind('click').click(function (e) {
        //alert('yep');
        var sid = $(this).attr('data-sid');

        $.get('/Company/GetStaff', { id: sid }, function (data) {
            $('#StaffFormContainer').html(data);
        });
        e.preventDefault();
    });

    $('.appOnStaffEdit').unbind('click').click(function (e) {
        //alert('yep');
        var sid = $(this).attr('data-sid');

        $.get('/Company/GetStaff', { id: sid, appOn: 'yes' }, function (data) {
            $('#StaffFormContainer').html(data);
        });
        e.preventDefault();
    });

    CheckForDirectors();
    CheckForStaffs();

});

function CheckForDirectors() {
    var dirs = $("#dirList li").length;
    if (dirs > 0) {
        $("#btnDirContinue").removeClass("hide").show();
    }
}

function CheckForStaffs() {
    var stfs = $("#stfList li").length;
    if (stfs > 0) {
        $("#btnStfContinue").removeClass("hide").show();
    }
}

function GetStaffDiv(noOfExisting) {
    var nxt = noOfExisting;
    var divId = 'sCert' + nxt;

    var divStr = "";

    var nm = '<div class="col-md-3 form-group"><label class="control-label">Certificate Name</label>' +
        '<div class=""><input type="text" name="sCert['+nxt+'].Name" value="" class="form-control" /></div></div>';
    
    var cn = '<div class="col-md-3 form-group"><label class="control-label">Certificate Number</label>' +
        '<div class=""><input type="text" name="sCert['+ nxt +'].Cert_No" value="" class="form-control" /></div></div>'
    
    var cis = '<div class="col-md-3 form-group"><label class="control-label">Certificate Issuer</label>' +
        '<div class=""><input type="text" name="sCert[' + nxt + '].Issuer" value="" class="form-control" /></div></div>'

    var yr = '<div class="col-md-2 form-group"><label class="control-label">Year</label>' +
        '<div class=""><input type="text" readonly name="sCert['+nxt+'].Year" value="" class="form-control datePickerY" /></div></div>'
    
    var dltBtn = '<div class="col-md-1"><span class="btn btn-sm btn-danger sCertDelete" data-divid="' + divId + '"><i class="glyphicon glyphicon-remove"></i></span></div>'
    
    divStr = '<div class="row sCert og_form" id="' + divId + '"><div class="well">' + nm + cis + cn + yr + dltBtn + ' <div class="clear"></div></div></div>';


    return divStr;
    /* 
                            //<div class="row sCert og_form" id="@divId">
                            //    <div class="well">
                                    <div class="col-md-5 form-group">
                                        <label class="control-label">Certificate Name</label>
                                        <div class="">
                                            <input type="text" name="sCert[0].Name" value="" class="form-control" />
                                        </div>
                                    </div>
                                    <div class="col-md-3 form-group">
                                        <label class="control-label">Certificate Number</label>
                                        <div class="">
                                            <input type="text" name="sCert[0].Cert_No" value="" class="form-control" />
                                        </div>
                                    </div>
                                    <div class="col-md-3 form-group">
                                        <label class="control-label">Year Issued</label>
                                        <div class="">
                                            <input type="text" readonly name="sCert[0].Year" value="" class="form-control datePickerY" />
                                        </div>
                                    </div>
                                    <div class="col-md-1">
                                        <span class="btn btn-sm btn-danger sCertDelete" data-divid="@divId">
                                            <i class="glyphicon glyphicon-remove"></i>
                                        </span>
                                    </div>
                                    <div class="clear"></div>
                            //    </div>
                            //</div>

    */
}
