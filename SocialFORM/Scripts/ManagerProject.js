let list_base = {};

$.get("/Question/getAnswerBase")
    .success(function (base_data) {
        $.each(base_data, function (i, b_item) {
            list_base[b_item.Id] = b_item;
        })
        console.log("First Start --- ", list_base);
    })

//Функция вывода функционала для редактора вопросов \
function SetQuestionBlock(block, item) {
    if (item != null & item != undefined) {
        var id_question = item.QuestionID;
        block.append("<div class='QuestionBlock NewBlock MainItem' id='" + item.IndexQuestion + "'><div name='QuestionInfo' style='display: none'>" +
            "<input name='groupInfo' type='hidden' value='" + item.Id + "' />" +
            "<input name='questionInfo' type='hidden' value='" + item.QuestionID + "'/>" +
            "</div>" +
            "<div class='QuestionText HideOff' id='" + item.QuestionID + "'><textarea id='example' style='width: 80 %; height: 300px; display: none;'></textarea></div>" +
            "<div class='SettingsPanel'><div class='NameType'>Type</div><div class='WallOfTypes' style='position: relative; z - index: 5; height: 30px; width: 30px'>></div></div>" +
            "<div class='AnswerBlock HideOff' style='overflow-y: auto;'><div class='AnswerSettings'>" +
            "</div><div class='PanelSettings'></div></div>" +
            "<div class='SaveChange'><div class='SaveChangeButton'>Сохранить</div></div></div>"
        );
    } 
    Init();
}

//Функция присвоения типа вопроса
function SetQuestionNew(item, parent, q_data) {
    $.get("/Question/getQuestion", { id: parent })
        .success(function (q_data) {
            var question_block = $('.QuestionText[id=' + parent + ']').parents(".QuestionBlock");
            sceditor.instance(item).val(q_data.TextQuestion);
            question_block.find("div[name=QuestionInfo]").append('<input name="questionType" type="hidden" value="' + q_data.TypeQuestion + '">');
            switch (q_data.TypeQuestion) {
                case 1:
                    question_block.find('.NameType').text('Single');
                    break;
                case 2:
                    question_block.find('.NameType').text('Multi');
                    break;
                case 3:
                    question_block.find('.NameType').text('Free');
                    break;
                case 4:
                    question_block.find('.NameType').text('Table');
                    break;
                case 5:
                    question_block.find('.NameType').text('Text');
                    break;
                case 6:
                    question_block.find('.NameType').text('Filter');
                    break;
                default:
                    question_block.find('.NameType').text('Other');
                    break;
            }
            SetAnswerNew(parent, $('.NewBlock'), q_data);
        })
}

//Функция вывода ответов относящиеся к вопросу
function SetAnswerNew(id_question, block, q_data) {
    //var type_question = $('.BlockQuestionEditor').find('div.QuestionText[id=' + id_question + ']').parent().find('div[name=QuestionInfo]').find('input[name=questionType]').val();
    var type_question = block.find('div[name=QuestionInfo]').find('input[name=questionType]').val();
    switch (Number(type_question)) {
        case 1: {
            $.get("/Question/getAnswerAll", { question_id: id_question })
                .success(function (answer_all) {
                    $.get("/Question/getAnswer", { id_question: id_question })
                        .success(function (a_data) {
                            var list_answer = {};
                            $.each(a_data, function (i, a_item) {
                                list_answer[a_item.Id] = a_item;
                            })
                            var tmp = block.find('div.QuestionText[id=' + id_question + ']').parent().find('div.AnswerBlock');
                            $.each(answer_all, function (i, item) {
                                if (item.AnswerType == 1)
                                    tmp.append('<div class="AnswerItem" id=' + item.AnswerKey + '><div style="margin-right: 15px"><input type="radio" disabled/></div><div class="IndexAnswer">' + list_answer[item.AnswerKey].Index + ') </div><div style="width: 70%;" class="TextAnswer">' + list_answer[item.AnswerKey].AnswerText +
                                        '</div>' +
                                        (item.BindGroup == null ? '<div class="SetBindGroup StyleButton" style="width: 30px; background-color: #81c784; font-size: 20px;">&infin;</div>' : '<div class= "BindGroupPanel" style = "width: 100px; background-color: #81c784;" > Группа ' + item.BindGroup + '<div class= "RemoveBindGroup StyleButton" style = "background-color: #f44336; width: 30px;" >&#10006;</div></div>') +
                                        '<div style="width: 5px;"></div>' +
                                        '<div class="FreeArea" style="display: inline-flex; width: 20%; height: 100%;"><input type="checkbox" name="FreeAreaCheckbox" ' + (list_answer[item.AnswerKey].isFreeArea ? "checked" : "") + '/>Свободное поле</div>' +
                                        '<div class="DeleteAnswerItem">&#10006;</div><div style="width: 5px;"></div><div class="EditAnswer">&#9000;</div>' +
                                        '<div style="width: 5px;"></div>' +
                                        '<div class="ChangeIndex">ID</div>' +
                                        '<div style="width: 5px;"></div>' +
                                        '<input type="hidden" name="IndexAnswer" value=' + list_answer[item.AnswerKey].Index + '></div>');
                                else {
                                    tmp.append('<div class="AnswerItem BaseAnswer Existed" id=' + item.Id + '><div style="margin-right: 15px">' +
                                        '<input type="radio" disabled/></div><div class="IndexAnswer">' + -1 * Number(list_base[item.AnswerKey].BaseIndex) + ') </div>' +
                                        '<div style="width: 100%;" class="TextAnswer">' + list_base[item.AnswerKey].AnswerText + '</div>' +
                                        '<div class="DeleteAnswerItem">&#10006;</div></div>');
                                }
                            })
                            LoadSettingsSingle(tmp.find('.AnswerSettings'), q_data);
                        })
                })
        }
            break;
        case 2: {
            $.get("/Question/getAnswerAll", { question_id: id_question })
                .success(function (answer_all) {
                    $.get("/Question/getAnswer", { id_question: id_question })
                        .success(function (a_data) {
                            var list_answer = {};
                            $.each(a_data, function (i, a_item) {
                                list_answer[a_item.Id] = a_item;
                            })
                            var tmp = block.find('div.QuestionText[id=' + id_question + ']').parent().find('div.AnswerBlock');
                            $.each(answer_all, function (i, item) {
                                if (item.AnswerType == 1)
                                    tmp.append('<div class="AnswerItem" id=' + item.AnswerKey + '><div style="margin-right: 15px"><input type="checkbox" disabled/></div><div class="IndexAnswer">' + list_answer[item.AnswerKey].Index + ') </div><div style="width: 70%;" class="TextAnswer">' + list_answer[item.AnswerKey].AnswerText +
                                        '</div>' +
                                        (item.BindGroup == null ? '<div class="SetBindGroup StyleButton" style="width: 30px; background-color: #81c784;">&infin;</div>' : '<div class="BindGroupPanel" style="width: 100px; background-color: #81c784;">Группа ' + item.BindGroup + '<div class="RemoveBindGroup StyleButton" style="background-color: #f44336; width: 30px;">&#10006;</div></div>') +
                                        '<div style="width: 5px;"></div>' +
                                        '<div class="FreeArea" style="display: inline-flex; width: 20%; height: 100%;"><input type="checkbox" name="FreeAreaCheckbox" ' + (list_answer[item.AnswerKey].isFreeArea ? "checked" : "") + '/>Свободное поле</div>' +
                                        '<div class="DeleteAnswerItem">&#10006;</div><div style="width: 5px;"></div><div class="EditAnswer">&#9000;</div>' +
                                        '<div style="width: 5px;"></div>' +
                                        '<div class="ChangeIndex">ID</div>' +
                                        '<div style="width: 5px;"></div>' +
                                        '<input type="hidden" name="IndexAnswer" value=' + list_answer[item.AnswerKey].Index + '></div>');
                                else
                                    tmp.append('<div class="AnswerItem BaseAnswer" id=' + item.Id + '><div style="margin-right: 15px"><input type="checkbox" disabled/></div><div class="IndexAnswer">' + -1 * Number(list_base[item.AnswerKey].BaseIndex) + ') </div><div style="width: 100%;" class="TextAnswer">' + list_base[item.AnswerKey].AnswerText + '</div><div class="DeleteAnswerItem">&#10006;</div></div>');
                            })
                            LoadSettingsMilti(tmp.find('.AnswerSettings'), q_data);
                        })
                })
        }
            break;
        case 3: {
            $.get("/Question/getAnswerAll", { question_id: id_question })
                .success(function (answer_all) {
                    $.get("/Question/getAnswer", { id_question: id_question })
                        .success(function (a_data) {
                            var list_answer = {};
                            $.each(a_data, function (i, a_item) {
                                list_answer[a_item.Id] = a_item;
                            })
                            var tmp = block.find('div.QuestionText[id=' + id_question + ']').parent().find('div.AnswerBlock');
                            $.each(answer_all, function (i, item) {
                                if (item.AnswerType == 1)
                                    tmp.append('<div class="AnswerItem" id=' + item.AnswerKey + '><div style="margin-right: 15px"><input type="text" placeholder="Текстовый блок" disabled/></div><div style="width: 100%;" class="TextAnswer">' + list_answer[item.AnswerKey].AnswerText +
                                        '</div><div class="DeleteAnswerItem">&#10006;</div><div style="width: 5px;"></div><div class="EditAnswer">&#9000;</div>' +
                                        '<div style="width: 5px;"></div>' +
                                        '<div class="ChangeIndex">ID</div>' +
                                        '<div style="width: 5px;"></div>' +
                                        '<input type="hidden" name="IndexAnswer" value=' + list_answer[item.AnswerKey].Index + '></div>');
                                else
                                    tmp.append('<div class="AnswerItem BaseAnswer" id=' + item.Id + '><div style="margin-right: 15px"><input type="checkbox" disabled/></div><div style="width: 100%;">' + list_base[item.AnswerKey].AnswerText + '</div><div class="DeleteAnswerItem">&#10006;</div></div>');
                            })
                            LoadSettingsFree(tmp.find('.AnswerSettings'), q_data);
                        })
                })
        }
            break;
        case 4: {
            $.get("/Question/getAnswerAll", { question_id: id_question })
                .success(function (answer_all) {
                    $.get("/Question/getAnswer", { id_question: id_question })
                        .success(function (a_data) {
                            var list_answer = {};
                            $.each(a_data, function (i, a_item) {
                                list_answer[a_item.Id] = a_item;
                            })
                            var tmp = block.find('div.QuestionText[id=' + id_question + ']').parent().find('div.AnswerBlock');
                            $.get("/Question/getTableRow", { id_q: id_question, id_a: 0 })
                                .success(function (row_data) {
                                    if (row_data.length > 0) {
                                        var row_length = row_data.length;
                                        var column_length = a_data.length;
                                        var code = "<div style='position: relative'><div style='overflow-x: scroll; overflow-y: visible; margin-left: 200px;'><table class='TableStyle' style='width: 100%;'>";
                                        for (var row_i = 0; row_i <= row_length; row_i++) {
                                            if (row_i == 0) {
                                                code += "<tr class='AnswerTableLine'>";
                                            } else {
                                                code += "<tr>";
                                            }
                                            for (var col_i = 0; col_i <= column_length; col_i++) {
                                                if (row_i == 0 && col_i == 0) code += "<td></td>";
                                                if (row_i == 0 && col_i > 0) {
                                                    code += "<td style='min-width: 200px; max-width: 200px; vertical-align: top;' class='TableColumnText Column" + col_i + "' id=" + col_i + ">" +
                                                        "<div style='display: inline-flex; flex-direction: row; width: 100%;'>" +
                                                        "<div style='display: inline-flex; width: 100%;' class='TextAnswerDiv' id='" + a_data[(col_i - 1)].Id + "'>" + a_data[(col_i - 1)].AnswerText + "</div>" +
                                                        "<div class='PanelColumn' style='display: flex; flex-direction: column; vartical-align: top;'>" +
                                                        "<div style='width: 20px; height: 20px; background-color: #f44336;' class='DeleteColumn StyleButton'>&#10006;</div>" +
                                                        "<div style='width: 20px; height: 20px; background-color: #f44336;' class='StyleButton EditTextColumn'>&#9000;</div>" +
                                                        "</div></div></td>";
                                                }
                                                if (row_i > 0 & col_i == 0) {
                                                    code += "<td style='min-width: 200px; max-width: 200px; position: absolute; left: 0;' class='TableRowText'>" +
                                                        "<div style='display: inline-flex; flex-direction: row; width: 100%'>" +
                                                        "<div style='display: inline-flex; width: 100%' class='TextTableRowDiv' id='" + row_data[(row_i - 1)].Id + "'>" + row_data[(row_i - 1)].TableRowText + "</div>" +
                                                        "<div class='PanelRow' style='display: flex; flex-direction: column'>" +
                                                        "<div style='width: 20px; height: 20px; background-color: #f44336;' class='StyleButton DeleteRow'>&#10006;</div>" +
                                                        "<div style='width: 20px; height: 20px; background-color: #f44336;' class='StyleButton EditTextRow'>&#9000;</div>" +
                                                        "</div></div>" +
                                                        "<div class='InputOwnIndex' style='display: inline-flex; width: 100%; background-color: #008dd2;'><div>Индекс</div><input type='text' name='OwnIndex' style='width: 100%;' value='" + row_data[(row_i - 1)].IndexRow + "' /></div>" +
                                                        "</td>";
                                                }
                                                if (row_i > 0 & col_i > 0) code += "<td class='TypeItemTable Column" + col_i + "' id=" + col_i + "><div><input type='radio' disabled></div></td>";
                                            }
                                            code += "</tr>";
                                        }
                                        code += "</table></div></div>";
                                        tmp.append(code);
                                        ResizeItem(tmp);
                                    }
                                    LoadSettingsTable(tmp.find(".AnswerSettings"), q_data);
                                })
                        })
                })
        }
            break;
        case 6: {
            var tmp = block.find('div.QuestionText[id=' + id_question + ']').parent().find('div.AnswerBlock');
            LoadSettingsFilter(tmp.find('.AnswerSettings'), q_data);
        }
            break;
        default:
            break;
    }
}

