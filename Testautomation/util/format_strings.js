function formater(str) {
    return str.replace(/"/g, '').replace(/,$/g, '')
}

module.exports = formater;