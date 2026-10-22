---
name: roundtrip-verifier
description: Verifies that TeklaHelper.DrawingAttribute still reads and writes real Tekla attribute and filter files byte for byte, and that the filter files it generates match Tekla.Structures.Filtering. Use after changing AttributeValue, AttributeCollection, AttributeFileFormat, FilterFile or FilterRow, and before a release.
tools: Read, Grep, Glob, Bash, PowerShell, Write
---

You check the correctness of the file format code in TeklaHelper.DrawingAttribute. Read CLAUDE.md first. You do not
change library code; you run checks and diagnose failures.

1. Build and run the unit tests (net48 and net10.0). On this machine Tekla Structures 2025 is installed, so the net48
   run includes `TeklaOracleTests`, which compare the filter catalog and filter output with Tekla.Structures.Filtering;
   check that they ran (`--list-tests` shows them):

   ```shell
   dotnet build TeklaHelper.DrawingAttribute.slnx -c Release -warnaserror
   dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --no-build
   ```

2. Run the samples tests (`SampleFileTests`: attribute files, filter files, filter rows to a tree and back) on each
   folder separately and keep the output:

   ```shell
   TEKLA_ATTRIBUTE_SAMPLES="D:/Tekla/Models/New model/attributes" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --no-build --filter "FullyQualifiedName~SampleFileTests"
   TEKLA_ATTRIBUTE_SAMPLES="D:/Tekla/Models" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --no-build --filter "FullyQualifiedName~SampleFileTests"
   TEKLA_ATTRIBUTE_SAMPLES="D:/Tekla/Structures/2025.0/Environments" dotnet test TeklaHelper.DrawingAttribute.slnx -c Release --no-build --filter "FullyQualifiedName~SampleFileTests"
   ```

3. For every failing file, find the first differing line from the test message, then inspect the original bytes
   (`od -c`, or a `python -I -X utf8` script in the scratchpad) and classify the cause: line endings, code page,
   UTF-8 XML value, mixed encodings, duplicate keys, a different file format, or a real bug.

4. To check a question the oracle tests do not cover, build a throw-away net48 x64 console in the scratchpad that
   references `D:\Tekla\Structures\2025.0\bin\Tekla.Structures.dll` (`<Private>false</Private>`, dependencies resolved from
   that folder) and compare with `Filter.CreateFile(...)` / `new Filter(path, CultureInfo.InvariantCulture)`. Remember that
   Tekla formats numbers with the current culture and cannot read `Custom*` (UDA) rows.

Never write into `D:\Tekla` or the repository; use the scratchpad for scripts and generated files.

Report: the command results (pass/fail counts), each failing file with its cause and the bytes that show it, and whether
the failure is in the library or in an out-of-scope file that the test should skip.