//Блок инициализации анимации и действий для редактора вопроса
function Init() {

    //BEGIN Animation block
    $('.NewBlock').on("mouseenter", ".ButtonType", function () {
        $(this).addClass('Scaled');
    })

    $('.NewBlock').on("mouseleave", ".ButtonType", function () {
        $(this).removeClass('Scaled');
    })

    $('.NewBlock').on("click", ".ButtonSlideSettings", function () {
        if ($(this).hasClass('ShowSlide')) {
            $(this).parents(".AnswerBlock").find('.PanelSettings').slideUp(1000);
            $(this).removeClass('ShowSlide');
        } else {
            $(this).addClass('ShowSlide');
            $(this).parents(".AnswerBlock").find('.PanelSettings').slideDown(1000);
        }
    })

    $('.NewBlock').on("mouseenter", ".SaveChangeAnswer", function () {
        $(this).addClass("Scaled");
    })

    $('.NewBlock').on("mouseleave", ".SaveChangeAnswer", function () {
        $(this).removeClass("Scaled");
    })

    $('.NewBlock').on("mouseenter", ".SaveChangeButton", function () {
        $(this).addClass("Scaled");
    })

    $('.NewBlock').on("mouseleave", ".SaveChangeButton", function () {
        $(this).removeClass("Scaled");
    })

    $('.NewBlock').on("mouseenter", ".EditAnswer", function () {
        $(this).addClass("Scaled");
    })

    $('.NewBlock').on("mouseleave", ".EditAnswer", function () {
        $(this).removeClass("Scaled");
    })

    $('.NewBlock').on("mouseenter", ".ChangeIndex", function () {
        $(this).addClass('Scaled');
    })

    $('.NewBlock').on("mouseleave", ".ChangeIndex", function () {
        $(this).removeClass('Scaled');
    })

    $('.NewBlock').on("mouseenter", ".DeleteAnswerItem", function () {
        $(this).addClass("Scaled");
    })

    $('.NewBlock').on("mouseleave", ".DeleteAnswerItem", function () {
        $(this).removeClass("Scaled");
    })

    $('.NewBlock').on("mouseenter", ".SaveTransition", function () {
        $(this).addClass('Scaled');
    })

    $('.NewBlock').on("mouseleave", ".SaveTransition", function () {
        $(this).removeClass('Scaled');
    })

    $('.NewBlock').on("mouseenter", ".QNo", function () {
        $(this).addClass('Scaled');
    })

    $('.NewBlock').on("mouseleave", ".QNo", function () {
        $(this).removeClass('Scaled');
    })

    $('.PanelQuestion').on("mouseenter", ".CloseBlockWallTrans", function () {
        $(this).addClass('Scaled');
    })

    $('.PanelQuestion').on("mouseleave", ".CloseBlockWallTrans", function () {
        $(this).removeClass('Scaled');
    })

    $('.NewBlock').on("mouseenter", ".QYes", function () {
        $(this).addClass('Scaled');
    })

    $('.NewBlock').on("mouseleave", ".QYes", function () {
        $(this).removeClass('Scaled');
    })

    $('.NewBlock').on("mouseenter", ".AddColumnTable", function () {
        $(this).addClass('Scaled');
    })

    $('.NewBlock').on("mouseleave", ".AddColumnTable", function () {
        $(this).removeClass('Scaled');
    })

    $('.NewBlock').on("mouseenter", ".AddRowTable", function () {
        $(this).addClass('Scaled');
    })

    $('.NewBlock').on("mouseleave", ".AddRowTable", function () {
        $(this).removeClass('Scaled');
    })
    // END Animation block

    $('.NewBlock').on("click", ".WallOfTypes", function () {
        var wall_left = $(this).offset().left;
        var wall_top = $(this).offset().top;
        var wall_width = $(this).width();
        var wall_height = $(this).height();
        $(this).css({ position: "absolute", "z-index": 9 });
        $(this).animate({ width: 100 }, 100);
        $(this).animate({ height: (wall_height + 100) }, 100);
        $(this).empty().append('<div class="ButtonType">Single</div><div class="ButtonType">Multi</div><div class="ButtonType">Free</div><div class="ButtonType">Table</div>' +
            '<div class="ButtonType">Text</div><div class="ButtonType">Filter</div>');
        $(this).removeClass("WallOfTypes").addClass("ChangeWall");
        $(document).mouseup(function (event) {
            var container = $('.ChangeWall');
            if (container.has(event.target).length === 0) {
                container.css({ position: "relative", "z-index": 5, height: 30, width: 30 });
                container.removeClass("ChangeWall").addClass("WallOfTypes");
                container.empty().text(">");
            }
        });
    })

    $('.NewBlock').on("click", ".ButtonType", function () {
        var string_type = $(this).text();
        CheckTypeQuestion(string_type, $(this).parents(".QuestionBlock"));
        var wall_height = $(this).parent().height() - 100;
        $(this).parent().css({ position: "relative", "z-index": 5, height: wall_height, width: 30 });
        $(this).parent().removeClass("ChangeWall").addClass("WallOfTypes");
        $(this).parents(".SettingsPanel").find('.NameType').text(string_type);
        $(this).parent().empty().text(">");
    })

    $('.NewBlock').on("click", ".AddColumnTable", function () {
        $(this).parents(".AnswerBlock").css("overflow_y", "auto");
        var table_item = $(this).parents(".AnswerBlock").find(".TableStyle");
        var count_row = table_item.find('tr').length;
        table_item.find('tr').each(function () {
            if ($(this).hasClass('AnswerTableLine')) {
                var code = "<td style='min-width: 200px; max-width: 200px; vertical-align: top' class='TableColumnText'>" +
                    "<div style='display: inline-flex; flex-direction: row; width: 100%;'>" +
                    "<div style='display: inline-flex; width: 100%;' class='TextAnswerDiv' id='0'>New Item</div>" +
                    "<div class='PanelColumn' style='display: flex; flex-direction: column; vartical-align: top;'>" +
                    "<div style='width: 20px; height: 20px; background-color: #f44336;' class='DeleteColumn StyleButton'>&#10006;</div>" +
                    "<div style='width: 20px; height: 20px; background-color: #f44336;' class='StyleButton EditTextColumn'>&#9000;</div>" +
                    "</div></div></td>";
                $(this).append(code);
            } else {
                var code = "<td class='TypeItemTable'><div><input type='radio' disabled></div></td>";
                $(this).append(code);
            }

        });

    })

    $('.NewBlock').on("click", ".AddRowTable", function () {
        var count_td = Number($(this).parents(".AnswerBlock").find('.TableStyle').find('tr:first').find('td').length);
        var item_table = $(this).parents(".AnswerBlock").find('.TableStyle').find('tbody');
        var code = "<tr>"
        for (var i_td = 0; i_td < count_td; i_td++) {
            if (i_td == 0) {
                code += "<td style='min-width: 200px; max-width: 200px;  position: absolute; left: 0;' class='TableRowText'>" +
                    "<div style='display: inline-flex; flex-direction: row; width: 100%'>" +
                    "<div style='display: inline-flex; width: 100%' class='TextTableRowDiv' id='0'>New question</div>" +
                    "<div class='PanelRow' style='display: flex; flex-direction: column'>" +
                    "<div style='width: 20px; height: 20px; background-color: #f44336;' class='DeleteRow StyleButton'>&#10006;</div>" +
                    "<div style='width: 20px; height: 20px; background-color: #f44336;' class='EditTextRow StyleButton'>&#9000;</div>" +
                    "</div></div></td>";
            } else {
                code += "<td class='TypeItemTable'><div><input type='radio' disabled></div></td>";
            }
        }
        code += "</tr>"
        item_table.append(code);
        ResizeItem($(this).parents(".AnswerBlock"));
    })

    $('.NewBlock').on("click", ".EditTextRow", function () {
        $(this).addClass("SaveChangeTextRow").removeClass('EditTextRow');
        $(this).css({ "background-color": "#008dd2" });
        var block = $(this).parents(".PanelRow").prev();
        var str_text = block.text();
        block.empty().append('<textarea style="resize: none" name="ChangeTextRow">' + str_text + '</textarea>');
    })

    $('.NewBlock').on("click", ".EditTextColumn", function () {
        $(this).addClass("SaveChangeTextColumn").removeClass('EditTextColumn');
        $(this).css({ "background-color": "#008dd2" });
        var block = $(this).parents(".PanelColumn").prev();
        var str_text = block.text();
        block.empty().append('<textarea style="resize: none" name="ChangeTextColumn">' + str_text + '</textarea>');
    })

    $('.NewBlock').on("click", ".SaveChangeTextRow", function () {
        $(this).addClass("EditTextRow").removeClass("SaveChangeTextRow");
        $(this).css({ "background-color": "#f44336" })
        var block = $(this).parents(".PanelRow").prev();
        var str_textarea = block.find("textarea[name=ChangeTextRow]").val();
        block.empty().text(str_textarea);
        ResizeItem($(this).parents(".AnswerBlock"));
    })

    $('.NewBlock').on("click", ".SaveChangeTextColumn", function () {
        $(this).addClass("EditTextColumn").removeClass("SaveChangeTextColumn");
        $(this).css({ "background-color": "#f44336" })
        var block = $(this).parents(".PanelColumn").prev();
        var str_textarea = block.find("textarea[name=ChangeTextColumn]").val();
        block.empty().text(str_textarea);
    })

    $('.NewBlock').on("click", ".DeleteRow", function () {
        var block = $(this).parents(".TableStyle");
        var remove_block = $(this);
        var id_table_row = Number($(this).parents(".TableRowText").find(".TextTableRowDiv").attr("id"));
        if (block.find("tr").length > 2) {
            $.post("/Question/DeleteTableRow", { id_table_row: id_table_row })
                .success(function () { remove_block.parents("tr").remove(); })
        } else {
            alert("Ограничение удаления строк");
        }
    })

    $('.NewBlock').on("click", ".DeleteColumn", function () {
        var block = $(this).parents(".TableStyle");
        var item = $(this).parents(".TableColumnText");
        var id_answer = item.find("div.TextAnswerDiv").attr("id");
        block.find("tr:not(AnswerTableLine)").each(function () {
            $(this).find(".TypeItemTable:first").remove();
        })
        $.post("/Question/deleteAnswer", { Id: id_answer })
            .success(function () { })
        ResizeItem($(this).parents(".AnswerBlock"));
        item.remove();
    })

    $('.NewBlock').on("click", ".BuildTable", function () {
        var panel = $(this).parents(".SettingsBuildTable");
        var count_column = Number(panel.find(".ColumnBuild").next().val());
        var count_row = Number(panel.find(".RowBuild").next().val());
        console.log("Column " + count_column + " Row " + count_row);
        var code = "<div style='position: relative'><div style='overflow-x: scroll; overflow-y: visible; margin-left: 200px;'><table class='TableStyle' style='width: 100%;'>";
        for (var row_i = 0; row_i <= count_row; row_i++) {
            if (row_i == 0) {
                code += "<tr class='AnswerTableLine'>";
            } else {
                code += "<tr>";
            }
            for (var col_i = 0; col_i <= count_column; col_i++) {
                if (row_i == 0 && col_i == 0) code += "<td></td>";
                if (row_i == 0 && col_i > 0) {
                    code += "<td style='min-width: 200px; max-width: 200px; vertical-align: top;' class='TableColumnText Column" + col_i + "' id=" + col_i + ">" +
                        "<div style='display: inline-flex; flex-direction: row; width: 100%;'>" +
                        "<div style='display: inline-flex; width: 100%;' class='TextAnswerDiv' id='0'>New Item</div>" +
                        "<div class='PanelColumn' style='display: flex; flex-direction: column; vartical-align: top;'>" +
                        "<div style='width: 20px; height: 20px; background-color: #f44336;' class='DeleteColumn StyleButton'>&#10006;</div>" +
                        "<div style='width: 20px; height: 20px; background-color: #f44336;' class='StyleButton EditTextColumn'>&#9000;</div>" +
                        "</div></div></td>";
                }
                if (row_i > 0 & col_i == 0) {
                    code += "<td style='min-width: 200px; max-width: 200px; position: absolute; left: 0;' class='TableRowText'>" +
                        "<div style='display: inline-flex; flex-direction: row; width: 100%'>" +
                        "<div style='display: inline-flex; width: 100%' class='TextTableRowDiv' id='0'>New question</div>" +
                        "<div class='PanelRow' style='display: flex; flex-direction: column'>" +
                        "<div style='width: 20px; height: 20px; background-color: #f44336;' class='StyleButton DeleteRow'>&#10006;</div>" +
                        "<div style='width: 20px; height: 20px; background-color: #f44336;' class='StyleButton EditTextRow'>&#9000;</div>" +
                        "</div></div>" +
                        "</td>";
                }
                if (row_i > 0 & col_i > 0) code += "<td class='TypeItemTable Column" + col_i + "' id=" + col_i + "><div><input type='radio' disabled></div></td>";
            }
            code += "</tr>";
        }
        code += "</table></div></div>";
        $(this).parents(".QuestionBlock").find("div.AnswerBlock").append(code);
        ResizeItem($(this).parents(".QuestionBlock").find(".AnswerBlock"));
        // $(this).parents(".QuestionBlock").find(".AnswerSettings").find(".SettingsBuildTable").remove();
        // $(this).parents(".QuestionBlock").find(".AnswerSettings").append('<div class="StyleButton">Button</div>');
        LoadSettingsTable($(this).parents(".QuestionBlock").find(".AnswerSettings"));
        $(this).parents(".QuestionBlock").find(".AnswerSettings").find(".SettingsBuildTable").remove();
    })

    $('.NewBlock').on("click", ".SaveChangeButton", function () {
        var item_block = $(this).parents(".QuestionBlock");
        var type_question = Number($(this).parents(".QuestionBlock").find('div[name=QuestionInfo]').find('input[name=questionType]').val());
        var id_question = Number($(this).parents(".QuestionBlock").find('div[name=QuestionInfo]').find('input[name=questionInfo]').val());
        switch (type_question) {
            case 1:
                {
                    var textarea_list = item_block.find(".QuestionText").find('textarea');
                    var textarea;
                    $.each(textarea_list, function (i, item) {
                        textarea = item;
                    })
                    var text_question = sceditor.instance(textarea).val();
                    var q_query = {
                        Id: id_question,
                        TypeQuestion: type_question,
                        TextQuestion: text_question != "" ? text_question : "empty text",
                        ProjectID: $('div[name=ProjectInfo]').find('input[name=project_id]').val(),
                        LimitCount: null,
                        TypeMassk: null,
                        IsKvot: item_block.find('.AnswerBlock').find('.QuotaDiv').find('.QYes').hasClass('QOn') ? true : false,
                        IsRotate: item_block.find('.AnswerBlock').find('.RotateDiv').find('.QYes').hasClass('QOn') ? true : false
                    }

                    $.post("/Question/Question", q_query)
                        .success(function () {
                            var list_answer = [];
                            var list_base_answer = [];
                            item_block.find('.AnswerItem').each(function () {
                                if ($(this).hasClass('BaseAnswer')) {
                                    if (!$(this).hasClass('Existed')) {
                                        var query = {
                                            AnswerKey: $(this).attr('id'),
                                            AnswerType: 2,
                                            QuestionID: id_question
                                        }
                                        list_base_answer.push(query);
                                    }
                                } else {
                                    if (Number($(this).attr('id')) >= 0) {
                                        var query = {
                                            Id: Number($(this).attr('id')),
                                            AnswerText: $(this).find('.TextAnswer').text(),
                                            QuestionID: id_question,
                                            Index: Number($(this).find("input[name=IndexAnswer]").val()),
                                            isFreeArea: $(this).find("input[name=FreeAreaCheckbox]").prop("checked") ? true : false
                                        };
                                        list_answer.push(query)
                                    }
                                }
                            })
                            list_answer.sort(sortFN("Index"));
                            $.post("/Question/SaveAnswerRange", { tmp: list_answer })
                                .success(function () {
                                    $.each(list_base_answer, function (i, item) {
                                        $.ajax({
                                            type: "POST",
                                            url: "/Question/setAnswerAll",
                                            async: false,
                                            data: item,
                                            success: function () { }
                                        })
                                    })
                                    item_block.find('.AnswerItem').each(function () {
                                        if ($(this).not('.BaseAnswer')) {
                                            $(this).attr('id', -1);
                                        }
                                    })
                                })
                        })
                }
                break;
            case 2:
                {
                    var textarea_list = item_block.find(".QuestionText").find('textarea');
                    var textarea;
                    $.each(textarea_list, function (i, item) {
                        textarea = item;
                    })
                    var text_question = sceditor.instance(textarea).val();
                    var q_query = {
                        Id: id_question,
                        TypeQuestion: type_question,
                        TextQuestion: text_question != "" ? text_question : "empty text",
                        ProjectID: $('div[name=ProjectInfo]').find('input[name=project_id]').val(),
                        LimitCount: isNaN(Number(item_block.find('.LimitCountAnswer').find('.CountLimit').find('input').val())) ? 0 : Number(item_block.find('.LimitCountAnswer').find('.CountLimit').find('input').val()),
                        IsKvot: false,
                        TypeMassk: null,
                        IsRotate: item_block.find('.AnswerBlock').find('.RotateDiv').find('.QYes').hasClass('QOn') ? true : false
                    }
                    $.post("/Question/Question", q_query)
                        .success(function () {
                            var list_answer = [];
                            var list_base_answer = [];
                            item_block.find('.AnswerItem').each(function () {
                                if ($(this).hasClass('BaseAnswer')) {
                                    var query = {
                                        AnswerKey: $(this).attr('id'),
                                        AnswerType: 2,
                                        QuestionID: id_question
                                    };
                                    list_base_answer.push(query);
                                } else {
                                    if (Number($(this).attr('id')) >= 0) {
                                        var query = {
                                            Id: Number($(this).attr('id')),
                                            AnswerText: $(this).find('.TextAnswer').text(),
                                            QuestionID: id_question,
                                            Index: Number($(this).find("input[name=IndexAnswer]").val()),
                                            isFreeArea: $(this).find("input[name=FreeAreaCheckbox]").prop("checked") ? true : false
                                        };
                                        list_answer.push(query)
                                    }
                                }
                            })
                            list_answer.sort(sortFN("Index"));
                            $.post("/Question/SaveAnswerRange", { tmp: list_answer })
                                .success(function () {
                                    $.each(list_base_answer, function (i, item) {
                                        $.ajax({
                                            type: "POST",
                                            url: "/Question/setAnswerAll",
                                            async: false,
                                            data: item,
                                            success: function () { }
                                        })
                                    })
                                    item_block.find('.AnswerItem').each(function () {
                                        if ($(this).not('.BaseAnswer')) {
                                            $(this).attr('id', -1);
                                        }
                                    })
                                })
                        })
                }
                break;
            case 3:
                {
                    var textarea_list = item_block.find(".QuestionText").find('textarea');
                    var textarea;
                    $.each(textarea_list, function (i, item) {
                        textarea = item;
                    })
                    var text_question = sceditor.instance(textarea).val();
                    var q_query = {
                        Id: id_question,
                        TypeQuestion: type_question,
                        TextQuestion: text_question != "" ? text_question : "empty text",
                        ProjectID: $('div[name=ProjectInfo]').find('input[name=project_id]').val(),
                        TypeMassk: isNaN(Number(item_block.find(".MasskDiv").find(".CheckTypeMassk").find("option:checked").val())) ? null : Number(item_block.find(".MasskDiv").find(".CheckTypeMassk").find("option:checked").val()),
                        LimitCount: null,
                        IsKvot: item_block.find('.AnswerBlock').find('.QuotaDiv').find('.QYes').hasClass('QOn') ? true : false,
                        IsRotate: false
                    }
                    if (q_query.TypeMassk != 1) {
                        q_query.IsKvot = false;
                        if (item_block.find(".PanelOfRange").find(".RangeItem").length > 0) {
                            item_block.find(".PanelOfRange").find(".RangeItem").each(function () {
                                var block_for_remove = $(this);
                                $.post("/Question/DeleteRangePos", { id_range: Number($(this).find("input[name=RangeID]").val()), id_q: id_question })
                                    .success(function () {
                                        block_for_remove.remove();
                                    })
                            })
                        }
                    }
                    $.post("/Question/Question", q_query)
                        .success(function () {
                            var list_answer = [];
                            item_block.find('.AnswerItem').each(function () {
                                if (Number($(this).attr('id')) >= 0) {
                                    var query = {
                                        Id: Number($(this).attr('id')),
                                        AnswerText: $(this).find('.TextAnswer').text(),
                                        QuestionID: id_question,
                                        Index: Number($(this).find("input[name=IndexAnswer]").val()),
                                        isFreeArea: false
                                    };
                                    list_answer.push(query)
                                }
                            })
                            list_answer.sort(sortFN("Index"));
                            $.post("/Question/SaveAnswerRange", { tmp: list_answer })
                                .success(function () {
                                    item_block.find('.AnswerItem').each(function () {
                                        if ($(this).not('.BaseAnswer')) {
                                            $(this).attr('id', -1);
                                        }
                                    })
                                });
                        })
                    //if (pool_data.DeleteListAnswerSM != undefined) {
                    //    $.post("/Question/DeleteListAnswer", pool_data.DeleteListAnswerSM);
                    //}

                }
                break;
            case 4: {
                var textarea_list = item_block.find(".QuestionText").find('textarea');
                var textarea;
                $.each(textarea_list, function (i, item) {
                    textarea = item;
                })
                var text_question = sceditor.instance(textarea).val();
                var bind_id_question = null;
                if (item_block.find(".BindQuestionDiv").find(".BindYes").hasClass("QOn")) {
                    bind_id_question = Number(item_block.find(".BlockChain").find("select option:checked").val());
                }
                var q_query = {
                    Id: id_question,
                    TypeQuestion: type_question,
                    TextQuestion: text_question != "" ? text_question : "empty text",
                    ProjectID: $('div[name=ProjectInfo]').find('input[name=project_id]').val(),
                    TypeMassk: null,
                    LimitCount: null,
                    IsKvot: false,
                    IsRotate: false,
                    Bind: bind_id_question
                }
                $.post("/Question/Question", q_query)
                    .error(function () { alert("ERROR") });
                var a_query = [];
                item_block.find(".AnswerBlock").find(".TableStyle").find("tr.AnswerTableLine").find(".TableColumnText").each(function () {
                    if (Number($(this).find('div.TextAnswerDiv').attr('id')) >= 0) {
                        a_query.push({ AnswerText: $(this).find("div.TextAnswerDiv").text(), Id: $(this).find("div.TextAnswerDiv").attr('id') });
                    }
                })
                var count = 1;
                $.each(a_query, function (i, item) {
                    item.Index = count++;
                    item.QuestionID = id_question;
                })
                var bl_query = [];
                count = 1;
                var is_own_index = item_block.find(".OwnIndexDiv").find(".QYes").hasClass("QOn") ? true : false;
                item_block.find(".AnswerBlock").find(".TableStyle").find("tr:not(.AnswerTableLine)").each(function () {
                    if (is_own_index == false) {
                        if (Number($(this).find("td.TableRowText").find("div.TextTableRowDiv").attr('id')) >= 0) {
                            bl_query.push({
                                TableRowText: $(this).find("td.TableRowText").find("div.TextTableRowDiv").text(),
                                Id: $(this).find("td.TableRowText").find("div.TextTableRowDiv").attr("id")
                            });
                        }
                    }
                    else {
                        if (Number($(this).find("td.TableRowText").find("div.TextTableRowDiv").attr('id')) >= 0) {
                            bl_query.push({
                                TableRowText: $(this).find("td.TableRowText").find("div.TextTableRowDiv").text(),
                                Id: $(this).find("td.TableRowText").find("div.TextTableRowDiv").attr("id"),
                                IndexRow: Number($(this).find("div.InputOwnIndex").find("input[name=OwnIndex]").val())
                            });
                        }
                    }
                })
                $.each(bl_query, function (i, item) {
                    if (is_own_index == false) item.IndexRow = count++;
                    item.TableID = id_question;
                })
                var query = {
                    id_q: id_question,
                    text_row: bl_query,
                    data: a_query
                }
                $.post("/Question/setTableRow", query)
                    .success(function () {
                        item_block.find(".AnswerBlock").find(".TableStyle").find("tr.AnswerTableLine").find(".TableColumnText").each(function () {
                            $(this).find("div.TextAnswerDiv").attr('id', -1);
                        })
                        item_block.find(".AnswerBlock").find(".TableStyle").find("tr:not(.AnswerTableLine)").each(function () {
                            if (Number($(this).find("td.TableRowText").find("div.TextTableRowDiv").attr('id')) == 0) {
                                $(this).find("td.TableRowText").find("div.TextTableRowDiv").attr('id', -1);
                            }
                        })
                    });
            }
                break;
            case 5: {
                alert("Text");
                var textarea_list = item_block.find(".QuestionText").find('textarea');
                var textarea;
                $.each(textarea_list, function (i, item) {
                    textarea = item;
                })
                var text_question = sceditor.instance(textarea).val();
                var q_query = {
                    Id: id_question,
                    TypeQuestion: type_question,
                    TextQuestion: text_question != "" ? text_question : "empty text",
                    ProjectID: $('div[name=ProjectInfo]').find('input[name=project_id]').val(),
                    LimitCount: null,
                    IsKvot: false,
                    IsRotatr: false
                }
                $.post("/Question/Question", q_query)
                    .success(function () {
                        $.post("/Question/deleteAllAnswer", { id_question: id_question });
                        $.post("/Question/DeleteAllTableRow", { id_q: id_question });
                    })
            }
                break;
            case 6: {
                var textarea_list = item_block.find(".QuestionText").find('textarea');
                var textarea;
                $.each(textarea_list, function (i, item) {
                    textarea = item;
                })
                var text_question = sceditor.instance(textarea).val();
                var q_query = {
                    Id: id_question,
                    TypeQuestion: type_question,
                    TextQuestion: text_question != "" ? text_question : "empty text",
                    ProjectID: $('div[name=ProjectInfo]').find('input[name=project_id]').val(),
                    TypeMassk: null,
                    LimitCount: null,
                    IsKvot: false,
                    IsRotate: false
                }
                $.post("/Question/Question", q_query)
                    .success(function () {
                        var panel = item_block.find(".AnswerSettings");
                        var l_file;
                        $.each(panel.find('input#filter_file'), function (i, item) {
                            l_file = item;
                        })
                        var files = l_file.files;
                        if (files.length > 0) {
                            if (window.FormData != undefined) {
                                var data = new FormData();
                                for (var i = 0; i < files.length; i++)
                                    data.append('file' + i, files[i]);

                                $.ajax({
                                    type: "POST",
                                    url: "/Group/Upload",
                                    contentType: false,
                                    processData: false,
                                    data: data,
                                    success: function (server_data) {
                                        $.post("/Group/BindIDFilter", { id_p: q_query.ProjectID, id_q: q_query.Id, path: server_data })
                                            .success(function () {
                                                panel.empty();
                                                LoadSettingsFilter(panel, q_query);
                                            })
                                    },
                                    error: function (xhr, status, p3) {
                                        alert(xhr.responseText)
                                    }
                                })
                            }
                        }
                    })
            }
                break;
            default:
                break;
        }
        //alert("Вопрос сохранен");
        $('.PanelQuestion').remove();
    })

    $('.NewBlock').on("click", ".AddAnswerSingle", function () {
        var item_block = $(this).parents(".QuestionBlock");
        var index_answer = 0;
        if (item_block.find(".AnswerBlock").find(".AnswerItem:not('BaseAnswer')").length != 0) {
            index_answer = Number(item_block.find(".AnswerBlock").find(".AnswerItem:not(.BaseAnswer)").last().find('input[name=IndexAnswer]').val());
        }
        $(this).parents(".QuestionBlock").find(".SaveChangeButton").addClass("NewChangeQuestion");
        $(this).parents(".AnswerBlock").append('<div class="AnswerItem NewItem" id=0><div style="margin-right: 15px"><input type="radio" disabled/>' +
            '</div><div class="IndexAnswer">' + (index_answer + 1) + ') </div><div style="width: 80%;" class="TextAnswer TextNewItem">New answer</div>' +
            '<div class="FreeArea" style="display: inline-flex; width: 20%; height: 100%;"><input type="checkbox" name="FreeAreaCheckbox" />Свободное поле</div>' +
            '<div class="DeleteAnswerItem">&#10006;</div><div style="width: 5px;"></div>' +
            '<div class="EditAnswer">&#9000;</div>' +
            '<div class="ChangeIndex">ID</div>' +
            '<input type="hidden" name="IndexAnswer" value="' + (index_answer + 1) + '"></div>');
    })

    $('.NewBlock').on("click", ".EditAnswer", function () {
        var item = $(this).parent().find(".TextAnswer");
        $(this).removeClass("EditAnswer").addClass("SaveChangeAnswer").html("&#10004;");
        $(document).mouseup(function (event) {
            var container = $('.SaveChangeAnswer');
            if (container.has(event.target).length === 0 && container.parent().find('.TextAnswer').has(event.target).length === 0) {
                var str_tmp = container.parent().find('.TextAnswer').find('input').val();
                if (str_tmp == undefined || str_tmp == "") { return; }
                
                container.parent().find('.TextAnswer').empty().text(str_tmp);
                container.removeClass("SaveChangeAnswer").addClass("EditAnswer");
                container.empty().html("&#9000;");
            }
        });
        var width_text = item.width();
        var text_str = item.text();
        item.empty();
        item.append('<input type="text" value="' + text_str + '" style="max-width: 99%; width: 99%;"/>');
        item.find('input').focus().select().keydown(function (event) {
            if (event.keyCode == 13) {
                var str_tmp = item.find('input').val();
                item.empty().text(str_tmp);
                item.parent().find('.SaveChangeAnswer').removeClass('SaveChangeAnswer').addClass('EditAnswer').empty().html("&#9000;");
            }
        });
    })

    $('.NewBlock').on("click", ".AddAnswerMulti", function () {
        var item_block = $(this).parents(".QuestionBlock");
        var index_answer = 0;
        if (item_block.find(".AnswerBlock").find(".AnswerItem:not('BaseAnswer')").length != 0) {
            index_answer = Number(item_block.find(".AnswerBlock").find(".AnswerItem:not(.BaseAnswer)").last().find('input[name=IndexAnswer]').val());
        }
        $(this).parents(".QuestionBlock").find(".SaveChangeButton").addClass("NewChangeQuestion");
        $(this).parents(".AnswerBlock").append('<div class="AnswerItem NewItem" id=0><div style="margin-right: 15px"><input type="checkbox" disabled/>' +
            '</div><div class="IndexAnswer">' + (index_answer + 1) + ') </div>' +
            '<div style="width: 80%;" class="TextAnswer TextNewItem">New answer</div>' +
            '<div class="FreeArea" style="display: inline-flex; width: 20%; height: 100%;"><input type="checkbox" name="FreeAreaCheckbox" />Свободное поле</div>' +
            '<div class="DeleteAnswerItem">&#10006;</div><div style="width: 5px;"></div>' +
            '<div class="EditAnswer">&#9000;</div>' +
            '<div style="width: 5px;"></div>' +
            '<div class="ChangeIndex">ID</div>' +
            '<div style="width: 5px;"></div>' +
            '<input type="hidden" name="IndexAnswer" value="' + (index_answer + 1) + '"></div>');
    })

    $('.NewBlock').on("click", ".AddAnswerFree", function () {
        var item_block = $(this).parents(".QuestionBlock");
        var index_answer = 0;
        if (item_block.find(".AnswerBlock").find(".AnswerItem:not('BaseAnswer')").length != 0) {
            index_answer = Number(item_block.find(".AnswerBlock").find(".AnswerItem:not(.BaseAnswer)").last().find('input[name=IndexAnswer]').val());
            console.log("Index Multi >>>> ", item_block.find(".AnswerBlock").find(".AnswerItem:not(.BaseAnswer)").last().find('input[name=IndexAnswer]').val());
        }
        $(this).parents(".QuestionBlock").find(".SaveChangeButton").addClass("NewChangeQuestion");
        $(this).parents(".AnswerBlock").append('<div class="AnswerItem NewItem" id=0><div style="margin-right: 15px"><input type="text" placeholder="Текстовый блок" disabled/>' +
            '</div><div style="width: 100%;" class="TextAnswer TextNewItem">New answer</div><div class="DeleteAnswerItem">&#10006;</div><div style="width: 5px;"></div>' +
            '<div class="EditAnswer">&#9000;</div>' +
            '<div style="width: 5px;"></div>' +
            '<div class="ChangeIndex">ID</div>' +
            '<div style="width: 5px;"></div>' +
            '<input type="hidden" name="IndexAnswer" value="' + (index_answer + 1) + '"></div>');
    })

    $('.NewBlock').on("click", ".AddBase", function () {
        var type_question = Number($(this).parents(".QuestionBlock").find('div[name=QuestionInfo]').find('input[name=questionType]').val());
        var block = $(this).parents(".QuestionBlock");
        $(this).parent().after("<div class='AddBaseAnswer StyleButton' style='width: 120px; background-color: #81c784; height: 100%;'>&#10010; Базовый ответ</div>");
        var id_base_answer = Number($(this).parent().find("select option:checked").val());
        var str_base_answer = $(this).parent().find("select option:checked").text()
        var is_exist = false;
        block.find(".BaseAnswer").each(function () {
            if ($(this).find(".IndexAnswer").next().text() == str_base_answer) {
                is_exist = true;
                alert("Уже есть");
                return false;
            }
        })
        if (is_exist == false) {
            switch (type_question) {
                case 1: {
                    block.find(".AnswerBlock").append('<div class="AnswerItem BaseAnswer" id=' + id_base_answer + '><div style="margin-right: 15px"><input type="radio" disabled/></div><div class="IndexAnswer">' + -1 * Number(list_base_answer[id_base_answer].BaseIndex) + ') </div><div style="width: 100%;">' + str_base_answer + '</div><div class="DeleteAnswerItem">&#10006;</div></div>');
                }
                    break;
                case 2: {
                    block.find(".AnswerBlock").append('<div class="AnswerItem BaseAnswer" id=' + id_base_answer + '><div style="margin-right: 15px"><input type="checkbox" disabled/></div><div class="IndexAnswer">' + -1 * Number(list_base_answer[id_base_answer].BaseIndex) + ') </div><div style="width: 100%;">' + str_base_answer + '</div><div class="DeleteAnswerItem">&#10006;</div></div>');
                }
                    break;
                default:
                    break;
            }
        }
        $(this).parent().remove();
    })

    $('.NewBlock').on("click", ".AddBaseAnswer", function () {
        var panel = $(this);
        $.get("/Question/getListAnswerBase")
            .success(function (base_data) {
                list_base_answer = {};
                $.each(base_data, function (i, item) {
                    list_base_answer[item.Id] = item;
                })
                var code = "<div style='height: 100%;'><select style='height: 100%;'>";
                $.each(base_data, function (i, item) {
                    code += "<option value='" + item.Id + "'>" + item.AnswerText + "</option>";
                })
                code += "</select><div class='AddBase StyleButton' style='background-color: #81c784; height: 100%;'>Добавить</div></div>";
                panel.after(code);
                panel.remove();
            })
    })

    $('.NewBlock').on("click", ".ChangeIndex", function () {
        var index_answer = $(this).parent().find('input[name=IndexAnswer]').val()
        $(this).empty().after("<div><input class='EditIndexAnswer' style='width: 50px;' type='text' value='" + index_answer + "' /></div>");
        $(this).remove();
        $(document).mouseup(function (event) {
            var container = $('.EditIndexAnswer');
            var q_block = $('.EditIndexAnswer').parents(".AnswerItem");
            var par_container = $('.EditIndexAnswer').parent();
            if (par_container.has(event.target).length === 0) {
                var str_index = container.val();
                var block = container.parent().parent();
                q_block.find(".IndexAnswer").text(str_index + ") ");
                block.find('input[name=IndexAnswer]').val(str_index);
                par_container.after('<div class="ChangeIndex">ID</div>');
                par_container.remove();
            }
        })
    })

    $('.NewBlock').on("keyup", ".EditIndexAnswer", function (event) {
        if (event.keyCode == 13) {
            var str_index = $(this).val()
            var block = $(this).parent().parent();
            var q_block = $('.EditIndexAnswer').parents(".AnswerItem");
            q_block.find(".IndexAnswer").text(str_index + ") ");
            block.find('input[name=IndexAnswer]').val(str_index);
            $(this).parent().after('<div class="ChangeIndex">ID</div>');
            $(this).parent().remove();
        }
    })

    $('.NewBlock').on("click", ".DeleteAnswerItem", function () {
        var id_answer = Number($(this).parent().attr('id'));
        var block = $(this).parents(".AnswerBlock");
        var item = $(this).parent();

        if (item.hasClass('BaseAnswer')) {
            $.post("/Question/deleteAnswerAll", { id_answer: id_answer })
                .success(function () {
                    item.remove();
                })
        } else {
            var index_answer = Number($(this).parents(".AnswerItem").find("input[name=IndexAnswer]").val());
            if (!item.hasClass('NewItem')) {
                $.post("/Question/deleteAnswer", { id: id_answer })
                    .success(function () {
                        item.remove();
                    })
            } else {
                item.remove();
            }
            ReplaceIndexAnswer(block, index_answer);
        }
    })

    $('.NewBlock').on("click", ".SetBindGroup", function (event) {
        var question_id = Number($(this).parents(".QuestionBlock").find("input[name=questionInfo]").val());
        var answer_id = Number($(this).parents(".AnswerItem").attr('id'));
        var offset_x = $(this).offset().left;
        var offset_y = $(this).offset().top;
        var height = $(this).height();
        var x = event.clientX;
        var y = event.clientY;
        var block_change = $(this);
        var code = "";
        $('.NewBlockQuestions').find(".NewGroup").each(function () {
            code += "<div class='StyleButton SetBindGroupButton' style='background-color: #008dd2; color: #fff;' id='" + $(this).find('.ShowPanelGroupText').attr('id') + "'>" + $(this).find("div:first").text() + "</div>";
        })
        $('body').append("<div style='width: 100px; height: auto; position: absolute; left: " + offset_x + "px; top: " + offset_y + "px;z-index: 5000;' class='RemovePanel'>" +
            code +
            "</div>");
        var cont_group = $('.RemovePanel');
        cont_group.css({
            top: (offset_y - (cont_group.height() - height)) + "px",
            opacity: 0
        })

        $('.SetBindGroupButton').click(function () {
            var group_id = $(this).attr("id");
            console.log("List repwopeowpr ----- ", { id_q: question_id, id_a: answer_id, id_g: group_id });
            $.post("/Group/SetBindGroup", { id_q: question_id, id_a: answer_id, id_g: group_id })
                .success(function () {
                    block_change.after('<div style = "width: 100px; background-color: #81c784;" > Группа ' + group_id + '<div class= "RemoveBindGroup StyleButton" style = "background-color: #f44336; width: 30px;" >&#10006;</div></div>');
                    block_change.remove();
                    cont_group.remove();
                });
        })

        cont_group.animate({ left: (cont_group.offset().left - cont_group.width()) + "px", opacity: 1 }, 200);
        $(document).mouseup(function (event) {
            var cont = $('.RemovePanel');
            if (cont.has(event.target).length === 0) {
                //  alert();
                cont.remove();
            }
        })
    })

    $('.NewBlock').on("click", ".RemoveBindGroup", function () {
        var answer_id = Number($(this).parents(".AnswerItem").attr('id'));
        var question_id = Number($(this).parents(".QuestionBlock").find("input[name=questionInfo]").val());
        var block = $(this).parent();
        var block_item = $(this).parents(".AnswerItem");
        var block_change = $(this);
        $.post("/Group/DeleteBindGroup", { id_q: question_id, id_a: answer_id })
            .success(function () {
                block.after('<div class="SetBindGroup StyleButton" style="width: 30px; background-color: #81c784; font-size: 20px;">&infin;</div>');
                block.remove();
            })
    })

    $('.NewBlock').on("click", ".AddTransition", function () {
        var project_id = Number($('div[name=ProjectInfo]').find('input[name=project_id]').val());
        var question_id = Number($(this).parents('.QuestionBlock').find('div[name=QuestionInfo]').find('input[name=questionInfo]').val());
        console.log("Project Info ---- ", project_id);
        console.log("Question Info ---- ", question_id);
        var code = '<div class="BlockWallTrans" style="z-index: 5000;">' +
            '<div class="TransitionSettingsPanel">' +
            '<input type="hidden" name="Project_id" value="' + project_id + '" />' +
            '<input type="hidden" name="Question_id" value="' + question_id + '" />' +
            '<div style="display: inline-flex; width: 100%; height: 30px; font-family: Century Gothic;"><div class="ToQuestion" style="width: 50%;">Перейти к </div></div>' +
            '<div style="display: inline-flex; width: 100%; height: 30px; font-family: Century Gothic;"><div class="TriggerAnswer" style="width: 50%;">По вопросу </div></div>' +
            '<div class="StyleButton SaveTransition" style="background-color: #81c784;">Сохранить</div>' +
            '</div>' +
            '<div class="CloseBlockWallTrans">&#10006;</div>'
        '</div>';
        $('.PanelQuestion').append(code);
        $.get("/Group/getGroup", { id_p: project_id })
            .success(function (g_data) {
                var q_code = '<div style="width: 30%;"><select style="width: 100%;">'
                $.each(g_data, function (i, item) {
                    if (item.GroupID == 0 & item.Group == null)
                        q_code += '<option value="' + item.QuestionID + '">' + item.GroupName + '</option>';
                })
                q_code += '</select></div>';
                $('.ToQuestion').after(q_code);
            })
        var count_save_answer = 0;
        var an_code = '<div style="width: 30%;"><select style="width: 100%;">'
        $(this).parents(".QuestionBlock").find(".AnswerBlock").find(".AnswerItem").each(function () {
            if (Number($(this).attr('id')) > 0) {
                count_save_answer++;
                an_code += '<option value="' + $(this).attr('id') + '">' + $(this).find(".TextAnswer").text() + '</option>';
            }
        })
        an_code += '</select></div>';
        $('.TransitionSettingsPanel').find('.TriggerAnswer').after(an_code);
        disableScroll();
    })

    $('.PanelQuestion').on("click", ".SaveTransition", function () {
        var block = $(this).parents(".TransitionSettingsPanel");
        var pro_id = Number(block.find('input[name=Project_id]').val());
        var fromQ = Number(block.find('input[name=Question_id]').val());
        var toQ = Number(block.find('.ToQuestion').next().find('select').find('option:checked').val());
        var trigger = Number(block.find('.TriggerAnswer').next().find('select').find('option:checked').val());
        var query = {
            ProjectID: pro_id,
            fromQuestion: fromQ,
            toQuestion: toQ,
            TriggerAnswer: trigger
        }
        var is_save = false;
        $('.NewBlock')
            .find(".TransitionListDiv")
            .find(".TransItem").each(function () {
                console.log($(this).find(".TriggerAnswer").next().text() + " " + trigger);
                var id_answer = Number($(this).find(".TriggerAnswer").next().text());
                if (id_answer == trigger) {
                    alert("Ответ уже под переходом");
                    is_save = true;
                    return false;
                }
            })
        if (is_save == false) {
            $.post("/Question/setTransition", query)
                .success(function (id_trans) {
                    $('.NewBlock')
                        .find(".TransitionListDiv")
                        .append('<div class="TransItem" id="' + id_trans + '" style="display: inline-flex; width: 100%;">' +
                            '<div style="display: inline-flex; width: 30%;"><div class="fromQuestion" style="margin-right: 20px;">Преход от </div><div>' + query.fromQuestion + '</div></div>' +
                            '<div style="display: inline-flex; width: 30%;"><div class="toQuestion" style="margin-right: 20px;">Переход к </div><div>' + query.toQuestion + '</div></div>' +
                            '<div style="display: inline-flex; width: 30%;"><div class="TriggerAnswer" style="margin-right: 20px;">По вопросу </div><div>' + query.TriggerAnswer + '</div></div>' +
                            '<div class="StyleButton DeleteTrans" style="width: 30px; height: 30px; background-color: #f44336;">&#10006;</div>' +
                        '</div>')
                    $('.AddAnswer').find('.AnswerItem[id=' + query.TriggerAnswer + ']').append('<div class="isTransition" style="width:10px;height:10px;border-radius:10px;background-color:#71c874;"></div>')
                    enableScroll();
                    block.parents(".BlockWallTrans").remove();
                })
                .error(function () {
                    alert("Error");
                })
        }
    })

    $('.NewBlock').on("click", ".DeleteTrans", function () {
        var block = $(this).parent();
        var id_trans = Number(block.attr('id'));
        var trigger_answer = $(this).parent().find('.TriggerAnswer').next().text();
        $.post("/Question/deleteTransition", { id_transition: id_trans })
            .success(function () {
                block.remove();
                $('.AddAnswer').find('.AnswerItem[id=' + trigger_answer + ']').find('.isTransition').remove();
            })
    })

    $('.NewBlock').on("click", ".DeleteAllTrans", function () {
        var id_q = Number($(this).parents(".QuestionBlock").find(".QuestionText").attr("id"));
        var block = $(this).parents(".QuestionBlock");
        $.post("/Question/deleteAllTransition", { id_question: id_q })
            .success(function () {
                block.find('.TransitionListDiv').find(".TransItem").each(function () {
                    $(this).remove();
                })
            })
    })

    $('.NewBlock').on("click", ".BindYes", function () {
        $(this).next().removeClass("QOn");
        $(this).addClass("QOn");
        if ($(this).parent().find(".BlockChain").length == 0) {
            $(this).after("<div class='BlockChain' style='background-color: #f44336; width:0px'></div>");
            $(this).next().animate({ width: "150px" }, 1000);
            var block = $(this).next();
            var code = "<select style='width: 100%; height: 100%;'>";
            $(this).parents(".NewBlockQuestions").find(".NewClass").each(function () {
                if (!$(this).parent().hasClass('BlockQuestionGroup'))
                    code += "<option value='" + $(this).find(".ShowPanelQuestion").attr("id") + "'>" + $(this).find(".ShowPanelQuestion").find('div:first').text() + "</option>";
            })
            code += "</select>";
            block.append(code);
        }
    })

    $('.NewBlock').on("click", ".BindNo", function () {
        $(this).prev().remove();
        $(this).prev().removeClass("QOn");
        $(this).addClass("QOn");
    })

    $('.NewBlock').on("click", ".QNo", function () {
        var item = $(this).parent();
        if (item.hasClass('QuotaDiv')) { // Отключение квотирование в вопросе
            if (!$(this).hasClass('QOn')) {
                item.find('.QOn').removeClass('QOn');
                $(this).addClass('QOn');
            }
        }
        if (item.hasClass('RotateDiv')) { // Отключение ротирование в вопросе
            if (!$(this).hasClass('QOn')) {
                item.find('.QOn').removeClass('QOn');
                $(this).addClass('QOn');
            }
        }
        if (item.hasClass('OwnIndexDiv')) { // Отключение собственных индексов в вопросе
            if (!$(this).hasClass('QOn')) {
                item.find('.QOn').removeClass('QOn');
                $(this).addClass('QOn');
                if ($(this).hasClass('QNo')) {
                    OwnIndexPanelRemove($(this).parents(".QuestionBlock"));
                }
            }
        }
    })

    $('.NewBlock').on("click", ".QYes", function () {
        var item = $(this).parent();
        if (item.hasClass('QuotaDiv')) {  // Включение квотирование в вопросе
            if (!$(this).hasClass('QOn')) {
                item.find('.QOn').removeClass('QOn');
                $(this).addClass('QOn');
            }
        }
        if (item.hasClass('RotateDiv')) { // Включение ротирование в вопросе
            if (!$(this).hasClass('QOn')) {
                item.find('.QOn').removeClass('QOn');
                $(this).addClass('QOn');
            }
        }
        if (item.hasClass('OwnIndexDiv')) { // Включение собственные индексы в вопросе
            if (!$(this).hasClass('QOn')) {
                item.find('.QOn').removeClass('QOn');
                $(this).addClass('QOn');
                if ($(this).hasClass('QYes')) {
                    OwnIndexPanelBuild($(this).parents(".QuestionBlock"));
                }
            }
        }
    })

    $('.PanelQuestion').on("click", ".CloseBlockWallTrans", function () {
        $(this).parent().remove();
        enableScroll();
    })

    $('.NewBlock').on("click", ".AddBlocking", function () {
        var project_id = Number($('div[name=ProjectInfo]').find('input[name=project_id]').val());
        var question_id = Number($(this).parents('.QuestionBlock').find('div[name=QuestionInfo]').find('input[name=questionInfo]').val());
        console.log("Project Info ---- ", project_id);
        console.log("Question Info ---- ", question_id);
        var code = '<div class="BlockWallTrans">' +
            '<div class="BlockingSettingsPanel">' +
            '<input type="hidden" name="Project_id" value="' + project_id + '" />' +
            '<input type="hidden" name="Question_id" value="' + question_id + '" />' +
            '<div style="display: inline-flex; width: 100%; height: 30px; font-family: Century Gothic;"><div class="ToQuestion" style="width: 50%;">Блокировать по</div></div>' +
            '<div style="display: inline-flex; width: 100%; height: 30px; font-family: Century Gothic;"><div class="TypeBlocking" style="width: 50%;">Тип блокировки </div></div>' +
            '<div class="StyleButton SaveBlocking" style="background-color: #81c784;">Сохранить</div>' +
            '</div>' +
            '<div class="CloseBlockWallTrans">&#10006;</div>'
        '</div>';
        $('.PanelQuestion').append(code);
        $.get("/Group/getGroup", { id_p: project_id })
            .success(function (g_data) {
                var q_code = '<div style="width: 30%;"><select style="width: 100%;">'
                $.each(g_data, function (i, item) {
                    q_code += '<option value="' + item.QuestionID + '">' + item.GroupName + '</option>';
                })
                q_code += '</select></div>';
                $('.ToQuestion').after(q_code);
            })
        var count_save_answer = 0;
        var an_code = '<div style="width: 49%;"><select style="width: 100%;">';
        an_code += '<option value="1">Блокировка выбранных ответов</option>';
        an_code += '<option value="2">Блокировка не выбранных ответов</option>';
        an_code += '</select></div>';
        $('.BlockingSettingsPanel').find('.TypeBlocking').after(an_code);
        disableScroll();
    })

    $('.PanelQuestion').on("click", ".SaveBlocking", function () {
        var block = $(this).parents(".BlockingSettingsPanel");
        var pro_id = Number(block.find('input[name=Project_id]').val());
        var fromQ = Number(block.find('input[name=Question_id]').val());
        var toQ = Number(block.find('.ToQuestion').next().find('select').find('option:checked').val());
        var type = Number(block.find('.TypeBlocking').next().find('select').find('option:checked').val());
        var query = {
            ProjectID: pro_id,
            fromQuestion: fromQ,
            toQuestion: toQ,
            typeBlock: type
        }
        console.log("Blocking ---- ", query);
        var is_save = false;
        $('.NewBlock')
            .find(".BlockingListDiv")
            .find(".BlockItem").each(function () {
                var id_answer = Number($(this).find(".toQuestion").next().text());
                if (id_answer == toQ) {
                    alert("Блокировка по вопросу уже установлена");
                    is_save = true;
                    return false;
                }
            })
        if (is_save == false) {
            $.post("/Question/setBlock", query)
                .success(function (id_block) {
                    $('.NewBlock')
                        .find(".BlockingListDiv")
                        .append('<div class="BlockItem" id="' + id_block + '" style="display: inline-flex; width: 100%;">' +
                            '<div style="display: inline-flex; width: 30%;"><div class="toQuestion" style="margin-right: 20px;">Блокировка по вопросу </div><div>' + query.toQuestion + '</div></div>' +
                            '<div style="display: inline-flex; width: 30%;"><div class="TypeBlocking" style="margin-right: 20px;">Тип блокировки </div><div>' + query.typeBlock + '</div></div>' +
                            '<div class="StyleButton DeleteBlock" style="width: 30px; height: 30px; background-color: #f44336;">&#10006;</div>' +
                            '</div>')
                    enableScroll();
                    $('.PanelQuestion').off("click", ".SaveBlocking");
                    block.parents(".BlockWallTrans").remove();
                })
        }
    })

    $('.NewBlock').on("click", ".DeleteBlock", function () {
        var block = $(this).parents(".BlockItem");
        var id_block = block.attr("id");
        $.post("/Question/deleteBlocks", { id_block: id_block })
            .success(function () {
                block.remove();
            })
    })

    $('.NewBlock').on("click", ".ClearBlocking", function () {
        var id_q = Number($(this).parents(".QuestionBlock").find(".QuestionText").attr("id"));
        var block = $(this).parents(".QuestionBlock");
        $.post("/Question/deleteAllBlocks", { id_question: id_q })
            .success(function () {
                block.find('.BlockingListDiv').find(".BlockItem").each(function () {
                    $(this).remove();
                })
            })
    })

    $('.NewBlock').on("click", ".DeleteListRange", function () {
        var block = $(this).parents(".QuestionBlock");
        var id_question = Number(block.find("div[name=QuestionInfo]").find("input[name=questionInfo]").val());
        block.find(".PanelOfRange").find(".RangeItem").each(function () {
            var id_range;
            if ($(this).find("input[name=RangeID]").length > 0) {
                var remove_block = $(this);
                id_range = Number($(this).find("input[name=RangeID]").val());
                console.log("Id range --- ", id_range);
                $.post("/Question/DeleteRangePos", { id_range: id_range, id_q: id_question })
                    .success(function () { remove_block.remove() })
            } else {
                $(this).remove();
            }
        })
    })

    $('.NewBlock').on("click", ".AddRange", function () {
        var container_range = $(this).parents(".RangeDiv");
        var LowerContainer = container_range.find('input.LowerLimit');
        var UpperContainer = container_range.find('input.UpperLimit');
        var lower_limit = LowerContainer.val() != "" ? Number(LowerContainer.val()) : -Infinity;
        var upper_limit = UpperContainer.val() != "" ? Number(UpperContainer.val()) : Infinity;
        if (upper_limit <= lower_limit) {
            alert("Нижний предел не может быть больше верхнего");
            return;
        }
        var is_out = false
        console.log(lower_limit + "-" + upper_limit);
        container_range.find('.PanelOfRange').find('.StringRange').each(function () {
            var e_lower_limit = !isNaN(Number($(this).text().split('-')[0])) ? Number($(this).text().split('-')[0]) : -Infinity;
            var e_upper_limit = !isNaN(Number($(this).text().split('-')[1])) ? Number($(this).text().split('-')[1]) : Infinity;
            if (((e_lower_limit <= lower_limit) && (lower_limit <= e_upper_limit)) || ((e_lower_limit <= upper_limit) && (upper_limit <= e_upper_limit))) {
                alert("Диапозоны пересекаются");
                is_out = true;
                return false;
            }
        })
        if (is_out) { return; }
        var index_range = 1;
        if (container_range.find('.PanelOfRange').find('.IndexRange').length >= 1) {
            index_range = Number($('.PanelOfRange').find(".IndexRange:last").attr('id')) + 1;
        }
        var code = '<div class="RangeItem" style="display: inline-flex; justify-content: center; align-items: center; flex-direction: row; align-content: space-between;"><div class="IndexRange" id="' + index_range + '">' + index_range + ')</div><div class="StringRange" style="margin-left: 5px;">' + (lower_limit != -Infinity ? lower_limit : ("меньше " + upper_limit)) + ' - ' + (upper_limit != Infinity ? upper_limit : "больше " + lower_limit) + '</div><div style="background-color: #f44336; width: 15px; height: 15px; margin-left: 10px;" class="RemoveRangePos StyleButton">&#10006;</div></div>';
        container_range.find('.PanelOfRange').append(code);
        LowerContainer.val(upper_limit + 1);
        UpperContainer.val("");
    });

    $('.NewBlock').on("click", ".RemoveRangePos", function () {
        var item_block = $(this).parents(".RangeItem");
        var index_range = Number(item_block.find(".IndexRange").attr("id"));
        item_block.nextAll().each(function () {
            $(this).find(".IndexRange").attr("id", index_range).text(index_range + ")");
            index_range++;
        })
        item_block.remove();
        var block = $(this).parents(".PanelOfRange");

    })

    $('.NewBlock').on("click", ".SaveRangeList", function () {
        var block = $(this).parents(".QuestionBlock");
        var project_id = Number($('div[name=ProjectInfo]').find("input[name=project_id]").val());
        var question_id = Number(block.find("div[name=QuestionInfo]").find("input[name=questionInfo]").val());
        var list_range = [];
        block.find(".RangeDiv").find(".RangeItem").each(function () {
            list_range.push($(this).find("div.StringRange").text() + "#" + $(this).find(".IndexRange").attr('id'));
        })
        var query = {
            id_p: project_id,
            bind_q: question_id,
            list_range: list_range
        }
        $.post("/Question/SaveRange", query)
            .success(function () {
                alert("Диапозоны сохранены");
            })
        //console.log("Query range --- ", query);
    })

    $('.NewBlock').on("change", ".CheckTypeMassk", function () {
        var container = $(this).parents(".PanelSettings");
        var type_massk = Number($(this).find("option:checked").val());
        if (type_massk == 1) {
            container.find(".QuotaDiv").css("display", "inline-flex");
            container.find(".RangeDiv").slideDown(200);
        } else {
            container.find(".RangeDiv").slideUp(200);
            container.find(".QuotaDiv").slideUp(200);
        }
    })

    $('.NewBlock').on("click", ".DeleteFile", function () {
        var str_path = $(this).parents(".AnswerSettings").find("p[name=path_file]").text();
        var block = $(this).parents(".AnswerSettings");
        alert(str_path);
        $.post("/Group/DeleteFile", { path: str_path })
            .success(function () {
                block.empty();
                LoadSettingsFilter(block);
            })
    })
}

