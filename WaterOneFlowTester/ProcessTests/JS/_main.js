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

var totalTestCasesCtr = 0;
var processedTestsCasesCtr = 0;

$(function () {
    $("#accordion").accordion();

    function PerformTest(controlPlayObject) {
        var whichInstance = $(controlPlayObject);

        //var testInstance = $(this);
        var testInstance = whichInstance;
        $(testInstance).parent().find('.control-passed').hide();
        $(testInstance).parent().find('.control-failed').hide();

        //var testId = $(this).attr('data-testId');
        var testId = $(whichInstance).attr('data-testId');
        //var constructedUrl = $(this).attr('data-constructedUrl');
        var constructedUrl = $(whichInstance).attr('data-constructedUrl');
        $('#title-' + testId).find('img').remove();

        $('#currently-processing,#wait').css('visibility', 'visible');
        $('#currently-processing').find('a').attr('href', constructedUrl);
        

        $.post(GLOBAL_RUNTEST_URL, { testId: testId, constructedUrl: constructedUrl }, function (data) {
            $('#test-progress').children().fadeOut(500);
            $('#test-progress').html('');

            var cleanData = replaceAll(data.ErrorMessage, '\n', '<br /><br />');

            var title = '<center><font color="blue"><b>' + testId + '</b></font></center><br/><br />';
            //Read ErrorCode
            if (parseInt(data.ErrorCode) === 0) {
                $(testInstance).parent().find('.control-passed').show();
                $('#test-progress').html(title + '<font color="green"><b>Test Passed!</b></font>');

                $('#title-' + testId).append('<img class="float-right" width="20px" src="../IMG/accept.png" alt="Passed" />');

            } else {
                $(testInstance).parent().find('.control-failed').show();
                $('#test-progress').html(title + '<font color="red"><b>' + cleanData + '</b></font>');

                $('#title-' + testId).append('<img class="float-right" width="20px" src="../IMG/cross.png" alt="Failed" />');
            }
            $('#wait').css('visibility', 'hidden');
            processedTestsCasesCtr++;
        });
    }

    $('.control-play').click(function () {
        totalTestCasesCtr = 0;
        processedTestsCasesCtr = 0;
        PerformTest($(this));
    });


    $('#test-all').click(function () {
        processedTestsCasesCtr = 0;
        totalTestCasesCtr = $('.control-play').length;
        $('.control-play').each(function () {
            PerformTest($(this));
        });
        setInterval(function () {
            if (totalTestCasesCtr === processedTestsCasesCtr) {
                $('#wait').css('visibility', 'hidden');
                return false;
            } else {
                $('#wait').css('visibility', 'visible');
            }
        }, 3000);
    });
});

 

