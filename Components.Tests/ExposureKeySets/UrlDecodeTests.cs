using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    [TestClass]
    public class UrlDecodeTests
    {
        [DataRow("QUf0TE1gctDRvg0L4YRCd3GVgRckbS0+jWn2migAKuAGrSAh+KbC88fuhmZ3oTR5iLh0a5080riYtR8vChqU7A==", "QUf0TE1gctDRvg0L4YRCd3GVgRckbS0+jWn2migAKuAGrSAh+KbC88fuhmZ3oTR5iLh0a5080riYtR8vChqU7A==")]
        [DataRow("AtlRlHfEl7XNAWFropI17bkCQ9u8lHfaj66NVWd6u++Zm/3GLf2pjRukx02M9VM8Q6/7wddk9ShkEfP8ro1NhA==", "AtlRlHfEl7XNAWFropI17bkCQ9u8lHfaj66NVWd6u++Zm/3GLf2pjRukx02M9VM8Q6/7wddk9ShkEfP8ro1NhA==")]
        //AtlRlHfEl7XNAWFropI17bkCQ9u8lHfaj66NVWd6u  Zm/3GLf2pjRukx02M9VM8Q6/7wddk9ShkEfP8ro1NhA==
        [DataRow("AtlRlHfEl7XNAWFropI17bkCQ9u8lHfaj66NVWd6u++Zm%2F3GLf2pjRukx02M9VM8Q6%2F7wddk9ShkEfP8ro1NhA==", "AtlRlHfEl7XNAWFropI17bkCQ9u8lHfaj66NVWd6u++Zm/3GLf2pjRukx02M9VM8Q6/7wddk9ShkEfP8ro1NhA==")]
        [DataTestMethod]
        public void MatchUriToDb(string uri, string db)
        {
            var decoded = HttpUtility.UrlDecode(uri).Replace(" ", "+");
            Assert.AreEqual(db, decoded);
        }
    }
}