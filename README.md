# ETS5 Password Recovery Tool

## Table of Contents

- [Introduction](#introduction)
- [Installation](#installation)
- [Requirements](#requirements)
- [How does the password recovery work?](#how-does-the-password-recovery-work)
- [How was the design flaw discovered?](#how-was-the-design-flaw-discovered)
- [How can the risk be mitigated?](#how-can-the-risk-be-mitigated)
- [Coordinated Vulnerability Disclosure](#coordinated-vulnerability-disclosure)
- [License](#license)
- [Change log](#change-log)

## Introduction

Have you forgotten the password to one of your ETS5 projects and cannot access the configuration for the KNX installation anymore? The ETS5 Password Recovery Tool allows you to retrieve the project password and other secrets saved in the project store of the ETS5. This is possible because the ETS5 has a significant design flaw, it uses a hard-coded password and salt to encrypt the project information ([CVE-2021-36799](https://cve.mitre.org/cgi-bin/cvename.cgi?name=CVE-2021-36799)). 

![Command prompt](./imgs/cmd.svg)

Storing cryptographic secrets in source code is ill-advised because they can be recovered by reverse engineering the software, thus offering little more protection than storing the information as cleartext. This can pose a threat to the security of the KNX installations. If an attacker is able to gain access to the files in the project store, they can decrypt them despite not knowing the project password. The information contained within allow to eaves-drop on, impersonate and reconfigure KNX devices. This is particularly problematic, because the ETS5 gives the users the impression that project password would be used to encrypt the project information, not just for exported projects. Hence, it is likely that many users and system integrators have not taken additional steps to ensure the confidentiality of the project store. If the ETS5 would properly implement the encryption and a strong project password is chosen, it would provide a harder challenge for an attacker, even if they managed to get remote access to the computer.

The following confidential information are improperly encrypted:
- Project passwords
- FDSKs
- Backbone keys
- Device authentication codes and derived keys
- Device management passwords and derived keys
- User / tunneling passwords and derived keys
- Tool keys

The ETS5 Password Recovery Tool is a proof of concept that demonstrates the issue by decrypting and displaying the sensitive information. It was developed as part of the [coordinated vulnerability disclosure](#coordinated-vulnerability-disclosure) and is released with permission by the KNX Association. Publishing the tool serves the following purposes:
1. It publicly documents the security issue, thus allowing users to take precautions to mitigate the risks.
2. ~~The KNX Association does not plan to fix the issue in current or future versions of the ETS. Raising awareness about the design flaw might change their mind.~~ (see [coordinated vulnerability disclosure](#coordinated-vulnerability-disclosure) section for an update)
3. Disclosing the design flaw hopefully encourages the KNX Association and anyone reading this document to adopt better software engineering practices.
4. The ETS5 Password Recovery Tool could actually be useful in case someone forgot the password to their own project.

> **WARNING**: Only use this tool if you are legally authorized to view the project information. Circumventing security measurements, even ineffective ones, to gain access to information you are not permitted to see, may be a felony in your jurisdiction.

## Installation

The executable can be downloaded from the [release section](https://github.com/robertguetzkow/ets5-password-recovery/releases). It does not need to be installed and can be placed in any directory of choice. 

## Requirements

The software depends on .NET Framework 4.6 or later. Windows 10 already includes a suitable .NET version by default. Users of earlier Windows versions will have to [install a current .NET Framework version](https://dotnet.microsoft.com/download/dotnet-framework) to run the software.

## How does the password recovery work?

Contrary to what the user interface suggests, the ETS5 does not encrypt your locally stored project files in `C:\ProgramData\KNX\ETS5\ProjectStore` with the project password. Instead, it uses the hard-coded password `ETS5Password` and salt `Ivan Medvedev` to obfuscate specific attributes in the project's XML files. Hard-coded cryptographic secrets are against best practice, as explained in [CWE-798](https://cwe.mitre.org/data/definitions/798.html) and [CWE-321](https://cwe.mitre.org/data/definitions/321.html).

The process for the deobfuscation is:

1. The obfuscated attribute is Base64 encoded and needs to be decoded, see RFC 4648.
2. Get the byte representation of `Ivan Medvedev` as ASCII or UTF-8 encoded string.
3. Use the key derivation function implemented by [PasswordDeriveBytes](https://referencesource.microsoft.com/#mscorlib/system/security/cryptography/passwordderivebytes.cs) in the .NET Framework. [It is based on PBKDF1, but adds a counter to the key derivation algorithm](https://crypto.stackexchange.com/questions/22271/what-is-the-algorithm-behind-passwordderivebytes). In the ETS5 it is used with SHA-1 as hash function, 100 iterations, `ETS5Password` as password and the byte representation of `Ivan Medvedev` as salt. The first 32 bytes of the key derivation output will be used as key and the following 16 bytes as IV.
4. Decrypt the decoded attribute using AES-256 in CBC mode with the key and IV from step 3.
5. Remove the PKCS#7 padding and the result is the original value of the attribute.

An implementation of the deobfuscation can be found in the [Deobfuscator.cs](./ETS5PasswordRecovery/ETS5PasswordRecovery/Deobfuscator.cs) file. Since the password and salt are constant, it would be possible to precompute the key and IV to skip the key derivation. This is not done in the implementation of this software, as it is meant to show all steps of the deobfuscation. However, if you need the key and IV, they are listed below.

|     | Hex                                                                | Base64                                         |
| --- |------------------------------------------------------------------- | ---------------------------------------------- |
| Key | `22BD16CDBB96B0E18E977BB3FEFADD8886E7E38A2F8A6FD9D2F2F5663AC20371` | `Ir0WzbuWsOGOl3uz/vrdiIbn44ovim/Z0vL1ZjrCA3E=` |
| IV  | `8E977BB3FEFADD88E6AE6CBEAE3E7CAF`                                 | `jpd7s/763Yjmrmy+rj58rw==`                     |

Exported project files (`.knxproj`) are not affected by this design flaw, and thus this tool cannot be used to recover the project password for them. The `.knxproj` file is a ZIP file that contains another ZIP file with the sensitive information. The latter uses Deflate compression, ZipCrypto / PKWARE encryption and the project password for the derivation of the encryption key. 

## How was the design flaw discovered?

During the preparation for my thesis "Security Analysis of the KNXnet/IP Secure Protocol", I have investigated how the ETS5 stores project information. Since the ETS generates and stores cryptographic keys and passwords that are used by the KNX IP Secure devices to authenticate each other, provide confidentiality for multicast communication and secure the configuration of the devices, it is important to keep the information secret. If an attacker were to gain access to the project information stored by the ETS, it would fully compromise the security of the KNX installation.

For this reason, the project store of the ETS5 was inspected to check whether the data is stored in a way that ensures confidentiality. The project files in the `C:\ProgramData\KNX\ETS5\ProjectStore` are readable by every user account, no administrator rights are required. The following indicators were found that raised the suspicion that the data is not properly encrypted:
1. The XML configuration files are not encrypted as a whole. Only the sensitive attributes, such as the device authentication codes, device management passwords, FDSKs and tool keys, have been modified to not contain their plaintext value.
2. An attribute for the project password is stored in one of the XML files. This seemed slightly strange since with a proper implementation the encryption would derive the key from the project password, thus storing it would not have been strictly necessary. However, hypothetically, this could have been used to check if the entered password is correct before attempting to decrypt other attributes.
3. Two projects with different project passwords, but identical devices had the same values for some device specific attributes, like the FDSK. 

The last point clearly indicated that the project password was not used in the algorithm that modifies the attribute values. An example can be seen below, where the device authentication codes in two projects `P-02FB` and `P-0117` were set to identical values. The obfuscated outputs are also the same, despite different project passwords being used. Since there is no prompt to enter anything besides the project password when opening the project in the ETS5, this meant the key either had to be stored somewhere or it would be a simple obfuscation algorithms that does not require any key. It seemed likely that the solution was not ideal to ensure the confidentiality and potentially put KNX installations at risk.

#### The configuration files are not encrypted in their entirety.
```xml
<?xml version="1.0" encoding="utf-8"?>
<Area Id="P-02FB-0_A-2" Address="1" Name="New area" Puid="3">
  <Line Id="P-02FB-0_L-2" Address="0" Name="Main line" MediumTypeRefId="MT-5" Puid="4" />
  <Line Id="P-02FB-0_L-3" Address="1" Name="New line" MediumTypeRefId="MT-0" Puid="5">
    <DI Id="P-02FB-0_DI-1" Address="0" Name="" ProductRefId="M-0083_H-136-5-O0072_P-SCN.2DIP100.2E03" Hardware2ProgramRefId="M-0083_H-136-5-O0072_HP-0139-22-F35B-O0072" Comment="" Description="" SerialNumber="AIN1QBHl" ApplicationProgramLoaded="true" CommunicationPartLoaded="true" IndividualAddressLoaded="true" MediumConfigLoaded="true" ParametersLoaded="true" IsActivityCalculated="true" LastModified="2021-02-17T11:40:48.7900352Z" LastDownload="2021-02-17T11:41:20.6493908Z" LastUsedAPDULength="239" ReadMaxAPDULength="239" ReadMaxRoutingAPDULength="240" Puid="10">
      <AA>
        <Addr Address="1" Name="" />
        <Addr Address="2" Name="" />
        <Addr Address="3" Name="" />
        <Addr Address="4" Name="" />
      </AA>
      <IP MACAddress="CC:1B:E0:80:73:52" />
      <Security LoadedIPRoutingBackboneKey="QtyMMyRL83ckbNM16LkkGDqbFfKZFfvDFcxP/pcRYamQ+CbfDXRgF4AUs4O2zDPhSKtmb1Bh4gtdFl4f15ru8w==" DeviceAuthenticationCode="asYi9WMEdWQd3b5ix4zC6AnVDwf6q9RNu3CVg7kU3dg=" DeviceAuthenticationCodeHash="2jdAUgOzz58DDsCaJBuEJTyYphSlQIu1hlu7jWLPJN2k/8tUwijx66vq5L+CMOGlCULbZf46+HKdhKNnPTRnng==" LoadedDeviceAuthenticationCodeHash="2jdAUgOzz58DDsCaJBuEJTyYphSlQIu1hlu7jWLPJN2k/8tUwijx66vq5L+CMOGlCULbZf46+HKdhKNnPTRnng==" DeviceManagementPassword="7gFka6FhsAL1zVNBOFh0hnq1CV9cnXyGilxvTBcWTGM=" DeviceManagementPasswordHash="iWrnYw9Hrdwlwj4lxaIZMHRs2yoUITUb/yAM60UDNzYmC6uqytFHV0YNYy2X8aYK6OF3Azat3nl6tDLS8Nz+7A==" LoadedDeviceManagementPasswordHash="iWrnYw9Hrdwlwj4lxaIZMHRs2yoUITUb/yAM60UDNzYmC6uqytFHV0YNYy2X8aYK6OF3Azat3nl6tDLS8Nz+7A==" ToolKey="0K4xsJTiuldKFcWY7oJA/U6LfpGvQFIU2Tvfs3KvmromGujBOUBZzHZ2XSWghkqKPtw6VGkuCI2krK9etockjg==" LoadedToolKey="0K4xsJTiuldKFcWY7oJA/U6LfpGvQFIU2Tvfs3KvmromGujBOUBZzHZ2XSWghkqKPtw6VGkuCI2krK9etockjg==" SequenceNumber="98451652268" SequenceNumberTimestamp="2021-02-17T11:41:20.65441Z" />
      <BIs>
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-0" Password="FLFWR48BDHdiqhuTM+MvQxoNESTOS1aJZ3GzQR46/Og=" PasswordHash="apJu90et/dP7QaqlhT5vyMheYh20wVcR0Wscfn7+J/pa57SHo+f/FLBcqlwsDfgwtfPTXZfe0qM1SYqPa4fFOA==" />
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-1" Password="ZeF/OgICXjr8o8zM7YKuBtGmBlGZWqwy2xBVCBiPBrA=" PasswordHash="x0AcMNa6dKkZHjhH+TUL7LqJLpzUsCWLqhNifPBtC01SIIt6Grf/sEXS2aS9uw78C530aqFfhAb4sgDrt3LEqw==" />
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-2" Password="uERNFwucDdwS++Dk/uDXEGRTWfOW8cjtOpVefnqmqxE=" PasswordHash="sBUGjZn2sItVy0HiXoakuY4S2RpwWmEa56XUwUbnLlqfOSjTKsmdHVnmEIqoPr4jtvY8eYZfjHRGu1lkaQ2Dxg==" />
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-3" Password="hGLmGZG/rzCATiGFbUFswAr3bvKvF3OjNRfRi3e6XJ8=" PasswordHash="XunCV3zPe8KU9ZICNmIqF92aqf1YdWpstqJGwjfVGayjL/qtHo+ppAcscu36SZN6bo9+B6g3UAKe/9irETgwBw==" />
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-4" Password="C4eoaXfsVLUCPk6NfA/JXp/aECFQ5p/RFr+ZuHkMTFU=" PasswordHash="5VgGa19+oSu09/n93tMqJlZGN/LGbubqprf8n52nBjQSeNMvWNJwPyT7GWgVhBz05TPg2oaFpPZtuHp+oWqznQ==" />
      </BIs>
    </DI>
  </Line>
</Area>
```

#### A different project password does not change the output if the original attribute values are identical
```xml
<?xml version="1.0" encoding="utf-8"?>
<Area Id="P-0117-0_A-2" Address="1" Name="New area" Puid="3">
  <Line Id="P-0117-0_L-2" Address="0" Name="Main line" MediumTypeRefId="MT-5" Puid="4" />
  <Line Id="P-0117-0_L-3" Address="1" Name="New line" MediumTypeRefId="MT-0" Puid="5">
    <DI Id="P-0117-0_DI-1" Address="0" Name="" ProductRefId="M-0083_H-136-5-O0072_P-SCN.2DIP100.2E03" Hardware2ProgramRefId="M-0083_H-136-5-O0072_HP-0139-22-F35B-O0072" Comment="" Description="" SerialNumber="AIN1QBHl" ApplicationProgramLoaded="true" CommunicationPartLoaded="true" IndividualAddressLoaded="true" MediumConfigLoaded="true" ParametersLoaded="true" IsActivityCalculated="true" LastModified="2021-03-23T18:06:36.6665934Z" LastDownload="2021-03-23T18:07:50.953092Z" LastUsedAPDULength="239" ReadMaxAPDULength="239" ReadMaxRoutingAPDULength="240" Puid="10">
      <AA>
        <Addr Address="1" Name="" />
        <Addr Address="2" Name="" />
        <Addr Address="3" Name="" />
        <Addr Address="4" Name="" />
      </AA>
      <IP MACAddress="CC:1B:E0:80:73:52" />
      <Security LoadedIPRoutingBackboneKey="2UG/kJv37OUdas3dk6y0R4sOD1iY2Ka6g3gRljXMPmxzvtrrM4pfoa2xiCzK/G0Qob+eWPDV/+gUAv4QNuUrCQ==" DeviceAuthenticationCode="asYi9WMEdWQd3b5ix4zC6AnVDwf6q9RNu3CVg7kU3dg=" DeviceAuthenticationCodeHash="2jdAUgOzz58DDsCaJBuEJTyYphSlQIu1hlu7jWLPJN2k/8tUwijx66vq5L+CMOGlCULbZf46+HKdhKNnPTRnng==" LoadedDeviceAuthenticationCodeHash="2jdAUgOzz58DDsCaJBuEJTyYphSlQIu1hlu7jWLPJN2k/8tUwijx66vq5L+CMOGlCULbZf46+HKdhKNnPTRnng==" DeviceManagementPassword="7gFka6FhsAL1zVNBOFh0hnq1CV9cnXyGilxvTBcWTGM=" DeviceManagementPasswordHash="iWrnYw9Hrdwlwj4lxaIZMHRs2yoUITUb/yAM60UDNzYmC6uqytFHV0YNYy2X8aYK6OF3Azat3nl6tDLS8Nz+7A==" LoadedDeviceManagementPasswordHash="iWrnYw9Hrdwlwj4lxaIZMHRs2yoUITUb/yAM60UDNzYmC6uqytFHV0YNYy2X8aYK6OF3Azat3nl6tDLS8Nz+7A==" ToolKey="OURAfvcx6fA5geWOdix7RKxFjOfU7Ru4uqB1/N1ZL9bELq+QFCRNYVtwmtEdLxAMNpAu4mEupE7uIzsyuzoWag==" LoadedToolKey="OURAfvcx6fA5geWOdix7RKxFjOfU7Ru4uqB1/N1ZL9bELq+QFCRNYVtwmtEdLxAMNpAu4mEupE7uIzsyuzoWag==" SequenceNumber="98454853930" SequenceNumberTimestamp="2021-03-23T18:07:50.9689425Z" />
      <BIs>
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-0" Password="kRilPgLr/m37OtDWtD3C1LnnMc9kLjwB3pqOpA0ulEg=" PasswordHash="ctQi03SyQ+LwEio/zA+uQZW3SLKM/xdC3EWK5kb1W7/2e5ulKNT3BL83GH2zfKh796R+N4fW/9Q4qh7DT6+WjQ==" />
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-1" Password="ZeF/OgICXjr8o8zM7YKuBtGmBlGZWqwy2xBVCBiPBrA=" PasswordHash="x0AcMNa6dKkZHjhH+TUL7LqJLpzUsCWLqhNifPBtC01SIIt6Grf/sEXS2aS9uw78C530aqFfhAb4sgDrt3LEqw==" />
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-2" Password="uERNFwucDdwS++Dk/uDXEGRTWfOW8cjtOpVefnqmqxE=" PasswordHash="sBUGjZn2sItVy0HiXoakuY4S2RpwWmEa56XUwUbnLlqfOSjTKsmdHVnmEIqoPr4jtvY8eYZfjHRGu1lkaQ2Dxg==" />
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-3" Password="hGLmGZG/rzCATiGFbUFswAr3bvKvF3OjNRfRi3e6XJ8=" PasswordHash="XunCV3zPe8KU9ZICNmIqF92aqf1YdWpstqJGwjfVGayjL/qtHo+ppAcscu36SZN6bo9+B6g3UAKe/9irETgwBw==" />
        <BI RefId="M-0083_A-0139-22-F35B-O0072_BI-4" Password="C4eoaXfsVLUCPk6NfA/JXp/aECFQ5p/RFr+ZuHkMTFU=" PasswordHash="5VgGa19+oSu09/n93tMqJlZGN/LGbubqprf8n52nBjQSeNMvWNJwPyT7GWgVhBz05TPg2oaFpPZtuHp+oWqznQ==" />
      </BIs>
    </DI>
  </Line>
</Area>
```

As the observations strongly hinted at the use of an insecure approach, possibly due to the use of a hard-coded cryptographic key, it was necessary to investigate how the attribute values were modified. The intention was to identify a potential security flaw, which could then be reported to the vendor and have it fixed, improving the security for all users. Assessing whether the implementation provides adequate confidentiality meant the ETS5 had to be reverse engineered. 

Since the ETS5 is based on the .NET framework, which was immediately apparent from the DLLs used, decompiling could be easily accomplished through [ILSpy](https://github.com/icsharpcode/ILSpy). The binary had been obfuscated with Dotfuscator, presumably to make reverse engineering efforts more difficult. However, class and function names were surprisingly left mostly intact. Hence, the chosen approach was to search for classes and functions which seemed related to processing the XML files, encryption, decryption, obfuscation, deobfuscation, keys or passwords. This led to the discovery of `Knx.Ets.ObjectModel.Import.PasswordDescrambler.Scramble` and `Knx.Ets.ObjectModel.Import.Encryption.EncryptString`, that are called on the obfuscated values of attributes stored in the XML files. There was no key material being passed into the functions, it only used constant values to derive a key which was then utilized to encrypt / decrypt the attributes using AES-256 in CBC mode. It was apparent that hard-coded credentials were used to derive a key. The Dotfuscator changed the control flow and inserted superfluous operations, but the calls to the functions from the .NET framework could not be hidden. Thus, it was possible to write a specification for the (de-)obfuscation at this stage for a semi-clean-room approach. The only missing part was the string used in the key derivation that had been obscured by dotfuscator. [De4dot](https://github.com/de4dot/de4dot) was chosen to reverse the string obfuscation, which revealed the `ETS5Password` password. The IV was already readable prior to applying De4dot as it had been defined as a byte sequence. Due to personal curiosity, it was discovered that those were not random bytes, but actually the ASCII / UTF-8 byte representation of the string `Ivan Medvedev`.

As the design flaw poses a risk to KNX installations, the issue had to be reported to the KNX Association. A proof of concept implementation was needed to ensure that the issue could be demonstrated, if asked for. To avoid any copyright violations, the proof of concept was implemented based on the specification that had been noted. This was done to avoid any reuse of code from the original software. The application of Dotfuscator also made sure that the original and even unobfuscated code were unusable for a clean implementation anyway, which ensured that even unintentional copying of the original was unlikely.

For details on the coordinated disclosure following the development of the proof of concept, see the section [Coordinated Vulnerability Disclosure](#coordinated-vulnerability-disclosure).

## How can the risk be mitigated?

Unfortunately, as of 2021-07-18, there is no patched ETS version available. Therefore, additional measurements outside the ETS5 are necessary to address the risks. The subsections below explain different approaches that can be taken depending on the threat model you are assuming and trying to protect against.

### Full disk encryption
- Solution:
    - Encrypt the entire hard drive with Windows BitLocker or a third-party software like VeraCrypt.
- Pros:
    - All data on the hard drive is encrypted and inaccessible to attackers as long as the device is turned off. This assumes that a complex password was used.
    - Windows already provides an easy-to-use solution with BitLocker on certain Windows versions, and open source software solutions are also readily available.
- Cons:
    - It does not provide confidentiality while the computer is running. If an attacker is able to gain access to one of the user accounts / exploit an RCE, they are able to access the project information as plaintext.

### File / folder encryption
- Solution:
    - Encrypt the `C:\ProgramData\KNX\ETS5\ProjectStore` directory and all files contained within using Windows Encrypted File System (EFS).
- Pros:
    - The project information are encrypted and inaccessible to attackers as long as the device is turned off.
    - If the EFS is set up by the administrator account or a dedicated account for running the ETS, other user accounts are unable to access the files. This should provide protection if an attacker gains access to a user account on the computer but not the one that configured the EFS. A strong password for the administrator or ETS account is needed, as it is used to protect the key material.
- Cons:
    - It does not always provide confidentiality while the computer is running. If an attacker is able to gain access to the user account that set up the EFS or is able to run code in that user's context, then they are still able to access the project information as plaintext.

### Encrypted volume
- Solution:
    - Create an encrypted volume with a third-party software like VeraCrypt and only store the project information there.
- Pros:
    - The project information are encrypted and inaccessible to attackers as long as the volume is not mounted. This assumes that a complex password or hardware token was used for the volume encryption.
    - Provides limited protection even in the case that the attacker is able to gain administrator rights. As long as the volume is not mounted while the attacker has access to the system, the data in the encrypted volume should stay confidential.
- Cons:
    - The original project files need to be transferred into the encrypted volume and then securely deleted, so that the original unencrypted files cannot be recovered.
    - A symbolic link needs to be created in order to make the mounted volume appear under `C:\ProgramData\KNX\ETS5\ProjectStore`.
    - In general, it is more complicated to set up.

## Coordinated Vulnerability Disclosure

- 2021-06-26 - Issue reported to the KNX Association
- 2021-07-09 - KNX Association confirmed issue
- 2021-07-12 - KNX Association permitted immediate disclosure
- 2021-07-18 - Public disclosure
- 2021-07-19 - [CVE-2021-36799](https://cve.mitre.org/cgi-bin/cvename.cgi?name=CVE-2021-36799) assigned

According to Joost Demarest, CTO and CFO of the KNX Association, ETS5 will not receive any patches as development for that version has already been concluded. He permitted immediate publication of the issue on 2021-07-12, forgoing the offered 90 days delay for the disclosure.

### Update 2021-11-08
Due to a misunderstanding the README previously claimed that the KNX Association plans to address the issue in ETS6. This is not the case. The KNX Association has clarified on 2021-10-25 that they do not plan to fix this issue, as they do not consider it the responsibility of the ETS to securely store cryptographic key material when it is not being exported.

### Update 2021-11-10
The KNX Association has contacted me and explained that they have revised their plans. They now intend to document the shortcomings of the current ETS version and properly encrypt the project store in a future version of ETS6.

## License

The project is distributed under the [MIT license](./LICENSE).

## Change log

### 1.0.0 - 2021-07-18
**Commit hash:** 
- [c6a3750cefa74d84c5886097cdd0f30dc1bd0dd1](https://github.com/robertguetzkow/ets5-password-recovery/commit/c6a3750cefa74d84c5886097cdd0f30dc1bd0dd1)

**Download:** 
- [Source code](https://github.com/robertguetzkow/ets5-password-recovery/archive/refs/tags/v1.0.0.zip)
- [Executable](https://github.com/robertguetzkow/ets5-password-recovery/releases/download/v1.0.0/ETS5PasswordRecovery.exe)

**Changes:**
- Initial version