function CheckTypeQuestion(str_type, q_block) {
    var id_question = Number(q_block.find('div[name=QuestionInfo]').find('input[name=questionInfo]').val());
    switch (str_type) {
        case 'Single': {
            var type_question = Number(q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val());
            q_block.find('.AnswerBlock').find('.AnswerItem').remove();
            if (type_question == 3 || type_question == 2) {
                q_block.find(".AnswerBlock").find(".AnswerItem").each(function () {
                    $(this).find('div:first').find('input').attr("type", "radio");
                })
                if (type_question == 2) {
                    if (q_block.find('.BlockingListDiv').find('.BlockItem').length > 0) {
                        $.post("/Question/deleteAllBlocks", { id_question: id_question })
                    }
                }
                if (type_question == 3) {
                    if (q_block.find('.PanelOfRange').find('.RangeItem').length > 0) {
                        $.post("/Question/DeletaAllRangeQ", { id_question: id_question });
                    }
                }
            }
            if (type_question == 4) {
                $.post("/Question/DeleteAllTableRow", { id_q: id_question })
                var list_answer = [];
                q_block.find(".TableStyle").find(".AnswerTableLine").find(".TextAnswerDiv").each(function () {
                    list_answer.push(Number($(this).attr("id")));
                })
                if (list_answer.length > 0) $.post("/Question/DeleteListAnswer", { list: list_answer });
                q_block.find(".TableStyle").parent().parent().remove();
            }
            if (type_question == 6) {
                var str_path = q_block.find(".AnswerBlock").find(".AnswerSettings").find("p[name=path_file]").text();
                $.post("/Group/DeleteFile", { path: str_path });
            }
            q_block.find('.PanelSettings').empty();
            q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val("1");
            LoadSettingsSingle(q_block.find('.AnswerSettings'));
        }
            break;
        case 'Multi': {
            var type_question = Number(q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val());
            if (type_question == 1 || type_question == 3) {
                q_block.find(".AnswerBlock").find(".AnswerItem").each(function () {
                    $(this).find('div:first').find('input').attr("type", "checkbox");
                })
                if (type_question == 1) {
                    if (q_block.find('.TransitionListDiv').find('.TransItem').length > 0) {
                        $.post("/Question/deleteAllTransition", { id_question: id_question })
                    }
                }
                if (type_question == 3) {
                    if (q_block.find('.PanelOfRange').find('.RangeItem').length > 0) {
                        console.log("ID Question ---- ", id_question);
                        $.post("/Question/DeletaAllRangeQ", { id_question: id_question });
                    }
                }
            }
            if (type_question == 4) {
                $.post("/Question/DeleteAllTableRow", { id_q: id_question })
                var list_answer = [];
                q_block.find(".TableStyle").find(".AnswerTableLine").find(".TextAnswerDiv").each(function () {
                    list_answer.push(Number($(this).attr("id")));
                })
                if (list_answer.length > 0) $.post("/Question/DeleteListAnswer", { list: list_answer });
                q_block.find(".TableStyle").parent().parent().remove();
            }
            if (type_question == 6) {
                var str_path = q_block.find(".AnswerBlock").find(".AnswerSettings").find("p[name=path_file]").text();
                $.post("/Group/DeleteFile", { path: str_path });
            }
            q_block.find('.PanelSettings').empty();
            q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val("2");
            var query = {
                Id: id_question,
                IsKvot: false,
                IsRotate: false,
                LimitCount: null
            }
            LoadSettingsMilti(q_block.find('.AnswerSettings'), query);
        }
            break;
        case 'Free': {
            var type_question = Number(q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val());
            if (type_question == 1 || type_question == 2) {
                q_block.find(".AnswerBlock").find(".AnswerItem").each(function () {
                    $(this).find('div:first').find('input').attr("type", "text").attr("placeholder", "Текстовый блок");
                })
                if (q_block.find('.TransitionListDiv').find('.TransItem').length > 0) {
                    $.post("/Question/deleteAllTransition", { id_question: id_question });
                }
                if (q_block.find('.BlockingListDiv').find('.BlockItem').length > 0) {
                    $.post("/Question/deleteAllBlocks", { id_question: id_question });
                }
            }
            if (type_question == 4) {
                $.post("/Question/DeleteAllTableRow", { id_q: id_question })
                var list_answer = [];
                q_block.find(".TableStyle").find(".AnswerTableLine").find(".TextAnswerDiv").each(function () {
                    list_answer.push(Number($(this).attr("id")));
                })
                if (list_answer.length > 0) $.post("/Question/DeleteListAnswer", { list: list_answer });
                q_block.find(".TableStyle").parent().parent().remove();
            }
            if (type_question == 6) {
                var str_path = q_block.find(".AnswerBlock").find(".AnswerSettings").find("p[name=path_file]").text();
                $.post("/Group/DeleteFile", { path: str_path });
            }
            q_block.find('.PanelSettings').empty();
            q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val("3");
            LoadSettingsFree(q_block.find('.AnswerSettings'));
        }
            break;
        case 'Table': {
            var type_question = Number(q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val());
            q_block.find('.AnswerBlock').find('.AnswerItem').remove();
            if (type_question == 3 || type_question == 2) {
                q_block.find(".AnswerBlock").find(".AnswerItem").each(function () {
                    $(this).find('div:first').find('input').attr("type", "radio");
                })
                if (type_question == 2) {
                    if (q_block.find('.BlockingListDiv').find('.BlockItem').length > 0) {
                        $.post("/Question/deleteAllBlocks", { id_question: id_question })
                    }
                }
                if (type_question == 3) {
                    if (q_block.find('.PanelOfRange').find('.RangeItem').length > 0) {
                        console.log("ID Question ---- ", id_question);
                        $.post("/Question/DeletaAllRangeQ", { id_question: id_question });
                    }
                }
            }
            if (type_question == 6) {
                var str_path = q_block.find(".AnswerBlock").find(".AnswerSettings").find("p[name=path_file]").text();
                $.post("/Group/DeleteFile", { path: str_path });
            }
            q_block.find('.AnswerSettings').empty();
            q_block.find('.PanelSettings').empty();
            q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val("4");
            LoadSettingsTable(q_block.find('.AnswerSettings'));
        }
            break;
        case 'Text': {
            var type_question = Number(q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val());
            if (type_question == 6) {
                var str_path = q_block.find(".AnswerBlock").find(".AnswerSettings").find("p[name=path_file]").text();
                $.post("/Group/DeleteFile", { path: str_path });
            }
            q_block.find('.AnswerSettings').empty();
            q_block.find('.PanelSettings').empty();
            q_block.find('.AnswerBlock').find('.AnswerItem').each(function () {
                $(this).remove();
            })
            q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val("5");
        }
            break;
        case 'Filter': {
            var type_question = Number(q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val());
            if (type_question == 3 || type_question == 2 || type_question == 1) {
                if (type_question == 2) {
                    if (q_block.find('.BlockingListDiv').find('.BlockItem').length > 0) {
                        $.post("/Question/deleteAllBlocks", { id_question: id_question })
                    }
                }
                if (type_question == 1) {
                    if (q_block.find('.TransitionListDiv').find('.TransItem').length > 0) {
                        $.post("/Question/deleteAllTransition", { id_question: id_question })
                    }
                    if (q_block.find('.BlockingListDiv').find('.BlockItem').length > 0) {
                        $.post("/Question/deleteAllBlocks", { id_question: id_question })
                    }
                }
                if (type_question == 3) {
                    if (q_block.find('.PanelOfRange').find('.RangeItem').length > 0) {
                        console.log("ID Question ---- ", id_question);
                        $.post("/Question/DeletaAllRangeQ", { id_question: id_question });
                    }
                }
            }
            if (type_question == 4) {
                $.post("/Question/DeleteAllTableRow", { id_q: id_question })
                var list_answer = [];
                q_block.find(".TableStyle").find(".AnswerTableLine").find(".TextAnswerDiv").each(function () {
                    list_answer.push(Number($(this).attr("id")));
                })
                if (list_answer.length > 0) $.post("/Question/DeleteListAnswer", { list: list_answer });
                q_block.find(".TableStyle").parent().parent().remove();
            }
            q_block.find('.AnswerSettings').empty();
            q_block.find('.PanelSettings').empty();
            q_block.find('div[name=QuestionInfo]').find('input[name=questionType]').val("6");
            LoadSettingsFilter(q_block.find('.AnswerSettings'));
        }
            break;
        default:
            alert("Other")
            break;
    }

   
}

