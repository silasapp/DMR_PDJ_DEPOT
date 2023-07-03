
$(document).ready(function () {

    if (parseInt($("#Accident").val()) > 0) {
        $("#Acdt_rpt").find("textarea").attr("required", "required");
    }
    else {
        $("#Acdt_rpt").hide();
        $("span.accReq").hide();
    }

    $('#ntifsAddButton').unbind("click").on("click", function (e) {
        var noOfNtifs = $('.ntifs').length;
        $('#ntifsDiv').append(GetNtifDiv(noOfNtifs))
    });
    $(document.body).on("click", ".ntifsDelete", function (e) {
        if (confirm('Do you want to remove this section from the application?')) {
            var divToremove = $(this).attr('data-DivId');

            $('#' + divToremove).remove();
        }
        //if ($('.ntifs').length > 1) {
        //    if (confirm('Do you want to remove this section from the application?')) {
        //        var divToremove = $(this).attr('data-DivId');

        //        $('#' + divToremove).remove();
        //    }
        //} else {
        //    alert('this section can not be empty');
        //}

        e.preventdefault();
    });

    $("#Accident, #Company_Accident").on("change", function () {
        //alert("Change: " + $(this).val());
        var value = $(this).val();
        console.log(value);
        if (parseInt(value) > 0) {
            $("#Acdt_rpt").show();
            $("span.accReq").show();
            $("#Acdt_rpt").find("textarea").attr("required", "required");
        }
        else {
            $("#Acdt_rpt").hide();
            $("span.accReq").hide();
            $("#Acdt_rpt").find("textarea").removeAttr("required");
        }
    });

    $('#compMedAddButton').unbind("click").on("click", function (e) {
        $('#compMedDiv').append(LoadingSmall());
        var noOfNtifs = $('.compMed').length;
        //alert("MED");
        var data = GetCompMedDiv(noOfNtifs);
        $('#compMedDiv div.busy').remove();
        $('#compMedDiv').append(data);

    });
    $(document.body).on("click", ".compMedDelete", function (e) {
        if (confirm('Are you sure you want to remove this selection?')) {
            var divToremove = $(this).attr('data-DivId');

            $('#' + divToremove).remove();
        }
        //if ($('.compMed').length > 1) {
            
        //}
        //else {
        //    alert('this section can not be empty');
        //}
        //e.preventdefault();
    });

    $('#compProfAddButton').unbind("click").on("click", function (e) {
        $('#compProfDiv').append(LoadingSmall());
        var noOfNtifs = $('.compProf').length;

        var data = GetConfDiv(noOfNtifs);
        $('#compProfDiv div.busy').remove();
        $('#compProfDiv').append(data);

    });
    $(document.body).on("click", ".compProfDelete", function (e) {
        if (confirm('Are you sure you want to remove this Selection?')) {
            var divToremove = $(this).attr('data-DivId');

            $('#' + divToremove).remove();
        }
        //if ($('.compProf').length > 1) {
            
        //}
        //else {
        //    alert('this section can not be empty');
        //}
        //e.preventdefault();
    });


    $('#compExpertAddButton').unbind("click").on("click", function (e) {
        var no = $('.compExpert').length;
        $('#compExpertDiv').append(LoadingSmall());
        $.get("/Company/ExtraExpertrate", { counter: no }, function (data) {
            $('#compExpertDiv div.busy').remove();
            $('#compExpertDiv').append(data);
        });
    });

    $(document.body).on("click", ".compExpertDelete", function (e) {
        if (confirm('Are you sure you want to remove this Selection?')) {
            var divToremove = $(this).attr('data-DivId');

            $('#' + divToremove).remove();
        }
    });

    $('#compTechAddButton').unbind("click").on("click", function (e) {
        var no = $('.compTech').length;
        $('#compTechDiv').append(LoadingSmall());
        $.get("/Company/ExtraMOU", { counter: no }, function (data) {
            $('#compTechDiv div.busy').remove();
            $('#compTechDiv').append(data);
        });
    });

    $(document.body).on("click", ".compTechDelete", function (e) {
        if (confirm('Are you sure you want to remove this Selection?')) {
            var divToremove = $(this).attr('data-DivId');

            $('#' + divToremove).remove();
        }
    });

    $(".counter").on("keypress", function (ev) {
        if (parseInt($(this).val().length) >= 2000) {
            return false;
        } else if (parseInt($(this).val().length) > 0) {
            //Countdown the Notifier
            var text = '<b style="color: #f00;">' + (2000 - (parseInt($(this).val().length) + 1)) + '</b> Characters left';
            $(this).parent("div").find("p.numb").html(text);
        }
        else {
            var text = 'Maximum of <b>2000</b> Characters';
            $(this).parent("div").find("p.numb").html(text);
        }
    });

    //'use strict';
    $(document.body).on("click", ".upLoad", function () {
        var me = $(this);
        var i = $(this).attr("data-i");
        var owner = $(this).data("owner");
        var id = $(this).data("id");
        var note = $(this).data("note");
        var urlinit = $(this).data("apiurl");
        console.log(urlinit);
        //var loc = '/Company/UploadFile?id=' + $("#Id").val() + '&comp=' + $("#companyName").val() + '&note=' + note;
        var loc = urlinit + 'Company/UploadFile';
        console.log(loc);
        $("#" + owner +"Prog-" + i).removeClass("hide").show();

        var frmData = new FormData();
        var file = $(this)[0];
        if (file.length > 0) {
            frmData.append("file", file.files[0])
        }

        $(this).fileupload({
            dataType: 'json',
            url: loc,
            //data: frmData,
            //cache: false,
            //contentType: false,
            //processData: false,
            done: function (e, data) {
                $("#" + owner + "FileBtn-" + i).hide();
                //var d = json.parent
                console.log(data.result);
                $("#" + owner + "-" + i).val(data.result.fileid);
                $('<p/>').text(data.result.name).appendTo("#" + owner + "FileName" + "-" + i);

                //$.each(data.result, function (index, file) {
                //    //var fid = $(fileId);
                //    //alert("Hello: " + file.fileid + " for: " + "#compExpert[" + i + "].FileId");
                //    console.log(file.fileid);
                //    $("#" + owner + "-" + i).val(file.fileid);
                //    $('<p/>').text(file.name).appendTo("#" + owner + "FileName" + "-" + i);
                //    //$("#" + owner + "Prog-" + i).removeClass("hide").hide();
                //});
            },
            progressall: function (e, data) {
                //alert(uid);
                var progress = parseInt(data.loaded / data.total * 100, 10);
                var progBox = '#' + owner + 'Prog-' + i + ' .progress-bar';

                $(progBox).css(
                    'width',
                    progress + '%'
                ).text(progress + '%');
            }
        }).prop('disabled', !$.support.fileInput)
        .parent().addClass($.support.fileInput ? undefined : 'disabled')
    });

});


