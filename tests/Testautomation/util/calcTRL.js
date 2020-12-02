const MillisecondsInDay = 1000*60*60*24;
const TransmissionRiskLevel = {
    None: 6,
    Low: 1,
    Medium: 2,
    High: 3
}
function calcTRL(rollingStartNumber, dateOfSymptomsOnset) {
    let rsnUnixEpoch = rollingStartNumber * 600;
    let rsnJsEpoch = rsnUnixEpoch * 1000;
    let rsnDate = new Date(rsnJsEpoch);
    let daysSinceSymptomOnset = Math.floor((rsnDate - dateOfSymptomsOnset) / MillisecondsInDay)
    if (daysSinceSymptomOnset <= -3) return TransmissionRiskLevel.None;
    if (daysSinceSymptomOnset <= -2) return TransmissionRiskLevel.Medium;
    if (daysSinceSymptomOnset <= 2) return TransmissionRiskLevel.High;
    if (daysSinceSymptomOnset <= 4) return TransmissionRiskLevel.Medium;
    if (daysSinceSymptomOnset <= 11) return TransmissionRiskLevel.Low;
    return TransmissionRiskLevel.None;
}

module.exports = calcTRL;