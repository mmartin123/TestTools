function replaceAll(string, token, newtoken) {
    if (token != newtoken)
        while (string.indexOf(token) > -1) {
            string = string.replace(token, newtoken);
        }
    return string;
}

function DisplayNotification(message, type) {
    $('.section-notifications').contents().remove();
    var notif = '<span class="notification-type-' + type + '">' + message + '</span>';
    $(".section-notifications").html(notif).show('slide', { direction: 'up' }, 500, function () {

    });
}

function validate(evt) {
    var theEvent = evt || window.event;
    var key = theEvent.keyCode || theEvent.which;
    key = String.fromCharCode(key);
    var regex = /[0-9]|\./;
    if (!regex.test(key)) {
        theEvent.returnValue = false;
        if (theEvent.preventDefault) theEvent.preventDefault();
    }
}

function getRandomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function showProgressBar() {
    $('#currently-processing').css('visibility', 'visible');
}

function hideProgressBar() {
    $('#currently-processing').css('visibility', 'hidden');
}

function showNotification(text, color) {
    $('#global-notifications').html(text).css('color', color).css('visibility', 'visible');
}

function hideNotification() {
    $('#global-notifications').html('').css('visibility', 'hidden');
}


$(function () {

    function fetchHistory() {
        $.post(GLOBAL_AUTHENTICATION_URL, { howManyDaysHistory: $('#ddlHistory').val() }, function (rdata) {
            if (parseInt(rdata.ErrorCode) > 0) {
                //alert('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage);
                showNotification('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage, 'red');
            } else {
                hideNotification();
                var history = JSON.parse(rdata.Data);
                $('#myHistory').contents().remove();
                $.each(history, function () {
                    var lineItem = '<p>[Seed: <font color="black">' + this.Seed + '</font><font color="blue">]</font> [<a target="_blank" href="' + this.WSDL + '">' + this.WSDL + '</a>] <br/><font color="blue">[Confirmation: </font><font color="black">' + this.ConfirmationNo + '</font><font color="blue">] [Timestamp: </font><font color="black">' + this.TimestampString + '</font>]</p>';
                    $('#myHistory').append(lineItem);
                });
            }
            PostLoginProcess();
        });
    }

    fetchHistory();

    $('#ddlHistory').on('change', function () {
        fetchHistory();
    });

    //$("#login-screen").dialog({
    //    modal: true,
    //    buttons: {
    //        Login: function () {
    //            var loginSucess = false;
    //            //TODO: Perform OpenID authentication
    //            var userName = $('#openIdUsername').val();
    //            var passWord = $('#openIdPassword').val();
    //            if (userName.length < 3 || passWord.length < 3) {
    //                //alert('Please enter a valid username/password!');
    //                showNotification('Please enter a valid username/password!', 'red');
    //            } else {
    //                //....... loginSuccess = performAuth(username, password);
    //                //For now, send a fake 'login success' response
    //                hideNotification();
    //                $.post(GLOBAL_AUTHENTICATION_URL, { username: userName, password: passWord }, function (rdata) {
    //                    if (parseInt(rdata.ErrorCode) > 0) {
    //                        //alert('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage);
    //                        showNotification('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage, 'red');
    //                    } else {
    //                        hideNotification();
    //                        var history = JSON.parse(rdata.Data);
    //                        $.each(history, function () {
    //                            var lineItem = '<p>[Seed: ' + this.Seed + '] [<a target="_blank" href="' + this.WSDL + '">' + this.WSDL + '</a>] [Confirmation: ' + this.ConfirmationNo + ']</p>';
    //                            $('#myHistory').append(lineItem);
    //                        });
    //                    }
    //                });
    //                //
    //                loginSuccess = true;
    //                if (loginSuccess) {
    //                    loggedInUsername = userName;
    //                    $(this).dialog("close");
    //                    $('#tabs').fadeIn(500);
    //                    PostLoginProcess();
    //                } else {
    //                    loggedInUsername = '';
    //                    //alert('Login failed. Please try again!');
    //                    showNotification('Login failed. Please try again!', 'red');
    //                }
    //            }
    //        }
    //    }
    //});

    function PostLoginProcess() {
        $("#tabs").tabs();

        $('#btnStep1').click(function () {
            if ($('#txtUrl').val().length > 5) {
                hideNotification();
                $('#txtUrl').val($('#txtUrl').val().trim());
                $("#tabs").tabs("option", "active", 1); //switch over to 'seed' tab
                $('#txtSeed').attr('autofocus', 'autofocus');
            } else {
                //alert('Please enter a valid URL first!');
                showNotification('Please enter a valid URL first!', 'red');
            }
        });

        $('#btnRunAll').click(function () {
            //alert('Under construction. This will start looping over all the WSDLs and use a fixed seed (0) to generate tests and dump their results in the database');
            showProgressBar();
            $.post(GLOBAL_RUNALL_URL, {}, function (rdata) {
                if (parseInt(rdata.ErrorCode) > 0) {
                    //alert('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage);
                    showNotification('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage, 'red');
                    hideProgressBar();
                } else {
                    //alert(rdata.Data);
                    showNotification(rdata.Data, 'green');
                    setTimeout(function () {
                        window.location.reload();
                    }, 3000);
                }
            });
        });

        $('#txtUrl').on('change', function () {
            $('#btnPreview').click(function () {
                window.location = $('#txtUrl').val();
            });
        });

        $('#btnStep2').click(function () {
            showProgressBar();
            $(this).attr('disabled', 'disabled');
            if ($('#txtSeed').val().length < 1) {
                $('#txtSeed').val(getRandomInt(1, 100));
            }
            $.post(GLOBAL_STARTROUTINE_URL, { url: $('#txtUrl').val(), seed: $('#txtSeed').val() }, function (rdata) {
                if (parseInt(rdata.ErrorCode) > 0) {
                    //alert('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage);
                    showNotification('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage, 'red');
                } else {
                    hideNotification();
                    $("#tabs").tabs("option", "active", 2); //switch over to 'generate test'
                    summaryObject = JSON.parse(rdata.Data);
                    //update the summary tab with the values generated
                    $('#summary').find('span').each(function () {
                        var spanId = $(this).attr('id');
                        switch (spanId) {
                            case 'summary1': { $(this).html(summaryObject.randomSite.SiteName); break; }
                            case 'summary2': { $(this).html(summaryObject.randomSite.SiteCode); break; }
                            case 'summary3': { $(this).html(summaryObject.randomSite.Network); break; }
                            case 'summary4': { $(this).html(summaryObject.randomVariable.VariableId); break; }
                            case 'summary5': { $(this).html(summaryObject.randomVariable.VariableCode); break; }
                            case 'summary6': { $(this).html(summaryObject.randomSiteInfo.BeginDateTime); break; }
                            case 'summary7': { $(this).html(summaryObject.randomSiteInfo.EndDateTime); break; }
                            case 'summary8': { $(this).html(summaryObject.randomSiteInfo.BeginDateTimeUTC); break; }
                            case 'summary9': { $(this).html(summaryObject.randomSiteInfo.EndDateTimeUTC); break; }
                            case 'summary10': { $(this).html(summaryObject.wsdl); break; }
                            case 'summary11': { $(this).html(summaryObject.seed); break; }
                        }
                    });
                }
                hideProgressBar();
            });
        });

        $('#btnStep3').click(function () {
            $(this).attr('disabled', 'disabled');
            $("#tabs").tabs("option", "active", 3); //switch over to 'run test'
        });

        $('#btnStep4').click(function () {
            //finally run the test.
            $(this).attr('disabled', 'disabled');
            showProgressBar();
            var dataToSend = JSON.stringify(summaryObject);
            $.post(GLOBAL_RUNTEST_URL, { summaryString: dataToSend }, function (rdata) {
                if (parseInt(rdata.ErrorCode) > 0) {
                    //alert('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage);
                    showNotification('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage, 'red');
                } else {
                    $('#confirmation>span').html(rdata.Data);
                    $('#confirmation').fadeIn(500);
                }
                hideProgressBar();
            });
        });

        var lookupType = 0;
        $('#lookup1').click(function () {
            $('#divLookup2').fadeOut(500);
            $('#divLookup1').fadeIn(500);
            $('#btnCompare').fadeIn(500);
            lookupType = 1;
        });
        $('#lookup2').click(function () {
            $('#divLookup1').fadeOut(500);
            $('#divLookup2').fadeIn(500);
            $('#btnCompare').fadeIn(500);
            lookupType = 2;
        });

        $('#btnStep5').click(function () {
            switch (lookupType) {
                default: { break; }
                case 1: {
                    var wsdl = $('#lookup1textbox1').val();
                    var seed = $('#lookup1textbox2').val();
                    if (wsdl.length > 5 && seed.length > 0) {
                        showProgressBar();
                        $.post(GLOBAL_COMPARETEST_URL, { lookupType: 1, value1: wsdl, value2: seed }, function (rdata) {
                            if (parseInt(rdata.ErrorCode) > 0) {
                                //alert('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage);
                                showNotification(alert('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage), 'red');
                            } else {
                                hideNotification();
                            }
                            hideProgressBar();
                        });
                    } else {
                        alert('Must enter WSDL URL and Seed Value to compare!');
                    }
                    break;
                }
                case 2: {
                    var confirmation = $('#lookup2textbox1').val();
                    if (confirmation.length > 5) {
                        showProgressBar();
                        $.post(GLOBAL_COMPARETEST_URL, { lookupType: 2, value1: confirmation, value2: '' }, function (rdata) {
                            if (parseInt(rdata.ErrorCode) > 0) {
                                //alert('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage);
                                showNotification('ERROR #' + rdata.ErrorCode + ': ' + rdata.ErrorMessage, 'red');
                            } else {
                                //alert("SUCCESS, test passed!");
                                showNotification('Success: test passed!', 'green');
                            }
                            hideProgressBar();
                        });
                    } else {
                        //alert('Must enter a valid confirmation number to compare!');
                        showNotification('Must enter a valid confirmation number to compare!', 'red');
                    }
                    break;
                }
            }
        });
    }   //PostLoginProcess ends
});