function GetNtifDiv(noOfExisting) {
    var nxt = noOfExisting;
    var divId = 'ntifs' + nxt;

    var divStr = "";

    var id = '<input type="hidden" name="ntifs[' + nxt + '].Id" value="" />'; //"ntifs[" + i + "].Id";
    var fi = '<input type="hidden" name="ntifs[' + nxt + '].FileId" value="" />'; //"ntifs[" + i + "].FileId";
    var npc = '<input type="number" required min="1" name="ntifs[' + nxt + '].No_People_Covered" value="" class="form-control" placeholder="No of People covered" />';// "ntifs[" + i + "].No_People_Covered";
    var pn = '<input type="text" required name="ntifs[' + nxt + '].Policy_No" value="" class="form-control" placeholder="Policy No" style="width: 100%; display: inline-block;" />';//"ntifs[" + i + "].Policy_No";
    var di = '<input type="text" required name="ntifs[' + nxt + '].Date_Issued" value="" class="form-control datePicker" data-dateDirection="backward" />';//"ntifs[" + i + "].Date_Issued";
    var file = '<input type="file" name="ntifs[' + nxt + '].file" value=" " />';// "ntifs[" + i + "].file";
    var dltBtn = '<span class="btn btn-xs btn-danger pull-right ntifsDelete" data-DivId="' + divId + '"><i class="glyphicon glyphicon-remove"></i></span>';

    divStr = '<div class="ntifs" id="' + divId + '"><div class="col-md-5"><label class="control-label">Policy Number</label>' + id + fi + pn
        + '</div><div class="col-md-3"><label class="control-label">No. of people covered</label>' + npc
        + '</div><div class="col-md-3"><label class="control-label">Date Issued</label><div class="input-group">' + di + '<span class="input-group-addon">'
        + '<i class="glyphicon glyphicon-calendar"></i></span></div></div><div class="col-md-1"><label class="control-label">&nbsp;</label>' + dltBtn + '</div><div class="clear">&nbsp;</div></div>';

    return divStr;

}