//Функиция загрузки настроек для Single вопросов
function LoadSettingsSingle(panel, q_data) {
    panel.empty();
    panel.append("<div class='AddAnswerSingle StyleButton' style='height: 100%;'>&#10010; Ответ</div>");
    panel.append("<div class='AddBaseAnswer StyleButton' style='width: 120px; background-color: #81c784; height: 100%;'>&#10010; Базовый ответ</div>")
    panel.append("<div class='ButtonSlideSettings StyleButton' style='height: 100%'>Настройки</div>");
    var block = panel.parent().find('.PanelSettings');
    block.append('<div class="QuotaDiv" style="width: 100%; display: inline-flex; background-color: #008dd2; padding: 5px;"><div class="TextSettingPanel">Квотирование</div><div class="QYes">Да</div><div class="QNo QOn">Нет</div></div>' +
        '<div class="RotateDiv" style="width: 100%; display: inline-flex; background-color: #008dd2; padding: 5px;"><div class="TextSettingPanel">Ротирование</div><div class="QYes">Да</div><div class="QNo QOn">Нет</div></div>' +
        '<div class="TransitionDiv" style="width: 100%; display: block; background-color: #008dd2; padding: 5px; border: 5px inset #b0bec5;">' +
        '<div style="width: 100%"><div class="StyleButton AddTransition" style="background-color: #81c874;">&#10010; Переход</div>' +
        '<div class="StyleButton DeleteAllTrans" style="background-color: #f44336;">Удалить все</div>' +
        '</div>' +
        '<div class="TransitionListDiv" style="width: 100%; display: block; background-color: #008dd2; padding: 5px; border: 1px solid black;"></div>' +
        '</div > ' +
        '<div class="BlockingDiv" style="width: 100%; display: block; background-color: #008dd2; padding: 5px; border: 5px inset #b0bec5;">' +
        '<div class="StyleButton AddBlocking" style="background-color: #81c874;">&#10010; Блок</div>' +
        '<div class="StyleButton ClearBlocking" style="background-color: #f44336;">Удалить все</div>' +
        '<div class="BlockingListDiv"></div>' +
        '</div>'

    );
    if (q_data != undefined) {
        if (q_data.IsKvot) {
            block.find('.QuotaDiv').find('.QOn').removeClass('QOn');
            block.find('.QuotaDiv').find('.QYes').addClass('QOn');
        }
        if (q_data.IsRotate) {
            block.find('.RotateDiv').find('.QOn').removeClass('QOn');
            block.find('.RotateDiv').find('.QYes').addClass('QOn');
        }
        $.get("/Question/getListTransitionQuestion", { id_q: q_data.Id })
            .success(function (tr_data) {
                if (tr_data.length > 0) {
                    $.each(tr_data, function (i, item) {
                        block.find(".TransitionListDiv").append('<div class="TransItem" id="' + item.Id + '" style="display: inline-flex; width: 100%;">' +
                            '<div style="display: inline-flex; width: 30%;"><div class="fromQuestion" style="margin-right: 20px;">Преход от </div><div>' + item.fromQuestion + '</div></div>' +
                            '<div style="display: inline-flex; width: 30%;"><div class="toQuestion" style="margin-right: 20px;">Переход к </div><div>' + item.toQuestion + '</div></div>' +
                            '<div style="display: inline-flex; width: 30%;"><div class="TriggerAnswer" style="margin-right: 20px;">По вопросу </div><div>' + item.TriggerAnswer + '</div></div>' +
                            '<div class="StyleButton DeleteTrans" style="width: 30px; height: 30px; background-color: #f44336;">&#10006;</div>' +
                            '</div>')
                        $('.AddAnswer').find('.AnswerItem[id=' + item.TriggerAnswer + ']').append('<div class="isTransition" style="width:10px;height:10px;border-radius:10px;background-color:#71c874;"></div>');
                    })
                }
            })
        $.get("/Question/getListBlocksQuestion", { id_q: q_data.Id })
            .success(function (bl_data) {
                if (bl_data.length > 0) {
                    $.each(bl_data, function (i, item) {
                        block.find(".BlockingListDiv").append('<div class="BlockItem" id="' + item.Id + '">' +
                            '<div style="display: inline-flex; width: 30%;"><div class="toQuestion" style="margin-right: 20px;">Блокировка по вопросу </div><div>' + item.toQuestion + '</div></div>' +
                            '<div style="display: inline-flex; width: 60%;"><div class="TypeBlocking" style="margin-right: 20px;">Тип блокировки </div><div>' +
                            (item.typeBlock == 1 ? "Блокировка выбранных ответов" : "Блокировка не выбраннх ответов") + '</div></div>' +
                            '<div class="StyleButton DeleteBlock" style="width: 30px; height: 30px; background-color: #f44336;">&#10006;</div>' +
                            '</div>'
                        )
                    })
                }
            })
    }
}

