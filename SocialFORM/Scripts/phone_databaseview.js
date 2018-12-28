let form_number_massive, backup_form_number_massive;
let is_filtred = false;
let lst_form_numb;

function Init() {
    $.get("/Phone/getListFO")
        .success(function (server_data) {
            if (server_data != null) {
                $.each(server_data, function (i, item) {
                    $('select[name=ListFO]').append("<option value='" + item.KodFO + "'>" + item.NameFO + "</option>")
                })
            }
        })

    $.get("/Phone/GetInfoNumber")
        .success(function (server_data) {
            lst_form_numb = server_data;
            $('div.NotLoader').removeClass('NotLoader');
        })

    $('input[name=TestImport]').change(function (e) {

        var file_elem = $(this);
        var files = e.target.files;
        var name_file;
        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                for (var x = 0; x < files.length; x++) {
                    data.append("file" + x, files[x]);
                    var file = files[x];
                    name_file = file.name;
                }
                //data.append("name_table", $('.IsCheckedTR').attr('name'));

                //var table_name = $('.IsCheckedTR').attr('id');
                $.ajax({
                    type: "POST",
                    url: "/Phone/TestImport",
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function () {
                        alert("Success");
                    }
                })
            } else {
                alert("This browser doesn't support HTML5 file uploads!");
            }
        }

    })
}

function GetFilePhone() {
    var KodFO = $('select[name=ListFO]').find('option:selected').val();
    var KodOB = $('select[name=ListOB]').find('option:selected').val();
    var KodGOR = $('select[name=ListGOR]').find('option:selected').val();
    document.location.href = "/Phone/ImportFile?FO=" + KodFO + "&OB=" + KodOB + "&GOR=" + KodGOR;
}

function WriteTable(container, mass, page_iter) {
    container.find('.item_form_numb').remove()
    var length_lst = mass.length;
    var page_length = Math.ceil(length_lst / 30);
    $('div[name=count_page]').text("/" + page_length);
    var limit_iter = (page_iter * 30) > mass.length ? mass.length : (page_iter * 30);
    for (var i = (page_iter * 30) - 30; i < limit_iter; i++) {
        container.append("<div class='item_form_numb phone_form' onclick='LoadEditer($(this).find(\"div[name=Phone]\").text(), $(this).find(\"input\").val())'>" +
            "<input type='hidden' value='" + i + "' />" +
            "<div name='NP'>" + mass[i].NP + "</div>" +
            "<div name='VGOR'>" + mass[i].VGOR + "</div>" +
            "<div name='Phone'>" + mass[i].Phone + "</div>" +
            "<div name='Sex'>" + mass[i].Sex + "</div>" +
            "<div name='Age'>" + mass[i].Age + "</div>" +
            "<div name='Address'>" + mass[i].Address + "</div>" +
            "</div>")
    }
}

function LoadEditer(numb_phone, iter) {
    $.get("/Phone/GetFormNumber", { numb: numb_phone })
        .success(function (server_data) {
            $('body').append("<div class='block_panel_editer' style='width:100%; height: 100%; position: fixed; left:0;top:0;background: rgba(100,100,100,0.5); display: flex;justify-content: center;align-content:space-between;align-items:center;flex-direction:column;'>" +
                "<div class='panel_editer'>" +
                "<div class='close_panel_editer' onclick='$(this).parents(\".block_panel_editer\").remove()'>X</div>" +
                "<input type='hidden' name='hidden_iter' value='" + iter + "'/>" +
                "<input type='hidden' name='hidden_FO' value='" + server_data.FO + "'' />" +
                "<input type='hidden' name='hidden_OB' value='" + server_data.OB + "'' />" +
                "<input type='hidden' name='hidden_GOR' value='" + server_data.GOR + "'' />" +
                "<input type='hidden' name='hidden_Phone' value='" + server_data.Phone + "'' />" +
                "<div class='editer_form_numb'>" +
                "<div name='rename_NP' class='col1_grid'>Населенный пункт</div><div name='text_rename_NP' class='col2_grid'><input type='text' value='" + server_data.NP + "'/></div>" +
                "<div name='rename_VGOR' class='col1_grid'>Внутигородской район</div><div name='text_rename_VGOR' class='col2_grid'><input type='text' value='" + server_data.VGOR + "'/></div>" +
                "<div name='rename_Sex' class='col1_grid'>Пол</div><div name='text_rename_Sex' class='col2_grid'><input type='text' value='" + server_data.Sex + "'/></div>" +
                "<div name='rename_Age' class='col1_grid'>Возрост</div><div name='text_rename_Age' class='col2_grid'><input type='text' value='" + server_data.Age + "'/></div>" +
                "<div name='rename_Address' class='col1_grid'>Адрес</div><div name='text_rename_Address' class='col2_grid'><input type='text' value='" + server_data.Address + "'/></div>" +
                "<div name='rename_Education' class='col1_grid'>Образование</div><div name='text_rename_Education' class='col2_grid'><input type='text' value='" + server_data.Education + "'/></div>" +
                "</div>" +
                "<div style='width:100px;height:30px;background:#F00;' onclick='SaveChangeFormNumb()'></div>" +
                "</div>" +
                "</div>");
        })

}

