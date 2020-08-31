function formater(str) {
    return str.replace(/"/g, '').replace(/,$/g, '')
}

function lab_comfirm_id(str){
    return str.replace(/-/g,'');
}

module.exports = {
    formater: formater,
    format_confirmation_Id: lab_comfirm_id
}