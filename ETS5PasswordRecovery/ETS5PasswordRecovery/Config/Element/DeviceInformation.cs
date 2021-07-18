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
using System.Collections.Generic;
using System.Xml;

namespace ETS5PasswordRecovery.Config.Element
{
    class DeviceInformation : Element
    {
        private readonly string id = string.Empty;
        private readonly string name = string.Empty;
        private readonly string serialNumber = string.Empty;
        private readonly string loadedIPRoutingBackboneKey = string.Empty;
        private readonly string deviceAuthenticationCode = string.Empty;
        private readonly string deviceAuthenticationCodeHash = string.Empty;
        private readonly string loadedDeviceAuthenticationCodeHash = string.Empty;
        private readonly string deviceManagementPassword = string.Empty;
        private readonly string deviceManagementPasswordHash = string.Empty;
        private readonly string loadedDeviceManagementPasswordHash = string.Empty;
        private readonly string toolKey = string.Empty;
        private readonly string loadedToolKey = string.Empty;
        private List<BusInterface> busInterfaces = new List<BusInterface>();
        public DeviceInformation(XmlNode deviceInformationNode)
        {
            if (deviceInformationNode.Name != "DI")
            {
                throw new XmlException($"Tag name {deviceInformationNode.Name} doesn't match expected value 'DI'");
            }

            this.id = GetAttribute(deviceInformationNode, "Id");
            this.name = GetAttribute(deviceInformationNode, "Name");
            this.serialNumber = GetAttribute(deviceInformationNode, "SerialNumber");

            XmlNode security = deviceInformationNode.SelectSingleNode("//Security");

            if (security == null)
            {
                return; // Not all devices are capable of KNX IP Secure
            }

            this.loadedIPRoutingBackboneKey = GetDeobfuscatedAttribute(security, "LoadedIPRoutingBackboneKey");
            this.deviceAuthenticationCode = GetDeobfuscatedAttribute(security, "DeviceAuthenticationCode");
            this.deviceAuthenticationCodeHash = GetDeobfuscatedAttribute(security, "DeviceAuthenticationCodeHash");
            this.loadedDeviceAuthenticationCodeHash = GetDeobfuscatedAttribute(security, "LoadedDeviceAuthenticationCodeHash");
            this.deviceManagementPassword = GetDeobfuscatedAttribute(security, "DeviceManagementPassword");
            this.deviceManagementPasswordHash = GetDeobfuscatedAttribute(security, "DeviceManagementPasswordHash");
            this.loadedDeviceManagementPasswordHash = GetDeobfuscatedAttribute(security, "LoadedDeviceManagementPasswordHash");
            this.toolKey = GetDeobfuscatedAttribute(security, "ToolKey");
            this.loadedToolKey = GetDeobfuscatedAttribute(security, "LoadedToolKey");

            XmlNode busInterfacesNode = deviceInformationNode.SelectSingleNode("//BIs");

            if (busInterfacesNode == null)
            {
                return;
            }

            XmlNodeList busInterfaceNodes = busInterfacesNode.SelectNodes("//BI");

            foreach (XmlNode busInterfaceNode in busInterfaceNodes)
            {
                BusInterface busInterface = new BusInterface(busInterfaceNode);
                this.busInterfaces.Add(busInterface);
            }
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Device Information");
            stringBuilder.AppendLine(new string('-', groupedLineLength));
            stringBuilder.Append("ID".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(id);
            stringBuilder.Append("Name".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(name);
            stringBuilder.Append("Serial Number".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(serialNumber);
            stringBuilder.Append("Loaded IP routing backbone key".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(loadedIPRoutingBackboneKey);
            stringBuilder.Append("Device authentication code".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(deviceAuthenticationCode);
            stringBuilder.Append("Device authentication code hash".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(deviceAuthenticationCodeHash);
            stringBuilder.Append("Loaded device authentication code hash".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(loadedDeviceAuthenticationCodeHash);
            stringBuilder.Append("Device management password".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(deviceManagementPassword);
            stringBuilder.Append("Device management password hash".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(deviceManagementPasswordHash);
            stringBuilder.Append("Loaded device management password hash".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(loadedDeviceManagementPasswordHash);
            stringBuilder.Append("Tool key".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(toolKey);
            stringBuilder.Append("Loaded tool key".PadRight(paddingLength, '.'));
            stringBuilder.Append(":");
            stringBuilder.AppendLine(loadedToolKey);
            stringBuilder.AppendLine();

            foreach (BusInterface busInterface in busInterfaces)
            {
                stringBuilder.Append(busInterface.ToString());
            }

            stringBuilder.AppendLine(new string('-', groupedLineLength));
            stringBuilder.AppendLine();

            return stringBuilder.ToString();
        }

    }
}
