﻿/*
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

using System.Text;
using System.Xml;

namespace ETS5PasswordRecovery.Config.Element
{
    class Installation : Element
    {
        private readonly string name = string.Empty;
        private readonly string defaultLine = string.Empty;
        private readonly string ipRoutingBackboneKey = string.Empty;

        public Installation(XmlNode installationNode)
        {
            if (installationNode.Name != "I")
            {
                throw new XmlException($"Tag name {installationNode.Name} doesn't match expected value 'I'");
            }

            this.name = GetAttribute(installationNode, "Name");
            this.defaultLine = GetAttribute(installationNode, "DefaultLine");
            this.ipRoutingBackboneKey = GetDeobfuscatedAttribute(installationNode, "IPRoutingBackboneKey");
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Installation");
            stringBuilder.AppendLine(new string('-', sectionLineLength));
            stringBuilder.Append("Name".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(name);
            stringBuilder.Append("Default line".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(defaultLine);
            stringBuilder.Append("IP routing backbone key".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(ipRoutingBackboneKey);
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }
    }
}