function SaveChangeFormNumb() {
    var container = $('body').find('div.panel_editer');
    var iter_update = Number(container.find('input[name=hidden_iter]').val());
    console.log("Save > ", container.find('div[name=text_rename_VGOR]').find('input').val());
    var query = {
        FO: container.find('input[name=hidden_FO]').val(),
        OB: container.find('input[name=hidden_OB]').val(),
        GOR: container.find('input[name=hidden_GOR]').val(),
        NP: container.find('div[name=text_rename_NP]').find('input').val() != "" ? container.find('div[name=text_rename_NP]').find('input').val() : " ",
        VGOR: container.find('div[name=text_rename_VGOR]').find('input').val() != "" ? container.find('div[name=text_rename_VGOR]').find('input').val() : " ",
        Phone: container.find('input[name=hidden_Phone]').val(),
        Sex: container.find('div[name=text_rename_Sex]').find('input').val() != "" ? container.find('div[name=text_rename_Sex]').find('input').val() : " ",
        Age: container.find('div[name=text_rename_Age]').find('input').val() != "" ? Number(container.find('div[name=text_rename_Age]').find('input').val()) : " ",
        Address: container.find('div[name=text_rename_Address]').find('input').val() != "" ? container.find('div[name=text_rename_Address]').find('input').val() : " ",
        Education: container.find('div[name=text_rename_Education]').find('input').val() != "" ? container.find('div[name=text_rename_Education]').find('input').val() : " "
    };
    $.post("/Phone/SetFormNumber", { _fn: query })
        .success(function (code) {
            if (code == 200) {
                $('body').find('div.block_panel_editer').remove()
                form_number_massive[iter_update] = query;
                var paging_number = Number($('div[name=paging_number]').attr('id'));
                WriteTable($('div[name=content_table]'), form_number_massive, paging_number);
            }
            if (code == 0) {
                alert("error");
            }
        })
        .error(function () {
            alert("ERROR");
        })
}

function ChangePage(page_numb) {
    page_numb = jQuery.isNumeric(page_numb) ? page_numb : 1;
    var page_length = Math.ceil(form_number_massive.length / 30);
    if (page_numb > page_length) {
        page_numb = page_length;
    }
    if (page_numb < 1) {
        page_numb = 1;
    }
    $('div[name=paging_number]').attr('id', page_numb);
    $('div[name=paging]').find('input').val(page_numb);
    WriteTable($('div[name=content_table]'), form_number_massive, page_numb);
}

function ChangeFO(code) {
    $('select[name=ListOB]').find('option:not(:disabled)').remove();
    $('select[name=ListOB]').find('option:disabled').prop('selected', true);
    $('select[name=ListGOR]').find('option:not(:disabled)').remove();
    $('select[name=ListGOR]').find('option:disabled').prop('selected', true);
    $.get("/Phone/getListOB", { code: code })
        .success(function (server_data) {
            if (server_data != null && server_data.length != 0) {
                $.each(server_data, function (i, item) {
                    $('select[name=ListOB]').append("<option value='" + item.KodOB + "'>" + item.NameOB + "</option>");
                })
            }
        })
}

