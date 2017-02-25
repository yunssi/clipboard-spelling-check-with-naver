using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace spellchk
{
    static class Program
    {
        static Mutex mutex = new Mutex(false, "5964effa-37a0-4fbb-bdab-89adcdc25520");

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.FromSeconds(1), false))
            {
                MessageBox.Show("spellchk가 이미 실행 중입니다~", "", MessageBoxButtons.OK);
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            finally { mutex.ReleaseMutex(); }
        }
    }
}
