$(function () {

    $("[validatetype]").on('keyup', function () {
        if (this.id != undefined) {
            Validate("#" + this.id);
        }
    }).on('blur', function () {
        if (this.id != undefined) {
            Validate("#" + this.id);
        }
    }).on('change', function () {
        if (this.id != undefined) {
            Validate("#" + this.id);
        }
    });

});


function Validate(selecter) {
    var isValidateSuccess = true;
    var validatType = $(selecter).attr("validatetype");

    if (validatType.indexOf(",") > 0) {
        var validatTypeList = validatType.split(',')
        for (var i = 0; i < validatTypeList.length; i++) {
            if (isValidateSuccess) {
                switch (validatTypeList[i]) {
                    case "required"://必填验证
                        isValidateSuccess = validate_Required(selecter);
                        break;
                    case "email"://邮箱验证
                        isValidateSuccess = validate_Email(selecter);
                        break;
                    case "phone"://验证手机号   
                        isValidateSuccess = validate_Phone(selecter);
                        break;
                    case "number"://整数验证
                        isValidateSuccess = validate_Number(selecter);
                        break;
                    case "decimalNumber"://整数或小数验证
                        isValidateSuccess = validate_decimalNumber(selecter);
                        break;
                    case "idCard"://身份证号验证
                        isValidateSuccess = validate_IdCard(selecter);
                        break;
                    case "length":
                        break;
                    case "english":
                        isValidateSuccess = validate_EN(selecter);
                        break;
                }
            }
        }
    }
    else {

        switch (validatType) {
            case "required":
                isValidateSuccess = validate_Required(selecter);
                break;
            case "email":
                isValidateSuccess = validate_Email(selecter);
                break;
            case "phone"://验证手机号    
                isValidateSuccess = validate_Phone(selecter);
                break;
            case "number":
                isValidateSuccess = validate_Number(selecter);
                break;
            case "decimalNumber"://整数或小数验证
                isValidateSuccess = validate_decimalNumber(selecter);
                break;
            case "money"://金额
                isValidateSuccess = validate_money(selecter);
                break;
            case "idCard"://身份证号验证
                isValidateSuccess = validate_IdCard(selecter);
                break;
            case "length":
                break;
            case "english":
                isValidateSuccess = validate_EN(selecter);
                break;
        }
    }
    return isValidateSuccess;
}

//验证必填
function validate_Required(selecter) {
    var isValidateSuccess = true;
    var val = $(selecter).val();

    var isAllEmpty = false;//是否输入的全部是空格

    var reg = /^[ ]+$/;
    isAllEmpty = reg.test(val);


    if (val == null || val == "" || isAllEmpty) {
        if (!$(selecter).hasClass("input_Validate")) {
            $(selecter).addClass("input_Validate");
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">必填！</div></div>");

        }
        else {
            $(selecter).next(".msg").remove();
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">必填！</div></div>");
        }
        isValidateSuccess = false;
    }
    else {
        isValidateSuccess = true;
        if ($(selecter).hasClass("input_Validate")) {
            $(selecter).removeClass("input_Validate");
            $(selecter).next(".msg").remove();

        }
    }
    return isValidateSuccess;
}

//验证邮箱
function validate_Email(selecter) {
    var isValidateSuccess = true;
    var reg = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;//邮箱格式
    var email = $(selecter).val();
    if (reg.test(email) && email.indexOf(" ") == -1) {
        isValidateSuccess = true;
        if ($(selecter).hasClass("input_Validate")) {
            $(selecter).removeClass("input_Validate");
            $(selecter).next(".msg").remove();

        }
    }
    else {
        if (!$(selecter).hasClass("input_Validate")) {
            $(selecter).addClass("input_Validate");
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">邮箱格式错误！</div></div>");
        }
        else {
            $(selecter).next(".msg").remove();
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">邮箱格式错误！</div></div>");

        }
        isValidateSuccess = false;
    }
    return isValidateSuccess;

}

//验证手机号
function validate_Phone(selecter) {
    var isValidateSuccess = true;
    var reg = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1})|(14[0-9]{1})|(17[0-9]{1}))+\d{8})$/;
    if (reg.test($(selecter).val())) {
        isValidateSuccess = true;
        if ($(selecter).hasClass("input_Validate")) {
            $(selecter).removeClass("input_Validate");
            $(selecter).next(".msg").remove();
        }
    }
    else {
        if (!$(selecter).hasClass("input_Validate")) {
            $(selecter).addClass("input_Validate");
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">手机号码格式错误！</div></div>");
        }
        else {
            $(selecter).next(".msg").remove();
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">手机号码格式错误！</div></div>");
        }
        isValidateSuccess = false;
    }
    return isValidateSuccess;
}



