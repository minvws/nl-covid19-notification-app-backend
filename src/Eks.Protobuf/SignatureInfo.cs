// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Protobuf
{
    public sealed partial class SignatureInfo : IMessage<SignatureInfo>
    {
        private static readonly MessageParser<SignatureInfo> parser = new MessageParser<SignatureInfo>(() => new SignatureInfo());
        private UnknownFieldSet _unknownFields;
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageParser<SignatureInfo> Parser { get { return parser; } }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageDescriptor Descriptor
        {
            get { return TemporaryExposureKeyExportReflection.Descriptor.MessageTypes[1]; }
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        MessageDescriptor IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public SignatureInfo()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public SignatureInfo(SignatureInfo other) : this()
        {
            _appBundleId = other._appBundleId;
            _androidPackage = other._androidPackage;
            _verificationKeyVersion = other._verificationKeyVersion;
            _verificationKeyId = other._verificationKeyId;
            _signatureAlgorithm = other._signatureAlgorithm;
            _unknownFields = UnknownFieldSet.Clone(other._unknownFields);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public SignatureInfo Clone()
        {
            return new SignatureInfo(this);
        }

        /// <summary>Field number for the "app_bundle_id" field.</summary>
        public const int AppBundleIdFieldNumber = 1;
        private static readonly string appBundleIdDefaultValue = "";

        private string _appBundleId;
        /// <summary>
        /// No longer needed by Google: App Store app bundle ID.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string AppBundleId
        {
            get { return _appBundleId ?? appBundleIdDefaultValue; }
            set
            {
                _appBundleId = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "app_bundle_id" field is set</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasAppBundleId
        {
            get { return _appBundleId != null; }
        }
        /// <summary>Clears the value of the "app_bundle_id" field</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearAppBundleId()
        {
            _appBundleId = null;
        }

        /// <summary>Field number for the "android_package" field.</summary>
        public const int AndroidPackageFieldNumber = 2;
        private static readonly string androidPackageDefaultValue = "";

        private string _androidPackage;
        /// <summary>
        /// No longer needed by Google: Android App package name
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string AndroidPackage
        {
            get { return _androidPackage ?? androidPackageDefaultValue; }
            set
            {
                _androidPackage = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "android_package" field is set</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasAndroidPackage
        {
            get { return _androidPackage != null; }
        }
        /// <summary>Clears the value of the "android_package" field</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearAndroidPackage()
        {
            _androidPackage = null;
        }

        /// <summary>Field number for the "verification_key_version" field.</summary>
        public const int VerificationKeyVersionFieldNumber = 3;
        private static readonly string verificationKeyVersionDefaultValue = "";

        private string _verificationKeyVersion;
        /// <summary>
        /// Key version for rollovers
        /// Must be in character class [a-zA-Z0-9_]. For example, 'v1'
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string VerificationKeyVersion
        {
            get { return _verificationKeyVersion ?? verificationKeyVersionDefaultValue; }
            set
            {
                _verificationKeyVersion = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "verification_key_version" field is set</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasVerificationKeyVersion
        {
            get { return _verificationKeyVersion != null; }
        }
        /// <summary>Clears the value of the "verification_key_version" field</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearVerificationKeyVersion()
        {
            _verificationKeyVersion = null;
        }

        /// <summary>Field number for the "verification_key_id" field.</summary>
        public const int VerificationKeyIdFieldNumber = 4;
        private static readonly string verificationKeyIdDefaultValue = "";

        private string _verificationKeyId;
        /// <summary>
        /// Alias with which to identify public key to be used for verification
        /// Must be in character class [a-zA-Z0-9_.]
        /// For example, the domain of your server: gov.health.foo
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string VerificationKeyId
        {
            get { return _verificationKeyId ?? verificationKeyIdDefaultValue; }
            set
            {
                _verificationKeyId = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "verification_key_id" field is set</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasVerificationKeyId
        {
            get { return _verificationKeyId != null; }
        }
        /// <summary>Clears the value of the "verification_key_id" field</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearVerificationKeyId()
        {
            _verificationKeyId = null;
        }

        /// <summary>Field number for the "signature_algorithm" field.</summary>
        public const int SignatureAlgorithmFieldNumber = 5;
        private static readonly string signatureAlgorithmDefaultValue = "";

        private string _signatureAlgorithm;
        /// <summary>
        /// ASN.1 OID for Algorithm Identifier. For example, `1.2.840.10045.4.3.2'
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string SignatureAlgorithm
        {
            get { return _signatureAlgorithm ?? signatureAlgorithmDefaultValue; }
            set
            {
                _signatureAlgorithm = ProtoPreconditions.CheckNotNull(value, "value");
            }
        }
        /// <summary>Gets whether the "signature_algorithm" field is set</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool HasSignatureAlgorithm
        {
            get { return _signatureAlgorithm != null; }
        }
        /// <summary>Clears the value of the "signature_algorithm" field</summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void ClearSignatureAlgorithm()
        {
            _signatureAlgorithm = null;
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other)
        {
            return Equals(other as SignatureInfo);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(SignatureInfo other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (AppBundleId != other.AppBundleId)
            {
                return false;
            }

            if (AndroidPackage != other.AndroidPackage)
            {
                return false;
            }

            if (VerificationKeyVersion != other.VerificationKeyVersion)
            {
                return false;
            }

            if (VerificationKeyId != other.VerificationKeyId)
            {
                return false;
            }

            if (SignatureAlgorithm != other.SignatureAlgorithm)
            {
                return false;
            }

            return Equals(_unknownFields, other._unknownFields);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode()
        {
            var hash = 1;
            if (HasAppBundleId)
            {
                hash ^= AppBundleId.GetHashCode();
            }

            if (HasAndroidPackage)
            {
                hash ^= AndroidPackage.GetHashCode();
            }

            if (HasVerificationKeyVersion)
            {
                hash ^= VerificationKeyVersion.GetHashCode();
            }

            if (HasVerificationKeyId)
            {
                hash ^= VerificationKeyId.GetHashCode();
            }

            if (HasSignatureAlgorithm)
            {
                hash ^= SignatureAlgorithm.GetHashCode();
            }

            if (_unknownFields != null)
            {
                hash ^= _unknownFields.GetHashCode();
            }
            return hash;
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString()
        {
            return JsonFormatter.ToDiagnosticString(this);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(CodedOutputStream output)
        {
            if (HasAppBundleId)
            {
                output.WriteRawTag(10);
                output.WriteString(AppBundleId);
            }
            if (HasAndroidPackage)
            {
                output.WriteRawTag(18);
                output.WriteString(AndroidPackage);
            }
            if (HasVerificationKeyVersion)
            {
                output.WriteRawTag(26);
                output.WriteString(VerificationKeyVersion);
            }
            if (HasVerificationKeyId)
            {
                output.WriteRawTag(34);
                output.WriteString(VerificationKeyId);
            }
            if (HasSignatureAlgorithm)
            {
                output.WriteRawTag(42);
                output.WriteString(SignatureAlgorithm);
            }
            if (_unknownFields != null)
            {
                _unknownFields.WriteTo(output);
            }
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize()
        {
            var size = 0;
            if (HasAppBundleId)
            {
                size += 1 + CodedOutputStream.ComputeStringSize(AppBundleId);
            }
            if (HasAndroidPackage)
            {
                size += 1 + CodedOutputStream.ComputeStringSize(AndroidPackage);
            }
            if (HasVerificationKeyVersion)
            {
                size += 1 + CodedOutputStream.ComputeStringSize(VerificationKeyVersion);
            }
            if (HasVerificationKeyId)
            {
                size += 1 + CodedOutputStream.ComputeStringSize(VerificationKeyId);
            }
            if (HasSignatureAlgorithm)
            {
                size += 1 + CodedOutputStream.ComputeStringSize(SignatureAlgorithm);
            }
            if (_unknownFields != null)
            {
                size += _unknownFields.CalculateSize();
            }
            return size;
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(SignatureInfo other)
        {
            if (other == null)
            {
                return;
            }
            if (other.HasAppBundleId)
            {
                AppBundleId = other.AppBundleId;
            }
            if (other.HasAndroidPackage)
            {
                AndroidPackage = other.AndroidPackage;
            }
            if (other.HasVerificationKeyVersion)
            {
                VerificationKeyVersion = other.VerificationKeyVersion;
            }
            if (other.HasVerificationKeyId)
            {
                VerificationKeyId = other.VerificationKeyId;
            }
            if (other.HasSignatureAlgorithm)
            {
                SignatureAlgorithm = other.SignatureAlgorithm;
            }
            _unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(CodedInputStream input)
        {
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (tag)
                {
                    default:
                        _unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                        break;
                    case 10:
                        {
                            AppBundleId = input.ReadString();
                            break;
                        }
                    case 18:
                        {
                            AndroidPackage = input.ReadString();
                            break;
                        }
                    case 26:
                        {
                            VerificationKeyVersion = input.ReadString();
                            break;
                        }
                    case 34:
                        {
                            VerificationKeyId = input.ReadString();
                            break;
                        }
                    case 42:
                        {
                            SignatureAlgorithm = input.ReadString();
                            break;
                        }
                }
            }
        }

    }
}
