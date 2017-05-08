namespace MravKraftAPI.Baze
{
    using Mravi;

    internal class MravProcess
    {
        internal MravType MravTip { get; private set; }
        internal byte DurationLeft { get; set; }
        internal byte CountLeft { get; set; }

        internal MravProcess(MravType tip, byte count)
        {
            MravTip = tip;
            CountLeft = count;

            ResetDuration();
        }

        internal void ResetDuration()
        {

            switch (MravTip)
            {
                case MravType.Radnik:
                    DurationLeft = Radnik.Duration;
                    break;
                case MravType.Scout:
                    DurationLeft = Scout.Duration;
                    break;
                case MravType.Vojnik:
                    DurationLeft = Vojnik.Duration;
                    break;
                case MravType.Leteci:
                    DurationLeft = Leteci.Duration;
                    break;
            }
        }

    }
}
