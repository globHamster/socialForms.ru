function export2Doc(element, name) {
    var text_e = "";
    var count = element.find(".QuestionBlock").length;
    element.find(".QuestionBlock").each(function () {
        var textarea_list = $(this).find(".QuestionText").find('textarea');
        var textarea;
        $.each(textarea_list, function (i, item) {
            textarea = item;
        })
        var type_question = Number($(this).find("input[name=questionType]").val());
        var text_question = sceditor.instance(textarea).val().replace(/<\/?[^>]+>/g, '').replace(/[&nbsp;]/g, '');
        text_e += "<table style='width: 100%; border: 1px solid black;'><tr><td style='border: 1px solid black; width: 15%;'>" + $(this).find(".ShortNameQuestion").find("div:first").text();
        switch (type_question) {
            case 1: {
                text_e += "<br /> (Single)";
            }
                break;
            case 2: {
                text_e += "<br /> (Multi)";
            }
                break;
            case 3: {
                text_e += "<br /> (Free)";
            }
                break;
            case 4: {
                text_e += "<br /> (Table)";
            }
                break;
            case 5: {
                text_e += "<br /> (Text)";
            }
                break;
            case 6: {
                text_e += "<br /> (Filter)";
            }
                break;
            default:
                break;
        }
        text_e += "</td>";
        text_e += "<td style='width: 85%;'>" + text_question+"</td>"
      //  var type_question = Number($(this).find("input[name=questionType]").val());
        switch (type_question) {
            case 1: {
                text_e += "</tr><tr><td colspan='2'>";
                $(this).find(".AnswerItem").each(function () {
                    text_e += "<div>" +$(this).find(".IndexAnswer").text()+" " + $(this).find(".TextAnswer").text() + "</div>";
                })
                text_e += "</td>";
            }
                break;
            case 2: {
                text_e += "</tr><tr><td colspan='2'>";
                $(this).find(".AnswerItem").each(function () {
                    text_e += "<div>" + $(this).find(".IndexAnswer").text() + " "+ $(this).find(".TextAnswer").text() + "</div>";
                })
                text_e += "</td>";
            }
                break;
            case 3: {
                text_e += "</tr><tr><td colspan='2'>(Введите текст вручную) _____________________________</td>";
            }
                break;
            case 4: {
                text_e += "</tr><tr><td colspan='2'>";
                text_e += "<table>";
                $(this).find(".TableStyle").find("tr").each(function () {
                    text_e += "<tr>"
                    if ($(this).hasClass("AnswerTableLine")) {
                        $(this).find("td").each(function () {
                            if ($(this).hasClass("TableColumnText")) {
                                text_e += "<td  style='border-right: 1px solid black;border-bottom: 1px solid black; min-width: 100px;'>" + $(this).find(".TextAnswerDiv").text() + "</td>";
                            } else {
                                text_e += "<td style='border-right: 1px solid black;border-bottom: 1px solid black'></td>";
                            }
                        })
                    } else {
                        $(this).find("td").each(function () {
                            if ($(this).hasClass("TableRowText")) {
                                text_e += "<td style='border-right: 1px solid black;border-bottom: 1px solid black'>" + $(this).find(".TextTableRowDiv").text() + "</td>";
                            } else {
                                text_e += "<td style='border-right: 1px solid black;border-bottom: 1px solid black'></td>";
                            }
                        })
                    }
                    text_e += "</tr>";
                })
                text_e += "</table>";
                text_e += "</td>";
            }
                break;
            case 6: {
                text_e += "</tr><tr><td colspan='2'>";
                    text_e += "<div>" + $(this).find("p[name=path_file]").text() + "</div>";
                text_e += "</td>";
            }
                break;
            default:
                break;
        }
        text_e += "</tr></table>";
        text_e += "<br/>"
    })

    var html, link, blob, url, css;

    css = (
        '<style>' +
        '@page WordSection1{size: 841.95pt 595.35pt;mso-page-orientation: landscape;}' +
        'div.WordSection1 {page: WordSection1;}' +
        '</style>'
    );

    html = text_e;
    blob = new Blob(['\ufeff', css + html], {
        type: 'application/msword'
    });
    url = URL.createObjectURL(blob);
    link = document.createElement('A');
    link.href = url;
    link.download = 'Document/'+name;  // default name without extension 
    document.body.appendChild(link);
    if (navigator.msSaveOrOpenBlob) navigator.msSaveOrOpenBlob(blob, 'Document.doc'); // IE10-11
    else link.click();  // other browsers
    document.body.removeChild(link);

}