function GetCompMedDiv(noOfExisting) {
    var nxt = noOfExisting;
    var divId = 'compMed' + nxt;

    var divStr = "";

    var id = '<input type="hidden" name="compMed[' + nxt + '].Id" value="" />'; //"ntifs[" + i + "].Id";
    var fi = '<input type="hidden" name="compMed[' + nxt + '].FileId" value="" />'; //"ntifs[" + i + "].FileId";
    var mo = '<input type="text" required name="compMed[' + nxt + '].Medical_Organisation" value="" class="form-control lefty check" placeholder="Medical Organization" style="width: 100%; display: inline-block;" />';// "ntifs[" + i + "].No_People_Covered";
    var pn = '<input type="text" required name="compMed[' + nxt + '].Phone" value=""class="form-control lefty" placeholder="Phone Number" />';
    var em = '<input type="email" required name="compMed[' + nxt + '].Email" value="" class="form-control lefty" placeholder="Email address" />';
    var di = '<input type="text" required name="compMed[' + nxt + '].Date_Issued" value="" class="form-control datePicker" data-dateDirection="backward" />';//"ntifs[" + i + "].Date_Issued";
    var ad = '<textarea required name="compMed[' + nxt + '].Address" class="form-control" placeholder="Address"></textarea>';
    //var file = '<input type="file" name="compMed[' + nxt + '].file" value=" " />';// "ntifs[" + i + "].file";
    var dltBtn = '<span class="btn btn-xs btn-danger pull-right compMedDelete" data-DivId="' + divId + '"><i class="glyphicon glyphicon-remove"></i></span>';

    divStr = '<div class="compMed" id="' + divId + '">' 
        + '<div class="col-md-7"><label class="control-label">Mediacal Organization (Hospital Name)</label>' + id + fi + mo
        + '<br /><br /><div style="width: 49%; margin-right: 2%; display: inline-block; float:left;"><label class="control-label">Telephone</label>'
        + pn + '</div><div style="width: 49%; display: inline-block; float: left;"><label class="control-label">Email</label>'
        + em + '</div><br class="clear" /><br /><label class="control-label">Address</label>'
        + ad + '</div>'
        + '<div class="col-md-3 col-md-offset-1"><label class="control-label">Date Issued</label><div class="input-group">' + di + '<span class="input-group-addon">'
        + '<i class="glyphicon glyphicon-calendar"></i></span></div></div><div class="col-md-1"><label class="control-label">&nbsp;</label>' + dltBtn + '</div><div class="clear">&nbsp;</div></div>';


    return divStr;

}

function GetConfDiv(noOfExisting) {
    var nxt = noOfExisting;
    var divId = 'compProf' + nxt;

    var divStr = "";

    var id = '<input type="hidden" name="compProf[' + nxt + '].Id" value="" />'; //"ntifs[" + i + "].Id";
    var fi = '<input type="hidden" name="compProf[' + nxt + '].FileId" value="" />'; //"ntifs[" + i + "].FileId";
    var po = ' <input type="text" required name="compProf[' + nxt + '].Proffessional_Organisation" value="" class="form-control lefty" placeholder="Professional Organization" style="width: 100%; display: inline-block;"/>';// "ntifs[" + i + "].No_People_Covered";
    //var cn = '<input type="text" name="compProf[' + nxt + '].Cert_No" value="" class="form-control"  placeholder="Policy Number" style="width: 50%; display: inline-block;" />';//"ntifs[" + i + "].Policy_No";
    var di = '<input type="text" required name="compProf[' + nxt + '].Date_Issued" value="" class="form-control datePicker" data-dateDirection="backward" />';//"ntifs[" + i + "].Date_Issued";
    //var file = '<input type="file" name="compProf[' + nxt + '].file" value=" " />';// "ntifs[" + i + "].file";
    var dltBtn = '<span class="btn btn-xs btn-danger pull-right compProfDelete" data-DivId="' + divId + '"><i class="glyphicon glyphicon-remove"></i></span>';

    //divStr = '<div class="compProf well" id="' + divId + '"><div class="col-md-7">' + id + fi
    //    + po + cn + '<br class="clear" /><br /><div class="" style="width: 50%; display: inline-block;"><div class="input-group">'
    //    + di + '<span class="input-group-addon"><i class="glyphicon glyphicon-calendar"></i></span></div></div></div>'
    //    + '<div class="col-md-3 col-md-offset-1">' + file + '</div><div class="col-md-1">' + dltBtn + '</div><div class="clear"></div></div>';

    divStr = '<div class="compMed" id="' + divId + '"><div class="col-md-7"><label class="control-label">Professional Organization</label>' + id + fi + po
        + '</div>'
        + '<div class="col-md-3 col-md-offset-1"><label class="control-label">Date Issued</label><div class="input-group">' + di + '<span class="input-group-addon">'
        + '<i class="glyphicon glyphicon-calendar"></i></span></div></div><div class="col-md-1"><label class="control-label">&nbsp;</label>' + dltBtn + '</div><div class="clear">&nbsp;</div></div>';



    return divStr;

}

