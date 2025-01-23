using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6._Interface.InterfacesAndExtensibility
{
    internal class InterfacesAndExtensibilityDemo
    {
        public static void Learn()
        {
            var dbMigrator = new DbMigrator(new ConsoleLogger());
            dbMigrator.Migrate();
            
            var dbMigrator1 = new DbMigrator(new FileLogger(@"C:\Projects\log.txt"));
            dbMigrator1.Migrate();
        }
    }
}