//Функция загрузки настроек для Multi вопросов
function LoadSettingsMilti(panel, q_data) {
    panel.empty();
    panel.append("<div class='AddAnswerMulti StyleButton' style='height: 100%;'>&#10010; Ответ</div>");
    panel.append("<div class='AddBaseAnswer StyleButton' style='width: 120px; background-color: #81c784; height: 100%;'>&#10010; Базовый ответ</div>");
    panel.append("<div class='ButtonSlideSettings StyleButton' style='height: 100%;'>Настройки</div>");
    var block = panel.parent().find('.PanelSettings');
    var len_trans_item = block.find('.TransitionListDiv').find('.TransItem').length;
    var len_block_item = block.find('.BlockingListDiv').find('.BlockItem').length;
    console.log("Length --- ", len_trans_item);
    block.append(
        '<div class="RotateDiv" style="width: 100%; display: inline-flex; background-color: #008dd2; padding: 5px;"><div class="TextSettingPanel">Ротирование</div><div class="QYes">Да</div><div class="QNo QOn">Нет</div></div>' +
        '<div class="LimitCountAnswer" style="width: 100%; display: inline-flex; background-color: #008dd2; padding: 5px;"><div class="TextSettingPanel">Ограничение выбора ответов</div><div class="CountLimit"><input type="text" style="width: 50px;"/></div></div>' +
        '<div class="BlockingDiv" style="width: 100%; display: block; background-color: #008dd2; padding: 5px; border: 5px inset #b0bec5;">' +
        '<div class="StyleButton AddBlocking" style="background-color: #81c874;">&#10010; Блок</div>' +
        '<div class="StyleButton ClearBlocking" style="background-color: #f44336;">Удалить все</div>' +
        '<div class="BlockingListDiv"></div>' +
        '</div>'
    );
    if (q_data != undefined) {
        if (q_data.IsRotate) {
            block.find('.RotateDiv').find('.QOn').removeClass('QOn');
            block.find('.RotateDiv').find('.QYes').addClass('QOn');
        }
        block.find('.LimitCountAnswer').find('.CountLimit').find('input').val(q_data.LimitCount);
        $.get("/Question/getListBlocksQuestion", { id_q: q_data.Id })
            .success(function (bl_data) {
                if (bl_data.length > 0) {
                    $.each(bl_data, function (i, item) {
                        block.find(".BlockingListDiv").append('<div class="BlockItem" id="' + item.Id + '">' +
                            '<div style="display: inline-flex; width: 30%;"><div class="toQuestion" style="margin-right: 20px;">Блокировка по вопросу </div><div>' + item.toQuestion + '</div></div>' +
                            '<div style="display: inline-flex; width: 60%;"><div class="TypeBlocking" style="margin-right: 20px;">Тип блокировки </div><div>' +
                            (item.typeBlock == 1 ? "Блокировка выбранных ответов" : "Блокировка не выбраннх ответов") + '</div></div>' +
                            '<div class="StyleButton DeleteBlock" style="width: 30px; height: 30px; background-color: #f44336;">&#10006;</div>' +
                            '</div>'
                        )
                    })
                }
            })
    }
}

