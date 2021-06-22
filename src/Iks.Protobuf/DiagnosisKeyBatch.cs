// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Iks.Protobuf
{
    public sealed partial class DiagnosisKeyBatch : IMessage<DiagnosisKeyBatch>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
#endif
    {
        private static readonly MessageParser<DiagnosisKeyBatch> parser = new MessageParser<DiagnosisKeyBatch>(() => new DiagnosisKeyBatch());
        private UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageParser<DiagnosisKeyBatch> Parser { get { return parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageDescriptor Descriptor
        {
            get { return global::Eu.Interop.EfgsReflection.Descriptor.MessageTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        MessageDescriptor IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public DiagnosisKeyBatch()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public DiagnosisKeyBatch(DiagnosisKeyBatch other) : this()
        {
            _keys = other._keys.Clone();
            _unknownFields = UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public DiagnosisKeyBatch Clone()
        {
            return new DiagnosisKeyBatch(this);
        }

        /// <summary>Field number for the "keys" field.</summary>
        public const int KeysFieldNumber = 1;
        private static readonly FieldCodec<global::Iks.Protobuf.DiagnosisKey> repeatedKeysCodec
            = FieldCodec.ForMessage(10, global::Iks.Protobuf.DiagnosisKey.Parser);
        private readonly RepeatedField<global::Iks.Protobuf.DiagnosisKey> _keys = new RepeatedField<global::Iks.Protobuf.DiagnosisKey>();
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public RepeatedField<global::Iks.Protobuf.DiagnosisKey> Keys
        {
            get { return _keys; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other)
        {
            return Equals(other as DiagnosisKeyBatch);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(DiagnosisKeyBatch other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (!_keys.Equals(other._keys))
            {
                return false;
            }

            return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode()
        {
            var hash = 1;
            hash ^= _keys.GetHashCode();
            if (_unknownFields != null)
            {
                hash ^= _unknownFields.GetHashCode();
            }
            return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString()
        {
            return JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(CodedOutputStream output)
        {
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
#else
            _keys.WriteTo(output, repeatedKeysCodec);
            if (_unknownFields != null)
            {
                _unknownFields.WriteTo(output);
            }
#endif
        }

#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      keys_.WriteTo(ref output, _repeated_keys_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
#endif

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize()
        {
            var size = 0;
            size += _keys.CalculateSize(repeatedKeysCodec);
            if (_unknownFields != null)
            {
                size += _unknownFields.CalculateSize();
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(DiagnosisKeyBatch other)
        {
            if (other == null)
            {
                return;
            }
            _keys.Add(other._keys);
            _unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(CodedInputStream input)
        {
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
#else
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
                            _keys.AddEntriesFrom(input, repeatedKeysCodec);
                            break;
                        }
                }
            }
#endif
        }

#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            keys_.AddEntriesFrom(ref input, _repeated_keys_codec);
            break;
          }
        }
      }
    }
#endif

    }
}
