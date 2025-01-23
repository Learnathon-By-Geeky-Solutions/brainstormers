using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3._Association_Between_Classes.Composition
{
    internal class CompositionDemo
    {
        public static void Learn()
        {
            var dbMigrator = new DBMigrator(new Logger());
            var installer = new Installer(new Logger());

            dbMigrator.Migrate();
            installer.Install();
        }
    }
}
