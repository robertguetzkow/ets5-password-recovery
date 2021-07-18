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

using System.Text;
using System.Xml;

namespace ETS5PasswordRecovery.Config.Element
{
    class BusInterface : Element
    {
        private readonly string refID;
        private readonly string password;
        private readonly string passwordHash;

        public BusInterface(XmlNode busInterfaceNode)
        {
            if (busInterfaceNode.Name != "BI")
            {
                throw new XmlException($"Tag name {busInterfaceNode.Name} doesn't match expected value 'BI'");
            }

            this.refID = GetAttribute(busInterfaceNode, "RefId");
            this.password = GetDeobfuscatedAttribute(busInterfaceNode, "Password");
            this.passwordHash = GetDeobfuscatedAttribute(busInterfaceNode, "PasswordHash");
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Bus Interface");
            stringBuilder.AppendLine(new string('-', sectionLineLength));
            stringBuilder.Append("RefID".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(refID);
            stringBuilder.Append("Password".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(password);
            stringBuilder.Append("Password Hash".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(passwordHash);
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }
    }
}