function ChangeOB(codeFO, codeOB) {
    $('select[name=ListGOR]').find('option:not(:disabled)').remove();
    $('select[name=ListGOR]').find('option:disabled').prop('selected', true);
    $.get("/Phone/getListGOR", { codeFO: codeFO, codeOB: codeOB })
        .success(function (server_data) {
            if (server_data != null && server_data.length != 0) {
                $.each(server_data, function (i, item) {
                    $('select[name=ListGOR]').append("<option value='" + item.KodGOR + "'>" + item.NameGOR + "</option>");
                })
            }
        })
}

function BeginSearch() {
    var KodFO = $('select[name=ListFO]').find('option:selected').val();
    var KodOB = $('select[name=ListOB]').find('option:selected').val();
    var KodGOR = $('select[name=ListGOR]').find('option:selected').val();
    $.get("/Phone/getListFormNumber", { KodFO: KodFO, KodOB: KodOB, KodGOR: KodGOR })
        .success(function (server_data) {
            $('div[name=paging_number]').attr('id', 1);
            $('div[name=paging]').find('input').val(1);
            var paging_number = Number(1);
            form_number_massive = server_data.clone();
            backup_form_number_massive = server_data.clone();
            WriteTable($('div[name=content_table]'), form_number_massive, paging_number);
            $('div[name=second_panel_filter]').css("display", "inline-flex");
            var lst_NP = {};
            var lst_VGOR = {};
            $.each(form_number_massive, function (i, item) {
                if (item.NP != null && item.NP != "" && item.NP != " ") {
                    lst_NP[item.NP] = item.NP;
                }
            })
            var container_NP = $('div[name=second_panel_filter]').find('div[name=filter_NP]').find('select');
            container_NP.find('option:not([value=0])').remove();
            $.each(lst_NP, function (i, item) {
                container_NP.append("<option value='" + item + "'>" + item + "</option>");
            })
        })
        .error(function (xhr, ajaxOptions, thrownError) {
            console.log(xhr);
            console.log(thrownError);
            console.log(ajaxOptions)
        })
}

function FilterSearch() {
    is_filtred = true;
}

function CheckFiltredItem() {
    form_number_massive = backup_form_number_massive.clone();
    let _filter_NP = $('div[name=second_panel_filter]').find('div[name=filter_NP]').find('option:selected').val();
    let _filter_VGOR = $('div[name=second_panel_filter]').find('div[name=filter_VGOR]').find('option:selected').val();
    let _sex = $('div[name=second_panel_filter]').find('div[name=filter_Sex]').find('option:selected').val();
    let _age_from = $('div[name=second_panel_filter]').find('input[name=text_age_from]').val();
    let _age_to = $('div[name=second_panel_filter]').find('input[name=text_age_to]').val();
    if (isNaN(Number(_age_from)) || _age_from == '') { _age_from = Number(0); }
    else { _age_from = Number(_age_from) }
    if (isNaN(Number(_age_to)) || _age_to == '') { _age_to = Number(150); }
    else { _age_to = Number(_age_to) }
    if (_age_from > _age_to) {
        let tmp = _age_from;
        _age_from = _age_to;
        _age_to = tmp;
    }
    $('div[name=second_panel_filter]').find('input[name=text_age_from]').val(_age_from);
    $('div[name=second_panel_filter]').find('input[name=text_age_to]').val(_age_to);

    var limit_mass = form_number_massive.length;
    for (var i = 0; i < limit_mass; i++) {
        if (_filter_NP != 0 && form_number_massive[i].NP != _filter_NP) {
            form_number_massive.splice(i, 1);
            limit_mass--;
            i--;
            continue;
        }
        if (_sex != 0 && String(form_number_massive[i].Sex).toLowerCase() != _sex.toLowerCase()) {
            form_number_massive.splice(i, 1);
            limit_mass--;
            i--;
            continue;
        }
        if (form_number_massive[i].Age < _age_from || form_number_massive[i].Age > _age_to) {
            form_number_massive.splice(i, 1);
            limit_mass--;
            i--;
            continue;
        }
    }
    $('div.TablePhone').find('.SortUp').removeClass('SortUp');
    $('div.TablePhone').find('.SortDown').removeClass('SortDown');
    $('div[name=paging_number]').attr('id', 1);
    $('div[name=paging]').find('input').val(1);
    WriteTable($('div[name=content_table]'), form_number_massive, 1);
}

