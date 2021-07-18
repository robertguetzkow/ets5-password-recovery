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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ETS5PasswordRecovery.Config;

namespace ETS5PasswordRecovery
{
    class PasswordRecovery
    {
        public void PrintProjectSecrets(string projectDirectory)
        {
            try
            {
                ConfigProject configProject = new ConfigProject();
                string configProjectPath = Path.Combine(projectDirectory, "P");
                configProject.Load(configProjectPath);
                Console.Write(configProject.ToString());
            }
            catch (Exception exception) when (exception is XmlException || exception is IOException)
            {
                Console.WriteLine();
                Console.WriteLine($"WARNING: {exception.Message}");
                Console.WriteLine("Skipping file.");
                Console.WriteLine();
            }
        }

        public void PrintDeviceCertificateSecrets(string projectDirectory)
        {
            try
            {
                ConfigDeviceCertificate configDeviceCertificate = new ConfigDeviceCertificate();
                string configDeviceCertificatePath = Path.Combine(projectDirectory, "D");
                configDeviceCertificate.Load(configDeviceCertificatePath);
                Console.Write(configDeviceCertificate.ToString());
            }
            catch (Exception exception) when (exception is XmlException || exception is IOException)
            {
                Console.WriteLine();
                Console.WriteLine($"WARNING: {exception.Message}");
                Console.WriteLine("Skipping file.");
                Console.WriteLine();
            }
        }

        public void PrintInstallationSecrets(string projectDirectory)
        {
            try
            {
                ConfigInstallation configInstallation = new ConfigInstallation();
                string configInstallationPath = Path.Combine(projectDirectory, "I");
                configInstallation.Load(configInstallationPath);
                Console.Write(configInstallation.ToString());
            }
            catch (Exception exception) when (exception is XmlException || exception is IOException)
            {
                Console.WriteLine();
                Console.WriteLine($"WARNING: {exception.Message}");
                Console.WriteLine("Skipping file.");
                Console.WriteLine();
            }
        }

        public void PrintAreaSecrets(string configAreaPath)
        {
            try
            {
                ConfigArea areaConfig = new ConfigArea();
                areaConfig.Load(configAreaPath);
                Console.Write(areaConfig.ToString());
            }
            catch (Exception exception) when (exception is XmlException || exception is IOException)
            {
                Console.WriteLine();
                Console.WriteLine($"WARNING: {exception.Message}");
                Console.WriteLine("Skipping file.");
                Console.WriteLine();
            }
        }

        public void PrintUnobfuscatedPasswords(string directory)
        {
            string[] projectDirectories = Directory.GetDirectories(directory, "P-*", SearchOption.TopDirectoryOnly);
            foreach (string projectDirectory in projectDirectories)
            {
                Console.WriteLine();
                Console.WriteLine(projectDirectory);
                Console.WriteLine(new String('=', 100));

                PrintProjectSecrets(projectDirectory);
                PrintDeviceCertificateSecrets(projectDirectory);
                PrintInstallationSecrets(projectDirectory);

                List<ConfigArea> configAreas = new List<ConfigArea>();
                string configAreaPrefix = new DirectoryInfo(projectDirectory).Name + "*.";
                string[] configAreaPaths = Directory.GetFiles(projectDirectory, configAreaPrefix, SearchOption.TopDirectoryOnly);

                foreach (string configAreaPath in configAreaPaths)
                {
                    PrintAreaSecrets(configAreaPath);
                }
            }
        }
    }
}
