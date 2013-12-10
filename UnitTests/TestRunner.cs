// The Sixty Project
//
// Copyright 2013 Richard Morrison
// All Rights Reserved
//
// See COPYRIGHT.TXT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    class TestRunner
    {

        [STAThread]
        static void Main(string[] args)
        {
            string[] my_args = { Assembly.GetExecutingAssembly().Location };

            int returnCode = NUnit.ConsoleRunner.Runner.Main(my_args);
        }
    }
}