function ResetMassive() {
    $('div.TablePhone').find('.SortUp').removeClass('SortUp');
    $('div.TablePhone').find('.SortDown').removeClass('SortDown');
    $('div[name=paging_number]').attr('id', 1);
    $('div[name=paging]').find('input').val(1);
    form_number_massive = backup_form_number_massive.clone();
    WriteTable($('div[name=content_table]'), form_number_massive, 1);
}


Array.prototype.clone = function () {
    return this.slice(0);
}

//BEGIN BLOCK "Sort"
function sortUP_String(option) {
    return function (a, b) {
        return a[option].toLowerCase().localeCompare(b[option].toLowerCase());
    }
}
function sortDOWN_String(option) {
    return function (a, b) {
        return (-1) * a[option].toLowerCase().localeCompare(b[option].toLowerCase());
    }
}
function sortUP_Number(option) {
    return function (a, b) {
        return a[option] - b[option];
    }
}
function sortDOWN_Number(option) {
    return function (a, b) {
        return -1 * (a[option] - b[option]);
    }
}
//END BLOCK

function SortToNP(container) {
    if (container.parents('.TablePhone').find('.phone_form').length == 0) {
        return;
    }
    $('div[name=paging_number]').attr('id', 1);
    $('div[name=paging]').find('input').val(1);
    if (!container.hasClass('SortUp') && !container.hasClass('SortDown')) {
        container.parent().find('.SortUp').removeClass('SortUp');
        container.parent().find('.SortDown').removeClass('SortDown');
        form_number_massive = form_number_massive.sort(sortUP_String("NP"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.addClass('SortUp');
        console.log("Empty");
        return;
    }
    if (container.hasClass('SortUp')) {
        form_number_massive = form_number_massive.sort(sortDOWN_String("NP"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.removeClass('SortUp').addClass('SortDown');
        console.log("SortUp");
        return;
    }
    if (container.hasClass('SortDown')) {
        form_number_massive = form_number_massive.sort(sortUP_String("NP"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.removeClass('SortDown').addClass('SortUp');
        return;
    }
}

function SortToSex(container) {
    if (container.parents('.TablePhone').find('.phone_form').length == 0) {
        return;
    }
    $('div[name=paging_number]').attr('id', 1);
    $('div[name=paging]').find('input').val(1);
    if (!container.hasClass('SortUp') && !container.hasClass('SortDown')) {
        container.parent().find('.SortUp').removeClass('SortUp');
        container.parent().find('.SortDown').removeClass('SortDown');
        form_number_massive = form_number_massive.sort(sortUP_String("Sex"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.addClass('SortUp');
        console.log("Empty");
        return;
    }
    if (container.hasClass('SortUp')) {
        form_number_massive = form_number_massive.sort(sortDOWN_String("Sex"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.removeClass('SortUp').addClass('SortDown');
        console.log("SortUp");
        return;
    }
    if (container.hasClass('SortDown')) {
        form_number_massive = form_number_massive.sort(sortUP_String("Sex"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.removeClass('SortDown').addClass('SortUp');
        return;
    }
}

function SortToAge(container) {
    if (container.parents('.TablePhone').find('.phone_form').length == 0) {
        return;
    }
    $('div[name=paging_number]').attr('id', 1);
    $('div[name=paging]').find('input').val(1);
    if (!container.hasClass('SortUp') && !container.hasClass('SortDown')) {
        container.parent().find('.SortUp').removeClass('SortUp');
        container.parent().find('.SortDown').removeClass('SortDown');
        form_number_massive = form_number_massive.sort(sortUP_Number("Age"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.addClass('SortUp');
        console.log("Empty");
        return;
    }
    if (container.hasClass('SortUp')) {
        form_number_massive = form_number_massive.sort(sortDOWN_Number("Age"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.removeClass('SortUp').addClass('SortDown');
        console.log("SortUp");
        return;
    }
    if (container.hasClass('SortDown')) {
        form_number_massive = form_number_massive.sort(sortUP_Number("Age"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.removeClass('SortDown').addClass('SortUp');
        return;
    }

}

function SortToPhone(container) {
    if (container.parents('.TablePhone').find('.phone_form').length == 0) {
        return;
    }
    $('div[name=paging_number]').attr('id', 1);
    $('div[name=paging]').find('input').val(1);
    if (!container.hasClass('SortUp') && !container.hasClass('SortDown')) {
        container.parent().find('.SortUp').removeClass('SortUp');
        container.parent().find('.SortDown').removeClass('SortDown');
        form_number_massive = form_number_massive.sort(sortUP_String("Phone"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.addClass('SortUp');
        console.log("Empty");
        return;
    }
    if (container.hasClass('SortUp')) {
        form_number_massive = form_number_massive.sort(sortDOWN_String("Phone"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.removeClass('SortUp').addClass('SortDown');
        console.log("SortUp");
        return;
    }
    if (container.hasClass('SortDown')) {
        form_number_massive = form_number_massive.sort(sortUP_String("Phone"));
        WriteTable($('div[name=content_table]'), form_number_massive, 1);
        container.removeClass('SortDown').addClass('SortUp');
        return;
    }
}

function Page_Move_Next() {
    var page = Number($('div[name=paging_number]').attr('id')) + 1;
    var page_length = Math.ceil(form_number_massive.length / 30);
    if (page > page_length) {
        return;
    }
    $('div[name=paging_number]').attr('id', page);
    $('div[name=paging]').find('input').val(page);
    WriteTable($('div[name=content_table]'), form_number_massive, page);
}

function Page_Move_Previos() {
    var page = Number($('div[name=paging_number]').attr('id')) - 1;
    var page_length = Math.ceil(form_number_massive.length / 30);
    if (page < 1) {
        return;
    }
    $('div[name=paging_number]').attr('id', page);
    $('div[name=paging]').find('input').val(page);
    WriteTable($('div[name=content_table]'), form_number_massive, page);
}

function GetInfoNumber() {
    $('body').append("<div class='wall_of_background' style='width:100%;height:100%;display:flex;justify-content:center;align-items:center;background: rgba(100,100,100,0.5); position: fixed;top:0;left:0;'>" +
        "<div style='width:80%;height:80%;border: 3px inset #008dd2;background-color:#fff;padding: 5px 10px;'>" +
        "<div style='width:30px;height:30px;background-color:red;position:relative;top:-20px;left:calc(100% - 5px);border-radius:15px;' onclick='$(this).parents(\".wall_of_background\").remove()'>X</div>" +
        "<div class='panel_info_numbers slide_panel' style='width:100%;height:95%;overflow-y:auto;padding: 0px 5px;'></div>" +
        "</div>" +
        "</div>")
    var container = $('div.panel_info_numbers');
    $('div.wall_of_background').keyup(function (event) {
        alert(event.keyCode);
    })


    RecusiveFunc(lst_form_numb, container);
    SlideUpAll(container);


}

function RecusiveFunc(mass, container) {
    if (jQuery.isPlainObject(mass) == false) return;
    else if (mass.length == 0) {
        container.append("<div>" +
            "<div class='block_info_text'><div class='text_end_count'>" + mass.name + "</div><div class='number_count'>" + mass.count + "</div></div>" +
            "</div>");
    }
    $.each(mass, function (i, item) {
        if (jQuery.isPlainObject(item) == true) {
            if (item.length != 0) {
                container.append("<div class='slide_panel'>" +
                    "<div class='block_info_text'><div class='btn_slide_panel' onclick='SlideSubPanel($(this).parents(\".slide_panel:first\").find(\".slide_panel:first\"))'></div><div class='text_count'>" + item.name + "</div><div class='number_count'>" + item.count + "</div></div>" +
                    "<div class='SlideDown slide_panel sub_panel" + i + "' style='padding-left:20px;'></div>" +
                    "</div>")
                var cont = container.find('div.sub_panel' + i + '');
                RecusiveFunc(item, cont);
            } else {
                RecusiveFunc(item, container);
            }
        }
    })
}

function SlideSubPanel(container) {
    if (container.hasClass('SlideDown')) {
        container.slideUp(200);
        container.removeClass('SlideDown').addClass('SlideUp')
    } else if (container.hasClass('SlideUp')) {
        container.slideDown(200);
        container.removeClass('SlideUp').addClass('SlideDown');
    }

}

function SlideUpAll(container) {
    container.find('div.SlideDown').each(function () {
        $(this).removeClass('SlideDown').addClass('SlideUp');
        $(this).slideUp(300);
    })
}  