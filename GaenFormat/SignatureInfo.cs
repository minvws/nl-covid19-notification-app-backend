// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat
{
    public sealed partial class SignatureInfo : IMessage<SignatureInfo> {
        private static readonly MessageParser<SignatureInfo> _parser = new MessageParser<SignatureInfo>(() => new SignatureInfo());
        private UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageParser<SignatureInfo> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageDescriptor Descriptor {
            get { return global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKeyExportReflection.Descriptor.MessageTypes[1]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        MessageDescriptor IMessage.Descriptor {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public SignatureInfo() {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public SignatureInfo(SignatureInfo other) : this() {
            verificationKeyVersion_ = other.verificationKeyVersion_;
            verificationKeyId_ = other.verificationKeyId_;
            signatureAlgorithm_ = other.signatureAlgorithm_;
            _unknownFields = UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public SignatureInfo Clone() {
            return new SignatureInfo(this);
        }

        /// <summary>Field number for the "verification_key_version" field.</summary>
        public const int VerificationKeyVersionFieldNumber = 3;
        private readonly static string VerificationKeyVersionDefaultValue = "";

        private string verificationKeyVersion_;
        /// <summary>
        /// Key version for rollovers
        /// Must be in character class [a-zA-Z0-9_]. For example, 'v1'
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string VerificationKeyVersion {
            get { return verificationKeyVersion_ ?? VerificationKeyVersionDefaultValue; }
            set {
                verificationKeyVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "verification_key_version" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasVerificationKeyVersion {
            get { return verificationKeyVersion_ != null; }
        }
        /// <summary>Clears the value of the "verification_key_version" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearVerificationKeyVersion() {
            verificationKeyVersion_ = null;
        }

        /// <summary>Field number for the "verification_key_id" field.</summary>
        public const int VerificationKeyIdFieldNumber = 4;
        private readonly static string VerificationKeyIdDefaultValue = "";

        private string verificationKeyId_;
        /// <summary>
        /// Alias with which to identify public key to be used for verification
        /// Must be in character class [a-zA-Z0-9_.]
        /// For example, the domain of your server: gov.health.foo
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string VerificationKeyId {
            get { return verificationKeyId_ ?? VerificationKeyIdDefaultValue; }
            set {
                verificationKeyId_ = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "verification_key_id" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasVerificationKeyId {
            get { return verificationKeyId_ != null; }
        }
        /// <summary>Clears the value of the "verification_key_id" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearVerificationKeyId() {
            verificationKeyId_ = null;
        }

        /// <summary>Field number for the "signature_algorithm" field.</summary>
        public const int SignatureAlgorithmFieldNumber = 5;
        private readonly static string SignatureAlgorithmDefaultValue = "";

        private string signatureAlgorithm_;
        /// <summary>
        /// ASN.1 OID for Algorithm Identifier. For example, `1.2.840.10045.4.3.2'
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string SignatureAlgorithm {
            get { return signatureAlgorithm_ ?? SignatureAlgorithmDefaultValue; }
            set {
                signatureAlgorithm_ = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "signature_algorithm" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasSignatureAlgorithm {
            get { return signatureAlgorithm_ != null; }
        }
        /// <summary>Clears the value of the "signature_algorithm" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearSignatureAlgorithm() {
            signatureAlgorithm_ = null;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
            return Equals(other as SignatureInfo);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(SignatureInfo other) {
            if (ReferenceEquals(other, null)) {
                return false;
            }
            if (ReferenceEquals(other, this)) {
                return true;
            }
            if (VerificationKeyVersion != other.VerificationKeyVersion) return false;
            if (VerificationKeyId != other.VerificationKeyId) return false;
            if (SignatureAlgorithm != other.SignatureAlgorithm) return false;
            return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
            int hash = 1;
            if (HasVerificationKeyVersion) hash ^= VerificationKeyVersion.GetHashCode();
            if (HasVerificationKeyId) hash ^= VerificationKeyId.GetHashCode();
            if (HasSignatureAlgorithm) hash ^= SignatureAlgorithm.GetHashCode();
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
            if (HasVerificationKeyVersion) {
                output.WriteRawTag(26);
                output.WriteString(VerificationKeyVersion);
            }
            if (HasVerificationKeyId) {
                output.WriteRawTag(34);
                output.WriteString(VerificationKeyId);
            }
            if (HasSignatureAlgorithm) {
                output.WriteRawTag(42);
                output.WriteString(SignatureAlgorithm);
            }
            if (_unknownFields != null) {
                _unknownFields.WriteTo(output);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
            int size = 0;
            if (HasVerificationKeyVersion) {
                size += 1 + CodedOutputStream.ComputeStringSize(VerificationKeyVersion);
            }
            if (HasVerificationKeyId) {
                size += 1 + CodedOutputStream.ComputeStringSize(VerificationKeyId);
            }
            if (HasSignatureAlgorithm) {
                size += 1 + CodedOutputStream.ComputeStringSize(SignatureAlgorithm);
            }
            if (_unknownFields != null) {
                size += _unknownFields.CalculateSize();
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(SignatureInfo other) {
            if (other == null) {
                return;
            }
            if (other.HasVerificationKeyVersion) {
                VerificationKeyVersion = other.VerificationKeyVersion;
            }
            if (other.HasVerificationKeyId) {
                VerificationKeyId = other.VerificationKeyId;
            }
            if (other.HasSignatureAlgorithm) {
                SignatureAlgorithm = other.SignatureAlgorithm;
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
                    case 26: {
                        VerificationKeyVersion = input.ReadString();
                        break;
                    }
                    case 34: {
                        VerificationKeyId = input.ReadString();
                        break;
                    }
                    case 42: {
                        SignatureAlgorithm = input.ReadString();
                        break;
                    }
                }
            }
        }

    }
}