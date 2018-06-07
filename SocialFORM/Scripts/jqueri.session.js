$(function () {
    // Ссылка на автоматически-сгенерированный прокси хаба
    var session = $.connection.sessionHub;
    var connectionId;
    // Функция, вызываемая при подключении нового пользователя
    session.client.onConnected = function (id, userName, time) {
        $('#session').html('<font color="#808080" size="1"><p align="right" name="StetLine" style="white-space: nowrap" id="' + id + '"> Status bar ConnectionId: ' + id + ' - Name: ' + userName + ' - TimeUp: ' + time + '<p></font>');
    }

    // Добавляем нового пользователя
    session.client.onNewUserConnected = function (allUsers_l) {
        OnMonitor(allUsers_l);
    }

    // Удаляем пользователя
    session.client.onUserDisconnected = function (id, userName) {
        $('#' + id).remove();
    }

    // Открываем соединение
    $.connection.hub.start().done(function () {
        var name = $('div.operatorID').text();
        var id = $('div.operatorID').attr('id');
        session.server.connect(id, name);
    });

    idleTimer = null;
    idleState = false; // состояние отсутствия
    idleWait = 1800000; // время ожидания в мс. (1/1000 секунды)

    var safkll = null;

    $(document).ready(function () {
        $(document).bind('mousemove keydown scroll', function () {
            clearTimeout(idleTimer); // отменяем прежний временной отрезок
            if (idleState == true) {
                // Действия на возвращение пользователя
                session.server.endafk(safkll);
                $('img[id=status]').attr('src', '../images/icon/status.png');
            }
            idleState = false;
            idleTimer = setTimeout(function () {
                // Действия на отсутствие пользователя
                session.server.startafk();
                Data = new Date();
                Hour = Data.getHours();
                Minutes = Data.getMinutes();
                Seconds = Data.getSeconds();
                safkll = Hour + ":" + Minutes + ":" + Seconds;
                $('img[id=status]').attr('src', '../images/icon/status_busy.png');
                idleState = true;
            }, idleWait);
        });
        //$("body").trigger("mousemove"); // сгенерируем ложное событие, для запуска скрипта
    });

    //
    //Мониторинг
    session.client.onMonitoring = function (allUsers_l) {
        OnMonitor(allUsers_l);
    }
    session.client.fsAFK = function (allUsers_l) {
        OnMonitor(allUsers_l);
    }
    session.client.feAFK = function (allUsers_l) {
        OnMonitor(allUsers_l);
    }
    $('li[id="Monitoring"]').click(function () {
        session.server.monitoring();

    });
});
//// Кодирование тегов
//function htmlEncode(value) {
//    var encodedValue = $('<div />').text(value).html();
//    return encodedValue;
//}


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