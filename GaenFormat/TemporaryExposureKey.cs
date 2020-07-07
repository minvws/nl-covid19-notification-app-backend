// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat
{
    public sealed partial class TemporaryExposureKey : IMessage<TemporaryExposureKey> {
        private static readonly MessageParser<TemporaryExposureKey> _parser = new MessageParser<TemporaryExposureKey>(() => new TemporaryExposureKey());
        private UnknownFieldSet _unknownFields;
        private int _hasBits0;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageParser<TemporaryExposureKey> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageDescriptor Descriptor {
            get { return global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKeyExportReflection.Descriptor.MessageTypes[2]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        MessageDescriptor IMessage.Descriptor {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TemporaryExposureKey() {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TemporaryExposureKey(TemporaryExposureKey other) : this() {
            _hasBits0 = other._hasBits0;
            keyData_ = other.keyData_;
            transmissionRiskLevel_ = other.transmissionRiskLevel_;
            rollingStartIntervalNumber_ = other.rollingStartIntervalNumber_;
            rollingPeriod_ = other.rollingPeriod_;
            reportType_ = other.reportType_;
            daysSinceOnsetOfSymptoms_ = other.daysSinceOnsetOfSymptoms_;
            _unknownFields = UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TemporaryExposureKey Clone() {
            return new TemporaryExposureKey(this);
        }

        /// <summary>Field number for the "key_data" field.</summary>
        public const int KeyDataFieldNumber = 1;
        private readonly static ByteString KeyDataDefaultValue = ByteString.Empty;

        private ByteString keyData_;
        /// <summary>
        /// Key of infected user
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public ByteString KeyData {
            get { return keyData_ ?? KeyDataDefaultValue; }
            set {
                keyData_ = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "key_data" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasKeyData {
            get { return keyData_ != null; }
        }
        /// <summary>Clears the value of the "key_data" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearKeyData() {
            keyData_ = null;
        }

        /// <summary>Field number for the "transmission_risk_level" field.</summary>
        public const int TransmissionRiskLevelFieldNumber = 2;
        private readonly static int TransmissionRiskLevelDefaultValue = 0;

        private int transmissionRiskLevel_;
        /// <summary>
        /// Varying risk associated with a key depending on diagnosis method
        /// </summary>
        [global::System.ObsoleteAttribute]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int TransmissionRiskLevel {
            get { if ((_hasBits0 & 1) != 0) { return transmissionRiskLevel_; } else { return TransmissionRiskLevelDefaultValue; } }
            set {
                _hasBits0 |= 1;
                transmissionRiskLevel_ = value;
            }
        }
        /// <summary>Gets whether the "transmission_risk_level" field is set</summary>
        [global::System.ObsoleteAttribute]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasTransmissionRiskLevel {
            get { return (_hasBits0 & 1) != 0; }
        }
        /// <summary>Clears the value of the "transmission_risk_level" field</summary>
        [global::System.ObsoleteAttribute]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearTransmissionRiskLevel() {
            _hasBits0 &= ~1;
        }

        /// <summary>Field number for the "rolling_start_interval_number" field.</summary>
        public const int RollingStartIntervalNumberFieldNumber = 3;
        private readonly static int RollingStartIntervalNumberDefaultValue = 0;

        private int rollingStartIntervalNumber_;
        /// <summary>
        /// The interval number since epoch for which a key starts
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int RollingStartIntervalNumber {
            get { if ((_hasBits0 & 2) != 0) { return rollingStartIntervalNumber_; } else { return RollingStartIntervalNumberDefaultValue; } }
            set {
                _hasBits0 |= 2;
                rollingStartIntervalNumber_ = value;
            }
        }
        /// <summary>Gets whether the "rolling_start_interval_number" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasRollingStartIntervalNumber {
            get { return (_hasBits0 & 2) != 0; }
        }
        /// <summary>Clears the value of the "rolling_start_interval_number" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearRollingStartIntervalNumber() {
            _hasBits0 &= ~2;
        }

        /// <summary>Field number for the "rolling_period" field.</summary>
        public const int RollingPeriodFieldNumber = 4;
        private readonly static int RollingPeriodDefaultValue = 144;

        private int rollingPeriod_;
        /// <summary>
        /// Increments of 10 minutes describing how long a key is valid
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int RollingPeriod {
            get { if ((_hasBits0 & 4) != 0) { return rollingPeriod_; } else { return RollingPeriodDefaultValue; } }
            set {
                _hasBits0 |= 4;
                rollingPeriod_ = value;
            }
        }
        /// <summary>Gets whether the "rolling_period" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasRollingPeriod {
            get { return (_hasBits0 & 4) != 0; }
        }
        /// <summary>Clears the value of the "rolling_period" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearRollingPeriod() {
            _hasBits0 &= ~4;
        }

        /// <summary>Field number for the "report_type" field.</summary>
        public const int ReportTypeFieldNumber = 5;
        private readonly static global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKey.Types.ReportType ReportTypeDefaultValue = global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKey.Types.ReportType.Unknown;

        private global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKey.Types.ReportType reportType_;
        /// <summary>
        /// Type of diagnosis associated with a key.
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKey.Types.ReportType ReportType {
            get { if ((_hasBits0 & 8) != 0) { return reportType_; } else { return ReportTypeDefaultValue; } }
            set {
                _hasBits0 |= 8;
                reportType_ = value;
            }
        }
        /// <summary>Gets whether the "report_type" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasReportType {
            get { return (_hasBits0 & 8) != 0; }
        }
        /// <summary>Clears the value of the "report_type" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearReportType() {
            _hasBits0 &= ~8;
        }

        /// <summary>Field number for the "days_since_onset_of_symptoms" field.</summary>
        public const int DaysSinceOnsetOfSymptomsFieldNumber = 6;
        private readonly static int DaysSinceOnsetOfSymptomsDefaultValue = 0;

        private int daysSinceOnsetOfSymptoms_;
        /// <summary>
        /// Number of days elapsed between symptom onset and the TEK being used.
        /// E.g. 2 means TEK is 2 days after onset of symptoms.
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int DaysSinceOnsetOfSymptoms {
            get { if ((_hasBits0 & 16) != 0) { return daysSinceOnsetOfSymptoms_; } else { return DaysSinceOnsetOfSymptomsDefaultValue; } }
            set {
                _hasBits0 |= 16;
                daysSinceOnsetOfSymptoms_ = value;
            }
        }
        /// <summary>Gets whether the "days_since_onset_of_symptoms" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasDaysSinceOnsetOfSymptoms {
            get { return (_hasBits0 & 16) != 0; }
        }
        /// <summary>Clears the value of the "days_since_onset_of_symptoms" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearDaysSinceOnsetOfSymptoms() {
            _hasBits0 &= ~16;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
            return Equals(other as TemporaryExposureKey);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(TemporaryExposureKey other) {
            if (ReferenceEquals(other, null)) {
                return false;
            }
            if (ReferenceEquals(other, this)) {
                return true;
            }
            if (KeyData != other.KeyData) return false;
            if (TransmissionRiskLevel != other.TransmissionRiskLevel) return false;
            if (RollingStartIntervalNumber != other.RollingStartIntervalNumber) return false;
            if (RollingPeriod != other.RollingPeriod) return false;
            if (ReportType != other.ReportType) return false;
            if (DaysSinceOnsetOfSymptoms != other.DaysSinceOnsetOfSymptoms) return false;
            return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
            int hash = 1;
            if (HasKeyData) hash ^= KeyData.GetHashCode();
            if (HasTransmissionRiskLevel) hash ^= TransmissionRiskLevel.GetHashCode();
            if (HasRollingStartIntervalNumber) hash ^= RollingStartIntervalNumber.GetHashCode();
            if (HasRollingPeriod) hash ^= RollingPeriod.GetHashCode();
            if (HasReportType) hash ^= ReportType.GetHashCode();
            if (HasDaysSinceOnsetOfSymptoms) hash ^= DaysSinceOnsetOfSymptoms.GetHashCode();
            if (_unknownFields != null) {
                hash ^= _unknownFields.GetHashCode();
            }
            return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
            return JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(CodedOutputStream output) {
            if (HasKeyData) {
                output.WriteRawTag(10);
                output.WriteBytes(KeyData);
            }
            if (HasTransmissionRiskLevel) {
                output.WriteRawTag(16);
                output.WriteInt32(TransmissionRiskLevel);
            }
            if (HasRollingStartIntervalNumber) {
                output.WriteRawTag(24);
                output.WriteInt32(RollingStartIntervalNumber);
            }
            if (HasRollingPeriod) {
                output.WriteRawTag(32);
                output.WriteInt32(RollingPeriod);
            }
            if (HasReportType) {
                output.WriteRawTag(40);
                output.WriteEnum((int) ReportType);
            }
            if (HasDaysSinceOnsetOfSymptoms) {
                output.WriteRawTag(48);
                output.WriteSInt32(DaysSinceOnsetOfSymptoms);
            }
            if (_unknownFields != null) {
                _unknownFields.WriteTo(output);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
            int size = 0;
            if (HasKeyData) {
                size += 1 + CodedOutputStream.ComputeBytesSize(KeyData);
            }
            if (HasTransmissionRiskLevel) {
                size += 1 + CodedOutputStream.ComputeInt32Size(TransmissionRiskLevel);
            }
            if (HasRollingStartIntervalNumber) {
                size += 1 + CodedOutputStream.ComputeInt32Size(RollingStartIntervalNumber);
            }
            if (HasRollingPeriod) {
                size += 1 + CodedOutputStream.ComputeInt32Size(RollingPeriod);
            }
            if (HasReportType) {
                size += 1 + CodedOutputStream.ComputeEnumSize((int) ReportType);
            }
            if (HasDaysSinceOnsetOfSymptoms) {
                size += 1 + CodedOutputStream.ComputeSInt32Size(DaysSinceOnsetOfSymptoms);
            }
            if (_unknownFields != null) {
                size += _unknownFields.CalculateSize();
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(TemporaryExposureKey other) {
            if (other == null) {
                return;
            }
            if (other.HasKeyData) {
                KeyData = other.KeyData;
            }
            if (other.HasTransmissionRiskLevel) {
                TransmissionRiskLevel = other.TransmissionRiskLevel;
            }
            if (other.HasRollingStartIntervalNumber) {
                RollingStartIntervalNumber = other.RollingStartIntervalNumber;
            }
            if (other.HasRollingPeriod) {
                RollingPeriod = other.RollingPeriod;
            }
            if (other.HasReportType) {
                ReportType = other.ReportType;
            }
            if (other.HasDaysSinceOnsetOfSymptoms) {
                DaysSinceOnsetOfSymptoms = other.DaysSinceOnsetOfSymptoms;
            }
            _unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(CodedInputStream input) {
            uint tag;
            while ((tag = input.ReadTag()) != 0) {
                switch(tag) {
                    default:
                        _unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                        break;
                    case 10: {
                        KeyData = input.ReadBytes();
                        break;
                    }
                    case 16: {
                        TransmissionRiskLevel = input.ReadInt32();
                        break;
                    }
                    case 24: {
                        RollingStartIntervalNumber = input.ReadInt32();
                        break;
                    }
                    case 32: {
                        RollingPeriod = input.ReadInt32();
                        break;
                    }
                    case 40: {
                        ReportType = (global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKey.Types.ReportType) input.ReadEnum();
                        break;
                    }
                    case 48: {
                        DaysSinceOnsetOfSymptoms = input.ReadSInt32();
                        break;
                    }
                }
            }
        }

        #region Nested types
        /// <summary>Container for nested types declared in the TemporaryExposureKey message type.</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static partial class Types {
            /// <summary>
            /// Data type representing why this key was published.
            /// </summary>
            public enum ReportType {
                /// <summary>
                /// Never returned by the client API.
                /// </summary>
                [OriginalName("UNKNOWN")] Unknown = 0,
                [OriginalName("CONFIRMED_TEST")] ConfirmedTest = 1,
                [OriginalName("CONFIRMED_CLINICAL_DIAGNOSIS")] ConfirmedClinicalDiagnosis = 2,
                [OriginalName("SELF_REPORT")] SelfReport = 3,
                /// <summary>
                /// Reserved for future use.
                /// </summary>
                [OriginalName("RECURSIVE")] Recursive = 4,
                /// <summary>
                /// Used to revoke a key, never returned by client API.
                /// </summary>
                [OriginalName("REVOKED")] Revoked = 5,
            }

        }
        #endregion

    }
}