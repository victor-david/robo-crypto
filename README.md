#README

**RoboCrypto** is a command line application that enables you to copy one directory (and its sub-directories) to another,
encrypting all files in the process. In addition, file names and directory names are converted to hash values to prevent
inferring their worth or purpose.

##Encryption

**RoboCrypto** works like RoboCopy (when used with its synchronizing switches) in the sense that only new and changed files
are copied (and encrypted) to the target. In addition, **RoboCrypto** hashes the file and directory names in the process.
For instance, when encrypting:

RoboCrypto C:\MySource C:\MyTarget H:\MyKeyFile.bin /e

C:\MySource\SecretFiles\FinanceReport.xlsx

becomes

C:\MyTarget\d9e8fb147ef70faa\6097aeb067f3d1c10f51ca8d.aes

The folder used as the target root is not hashed.

After running an encryption operation, a second run does nothing (until files are added or modified in the source)

Many thanks to Stan Drapkin @sdrapkin - the cryptographic methods used in **RoboCrypto** come from his crypto library:

[GitHub repository](https://github.com/sdrapkin/SecurityDriven.Inferno)

[Documentation](http://securitydriven.net/inferno/)

##Key File
You must use a key file. Entering the key manually is not supported.

##Encrypting Again
Normally, **RoboCrypto** only copies and encrypts a file if it is new or has changed. There may be times when you need to
force all files to be encrypted again, for example if you change the encryption key. To do so, use the /f switch

RoboCrypto C:\MySource C:\MyTarget H:\MyKeyFile.bin /e /f

This causes all files to be encrypted, whether they have changed or not. You can also use the force-with-timestamp switch:

RoboCrypto C:\MySource C:\MyTarget H:\MyKeyFile.bin /e /ft

As with /f - this causes all files to be encrypted. In addition, the timestamps of all files (source and target) are incremented
by one minute in order to ensure that any downstream processing (such as Dropbox, see Backup Scenario section below) recognizes
that the files need to be processed.

##Decryption

To decrypt:

RoboCrypto C:\MyTarget C:\MyRecoveredFiles H:\MyKeyFile.bin /d

Decryption requires that the target directory (the one in which the decrypted file and directories will go) be empty.

##Backup Scenario
My purpose for developing **RoboCrypto** was to encrypt a certain set of files as they were copied into my Dropbox directory
as part of my automated incremental backup. I prefer to push things to the cloud when I'm ready, so I don't work with files
directly in my Dropbox directory. Instead, I run a batch file at the end of the day:

backup.cmd

rem RoboCopy to copy only new and changed files

RoboCopy D:\MyFiles E:\Dropbox\MyFiles /E /R:2 /W:3 /NJH /NJS /NDL /NP /NS /TS /XJ /XO

RoboCopy (other directories) etc.

rem See if the USB stick is there

if exist H:\MyKeyFile.bin (

  RoboCrypto D:\MyConfidentialFiles E:\Dropbox\MyEncryptedFiles H:\MyKeyFile.bin /e

)

##Key Generation
**RoboCrypto** is accompanied by **KeyGen**, a simple utility to create a key file of various lengths.
