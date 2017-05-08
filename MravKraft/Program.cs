using System;
using System.Windows.Forms;

namespace MravKraft
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();

            using (var game = new GameMain())
                game.Run();
        }
    }
#endif
}
