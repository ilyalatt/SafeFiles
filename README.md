# SafeFiles

[![NuGet version](https://badge.fury.io/nu/SafeFiles.svg)](https://www.nuget.org/packages/SafeFiles)

A safe way to update files. Especially useful for sessions or settings.

## Why

You can not call `File.WriteAllText` and expect that the file will be consistent.
Power shortage or sudden death of a process can corrupt the file.
A chance is small but it accumulates over time.
There is a simple solution.
Create a `.part` file, write the content, then replace the original file with the `.part` file.
If this process is aborted then the original file can be used.
This library wraps this logic into a simple API.

## Usage

```C#
SafeFile.ReadAllText("test.txt");
SafeFile.WriteAllText("test.txt", "123");
SafeFile.Read("test.txt", async stream => stream.ReadAsync(...));
SafeFile.Write("test.txt", async stream => stream.WriteAsync(...));
SafeFile.Replace("directory/test.txt", "test-2.txt");
SafeFile.Exists("test.txt");
SafeFile.Delete("test.txt");
```

## Notes

The library relies on `File.Move` and `File.Replace` being atomic.
Linux `rename` [is atomic](`https://stackoverflow.com/a/7054851`).
Windows `ReplaceFileA` for Windows [is atomic](https://github.com/bobvanderlinden/sharpfilesystem/issues/8).
It is forgivable for them to leave source and destination files both pointing to the same inode.
However there are file systems like FAT32 that [do not guarantee consistency](https://yhbt.net/lore/all/20191022105413.pj6i3ydetnfgnkzh@pali).
File systems like EXT4, ZFS, BTRFS, APFS, NTFS should be fine.