function GetExprtQDiv(noOfExisting) {
    var nxt = noOfExisting;
    var divId = 'compExpert' + nxt;

    var divStr = "";

    var id = '<input type="hidden" name="compExpert[' + nxt + '].Id" value="" />'; //"ntifs[" + i + "].Id";
    //var fi = '<input type="hidden" name="compExpert[' + nxt + '].FileId" value="" />'; //"ntifs[" + i + "].FileId";
    var nm = ' <input type="text" required name="compExpert[' + nxt + '].Name" value="" class="form-control" placeholder="Expertraite Name" style="width: 100%; display: inline-block;" />';// "ntifs[" + i + "].No_People_Covered";
    //var file = '<input type="file" name="compProf[' + nxt + '].file" value=" " />';// "ntifs[" + i + "].file";
    var fi = '<input type="hidden" name="compExpert[' + nxt + '].FileId" data-fid="' + nxt + '" value="" />';
    var file = '<input type="file" required name="files" class="upLoad" data-owner="compExpert[' + nxt + '].FileId" data-id="compExpert[' + nxt + '].Id" data-i="' + nxt + '">';
    var dltBtn = '<span class="btn btn-xs btn-danger pull-right compExpertDelete" data-DivId="' + divId + '"><i class="glyphicon glyphicon-remove"></i></span>';

    divStr = '<div class="compExpert" id="' + divId + '"><div class="col-md-6"><label class="control-label">Name</label>' + id + fi
        + nm + '</div><div class="col-md-4 col-md-offset-1">'
        + '<label class="control-label">&nbsp;</label><span class="btn btn-success fileinput-button" id="filebtn-' + nxt + '">'
        + '<i class="glyphicon glyphicon-plus"></i><span>&nbsp;Select file</span>' + file
        + '</span><div id="fileName-' + nxt + '" class="files"></div><div id="prog-' + nxt + '" class="progress hide">'
        + '<div class="progress-bar progress-bar-success progress-bar-striped"></div></div></div>'
        + '<div class="col-md-1"><label class="control-label">&nbsp;</label>' + dltBtn + '</div><div class="clear">&nbsp;</div></div>';


    return divStr;

}

function GetTechADiv(noOfExisting) {
    var nxt = noOfExisting;
    var divId = 'compTech' + nxt;

    var divStr = "";

    var id = '<input type="hidden" name="compTech[' + nxt + '].Id" value="" />'; //"ntifs[" + i + "].Id";
    var fi = '<input type="hidden" name="compTech[' + nxt + '].FileId" value="" />'; //"ntifs[" + i + "].FileId";
    var nm = ' <input type="text" required name="compTech[' + nxt + '].Name" value="" class="form-control" placeholder="Name" style="width: 100%; display: inline-block;" />';// "ntifs[" + i + "].No_People_Covered";
    var file = '<input type="file" required name="compTech[' + nxt + '].file" value=" " />';// "ntifs[" + i + "].file";
    var dltBtn = '<span class="btn btn-xs btn-danger pull-right compTechDelete" data-DivId="' + divId + '"><i class="glyphicon glyphicon-remove"></i></span>';

    divStr = '<div class="compTech" id="' + divId + '"><div class="col-md-7"><label class="control-label">Technical Agreement or MOU</label>' + id + fi
        + nm + '</div><div class="col-md-3 col-md-offset-1"><label class="control-label">&nbsp;</label>' + file + '</div><div class="col-md-1"><label class="control-label">&nbsp;</label>'
        + dltBtn + '</div><div class="clear">&nbsp;</div></div>';


    return divStr;

}

