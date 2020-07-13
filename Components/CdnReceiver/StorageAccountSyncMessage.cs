namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class StorageAccountSyncMessage
    {
        /// <summary>
        /// DestinationArgs.Path + Name.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Manifest only = true
        /// </summary>
        public bool MutableContent { get; set; }
    }
}