//Функция загрузки настроек для Free вопросов
function LoadSettingsFree(panel, q_data) {
    panel.empty();
    panel.append("<div class='AddAnswerFree StyleButton' style='height: 100%;'>&#10010; Ответ</div>");
    panel.append("<div class='ButtonSlideSettings StyleButton' style='height: 100%;'>Настройки</div>");
    var block = panel.parent().find('.PanelSettings');
    block.append(
        '<div class="QuotaDiv" style="width: 100%; display: none; background-color: #008dd2; padding: 5px;"><div class="TextSettingPanel">Квотирование</div><div class="QYes">Да</div><div class="QNo QOn">Нет</div></div>' +
        '<div class="MasskDiv" style="width: 100%; display: inline-flex; background-color: #008dd2; padding: 5px;"><div class="TextSettingPanel">Тип масски текста</div><div class="TypeMassk"><select class="CheckTypeMassk" required>' +
        '<option value="" hidden>Выберите маску</option>' +
        '<option value="1">Возраст</option>' +
        '<option value="2">Число</option>' +
        '</select></div></div>' +
        '<div class="RangeDiv" style="width: 100%; display: none;">' +
        '<div class="SettingsRange" style="width: 100%; display: inline-flex; background-color: #b0bec5;">' +
        '<input class="LowerLimit" type="text" style="width: 50px;" />-<input class="UpperLimit" type="text" style="width: 50px;" /><div class="StyleButton AddRange" style="background-color: #81c784;">Добавить</div>' +
        '<div class="StyleButton DeleteListRange" style="background-color: #f44336;">Удалить все</div>' +
        '<div class="StyleButton SaveRangeList" style="background-color: #81c784;">Сохранить</div>' +
        '</div>' +
        '<div class="PanelOfRange" style="display: flex; flex-direction: column; align-items: flex-start; border: 5px inset #b0bec5;"></div>' +
        '</div>'
    );
    if (q_data != undefined) {
        if (q_data.IsRotate) {
            block.find('.QuotaDiv').find('.QOn').removeClass('QOn');
            block.find('.QuotaDiv').find('.QYes').addClass('QOn');
        }
        if (q_data.TypeMassk == 1) {
            block.find('.QuotaDiv').css("display", "inline-flex");
            if (q_data.IsKvot) {
                block.find('.QuotaDiv').find('.QOn').removeClass('QOn');
                block.find('.QuotaDiv').find('.QYes').addClass('QOn');
                block.find('.Q')
            }
            block.find('.RangeDiv').slideDown(200);
        }
        var type_massk = q_data.TypeMassk;
        block.find(".MasskDiv").find(".CheckTypeMassk").find("option[value='" + type_massk + "']").attr("selected", "selected");
        var project_id = q_data.ProjectID;
        var question_id = q_data.Id;
        console.log(project_id + " - " + question_id);
        $.get("/Question/GetRange", { id_p: project_id, bind_q: question_id })
            .success(function (r_data) {
                if (r_data != null && r_data != undefined) {
                    var code = "";
                    $.each(r_data, function (i, item) {
                        code += '<div class="RangeItem" style="display: inline-flex; justify-content: center; align-items: center; flex-direction: row; align-content: space-between;"><div class="IndexRange" id="' +
                            item.IndexRange + '">' + item.IndexRange + ')</div><div class="StringRange" style="margin-left: 5px;">' + item.RangeString +
                            '</div>' +
                            '<input type="hidden" name="RangeID" value="' + item.Id + '"></div>';
                    })
                    block.find(".PanelOfRange").append(code);
                }
            })
    }
}

