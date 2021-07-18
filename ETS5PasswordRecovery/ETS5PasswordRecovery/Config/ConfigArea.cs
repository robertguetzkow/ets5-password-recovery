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

using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ETS5PasswordRecovery.Config
{
    class ConfigArea : ConfigXml
    {
        public List<Element.DeviceInformation> deviceInformation = new List<Element.DeviceInformation>();

        public override void Load(string filepath)
        {
            base.Load(filepath);

            XmlNodeList deviceInformationNodes = document.DocumentElement.SelectNodes("//Line/DI");

            if (deviceInformationNodes.Count == 0)
            {
                return; // Not every area and their respective line needs to contain these entries.
            }

            foreach (XmlNode deviceInformationNode in deviceInformationNodes)
            {
                Element.DeviceInformation deviceInfo = new Element.DeviceInformation(deviceInformationNode);
                deviceInformation.Add(deviceInfo);
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Element.DeviceInformation deviceInfo in deviceInformation)
            {
                stringBuilder.Append(deviceInfo.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}
