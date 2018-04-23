
$('div.table_answe').on('mouseenter', '.check_td', function () {
    $(this).css('background-color', '#808080');
});
$('div.table_answe').on('mouseleave', '.check_td', function () {
    $(this).css('background-color', '#fff');
});
$('div.table_answe').on('click', '.check_td', function () {
    $(this).find('input').prop('checked', true);
});
$('div.table_answe').on('dblclick', '.item_answer', function () {
    $('div.table_answe').off('dblclick', '.item_answer');
    var td_text = $(this).text();
    $(this).find('span.edittext').empty().append('<input class="text_answer" type="text" value="' + td_text + '"/>');
    $('input.text_answer').focus().select();
    //$('input.text_answer').select();

});
$('div.table_answe').on('keyup', 'input.text_answer', function (event) {
    var parent, text_answer;
    if (event.keyCode == 13) {
        parent = $(this).parent();
        text_answer = $(this).val();
        parent.empty().append(text_answer);
        $('div.table_answe').on('dblclick', '.item_answer', function () {
            $('div.table_answe').off('dblclick', '.item_answer');
            var td_text = $(this).text();
            $(this).find('span.edittext').empty().append('<input class="text_answer" type="text" value="' + td_text + '"/>');
            $('input.text_answer').focus().select();
        });
    }
});
$('div.table_answe').on('blur', 'input.text_answer', function () {
    var parent, text_answer;

    parent = $(this).parent();
    text_answer = $(this).val();
    parent.empty().append(text_answer);
    $('div.table_answe').on('dblclick', '.item_answer', function () {
        $('div.table_answe').off('dblclick', '.item_answer');
        var td_text = $(this).text();
        $(this).find('span.edittext').empty().append('<input class="text_answer" type="text" value="' + td_text + '"/>');
        $('input.text_answer').focus().select();
    });

});
$('div.table_answe').on('dblclick', '.item_question', function () {
    $('div.table_answe').off('dblclick', '.item_question');
    var td_text = $(this).text();
    $(this).find('span.row_text').empty().append('<input class="text_question" type="text" value="' + td_text + '"/>');
    $('input.text_question').focus().select();
    //$('input.text_answer').select();

});
$('div.table_answe').on('keyup', 'input.text_question', function (event) {
    var parent, text_answer;
    if (event.keyCode == 13) {
        parent = $(this).parent();
        text_answer = $(this).val();
        parent.empty().append(text_answer);
        $('div.table_answe').on('dblclick', '.item_question', function () {
            $('div.table_answe').off('dblclick', '.item_question');
            var td_text = $(this).text();
            $(this).find('span.row_text').empty().append('<input class="text_question" type="text" value="' + td_text + '"/>');
            $('input.text_question').focus().select();
        });
    }
});
$('div.table_answe').on('blur', 'input.text_question', function () {
    var parent, text_answer;
    parent = $(this).parent();
    text_answer = $(this).val();
    parent.empty().append(text_answer);
    $('div.table_answe').on('dblclick', '.item_question', function () {
        $('div.table_answe').off('dblclick', '.item_question');
        var td_text = $(this).text();
        $(this).find('span.row_text').empty().append('<input class="text_question" type="text" value="' + td_text + '"/>');
        $('input.text_question').focus().select();
    });
});

$('div.table_answe').on('click', 'button.add_row', function () {
    var iter, code, colum;
    iter = $('tr.table_answer:last').attr('id');
    colum = $('tr.table_answer:last').find('td.table_answer').length;
    code = '<tr id=' + (Number(iter) + 1) + ' class="table_answer">';
    for (var i = 0; i < colum; i++) {
        if (i == 0)
            code += '<td id=' + i + ' class="item_question table_answer"><span class="row_text">Вопрос</span></td>';
        else
            code += '<td id=' + i + ' class="check_td table_answer"><input type="radio" name="group' + (Number(iter) + 1) + '" /></td>';
    }
    code += '</tr>';
    $('tr.table_answer:last').after(code);
});
$('div.table_answe').on('click', 'button.add_colum', function () {
    var iter, code, row;
    row = Number($('tr.table_answer:last').attr('id'));
    iter = Number($('tr.table_answer:last').find('td.table_answer:last').attr('id')) + 1;
    for (var i = 0; i <= row; i++) {
        if (i == 0)
            $('tr.table_answer[id=0] td.table_answer:last').after('<td id=' + iter + ' class="item_answer table_answer"><input type="hidden" class="index" id="' + (Number($('tr.table_answer[id=0] td.table_answer:last').attr('id')) + 1) + '"><span class="edittext">Ответ</span></td>');
        else
            $('tr.table_answer[id=' + i + '] td.table_answer:last').after('<td id=' + iter + ' class="check_td table_answer"><input type="radio" name="group' + i + '" /></td>');
    }
});

$('div.table_answe').on('mouseup', 'td.table_answer', function (event) {
    if (event.button == 2) {
        var elem_td = $(this);
        document.oncontextmenu = function () { return false; }
        $('.contextmenu').remove();
        $('body').append('<div class="contextmenu"><div>');
        $('.contextmenu').append('<p class="text_context" id=1>Удалить столбец</p>');
        $('.contextmenu').append('<p class="text_context" id=2>Удалить строку</p>');
        $('.contextmenu').focus();
        $('.contextmenu').css({
            'border': '1px solid black',
            'border-radius': '2px',
            'position': 'fixed',
            'display': 'block',
            'z-index': '1',
            'left': event.clientX,
            'top': event.clientY,
            'width': 'auto',
            'height': 'auto',
            'background': 'rgba(200, 200, 200 , 1)',
            'box-shadow': '0 8px 10px rgba(0, 0, 0, 0.5)',
            'padding': '5px'
        });
        $('.text_context').mouseenter(function () {
            $(this).css({
                'cursor': 'pointer',
                'background-color': '#eeeeee',
                'border': '1px solid red',
                'border-radius': '2px'
            })
        });
        $('.text_context').mouseleave(function () {
            $(this).removeAttr('style');
        });

        $('.text_context').click(function () {
            switch ($(this).attr('id')) {
                case '1':
                    // alert($('tr[id=0] td[id=' + elem_td.attr('id') + ']').find('span.edittext').attr('id'));
                    if ($('tr[id=0] td[id=' + elem_td.attr('id') + ']').find('span.edittext').attr('id') > 0) {
                        $.post("/Question/deleteAnswer", { Id: $('tr[id=0] td[id=' + elem_td.attr('id') + ']').find('span.edittext').attr('id') });
                    }
                    $('table.table_answer td[id=' + elem_td.attr('id') + ']').remove();
                    var answer_colum = $('table.table_answer tr[id=0] td.item_answer');
                    var new_index = 1;
                    $.each(answer_colum, function () {
                        $(this).find('input.index').attr('id', new_index);
                        ++new_index;
                    });
                    $('.contextmenu').blur();
                    break;
                case '2':
                    if (elem_td.parent('tr').attr('name') != null) {
                        $.post("/Question/DeleteTableRow", { id_table_row: Number(elem_td.parent('tr').attr('name')) })
                    }
                    elem_td.parent('tr').remove();
                    $('.contextmenu').blur();
                    break;
                default:
                    break;
            }

        });
        $('body').on('blur', '.contextmenu', function () {
            document.oncontextmenu = function () { return true; }
            $(this).remove();
        });
        $('body').click(function () {
            $('.contextmenu').blur();
        });
    }
});


