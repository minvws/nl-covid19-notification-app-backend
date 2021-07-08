// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Protobuf
{
    public sealed partial class TEKSignatureList : IMessage<TEKSignatureList>
    {
        private static readonly MessageParser<TEKSignatureList> parser = new MessageParser<TEKSignatureList>(() => new TEKSignatureList());
        private UnknownFieldSet _unknownFields;
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageParser<TEKSignatureList> Parser { get { return parser; } }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageDescriptor Descriptor
        {
            get { return TemporaryExposureKeyExportReflection.Descriptor.MessageTypes[3]; }
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        MessageDescriptor IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TEKSignatureList()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TEKSignatureList(TEKSignatureList other) : this()
        {
            _signatures = other._signatures.Clone();
            _unknownFields = UnknownFieldSet.Clone(other._unknownFields);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TEKSignatureList Clone()
        {
            return new TEKSignatureList(this);
        }

        /// <summary>Field number for the "signatures" field.</summary>
        public const int SignaturesFieldNumber = 1;
        private static readonly FieldCodec<TEKSignature> repeatedSignaturesCodec
            = FieldCodec.ForMessage(10, TEKSignature.Parser);
        private readonly RepeatedField<TEKSignature> _signatures = new RepeatedField<TEKSignature>();
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public RepeatedField<TEKSignature> Signatures
        {
            get { return _signatures; }
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other)
        {
            return Equals(other as TEKSignatureList);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(TEKSignatureList other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (!_signatures.Equals(other._signatures))
            {
                return false;
            }

            return Equals(_unknownFields, other._unknownFields);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode()
        {
            var hash = 1;
            hash ^= _signatures.GetHashCode();
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
            _signatures.WriteTo(output, repeatedSignaturesCodec);
            if (_unknownFields != null)
            {
                _unknownFields.WriteTo(output);
            }
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize()
        {
            var size = 0;
            size += _signatures.CalculateSize(repeatedSignaturesCodec);
            if (_unknownFields != null)
            {
                size += _unknownFields.CalculateSize();
            }
            return size;
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(TEKSignatureList other)
        {
            if (other == null)
            {
                return;
            }
            _signatures.Add(other._signatures);
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
                            _signatures.AddEntriesFrom(input, repeatedSignaturesCodec);
                            break;
                        }
                }
            }
        }

    }
}
