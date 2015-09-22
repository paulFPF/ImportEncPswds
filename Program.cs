using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ImportEncPswds.Models;

namespace ImportEncPswds
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() != 1)
            {
                Console.WriteLine("Usage: ImportEncPswds {csv file}\r\n");
                return;
            }

            SecurityCloneEntities dbContext = new SecurityCloneEntities();
            int cnt = 0;

            // read the CSV file
            using (StreamReader sr = new StreamReader(args[0]))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if ((++cnt % 10) == 0)
                    {
                        Console.Write('.');
                        if ((cnt % 100) == 0)
                            Console.Write(cnt);
                    }


                    string[] fields = line.Split(',');
                    string username = fields[0];
                    string encPswd = fields[1];

                    try
                    {
                        User user = dbContext.Users.Single(u => u.MembershipUserName == username);
                        user.EncLoginPswd = encPswd;

                        dbContext.Users.Attach(user);
                        dbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;

                        if ((cnt % 100) == 0)
                        {
                            try
                            {
                                Console.WriteLine("\r\nSaving Changes to Database\r\n");
                                ((System.Data.Entity.Infrastructure.IObjectContextAdapter)dbContext).ObjectContext.CommandTimeout = 600;
                                dbContext.SaveChanges();
                            }
                            catch( Exception ex )
                            {
                                Console.WriteLine("SaveChanges failed: " + ex.InnerException.InnerException.Message);
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\r\nexception for user \"" + username + "\" -- " + ex.Message);
                    }
                }
                Console.WriteLine("\r\nSaving Changes to Database\r\n");
                dbContext.SaveChanges();
            }

            Console.WriteLine("Imported " + cnt + " encrypted passwords.\r\nHit Enter to terminate.");
            Console.ReadLine();

            /*
             * could also use --
             * dbContext.Database.ExecuteSqlCommand(sql);
             */
        }
    }
}
