function remove_characters(str) {
    if(str != null){
        return str.replace(/"/g, '').replace(/,$/g, '')
    }else {
        return str = '';
    }

}

function remove_dash_lab_comfirm_id(str){
    if(str != null){
        return str.replace(/-/g,'')
    }else {
        return str = '';
    }

}

module.exports = {
    format_remove_characters: remove_characters,
    format_remove_dash: remove_dash_lab_comfirm_id
}