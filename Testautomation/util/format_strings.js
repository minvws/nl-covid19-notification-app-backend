function remove_characters(str) {
    return str.replace(/"/g, '').replace(/,$/g, '')
}

function remove_dash_lab_comfirm_id(str){
    return str.replace(/-/g,'');
}

module.exports = {
    format_remove_characters: remove_characters,
    format_remove_dash: remove_dash_lab_comfirm_id
}