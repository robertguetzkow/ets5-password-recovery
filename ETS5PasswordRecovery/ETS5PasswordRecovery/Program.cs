/*
MIT License
Copyright (c) 2021 Robert Gützkow

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;

namespace ETS5PasswordRecovery
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"   __________________   ___                                 __  ___                                  ______          __");
            Console.WriteLine(@"  / __/_  __/ __/ __/  / _ \___ ____ ____    _____  _______/ / / _ \___ _______ _  _____ ______ __  /_  __/__  ___  / /");
            Console.WriteLine(@" / _/  / / _\ \/__ \  / ___/ _ `(_-<(_-< |/|/ / _ \/ __/ _  / / , _/ -_) __/ _ \ |/ / -_) __/ // /   / / / _ \/ _ \/ / ");
            Console.WriteLine(@"/___/ /_/ /___/____/ /_/   \_,_/___/___/__,__/\___/_/  \_,_/ /_/|_|\__/\__/\___/___/\__/_/  \_, /   /_/  \___/\___/_/  ");
            Console.WriteLine(@"                                                                                           /___/                       ");
            Console.WriteLine("Distributed under MIT license");
            Console.WriteLine("Copyright © 2021 Robert Gützkow");
            Console.WriteLine();
            try
            {
                PasswordRecovery passwordRecovery = new PasswordRecovery();
                passwordRecovery.PrintUnobfuscatedPasswords(@"C:\ProgramData\KNX\ETS5\ProjectStore");
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                Console.WriteLine(@"Could not find the ETS5 project store in 'C:\ProgramData\KNX\ETS5\ProjectStore'.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            Console.WriteLine("Press enter to exit ...");
            Console.ReadLine();
        }
    }
}
