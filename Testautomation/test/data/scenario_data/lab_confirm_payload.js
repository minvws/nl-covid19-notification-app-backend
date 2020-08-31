const moment = require("moment");

const data = {
    payload: {
        valid_static: `{
            "labConfirmationId": "LABCONFIRMATIONID",
            "dateOfSymptomsOnset": "2020-07-23T16:11:19.487Z"
    }`,
        valid_dynamic: `{
            "labConfirmationId": "LABCONFIRMATIONID",
            "dateOfSymptomsOnset": "${generate_date(true,-14)}"
    }`,
        invalid_date: `{
            "labConfirmationId": "LABCONFIRMATIONID",
            "dateOfSymptomsOnset": "${generate_date(false)}"
    }`,future_date: `{
            "labConfirmationId": "LABCONFIRMATIONID",
            "dateOfSymptomsOnset": "${generate_date(true,+7)}"
    }`
        ,
        insufficient: "insufficient data for this scenario",
    },
};

function generate_date(valid,days) {

    days = days || '0';
    var date = moment()
        .add(days, 'days')
        .format('YYYY-MM-DDTHH:mm:ss.SSS')
        .toString();

    if (valid) {
        return date + 'Z';
    } else {
        return "2020-01-01";
    }
}

module.exports = data;