$(function () {

    $('#chatBody').hide();
    $('.blink').hide();

    // Ссылка на автоматически-сгенерированный прокси хаба
    var apphub = $.connection.appHub;
    var connectionId;

    $.connection.hub.logging = true;

    // Открываем соединение
    $.connection.hub.start().done(function () {
        console.log("Signal R connected");
        var name = $('div.operatorID').text();
        var id = $('div.operatorID').attr('id');
        apphub.server.connect(id, name);

        $('#sendmessage').click(function () {
            // Вызываем у хаба метод Send
            apphub.server.send($('#username').val(), $('#message').val());
            $('#message').val('');
        });

        // обработка логина
        $(".btnLogin").click(function () {

            var name = $('div.operatorID').text();
            $('.blink').hide();
            if (name.length > 0) {
                apphub.server.connect(name);
            }
            else {
                alert("Введите имя");
            }
        });

    }).fail(function () {
        console.log("Could not Connect to signal R hub!");
    });

    // Функция, вызываемая при подключении нового пользователя
    apphub.client.onConnected = function (id, userName, time, timeID) {
        $('#session').html('<font color="#808080" size="1"><p class="StatusBar" align="right" name="StetLine" style="white-space: nowrap" id="' + id + '"> Status bar ConnectionId: ' + id + ' - Name: ' + userName + ' - TimeUp: ' + time + '</p></font>');

        $('#chatBody').show();
        // установка в скрытых полях имени и id текущего пользователя
        $('#hdId').val(id);
        $('#username').val(userName);
    }

    // Добавляем нового пользователя
    apphub.client.onNewUserConnected = function (allUsers_l) {
        OnMonitor(allUsers_l);
    }

	// Connection Events
    apphub.connection.error(function (error) {                
        if (error) console.log("An error occurred: " + error.message);
    });

    //
    //Reconnect
    apphub.connection.disconnected(function (error) {                
		console.log("Connection lost. " + error);
        // IMPORTANT: continuously try re-starting connection
        setTimeout(function () {                    
            $.connection.hub.start().done(function () {
				var name = $('div.operatorID').text();
				var id = $('div.operatorID').attr('id');
                apphub.server.connect(id, name);
			});                    
        }, 2000);
    });

    idleTimer = null;
    idleState = false; // состояние отсутствия
    idleWait = $('#timea').text();//1800000; // время ожидания в мс. (1/1000 секунды)
    var safkll = null;

    //
    //Бездействие
    $(document).ready(function () {
        $(document).bind('mousemove keydown scroll', function () {
            clearTimeout(idleTimer); // отменяем прежний временной отрезок
            if (idleState == true) {
                // Действия на возвращение пользователя
                apphub.server.endafk(safkll);
                $('img[id=status]').attr('src', '../images/icon/status.png');
            }
            idleState = false;
            idleTimer = setTimeout(function () {
                // Действия на отсутствие пользователя
                apphub.server.startafk();
                Data = new Date();
                Hour = Data.getHours();
                Minutes = Data.getMinutes();
                Seconds = Data.getSeconds();
                safkll = Hour + ":" + Minutes + ":" + Seconds;
                $('img[id=status]').attr('src', '../images/icon/status_busy.png');
                idleState = true;
                //alert($('.StatusBar').attr('id'));
            }, idleWait);
        });
        //$("body").trigger("mousemove"); // сгенерируем ложное событие, для запуска скрипта
    });

    //
    //Мониторинг
    apphub.client.onMonitoring = function (allUsers_l) {
        OnMonitor(allUsers_l);
    }
    apphub.client.fsAFK = function (allUsers_l) {
        OnMonitor(allUsers_l);
    }
    apphub.client.feAFK = function (allUsers_l) {
        OnMonitor(allUsers_l);
    }
    $('li[id="Monitoring"]').click(function () {
        apphub.server.monitoring();
    });

    apphub.client.addMessage = function (name, message) {
        // Добавление сообщений на веб-страницу 
        $('#chatroom').append('<p><b>' + htmlEncode(name)
            + '</b>: ' + htmlEncode(message) + '</p>');
        $('.blink[name="Operator"]').show();
        $(function () {
            var flag = false;
            $('.blink[name="Operator"]').css("color", "black");
            setTimeout(function () {
                $('.blink[name="Operator"]').css("color", "black");
                setInterval(function () {
                    $('.blink[name="Operator"]').css("color", flag ? "black" : "red");
                    flag = !flag;
                }, 500)
            }, 1000);
        });
    };

});

// Кодирование тегов
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

//Обработка мониторинга
function OnMonitor(allUsers_l) {
    setTimeout(function () {
        $("#list_monitorig").empty();
        var codeTable = '';
        codeTable += '<table><tr>';
        wid_w = $(window).width();
        var countROW = 4; // Отсчет ведеться от 0 т.е. [0..2]
        if (wid_w < 992) countROW = 2;
        if (wid_w < 452) countROW = 1;

        var countROW_tmp = countROW;
        for (i = 0; i < allUsers_l.length; i++) {
            if (i == countROW_tmp) { codeTable += '</tr><tr>'; countROW_tmp = countROW + i + 1; }
            if (allUsers_l[i].IsAction == true && allUsers_l[i].EndTime == null) {
                codeTable += '<td style="padding:4px"><p style="text-align: center;" id="'
                    + allUsers_l[i].ConnectionId
                    + '"> <img id = "status_monitoring" class= "online" src = "../images/icon/status.png" /> <b>'
                    + allUsers_l[i].UserName
                    + '</b></p></td>';
            }
            if (allUsers_l[i].IsAction == false && allUsers_l[i].EndTime == null) {
                codeTable += '<td style="padding:4px"><p style="text-align: center;" id="'
                    + allUsers_l[i].ConnectionId
                    + '"> <img id = "status_monitoring" class= "online" src = "../images/icon/status_offline.png" /> <b>'
                    + allUsers_l[i].UserName
                    + '</b></p></td>';
            }
        }
        codeTable += '</tr></table>';
        $("#list_monitorig").append(codeTable);
    }, 2000);
}