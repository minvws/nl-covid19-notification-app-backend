function formater(str) {
    return str.replace(/"/g, '').replace(/,$/g, '')
}

function lab_comfirm_id(str){
    return str.replace(/-/g,'');
}

module.exports = {
    formater: formater,
    format_lab_confirm_id: lab_comfirm_id
}