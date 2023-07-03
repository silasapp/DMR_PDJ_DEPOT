

$(function () {



    function showLoadingMsg() {
        //$('body').append($('div').css({
        //    position: 'absolute',
        //    width: '10%',
        //    height: '10%',
        //    zIndex: 1000,
        //    background: '#000',
        //    opacity: 0.5
        //}).attr('id', 'loading-message'));
    }

    function hideLoadingMsg() {
        $('#loading-message').remove();
    }

    $("#category").on("change", function () {
      
        var cat = $(this).val();
        
        if (cat === '1') {
            // New
            $("#appStage").removeClass("hide");
            $("#appStage").addClass("loading");
            $("#AppContinueDiv").addClass("hide");
            $('#permitLabel').removeAttr('required');
            $("#sanctionContainer").addClass("hide");
            $("#permitId").removeAttr('required');
            $("#facfrm").attr('action', '/Application/Suitability');
            $("#facfrm").attr('method', 'post');
            ///
            var selBox = $("#Type");
            $.get("/Application/GetPhases", { id: cat }, function (data) {
                selBox.empty();
                selBox.append('<option value="">Select Application Stage</option>')
                $.each(data, function (i, item) {
                    if (i > 0) {
                        var pI = parseInt(i) - 1;
                        var prev = data[pI].Name;
                      
                        selBox.append("<option value='" + item.id + "' data-prev='" + prev + "'>" + item.name + "</option>");
                    }
                    else {
                        selBox.append("<option value='" + item.id + "' data-prev='" + "" + "'>" + item.name + "</option>");
                    }
                    $("#appStage").removeClass("loading");

                });
            });

        }
        else if (cat == '5') {

            $("#appStage").addClass("hide");
            $("#AppContinueDiv").addClass("hide");
            $("#sanctionContainer").removeClass("hide");
            //$("#sanctionContainer").addClass("loading");

            $("#sId").attr('required', '');
            $("#permitId").removeAttr('required');
            $("#facfrm").attr('method', 'get');
            $("#facfrm").attr('action', '/LTO/Review');
            $('#btnSubmit').text("Continue").removeClass("hide");
            $('#newSuitDiv').addClass("hide");
            $('#newSuitDiv').html('');

            ///
            var selBox = $("#sid");
            $.get("/Application/GetPhases", { id: cat }, function (data) {
                selBox.empty();
                selBox.append('<option value="">Select Sanction to Pay for</option>')
                $.each(data, function (i, item) {
                    if (i > 0) {
                        selBox.append("<option value='" + item.id + "'>" + item.name + "</option>");
                    }
                    else {
                        selBox.append("<option value='" + item.id + "'>" + item.name + "</option>");
                    }
                });
            });

        }
   
        else {
             
            if (cat === '7') {
                $('#permitLabel').html('Enter ATC or Regularization Reference');
                $('#permitId').val('').prop('readonly', false);
                $("#facfrm").attr('method', 'get');
                $("#facfrm").attr('action', '/LTO/Review');
            }

            else if (cat === '8' || cat === '1002') {
              
                $('#permitLabel').text("Click 'Continue' to proceed ");
                $('#permitId').val('REG').prop('readonly', true);
                $("#facfrm").attr('action', '/Application/Apply');


            }
            else {
                $('#permitLabel').html('Enter Your Depot LTO Ref.');
                $("#permitId").attr('required', '');

                $("#facfrm").attr('method', 'get');
                $("#facfrm").attr('action', '/LTO/Review');
            }
                $("#appStage").addClass("hide");
                $("#AppContinueDiv").removeClass("hide");
                $("#sanctionContainer").addClass("hide");
                $('#newSuitDiv').addClass("hide");
               
                $('#btnSubmit').text("Continue").removeClass("hide");
                $('#newSuitDiv').html('');
            
        };
    });

    $('#sid').on('change', function () {
        var san = $(this).val();
        if (san == '9' || san == '10') {

            $("#appStage").addClass("hide");
            $("#AppContinueDiv").removeClass("hide");
            $("#AppContinueDiv").addClass("loading");

            $("#permitId").attr('required', '');
            $('#permitLabel').html('Enter Your Current Depot LTO:');
            $("#AppContinueDiv").removeClass("loading");

        } else {

            $("#appStage").addClass("hide");
            $("#AppContinueDiv").addClass("hide");
            //$('#permitLabel').removeAttr('required');
            $("#permitId").removeAttr('required');
        }

    })

    $("#Type").on("change", function () {
        $('#phaseId').val('');
        var type = $(this).val();
        var $this = $(this);
        $('#phaseId').val(type);

        if (type === '1') {
            //Suitability selected
            $('#newSuitDiv').removeClass("hide").html();
            $('#newSuitDiv').addClass("loading");
            //change appstage icon
            $('#step1').removeClass("selected");
            $('#appstage').val("");
            $('#appstage2').val("50%");
            $('#step2').addClass("selected");


            $('#AppContinueDiv').addClass("hide");
            $("#sanctionContainer").addClass("hide");
            $('#facfrm').attr('action', '/Application/ApplyForNewDepot')
            //$('#facfrm').attr('action', '/Application/ApplyForNewDepot')
            $('#permitId').prop('required', false);
            $.get('/Application/getSuitForm', function (data) {
                $('#newSuitDiv').html(data);
                $('#newSuitDiv').removeClass("loading");
               
                $('#btnSubmit').text("Preview Application").removeClass("hide");
            });
        }
        else if (parseInt(type) > 1) {
            //Others
            //alert(type);
            var label = $this.find("option:selected").text();
          
            $('#permitLabel').html(label + ' Ref.');
           
            $('#newSuitDiv').html('');//addClass("hide");
            $("#AppContinueDiv").removeClass("hide");
            $("#sanctionContainer").addClass("hide");
            $('#facfrm').attr('action', '/Application/Apply')//
          
            if (type === '2') {
              
                $('#permitLabel').html("ENTER SUITABILITY APPROVAL REF.");
                $('#ATCMessage').text("Kindly provide your suitability approval/legacy reference.");
                $('#permitId').val('');
              
            }
            else if (type === '8' || type === '1002') {
              
                $('#ATCMessage').text("Click 'Continue' to proceed " );
                $('#permitId').val('REG').prop('readonly', true);
            }
            else if (type === '4') {
              
                $('#ATCMessage').text("");
                $('#permitLabel').html("ENTER CALIBRATION REFERENCE");
              
            }
            else {
                $('#ATCMessage').text("");
                $('#permitId').prop('required', true);
                $('#permitId').val('').prop('readonly', false);
            }
            $('#newSuitDiv').html('');
            $('#btnSubmit').text("Continue").removeClass("hide");
        }
        else {
            $('#ATCMessage').text("");
            $('#newSuitDiv').addClass("hide");
            $('#AppContinueDiv').addClass("hide");
            $("#sanctionContainer").addClass("hide");
            $('#facfrm').attr('action', '/Application/Apply')
            $('#permitId').prop('required', true);
            $('#btnSubmit').text("Preview Application").addClass("hide");
            //change appstage icon
            $('#step2').removeClass("selected");
            $('#appstage2').val("");
            $('#appstage').val("25%");
            $('#step1').addClass("selected");

        }
    });


    

    $(document.body).on("change", '#stateId', function () {
        var stt = $(this).val();

        $.get("/Helpers/getlga/" + stt, function (data) {
            $("#lga").empty();
            $('#lga').append($('<option/>', { Value: "", text: "Select Local Government" }));
            $.each(data, function (i, obj) {
                $('#lga').append($('<option>').text(obj.name).attr('value', obj.id));
            });
        });
    });

    $('#btnSubmit').click(function (e) {
        e.preventDefault();
        var select = parseInt($("#Type option:selected").val());
        if (select == 1) {
      
           
            var AlongPipeLine = $("input[name='ISAlongPipeLine']:checked").val();
            var UnderHighTension = $("input[name='IsUnderHighTension']:checked").val();
            var OnHighWay = $("input[name='IsOnHighWay']:checked").val();
            if (AlongPipeLine === undefined) {
                alert("Please, check if it is along pipeline.");
            }
            else if (UnderHighTension === undefined) {
                alert("Please, check if it is under high tension.");
            }
            else if (OnHighWay === undefined) {
                alert("Please, check if it is on high way.");
            }
            else {
                var validator = $("#facfrm").valid(); // obtain validator

                var anyError = false;
                $("#facfrm #suitWrapper").find(".form-control").each(function () {
                    if (validator != true) { // validate every input element inside this step
                        anyError = true;
                    }
                });

                if (anyError) {
                    console.log("ERRORS ABOUND"); // exit if any error found    
                }
                else {
                    console.log("ALL VALID");
                    $("#confirmSuit").on('show.bs.modal', function () {
                        var frmBody = $("#suitWrapper").html();
                        var modal = $(this);
                      
                        //Prefilling
                        //Textbox
                        $(frmBody).find("[type='text'], [type='number']").each(function () {
                            var ele = $(this).attr("name");
                            var value = $(".form-control[name='" + ele + "']").val();
                            modal.find('#prevDiv [data-name="' + ele + '"]').val(value);
                        });

                        //SelectBox
                        $(frmBody).find("select").each(function () {
                            var ele = $(this).attr("name");
                            var value = $("select[name='" + ele + "'] option:selected").text();
                            modal.find('#prevDiv [data-name="' + ele + '"]').val(value);
                        });

                        //Radio Buttons
                        var alongpl = $("input[name='ISAlongPipeLine']:checked").val();
                        var undht = $("input[name='IsUnderHighTension']:checked").val();
                        var onhw = $("input[name='IsOnHighWay']:checked").val();

                        $("#undt").val(undht);
                        $("#alongpl").val(alongpl);
                        $("#onhw").val(onhw);


                        //$(frmBody).find("[type='radio']").each(function () {
                        $("#suitWrapper input[type=radio]").each(function () {
                          

                            var ele = $(this).attr("name");
                            var value;
                            var pp = document.getElementsByName(ele);
                            for (var i = 0; i < pp.length; i++) {
                                if (pp[i].checked) {
                                    value = pp[i].value;
                                }
                            }
                            console.log(ele + " --- " + value);
                            modal.find('#prevDiv [data-name="' + ele + '"]').val(value);
                        });

                        modal.find('#btnConfirmSubmit').attr("data-form", "#facfrm");
                    });
                    $("#confirmSuit").modal('show');
                }
            }
        }
        else { 
            $("#facfrm").submit();
        }
    });

    $(document.body).on("click", "button#btnConfirmSubmit2", function (e) {
        e.preventDefault();

        var AlongPipeLine = $("input[name='ISAlongPipeLine']:checked").val();
        var UnderHighTension = $("input[name='IsUnderHighTension']:checked").val();
        var OnHighWay = $("input[name='IsOnHighWay']:checked").val();
        if (AlongPipeLine === undefined) {
            alert("Please, check if it is along pipeline.");
        }
        else if (UnderHighTension === undefined) {
            alert("Please, check if it is under high tension.");
        }
        else if (OnHighWay === undefined) {
            alert("Please, check if it is on high way.");
        }
      
     else {
            $("#confirmSuit").modal('hide');
            var frm = $("#facfrm");
            console.log(frm);
            $('[data-target="#confirmSuit"]').addClass("disabled");
            $.confirm({
                text: "Are you sure you want to submit this application ?",
                confirm: function () {
                    $("#facfrm").addClass('Submitloader');

                    $(frm).submit();

                },
                cancel: function () {
                    $('[data-target="#confirmSuit"]').removeClass("disabled");

                },
                confirmButton: "Yes",
                cancelButton: "No"
            })

        }
    });




   

});