//验证整数数字（包含0）
function validate_Number(selecter) {
    var isValidateSuccess = true;
    var reg = /^[0-9]*$/;
    if (reg.test($(selecter).val())) {
        isValidateSuccess = true;
        if ($(selecter).hasClass("input_Validate")) {
            $(selecter).removeClass("input_Validate");
            $(selecter).next(".msg").remove();
        }
    }
    else {
        if (!$(selecter).hasClass("input_Validate")) {
            $(selecter).addClass("input_Validate");
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">请输入整数！</div></div>");
        }
        else {
            $(selecter).next(".msg").remove();
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">请输入整数！</div></div>");
        }
        isValidateSuccess = false;
    }
    return isValidateSuccess;
}
//验证整数数字或小数点数字
function validate_decimalNumber(selecter) {
    var isValidateSuccess = true;
    //var reg = /^[0-9]+([.]{1}[0-9]+){0,1}$/;
    var reg = /^(\-?)\d+(\.\d+)?$/;
    if (reg.test($(selecter).val())) {
        isValidateSuccess = true;
        if ($(selecter).hasClass("input_Validate")) {
            $(selecter).removeClass("input_Validate");
            $(selecter).next(".msg").remove();
        }
    }
    else {
        if (!$(selecter).hasClass("input_Validate")) {
            $(selecter).addClass("input_Validate");
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + "; width:150px;\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">请输入整数或者小数！</div></div>");
        }
        else {
            $(selecter).next(".msg").remove();
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";width:150px;\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">请输入整数或者小数！</div></div>");
        }
        isValidateSuccess = false;
    }
    return isValidateSuccess;
}
//验证金额
function validate_money(selecter) {
    var isValidateSuccess = true;
    //var reg = /(^[1-9]([0-9]+)?(\.[0-9]{1,2})?$)|(^(0){1}$)|(^[0-9]\.[0-9]([0-9])?$)/;
    var reg =/^-?([0-9]+|[0-9]{1,3}(,[0-9]{3})*)(.[0-9]{1,2})?$/
    if (reg.test($(selecter).val())) {
        isValidateSuccess = true;
        if ($(selecter).hasClass("input_Validate")) {
            $(selecter).removeClass("input_Validate");
            $(selecter).next(".msg").remove();
        }
    }
    else {
        if (!$(selecter).hasClass("input_Validate")) {
            $(selecter).addClass("input_Validate");
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + "; width:150px;\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">请输入正确金额！</div></div>");
        }
        else {
            $(selecter).next(".msg").remove();
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";width:150px;\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">请输入正确金额！</div></div>");
        }
        isValidateSuccess = false;
    }
    return isValidateSuccess;
}

//验证英文
function validate_EN(selecter) {
    var isValidateSuccess = true;
    var reg = /^[A-Za-z]+$/;
    if (reg.test($(selecter).val())) {
        isValidateSuccess = true;
        if ($(selecter).hasClass("input_Validate")) {
            $(selecter).removeClass("input_Validate");
            $(selecter).next(".msg").remove();
        }
    }
    else {
        if (!$(selecter).hasClass("input_Validate")) {
            $(selecter).addClass("input_Validate");
        }
        isValidateSuccess = false;
    }
    return isValidateSuccess;
}

//验证长度
function validate_Length(selecter) {

}


//验证身份证号
function validate_IdCard(selecter) {
    var isValidateSuccess = true;
    // 身份证号码为15位或者18位，15位时全为数字，18位前17位为数字，最后一位是校验位，可能为数字或字符X  
    var reg = /(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)/;
    if (reg.test($(selecter).val())) {
        isValidateSuccess = true;
        if ($(selecter).hasClass("input_Validate")) {
            $(selecter).removeClass("input_Validate");
            $(selecter).next(".msg").remove();
        }
    }
    else {
        if (!$(selecter).hasClass("input_Validate")) {
            $(selecter).addClass("input_Validate");
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">身份证输入不合法！</div></div>");
        }
        else {
            $(selecter).next(".msg").remove();
            var width = $(selecter).css("width");
            $(selecter).after("<div class=\"msg\" style=\"margin-left:" + width + ";\"><img class=\"messg_icon\" src=\"/_theme/Validate/img/triangle_left.png\" /><div class=\"messg_Validate\">身份证输入不合法！</div></div>");
        }
        isValidateSuccess = false;
    }
    return isValidateSuccess;
}