//Функция загрузки настроек для Table вопросов
function LoadSettingsTable(panel, q_data) {
    var answer_block = panel.parents(".AnswerBlock")
    console.log("Length --- ", answer_block.find('.TableStyle').length)
    if (answer_block.find('.TableStyle').length > 0) {
        panel.append("<div class='AddColumnTable StyleButton' style='height: 100%; background-color: #81c784;'>&#10010;столбец</div>");
        panel.append("<div class='AddRowTable StyleButton' style='height: 100%; background-color: #81c784;'>&#10010;строку</div>");
        panel.append("<div class='ButtonSlideSettings StyleButton' style='height: 100%;'>Настройки</div>");
        var block = panel.parent().find('.PanelSettings');
        block.append(
            '<div class="BindQuestionDiv" style="width: 100%; display: inline-flex; background-color: #008dd2; padding: 5px;">' +
            '<div class="TextSettingPanel">Связать с вопросом</div>' +
            '<div class="BindYes StyleButton" style="width: 30px;">Да</div><div class="BindNo StyleButton QOn" style="width: 30px;">Нет</div>' +
            '</div>' +
            '<div class="OwnIndexDiv" style="width: 100%; display: inline-flex; background-color: #008dd2; padding: 5px;"><div class="TextSettingPanel">Задать свои индексы</div>' +
            '<div class="QYes">Да</div><div class="QNo QOn">Нет</div></div>' +
            '<div class="BlockingDiv" style="width: 100%; display: block; background-color: #008dd2; padding: 5px; border: 5px inset #b0bec5;">' +
            '<div class="StyleButton AddBlocking" style="background-color: #81c874;">&#10010; Блок</div>' +
            '<div class="StyleButton ClearBlocking" style="background-color: #f44336;">Удалить все</div>' +
            '<div class="BlockingListDiv"></div>' +
            '</div>'
        );
        if (q_data != undefined) {
            console.log("Question --- ", q_data);
            if (q_data.Bind != null) {
                var bind_id = Number(q_data.Bind)
                block.find(".BindQuestionDiv").find(".QOn").removeClass("QOn");
                block.find(".BindQuestionDiv").find(".BindYes").addClass("QOn");
                block.find(".OwnIndexDiv").find(".QOn").removeClass("QOn");
                block.find(".OwnIndexDiv").find(".QYes").addClass("QOn");
                var panel_bind = block.find(".BindQuestionDiv").find(".BindYes");
                panel_bind.after("<div class='BlockChain' style='background-color: #f44336; width:0px'></div>");
                panel_bind.next().animate({ width: "150px" }, 1000);
                var block = panel_bind.next();
                var code = "<select style='width: 100%; height: 100%;'>";
                $(".NewBlockQuestions").find(".NewClass").each(function () {
                    console.log("Bind >>>> " + bind_id);
                    if (bind_id == Number($(this).find(".ShowPanelQuestion").attr("id"))) code += "<option value='" + $(this).find(".ShowPanelQuestion").attr("id") + "' selected>" + $(this).find(".GroupName").text() + "</option>";
                    else code += "<option value='" + $(this).find(".ShowPanelQuestion").attr("id") + "'>" + $(this).find(".GroupName").text() + "</option>";
                })
                code += "</select>";
                block.append(code);
            } else {
                panel.parents(".AnswerBlock").find("div.InputOwnIndex").remove();
                ResizeItem(answer_block);
            }
            $.get("/Question/getListBlocksQuestion", { id_q: q_data.Id })
                .success(function (bl_data) {
                    if (bl_data.length > 0) {
                        $.each(bl_data, function (i, item) {
                            block.find(".BlockingListDiv").append('<div class="BlockItem" id="' + item.Id + '">' +
                                '<div style="display: inline-flex; width: 30%;"><div class="toQuestion" style="margin-right: 20px;">Блокировка по вопросу </div><div>' + item.toQuestion + '</div></div>' +
                                '<div style="display: inline-flex; width: 60%;"><div class="TypeBlocking" style="margin-right: 20px;">Тип блокировки </div><div>' +
                                (item.typeBlock == 1 ? "Блокировка выбранных ответов" : "Блокировка не выбраннх ответов") + '</div></div>' +
                                '<div class="StyleButton DeleteBlock" style="width: 30px; height: 30px; background-color: #f44336;">&#10006;</div>' +
                                '</div>'
                            )
                        })
                    }
                })
        }
    } else {
        panel.append("<div style='display: inline-flex;' class='SettingsBuildTable'>" +
            "<div style='display: flex; justify-content: center; align-items: center;' class='ColumnBuild'>Столбцов</div><input type='text' style='width: 40px;' />" +
            "<div style='display: flex; justify-content: center; align-items: center;' class='RowBuild'>Строк</div><input type='text' style='width: 40px;'>" +
            "<div class='StyleButton BuildTable' style='background-color: #81c784;'>Создать</div>" +
            "</div>");
    }
}

