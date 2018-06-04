$(function () {
    // Ссылка на автоматически-сгенерированный прокси хаба
    var session = $.connection.sessionHub;
    var connectionId;

    // Функция, вызываемая при подключении нового пользователя
    session.client.onConnected = function (id, userName, time, allUsers) {
        $('#session').html('<p name="StetLine" id="' + id + '"> Stet line : ' + id + ':' + userName + ' - ' + time + '<p>');

        for (i = 0; i < allUsers.length; i++) {
            AddUser(allUsers[i].ConnectionId, allUsers[i].UserName, allUsers[i].StartTime);
        }
    }

    // Добавляем нового пользователя
    session.client.onNewUserConnected = function (id, name, time) {

        AddUser(id, name, time);
    }

    // Удаляем пользователя
    session.client.onUserDisconnected = function (id, userName) {

        $('#' + id).remove();
    }

    // Открываем соединение
    $.connection.hub.start().done(function () {
        var name = $('div.operatorID').text();
        var id = $('div.operatorID').attr('id');
        session.server.connect(id,name);
        //$('#sendmessage').click(function () {
        //    // Вызываем у хаба метод Send
        //    chat.server.send($('#username').val(), $('#message').val());
        //    $('#message').val('');
        //});
    });


    idleTimer = null;
    idleState = false; // состояние отсутствия
    idleWait = 5000; // время ожидания в мс. (1/1000 секунды)
    
    var safkll = null;

    $(document).ready(function () {
        $(document).bind('mousemove keydown scroll', function () {
            clearTimeout(idleTimer); // отменяем прежний временной отрезок
            if (idleState == true) {
                // Действия на возвращение пользователя
                //alert("С возвращением!");
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


});
//// Кодирование тегов
//function htmlEncode(value) {
//    var encodedValue = $('<div />').text(value).html();
//    return encodedValue;
//}





//Добавление нового пользователя
function AddUser(id, name, time) {
    $("#session").append('<p id="' + id + '">' + id + ' : <b>' + name + '</b> - ' + time + '</p>');
}