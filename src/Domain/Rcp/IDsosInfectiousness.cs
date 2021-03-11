namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp
{
    public interface IDsosInfectiousness
    {
        public bool IsInfectious(int dsos);
    }
}