//Функция загрузки настроек для Filter вопросов 
function LoadSettingsFilter(panel, q_data) {

    if (q_data != undefined) {
        $.get("/Group/CheckFile", { id_q: q_data.Id })
            .success(function (f_data) {
                console.log("File --- ", typeof (f_data));
                if (f_data != null && f_data != "") {
                    panel.append("<div class='PathFile' style='width: 80%; height: 100%; display: inline-flex;'><div style='width: 20%; background-color: #008dd2; display: flex; justify-content: center; align-items: center;'>Путь файла</div>" +
                        "<div style='width: 70%; display: flex; justify-content: center; align-items: center;'><p name='path_file' style='white-space: nowrap; overflow: hidden; text-overflow: ellipsis;'>" + f_data + "</p></div></div>");
                    panel.append("<div class='StyleButton DeleteFile' style='background-color: #f44336; width: 30px; height: 30px; margin-left: 5px;'>&#10006;</div>")
                } else {
                    panel.append('<input type = "file" id="filter_file" multiple />');
                }
            })
    } else {
        panel.append('<input type = "file" id="filter_file" multiple />');
    }
}

//Функция перерисовки таблицы при её изменении
function ResizeItem(block) {
    var panel = block.find('.TableStyle');
    panel.find("tr:not(AnswerTableLine)").each(function () {
        var height = $(this).find(".TableRowText").height();
        $(this).find(".TypeItemTable").each(function () {
            $(this).find("div").css("height", height);
        })
    })
}

function sortFN(prop) {
    return function (a, b) {
        return a[prop] - b[prop];
    }
}

// Функция удаления блока собсвенных индексов
function OwnIndexPanelRemove(panel) {
    var block = panel.find(".AnswerBlock");
    block.find(".TableStyle").find(".InputOwnIndex").remove();
    ResizeItem(block);
}

// Функция добавления блока собственных индексов
function OwnIndexPanelBuild(panel) {
    var block = panel.find(".AnswerBlock");
    block.find(".TableRowText").each(function () {
        $(this).append("<div class='InputOwnIndex' style='display: inline-flex; width: 100%; background-color: #008dd2;'><div>Индекс</div><input type='text' name='OwnIndex' style='width: 100%;' /></div>");
    })
    $("input[name=OwnIndex]").bind("change keyup input", function () {
        if (this.value.match(RegExp("([0-9]{4,5})|(\\D)"))) {
            this.value = this.value.substr(0, this.value.length - 1);
        }

    });
    ResizeItem(block);
}

//Функция переприсвоения индексов для ответов
function ReplaceIndexAnswer(block, index) {
    var count = 0;
    block.find(".AnswerItem").each(function () {
        if (Number($(this).find("input[name=IndexAnswer]").val()) > Number(index)) {
            $(this).find("input[name=IndexAnswer]").val((index + count));
            count++;
        }
    })
}