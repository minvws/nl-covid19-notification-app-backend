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
        private static readonly MessageParser<DiagnosisKeyBatch> _parser = new MessageParser<DiagnosisKeyBatch>(() => new DiagnosisKeyBatch());
        private UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageParser<DiagnosisKeyBatch> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static MessageDescriptor Descriptor {
            get { return global::Eu.Interop.EfgsReflection.Descriptor.MessageTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        MessageDescriptor IMessage.Descriptor {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public DiagnosisKeyBatch() {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public DiagnosisKeyBatch(DiagnosisKeyBatch other) : this() {
            keys_ = other.keys_.Clone();
            _unknownFields = UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public DiagnosisKeyBatch Clone() {
            return new DiagnosisKeyBatch(this);
        }

        /// <summary>Field number for the "keys" field.</summary>
        public const int KeysFieldNumber = 1;
        private static readonly FieldCodec<global::Iks.Protobuf.DiagnosisKey> _repeated_keys_codec
            = FieldCodec.ForMessage(10, global::Iks.Protobuf.DiagnosisKey.Parser);
        private readonly RepeatedField<global::Iks.Protobuf.DiagnosisKey> keys_ = new RepeatedField<global::Iks.Protobuf.DiagnosisKey>();
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public RepeatedField<global::Iks.Protobuf.DiagnosisKey> Keys {
            get { return keys_; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
            return Equals(other as DiagnosisKeyBatch);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(DiagnosisKeyBatch other) {
            if (ReferenceEquals(other, null)) {
                return false;
            }
            if (ReferenceEquals(other, this)) {
                return true;
            }
            if(!keys_.Equals(other.keys_)) return false;
            return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
            int hash = 1;
            hash ^= keys_.GetHashCode();
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
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
#else
            keys_.WriteTo(output, _repeated_keys_codec);
            if (_unknownFields != null) {
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
        public int CalculateSize() {
            int size = 0;
            size += keys_.CalculateSize(_repeated_keys_codec);
            if (_unknownFields != null) {
                size += _unknownFields.CalculateSize();
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(DiagnosisKeyBatch other) {
            if (other == null) {
                return;
            }
            keys_.Add(other.keys_);
            _unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(CodedInputStream input) {
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
#else
            uint tag;
            while ((tag = input.ReadTag()) != 0) {
                switch(tag) {
                    default:
                        _unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                        break;
                    case 10: {
                        keys_.AddEntriesFrom(input, _repeated_keys_codec);
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